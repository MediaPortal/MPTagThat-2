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

#region

using System.Collections.ObjectModel;
using Prism.Mvvm;

#endregion

namespace MPTagThat.Treeview.Model
{
    public class TreeItem : BindableBase
    {
        #region Properties

        public string Name { get; set; }

        public object Item { get; set; }

        public string Path { get; set; }

        private ObservableCollection<TreeItem> _nodes = new ObservableCollection<TreeItem>();
        public ObservableCollection<TreeItem> Nodes
        {
            get => _nodes;
            set
            {
                SetProperty(ref _nodes, value);
            }
        }

        private bool _isSpecialFolder;
        public bool IsSpecialFolder
        {
            get => _isSpecialFolder;
            set { SetProperty(ref _isSpecialFolder, value); }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { SetProperty(ref _isExpanded, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { SetProperty(ref _isSelected, value); }
        }

        // DeleteChildren, used to 
        // 1) remove old tree 2) set children=null, so a new tree is build
        public void DeleteChildren()
        {
            if (_nodes != null)
            {
                for (int i = _nodes.Count - 1; i >= 0; i--)
                {
                    _nodes[i].DeleteChildren();
                    _nodes[i] = null;
                    _nodes.RemoveAt(i);
                }

                _nodes = null;
            }
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
