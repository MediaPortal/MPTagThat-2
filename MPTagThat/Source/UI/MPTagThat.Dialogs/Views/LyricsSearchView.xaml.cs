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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Syncfusion.UI.Xaml.Grid;

namespace MPTagThat.Dialogs.Views
{
  /// <summary>
  /// Interaction logic for LyricsSearchView.xaml
  /// </summary>
  public partial class LyricsSearchView : UserControl
  {
    public LyricsSearchView()
    {
      InitializeComponent();
      this.LyricsGrid.QueryRowHeight += QueryRowHeight;
    }

    private void QueryRowHeight(object sender, Syncfusion.UI.Xaml.Grid.QueryRowHeightEventArgs e)
    {
      var gridRowResizingOptions = new GridRowSizingOptions();
      if (this.LyricsGrid.GridColumnSizer.GetAutoRowHeight(e.RowIndex, gridRowResizingOptions, out var autoHeight))
      {
        if (autoHeight > 25 && e.RowIndex > 0)
        {
          e.Height = 80;
          e.Handled = true;
        }
      }
    }
  }
}
