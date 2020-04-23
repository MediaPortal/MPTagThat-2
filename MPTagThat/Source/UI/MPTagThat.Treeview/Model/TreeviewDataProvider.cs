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
using System.Text;
using System.Threading.Tasks;
using MPTagThat.Treeview.Model.Win32;
using MPTagThat.Treeview.ViewModels;

namespace MPTagThat.Treeview.Model
{
    public class TreeViewFolderBrowserDataProvider : ITreeviewDataProvider
    {
        #region ITreeViewFolderBrowserDataProvider Members

        public virtual object ImageList
        {
            get { return null; }
        }

        public virtual void QueryContextMenuItems(TreeViewFolderBrowserHelper helper, NavTreeItem node) { }

        public virtual ObservableCollection<NavTreeItem> RequestDriveCollection(TreeViewFolderBrowserHelper helper, bool isNetwork)
        {
            switch (helper.TreeView.RootFolder)
            {
                case Environment.SpecialFolder.Desktop:
                    return helper.TreeView.Nodes[0].Children;
                default:
                    return helper.TreeView.Nodes;
            }
        }

        public virtual void RequestSubDirs(TreeViewFolderBrowserHelper helper, NavTreeItem parent)
        {
            if (parent.Path == null) return;
            //
            DirectoryInfo directory = new DirectoryInfo(parent.Path);
            // check persmission
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, directory.FullName).Demand();
            //	

            // Sort the Directories, as Samba might return unsorted
            DirectoryInfo[] dirInfo = directory.GetDirectories();
            Array.Sort(dirInfo,
                       new Comparison<DirectoryInfo>(
                         delegate (DirectoryInfo d1, DirectoryInfo d2) { return string.Compare(d1.Name, d2.Name); }));


            foreach (DirectoryInfo dir in dirInfo)
            {
                if ((dir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }
                var newNode = CreateTreeNode(helper, dir.Name, dir.FullName, false, dir);
                parent.Children.Add(newNode);
            }
        }

        public virtual void RequestRoot(TreeViewFolderBrowserHelper helper)
        {
            bool populateDrives = true;
            ObservableCollection<NavTreeItem> rootNodeCollection = new ObservableCollection<NavTreeItem>();
            ObservableCollection<NavTreeItem> driveRootNodeCollection = new ObservableCollection<NavTreeItem>();
            // setup up root node collection
            switch (helper.TreeView.RootFolder)
            {
                case Environment.SpecialFolder.Desktop:
                    // create root node <Desktop>
                    var desktopNode = CreateTreeNode(helper, Environment.SpecialFolder.Desktop.ToString(), string.Empty,
                                                              true, null);
                    helper.TreeView.Nodes.Add(desktopNode);
                    rootNodeCollection = helper.TreeView.Nodes[0].Children;
                    // create child node <Personal>
                    string personalDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    rootNodeCollection.Add(CreateTreeNode(helper, Path.GetFileName(personalDirectory), personalDirectory, false, null));
                    // create child node <MyComuter>
                    var myComputerNode = CreateTreeNode(helper, Environment.SpecialFolder.MyComputer.ToString(),
                                                                 string.Empty, true, null);
                    rootNodeCollection.Add(myComputerNode);
                    driveRootNodeCollection = myComputerNode.Children;
                    break;
                case Environment.SpecialFolder.MyComputer:
                    rootNodeCollection = helper.TreeView.Nodes;
                    driveRootNodeCollection = rootNodeCollection;
                    break;
                default:
                    rootNodeCollection = helper.TreeView.Nodes;
                    driveRootNodeCollection = rootNodeCollection;
                    // create root node with specified SpecialFolder
                    rootNodeCollection.Add(CreateTreeNode(helper,
                                                          Path.GetFileName(Environment.GetFolderPath(helper.TreeView.RootFolder)),
                                                          Environment.GetFolderPath(helper.TreeView.RootFolder), false, null));
                    populateDrives = false;
                    break;
            }
            if (populateDrives)
            {
                // populate local machine drives
                foreach (Logicaldisk logicalDisk in Logicaldisk.GetInstances(null, GetWMIQueryStatement(helper.TreeView)))
                {
                    try
                    {
                        string name = string.Empty;
                        string path = logicalDisk.Name + "\\";
                        name = logicalDisk.Description;
                        //
                        name += (name != string.Empty) ? " (" + path + ")" : path;
                        // add node to root collection
                        driveRootNodeCollection.Add(CreateTreeNode(helper, name, path, false, logicalDisk));
                    }
                    catch (Exception doh)
                    {
                        throw doh;
                    }
                }
            }
        }

        #endregion

        #region internal interface

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
            return new NavTreeItem(text, isSpecialFolder) {Path = path, Item = item};
        }

        /// <summary>
        ///   Gets the WMI query string based on the current drive types.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetWMIQueryStatement(TreeviewViewModel treeView)
        {
            if ((treeView.DriveTypes & Enums.DriveTypes.All) == Enums.DriveTypes.All) return string.Empty;
            //
            string where = string.Empty;
            //
            Array array = Enum.GetValues(typeof(Enums.DriveTypes));
            //
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
