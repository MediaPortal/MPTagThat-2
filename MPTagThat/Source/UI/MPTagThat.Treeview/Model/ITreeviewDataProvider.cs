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

using Syncfusion.UI.Xaml.TreeView.Engine;

namespace MPTagThat.Treeview.Model
{
  public interface ITreeviewDataProvider
  {
    /// <summary>
    ///   Fired before the context menu popup.
    /// </summary>
    /// <param name = "helper">The helper instance which provides method's and properties related to create and get nodes.</param>
    /// <param name = "node">The node on which the context menu was requested.</param>
    void QueryContextMenuItems(TreeViewHelper helper, TreeViewNode node);

    /// <summary>
    ///   Fill the root level.
    /// </summary>
    /// <param name = "helper">The helper instance which provides method's and properties related to create and get nodes.</param>
    /// <param name = "parent">The expanding node.</param>
    void RequestRoot(TreeViewHelper helper, TreeViewNode parent);

    /// <summary>
    ///   Fill the Directory structure for a given path.
    /// </summary>
    /// <param name = "helper">The helper instance which provides method's and properties related to create and get nodes.</param>
    /// <param name = "parent">The expanding node.</param>
    void RequestSubDirs(TreeViewHelper helper, TreeViewNode parent);

    /// <summary>
    ///   Create Root Node containing the type of the DataProvider.
    /// </summary>
    /// <param name = "helper">The helper instance which provides method's and properties related to create and get nodes.</param>
    void CreateRootNode(TreeViewHelper helper);

    /// <summary>
    /// Clears the selceted Database Node
    /// </summary>
    void ClearSelectedDatabaseNode();
  }
}
