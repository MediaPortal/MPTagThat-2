using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Services.Dialogs;

namespace MPTagThat.Dialogs.ViewModels
{
  public static class DialogServiceExtensions
  {
    public static void ShowNotification(this IDialogService dialogService, string message, Action<IDialogResult> callBack)
    {
      dialogService.ShowDialog("NotificationDialog", new DialogParameters($"message={message}"), callBack);
    }

    public static void ShowConfirmation(this IDialogService dialogService, string message, Action<IDialogResult> callBack)
    {
      dialogService.ShowDialog("ConfirmationDialog", new DialogParameters($"message={message}"), callBack);
    }

    public static void ShowNotificationInAnotherWindow(this IDialogService dialogService, string dialogName, string windowName, string message, Action<IDialogResult> callBack)
    {
      dialogService.ShowDialog(dialogName, new DialogParameters($"message={message}"), callBack, windowName);
    }
  }
}
