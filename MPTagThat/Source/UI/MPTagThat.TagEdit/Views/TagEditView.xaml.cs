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

namespace MPTagThat.TagEdit.Views
{
  /// <summary>
  /// Interaction logic for TagEditView.xaml
  /// </summary>
  public partial class TagEditView : UserControl
  {
    public TagEditView()
    {
      InitializeComponent();
    }

    private void TagEditView_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (e.NewSize.Height < 300.00 || e.NewSize.Width > 600)
      {
        VisualStateManager.GoToElementState(GenrePanel, "Wide", false);
      }
      else
      {
        VisualStateManager.GoToElementState(GenrePanel, "Narrow", false);
      }
      Console.WriteLine($"Size Changed: {e.NewSize.Width} x {e.NewSize.Height}");
    }
  }
}
