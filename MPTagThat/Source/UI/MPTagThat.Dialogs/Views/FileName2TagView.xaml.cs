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

using System.Windows.Controls;
using System.Windows.Input;

namespace MPTagThat.Dialogs.Views
{
  /// <summary>
  /// Interaction logic for FileName2TagView.xaml
  /// </summary>
  public partial class FileName2TagView : UserControl
  {
    public FileName2TagView()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Ignore characters, which would result in invalid filenames
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ComboBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      switch (e.Text)
      {
        case "|":
        case "\"":
        case "/":
        case "*":
        case "?":
        case ":":
          e.Handled = true;
          break;
      }
    }
  }
}
