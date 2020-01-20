using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Syncfusion.Windows.Tools.Controls;

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


    /// <summary>
    /// Handle Size Changes, so that the layout adapts to multi column display
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
    }
  }
}
