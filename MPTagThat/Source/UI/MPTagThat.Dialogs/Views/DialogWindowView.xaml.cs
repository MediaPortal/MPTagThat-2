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
using Prism.Services.Dialogs;
using Syncfusion.Windows.Shared;

namespace MPTagThat.Dialogs.Views
{
  /// <summary>
  /// Interaction logic for DialogWindowView.xaml
  /// </summary>
  public partial class DialogWindowView : ChromelessWindow, IDialogWindow
  {
    public DialogWindowView()
    {
      InitializeComponent();
    }

    public IDialogResult Result { get; set; }
  }
}
