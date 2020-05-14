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

using Prism.Mvvm;

#endregion

namespace MPTagThat.SongGrid.Models
{
  public class ColumnChooserItems : BindableBase
  {
    private bool _isChecked;

    /// <summary>
    /// Gets or sets a value indicating whether this instance is checked.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is checked; otherwise, <c>false</c>.
    /// </value>
    public bool IsChecked
    {
      get => _isChecked;
      set => SetProperty(ref _isChecked, value);
    }

    private string _name;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name
    {
      get => _name;
      set => SetProperty(ref _name, value);
    }
  } 
}
