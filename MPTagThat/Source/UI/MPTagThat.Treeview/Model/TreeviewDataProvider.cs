#region Copyright (C) 2022 Team MediaPortal
// Copyright (C) 2022 Team MediaPortal
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

# region 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Treeview.Model.Win32;
using MPTagThat.Treeview.ViewModels;
using Prism.Ioc;
using Shell32;
using Syncfusion.UI.Xaml.TreeView.Engine;

#endregion

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
    public virtual void QueryContextMenuItems(TreeViewHelper helper, TreeViewNode node) { }

    /// <summary>
    /// Create the Root Collection when building the Tree View
    /// </summary>
    /// <param name="helper"></param>
    public virtual void CreateRootNode(TreeViewHelper helper)
    {
      log.Trace(">>>");
      switch (helper.Model.RootFolder)
      {
        case Environment.SpecialFolder.Desktop:
          Folder2 desktopFolder = (Folder2)_shell.GetDesktop();
          // create root node <Desktop>
          var desktopNode = CreateTreeNode(helper, desktopFolder.Title, desktopFolder.Self.Path,
            true, desktopFolder.Self);
          desktopNode.IsRoot = true;
          helper.Model.Nodes.Add(desktopNode);
          break;
      }
      log.Trace("<<<");
    }

    /// <summary>
    /// Only valid in Database Provider
    /// </summary>
    public void ClearSelectedDatabaseNode() {}

    /// <summary>
    /// Read Sub Folders of the selected Parent Node
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="parent"></param>
    public virtual void RequestSubDirs(TreeViewHelper helper, TreeViewNode parent)
    {
      log.Trace(">>>");
      helper.Model.Cursor = Cursors.Wait;

      FolderItem2 folderItem = ((FolderItem2)(parent.Content as TreeItem).Item);

      log.Trace($"Requesting Subfolders of {folderItem.Name}");

      if (_shell.Shell.NameSpace(ShellSpecialFolderConstants.ssfDRIVES).Title == folderItem.Name)
      {
        FillMyComputer(folderItem, parent, helper);
      }
      else
      {
        var nodes = new List<TreeItem>();
        foreach (FolderItem2 fi in ((Folder2)folderItem.GetFolder).Items())
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
          (p1, p2) => string.Compare(p1.Name, p2.Name));

        parent.PopulateChildNodes(nodesArray);
      }
      helper.Model.Cursor = Cursors.Arrow;
      log.Trace("<<<");
    }

    /// <summary>
    /// Create the Root Collection when building the Tree View
    /// </summary>
    /// <param name="helper"></param>
    public virtual void RequestRoot(TreeViewHelper helper, TreeViewNode parent)
    {
      log.Trace(">>>");
      // setup up root node collection
      switch (helper.Model.RootFolder)
      {
        case Environment.SpecialFolder.Desktop:
          Folder2 desktopFolder = (Folder2)_shell.GetDesktop();

          var nodes = new List<TreeItem>();
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
            nodes.Add(node);

            // Handle My Computer
            if (_shell.Shell.NameSpace(ShellSpecialFolderConstants.ssfDRIVES).Title == fi.Name)
            {
              //FillMyComputer(fi, node.Nodes, helper);
            }

            // Add to Network Node
            if (fi.Path == "::{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}")
            {
              //_rootCollectionNetwork = node.Nodes;
            }
          }
          parent.PopulateChildNodes(nodes);
          break;

        //case Environment.SpecialFolder.MyComputer:
        //FillMyComputer(((Folder2)_shell.Shell.NameSpace(ShellSpecialFolderConstants.ssfDRIVES)).Self,
        //  helper.Model.Nodes, helper);
        //break;

        default:
          // create root node with specified SpecialFolder
          Folder2 root = (Folder3)_shell.Shell.NameSpace(helper.Model.RootFolder);
          var rootNode = CreateTreeNode(helper, root.Title, root.Self.Path, false, root);
          //rootNode.Tag = root.Self;
          helper.Model.Nodes.Add(rootNode);
          //_rootCollection = rootNode.Nodes;
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
    protected virtual void FillMyComputer(FolderItem folderItem, TreeViewNode node,
      TreeViewHelper helper)
    {
      log.Trace(">>>");
      var selectedDrives = DriveInfo.GetDrives()
        .Where(drive => drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable || drive.DriveType == DriveType.Network);

      var items = new List<TreeItem>();
      foreach (FolderItem fi in ((Folder)folderItem.GetFolder).Items())
      {
        log.Trace($"Folder: {fi.Name} | Type: {fi.Type} | IsFolder: {fi.IsFolder} | IsFileSystem: {fi.IsFileSystem}");
        // only File System shell objects ?
        if (!_showAllShellObjects && !fi.IsFileSystem)
        {
          continue;
        }

        // check drive type 
        if (fi.IsFileSystem && selectedDrives.Count() > 0)
        {
          bool skipDrive = true;
          foreach (var drive in selectedDrives)
          {
            if (drive.Name == fi.Path)
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
        var newNode = CreateTreeNode(helper, fi.Name, fi.Path, false, fi);
        items.Add(newNode);
      }
      node.PopulateChildNodes(items);
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
      return new TreeItem(text, isSpecialFolder) { Path = path, Item = item, HasChildNodes = true };
    }

    #endregion

    public override string ToString()
    {
      return "Standard Provider";
    }
  }
}
