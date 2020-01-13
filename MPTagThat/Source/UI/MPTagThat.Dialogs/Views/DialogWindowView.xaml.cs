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
