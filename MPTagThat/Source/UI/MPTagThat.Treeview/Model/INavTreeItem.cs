#region Copyright (C) 2017 Team MediaPortal
// Copyright (C) 2017 Team MediaPortal
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

using System.ComponentModel;
using System.Collections.ObjectModel;

#endregion

namespace MPTagThat.Treeview.Model
{
  public interface INavTreeItem : INotifyPropertyChanged
  {
    // For text in treeItem
    string Name { get; set; }

    // Info used in TreeItem to show the Icon
    object Info { get; set; }

    // Drive/Folder/File naming scheme to retrieve children
    string FullPathName { get; set; }

    ObservableCollection<INavTreeItem> Children { get; }

    bool IsExpanded { get; set; }

    bool IsSelected { get; set; }

    // For resetting the tree
    void DeleteChildren();
  }
}
