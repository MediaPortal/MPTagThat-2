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

using MPTagThat.SongGrid.Models;
using Prism.Mvvm;
using System.Collections.ObjectModel;

#endregion

namespace MPTagThat.SongGrid.ViewModels
{
  public class CustomColumnChooserViewModel : BindableBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomColumnChooserViewModel"/> class.
    /// </summary>
    /// <param name="totalColumns">The total columns.</param>
    public CustomColumnChooserViewModel(ObservableCollection<ColumnChooserItems> totalColumns)
    {
      ColumnCollection = totalColumns;
    }

    /// <summary>
    /// Gets or sets the column collection.
    /// </summary>
    /// <value>The column collection.</value>
    public ObservableCollection<ColumnChooserItems> ColumnCollection
    {
      get;
      set;
    }
  }
}
