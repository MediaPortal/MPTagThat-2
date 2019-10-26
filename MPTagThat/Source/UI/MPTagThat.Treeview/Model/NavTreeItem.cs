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
  public abstract class NavTreeItem : INavTreeItem, INotifyPropertyChanged
  {
    public string Name { get; set; }

    public object Info { get; set; }

    public string FullPathName { get; set; }

    protected ObservableCollection<INavTreeItem> children;
    public ObservableCollection<INavTreeItem> Children
    {
      get => children ?? (children = GetMyChildren());
      set {
        children = value;
        OnPropertyChanged("Children");
      }
    }

    private bool _isExpanded;
    public bool IsExpanded
    {
      get => _isExpanded;
      set
      {
        _isExpanded = value;
        OnPropertyChanged("IsExpanded");
      }
    }

    private bool _isSelected;
    public bool IsSelected
    {
      get => _isSelected;
      set
      {
        _isSelected = value;
        OnPropertyChanged("IsSelected");
      }
    }

    public abstract ObservableCollection<INavTreeItem> GetMyChildren();


    // DeleteChildren, used to 
    // 1) remove old tree 2) set children=null, so a new tree is build
    public void DeleteChildren()
    {
      if (children != null)
      {
        for (int i = children.Count - 1; i >= 0; i--)
        {
          children[i].DeleteChildren();
          children[i] = null;
          children.RemoveAt(i);
        }

        children = null;
      }
    }

    public void OnPropertyChanged(string propertyname)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
