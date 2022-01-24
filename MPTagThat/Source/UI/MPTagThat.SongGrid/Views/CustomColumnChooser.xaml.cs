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

using System.Windows;
using MPTagThat.SongGrid.ViewModels;
using Syncfusion.Windows.Shared;

#endregion

namespace MPTagThat.SongGrid.Views
{
  /// <summary>
  /// Interaction logic for CustomColumnChooser.xaml
  /// </summary>
  public partial class CustomColumnChooser : ChromelessWindow
  {
    public CustomColumnChooser(CustomColumnChooserViewModel viewModel)
    {
      InitializeComponent();
      DataContext = viewModel;
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
    }
  }
}
