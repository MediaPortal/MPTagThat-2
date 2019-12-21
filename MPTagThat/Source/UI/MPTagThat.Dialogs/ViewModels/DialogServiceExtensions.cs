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

using System;
using Prism.Services.Dialogs;

#endregion

namespace MPTagThat.Dialogs.ViewModels
{
  public static class DialogServiceExtensions
  {
    public static void ShowNotification(this IDialogService dialogService, string message, Action<IDialogResult> callBack)
    {
      dialogService.ShowDialog("NotificationDialog", new DialogParameters($"message={message}"), callBack);
    }

    public static void ShowDialogInAnotherWindow(this IDialogService dialogService, string dialogName, string windowName, DialogParameters parameters, Action<IDialogResult> callBack)
    {
      dialogService.ShowDialog(dialogName, parameters , callBack, windowName);
    }
  }
}
