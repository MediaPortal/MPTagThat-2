#region Copyright (C) 2020 Team MediaPortal
// Copyright (C) 2020 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MPTagThat is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPTagThat is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPTagThat. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using MPTagThat.Treeview.Model.Win32;
using MPTagThat.Treeview.ViewModels;
using Shell32;

namespace MPTagThat.Treeview.Model
{
  public class TreeViewFolderBrowserDataProvider : ITreeviewDataProvider
  {
    #region fields

    /// <summary>
    ///   drive tree node (mycomputer) root collection
    /// </summary>
    private ObservableCollection<NavTreeItem> _rootCollection = new ObservableCollection<NavTreeItem>();

    /// <summary>
    ///   drive tree node (Network) root collection
    /// </summary>
    private ObservableCollection<NavTreeItem> _rootCollectionNetwork = new ObservableCollection<NavTreeItem>();

    /// <summary>
    ///   Shell32 Com Object
    /// </summary>
    private readonly Shell32Namespaces _shell = new Shell32Namespaces();


    /// <summary>
    ///   show only filesystem
    /// </summary>
    private bool _showAllShellObjects;

    #endregion

    #region ITreeViewFolderBrowserDataProvider Members

    public virtual void QueryContextMenuItems(TreeViewFolderBrowserHelper helper, NavTreeItem node) { }

    public virtual ObservableCollection<NavTreeItem> RequestDriveCollection(TreeViewFolderBrowserHelper helper, bool isNetwork)
    {
      if (isNetwork)
      {
        return _rootCollectionNetwork;
      }
      return _rootCollection;
    }

    public virtual void RequestSubDirs(TreeViewFolderBrowserHelper helper, NavTreeItem parent)
    {
      FolderItem2 folderItem = ((FolderItem2)parent.Item);

      if (_shell.Shell.NameSpace(ShellSpecialFolderConstants.ssfDRIVES).Title == folderItem.Name)
      {
        FillMyComputer(folderItem, parent.Nodes, helper);
      }
      else
      {
        var nodes = new List<NavTreeItem>();
        foreach (FolderItem2 fi in ((Folder2) folderItem.GetFolder).Items())
        {
          if (!_showAllShellObjects && !fi.IsFileSystem || !fi.IsFolder) continue;
          var node = CreateTreeNode(helper, fi.Name, fi.Path, false, fi);
          nodes.Add(node);
        }

        // Sort the Directories, as Samba might return unsorted
        var nodesArray = nodes.ToArray();
        Array.Sort(nodesArray,
          (p1, p2) => string.CompareOrdinal(p1.Name, p2.Name));

        parent.Nodes.AddRange(nodesArray);
      }
    }

    public virtual void RequestRoot(TreeViewFolderBrowserHelper helper)
    {
      // setup up root node collection
      switch (helper.TreeView.RootFolder)
      {
        case Environment.SpecialFolder.Desktop:
          Folder2 desktopFolder = (Folder2)_shell.GetDesktop();
          // create root node <Desktop>
          var desktopNode = CreateTreeNode(helper, desktopFolder.Title, desktopFolder.Self.Path,
                                                    true, desktopFolder.Self);
          helper.TreeView.Nodes.Add(desktopNode);
          foreach (FolderItem fi in desktopFolder.Items())
          {
            // Don't list Non-Folders, Control Panel and Waste BAsket
            if (!fi.IsFolder) continue;
            if (_shell.Shell.NameSpace(ShellSpecialFolderConstants.ssfBITBUCKET).Title == fi.Name) continue;
            if (fi.Path == "::{26EE0668-A00A-44D7-9371-BEB064C98683}") continue;

            var node = CreateTreeNode(helper, fi.Name, fi.Path, true, fi);
            desktopNode.Nodes.Add(node);

            if (_shell.Shell.NameSpace(ShellSpecialFolderConstants.ssfDRIVES).Title == fi.Name)
            {
              FillMyComputer(fi, node.Nodes, helper);
            }

            if (fi.Path == "::{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}")
            {
              _rootCollectionNetwork = node.Nodes;
            }
          }
          break;
        case Environment.SpecialFolder.MyComputer:
          FillMyComputer(((Folder2)_shell.Shell.NameSpace(ShellSpecialFolderConstants.ssfDRIVES)).Self,
            helper.TreeView.Nodes, helper);
          break;
        default:
          // create root node with specified SpecialFolder
          Folder2 root = (Folder3)_shell.Shell.NameSpace(helper.TreeView.RootFolder);
          var rootNode = CreateTreeNode(helper, root.Title, root.Self.Path, false, root);
          //rootNode.Tag = root.Self;
          helper.TreeView.Nodes.Add(rootNode);
          _rootCollection = rootNode.Nodes;
          break;
      }
    }

    #endregion

    #region internal interface

    protected virtual void FillMyComputer(FolderItem folderItem, ObservableCollection<NavTreeItem> parentCollection,
      TreeViewFolderBrowserHelper helper)
    {
      _rootCollection = parentCollection;
      Logicaldisk.LogicaldiskCollection logicalDisks = null;
      // get wmi logical disk's if we have to 			
      if (helper.TreeView.DriveTypes != Enums.DriveTypes.All)
        logicalDisks = Logicaldisk.GetInstances(null, GetWMIQueryStatement(helper.TreeView));
      //
      foreach (FolderItem fi in ((Folder)folderItem.GetFolder).Items())
      {
        // only File System shell objects ?
        if (!_showAllShellObjects && !fi.IsFileSystem) continue;
        // check drive type 
        if (fi.IsFileSystem && logicalDisks != null)
        {
          bool skipDrive = true;
          foreach (Logicaldisk lg in logicalDisks)
          {
            if (lg.Name + "\\" == fi.Path)
            {
              skipDrive = false;
              break;
            }
          }
          if (skipDrive) continue;
        }
        // create new node
        var node = CreateTreeNode(helper, fi.Name, fi.Path, false, fi);
        parentCollection.Add(node);
      }
    }


    /// <summary>
    ///   Creates a new node and assigns a icon
    /// </summary>
    /// <param name = "helper"></param>
    /// <param name = "text"></param>
    /// <param name = "path"></param>
    /// <param name="isSpecialFolder"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    protected virtual NavTreeItem CreateTreeNode(TreeViewFolderBrowserHelper helper, string text, string path,
                                                  bool isSpecialFolder, object item)
    {
      return new NavTreeItem(text, isSpecialFolder) { Path = path, Item = item };
    }

    /// <summary>
    ///   Gets the WMI query string based on the current drive types.
    /// </summary>
    /// <returns></returns>
    protected virtual string GetWMIQueryStatement(TreeviewViewModel treeView)
    {
      if ((treeView.DriveTypes & Enums.DriveTypes.All) == Enums.DriveTypes.All) return string.Empty;
      var where = string.Empty;
      var array = Enum.GetValues(typeof(Enums.DriveTypes));
      foreach (Enums.DriveTypes type in array)
      {
        if ((treeView.DriveTypes & type) == type)
        {
          if (where == string.Empty)
          {
            where += "drivetype = " +
                     Enum.Format(typeof(Enums.Win32_LogicalDiskDriveTypes),
                                 Enum.Parse(typeof(Enums.Win32_LogicalDiskDriveTypes), type.ToString(), true), "d");
          }
          else
          {
            where += " OR drivetype = " +
                     Enum.Format(typeof(Enums.Win32_LogicalDiskDriveTypes),
                                 Enum.Parse(typeof(Enums.Win32_LogicalDiskDriveTypes), type.ToString(), true), "d");
          }
        }
      }
      //
      return where;
    }

    #endregion

    public override string ToString()
    {
      return "Standard Provider";
    }
  }
}
