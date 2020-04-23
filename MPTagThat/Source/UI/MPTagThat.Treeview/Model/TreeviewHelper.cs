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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPTagThat.Treeview.ViewModels;

namespace MPTagThat.Treeview.Model
{
 public class TreeViewFolderBrowserHelper
  {
    #region fields

    /// <summary>
    ///   the managed tree view instance
    /// </summary>
    private readonly TreeviewViewModel _treeView;

    #endregion

    #region constructors

    /// <summary>
    ///   Initialize a new instance of TreeViewFolderBrowserHelper for the specified TreeViewFolderBrowser instance.
    /// </summary>
    /// <param name = "treeView"></param>
    internal TreeViewFolderBrowserHelper(TreeviewViewModel treeView)
    {
      _treeView = treeView;
    }

    #endregion

    #region public interface

    /// <summary>
    ///   Gets the underlying <see cref = "TreeViewFolderBrowser" /> instance.
    /// </summary>
    public TreeviewViewModel TreeView
    {
      get { return _treeView; }
    }

    #endregion
  }
}
