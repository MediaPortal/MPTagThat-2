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

#region

using Prism.Mvvm;

#endregion

namespace MPTagThat.Treeview.Model
{
  public class TreeItem : BindableBase
  {
    #region Properties

    /// <summary>
    /// The Name of the Node, whih is displayed in te Treeview
    /// </summary>
    private string _name;

    public string Name
    {
      get => _name;
      set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// The Item read. Used to get the Cover Art
    /// </summary>
    public object Item { get; set; }

    /// <summary>
    /// This is the Root Level (Desktop or Music Database)
    /// </summary>
    public bool IsRoot { get; set; }

    /// <summary>
    /// The Path of the Item
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Do we have a special Folder
    /// </summary>
    private bool _isSpecialFolder;
    public bool IsSpecialFolder
    {
      get => _isSpecialFolder;
      set { SetProperty(ref _isSpecialFolder, value); }
    }

    /// <summary>
    /// Denotes that we have Child Nodes
    /// </summary>
    private bool _hasChildNodes;
    public bool HasChildNodes
    {
      get => _hasChildNodes;
      set { SetProperty(ref _hasChildNodes, value); }
    }

    #endregion

    #region ctor

    public TreeItem(string text, bool isSpecialFolder)
    {
      Name = text;
      _isSpecialFolder = isSpecialFolder;
    }

    #endregion
  }
}
