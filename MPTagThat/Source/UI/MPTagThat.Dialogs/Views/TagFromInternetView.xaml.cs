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

using MPTagThat.Dialogs.ViewModels;
using System.Windows.Controls;

namespace MPTagThat.Dialogs.Views
{
  /// <summary>
  /// Interaction logic for TagFromInternetView.xaml
  /// </summary>
  public partial class TagFromInternetView : UserControl
  {
    public TagFromInternetView()
    {
      InitializeComponent();
      var vm = (TagFromInternetViewModel)DataContext;
      if (vm != null)
      {
        vm.MatchedSongsGrid = this.MatchedSongsGrid;
      }
    }
  }
}
