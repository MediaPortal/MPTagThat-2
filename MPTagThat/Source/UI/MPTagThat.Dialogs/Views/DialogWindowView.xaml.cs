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
using System.Windows.Shapes;
using MPTagThat.Core;
using MPTagThat.Core.Events;
using Prism.Services.Dialogs;
using Syncfusion.Windows.Shared;

namespace MPTagThat.Dialogs.Views
{
  /// <summary>
  /// Interaction logic for DialogWindowView.xaml
  /// </summary>
  public partial class DialogWindowView : Window, IDialogWindow
  {
    public DialogWindowView()
    {
      InitializeComponent();
      this.PreviewKeyDown += new KeyEventHandler(HandleKeys);
    }

    public IDialogResult Result { get; set; }

    private void HandleKeys(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape)
      {
        GenericEvent evt = new GenericEvent
         {
         Action = "closedialogrequested"
         };
         EventSystem.Publish(evt);
      }

      if (e.Key == Key.Enter)
      {
        GenericEvent evt = new GenericEvent
        {
          Action = "applychangesrequested"
        };
        EventSystem.Publish(evt);
      }
    }
  }
}
