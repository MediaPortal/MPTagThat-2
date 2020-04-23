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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTagThat.Treeview.Model
{
    public interface ITreeviewDataProvider
    {
        /// <summary>
        ///   Fired before the context menu popup.
        /// </summary>
        /// <param name = "helper">The helper instance which provides method's and properties related to create and get nodes.</param>
        /// <param name = "node">The node on which the context menu was requested.</param>
        void QueryContextMenuItems(TreeViewFolderBrowserHelper helper, NavTreeItem node);

        /// <summary>
        ///   Fill the root level.
        /// </summary>
        /// <param name = "helper">The helper instance which provides method's and properties related to create and get nodes.</param>
        void RequestRoot(TreeViewFolderBrowserHelper helper);

        /// <summary>
        ///   Fill the Directory structure for a given path.
        /// </summary>
        /// <param name = "helper">The helper instance which provides method's and properties related to create and get nodes.</param>
        /// <param name = "parent">The expanding node.</param>
        void RequestSubDirs(TreeViewFolderBrowserHelper helper, NavTreeItem parent);

        /// <summary>
        ///   Gets the tree node collection which holds the drive node's. The requested collection is than used to search a specific node.
        /// </summary>
        ObservableCollection<NavTreeItem> RequestDriveCollection(TreeViewFolderBrowserHelper helper, bool isNetwork);
    }
}
