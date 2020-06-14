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
using System.Windows.Input;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Treeview.Model.Win32;
using MPTagThat.Treeview.ViewModels;
using Prism.Ioc;
using Shell32;

namespace MPTagThat.Treeview.Model
{
  /// <summary>
  /// Data Provider for supporting Folders in Tree views
  /// </summary>
  public class TreeViewDataProvider : ITreeviewDataProvider
  {
    #region fields

    private readonly NLogLogger log = ContainerLocator.Current.Resolve<ILogger>()?.GetLogger;

    /// <summary>
    ///   drive tree node (My computer) root collection
    /// </summary>
    private ObservableCollection<TreeItem> _rootCollection = new ObservableCollection<TreeItem>();

    /// <summary>
    ///   drive tree node (Network) root collection
    /// </summary>
    private ObservableCollection<TreeItem> _rootCollectionNetwork = new ObservableCollection<TreeItem>();

    /// <summary>
    ///   Shell32 Com Object
    /// </summary>
    private readonly Shell32Namespaces _shell = new Shell32Namespaces();


    /// <summary>
    ///   show only filesystem
    /// </summary>
    private readonly bool _showAllShellObjects = true;

    #endregion

    #region ITreeViewFolderBrowserDataProvider Members

    /// <summary>
    /// Supporting Context menus
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="node"></param>
    public virtual void QueryContextMenuItems(TreeViewHelper helper, TreeItem node) { }

    /// <summary>
    /// Return the Drives for My Computer or Network
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="isNetwork"></param>
    /// <returns></returns>
    public virtual ObservableCollection<TreeItem> RequestDriveCollection(TreeViewHelper helper, bool isNetwork)
    {
      log.Trace(">>>");
      if (isNetwork)
      {
        return _rootCollectionNetwork;
      }
      log.Trace("<<<");
      return _rootCollection;
    }

    /// <summary>
    /// Read Sub Folders of the selected Parent Node
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="parent"></param>
    public virtual void RequestSubDirs(TreeViewHelper helper, TreeItem parent)
    {
      log.Trace(">>>");
      helper.TreeView.Cursor = Cursors.Wait;

      FolderItem2 folderItem = ((FolderItem2)parent.Item);

      log.Trace($"Requesting Subfolders of {folderItem.Name}");

      if (_shell.Shell.NameSpace(ShellSpecialFolderConstants.ssfDRIVES).Title == folderItem.Name)
      {
        FillMyComputer(folderItem, parent.Nodes, helper);
      }
      else
      {
        var nodes = new List<TreeItem>();
        foreach (FolderItem2 fi in ((Folder2) folderItem.GetFolder).Items())
        {
          if (!_showAllShellObjects && !fi.IsFileSystem || !fi.IsFolder)
          {
            continue;
          }
          var node = CreateTreeNode(helper, fi.Name, fi.Path, false, fi);
          nodes.Add(node);
        }

        // Sort the Directories, as Samba might return unsorted
        var nodesArray = nodes.ToArray();
        Array.Sort(nodesArray,
          (p1, p2) => string.CompareOrdinal(p1.Name, p2.Name));

        parent.Nodes.AddRange(nodesArray);
        helper.TreeView.Cursor = Cursors.Arrow;
      }
      log.Trace("<<<");
    }

    /// <summary>
    /// Create the Root Collection when building the Tree View
    /// </summary>
    /// <param name="helper"></param>
    public virtual void RequestRoot(TreeViewHelper helper)
    {
      log.Trace(">>>");
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
            log.Trace($"Folder: {fi.Name} | Type: {fi.Type} | IsFolder: {fi.IsFolder} | IsFileSystem: {fi.IsFileSystem}");
            // Don't list Non-Folders, Control Panel and Waste Basket
            if (!fi.IsFolder)
            {
              continue;
            }

            if (_shell.Shell.NameSpace(ShellSpecialFolderConstants.ssfBITBUCKET).Title == fi.Name)
            {
              continue;
            }

            if (fi.Path == "::{26EE0668-A00A-44D7-9371-BEB064C98683}")
            {
              continue;
            }

            // Create the Tree Node
            var node = CreateTreeNode(helper, fi.Name, fi.Path, true, fi);
            desktopNode.Nodes.Add(node);

            // Handle My Computer
            if (_shell.Shell.NameSpace(ShellSpecialFolderConstants.ssfDRIVES).Title == fi.Name)
            {
              FillMyComputer(fi, node.Nodes, helper);
            }

            // Add to Network Node
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
      log.Trace("<<<");
    }

    #endregion

    #region internal interface

    /// <summary>
    /// Fill the "My Computer" or "This Pc" collection 
    /// </summary>
    /// <param name="folderItem"></param>
    /// <param name="parentCollection"></param>
    /// <param name="helper"></param>
    protected virtual void FillMyComputer(FolderItem folderItem, ObservableCollection<TreeItem> parentCollection,
      TreeViewHelper helper)
    {
      log.Trace(">>>");
      _rootCollection = parentCollection;
      Logicaldisk.LogicaldiskCollection logicalDisks = null;
      
      // get wmi logical disk's if we have to 			
      if (helper.TreeView.DriveTypes != Enums.DriveTypes.All)
      {
        logicalDisks = Logicaldisk.GetInstances(null, GetWmiQueryStatement(helper.TreeView));
      }
      
      foreach (FolderItem fi in ((Folder)folderItem.GetFolder).Items())
      {
        log.Trace($"Folder: {fi.Name} | Type: {fi.Type} | IsFolder: {fi.IsFolder} | IsFileSystem: {fi.IsFileSystem}");
        // only File System shell objects ?
        if (!_showAllShellObjects && !fi.IsFileSystem)
        {
          continue;
        }
        
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

          if (skipDrive)
          {
            continue;
          }
        }
        // create new node
        var node = CreateTreeNode(helper, fi.Name, fi.Path, false, fi);
        parentCollection.Add(node);
      }
      log.Trace("<<<");
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
    protected virtual TreeItem CreateTreeNode(TreeViewHelper helper, string text, string path,
                                                  bool isSpecialFolder, object item)
    {
      return new TreeItem(text, isSpecialFolder) { Path = path, Item = item };
    }

    /// <summary>
    ///   Gets the WMI query string based on the current drive types.
    /// </summary>
    /// <returns></returns>
    protected virtual string GetWmiQueryStatement(TreeviewViewModel treeView)
    {
      if ((treeView.DriveTypes & Enums.DriveTypes.All) == Enums.DriveTypes.All)
      {
        return string.Empty;
      }

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
      
      return where;
    }

    #endregion

    public override string ToString()
    {
      return "Standard Provider";
    }
  }
}
