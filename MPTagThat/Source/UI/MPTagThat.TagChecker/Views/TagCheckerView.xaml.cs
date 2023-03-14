using MPTagThat.TagChecker.ViewModels;
using System.Windows.Controls;

namespace MPTagThat.TagChecker.Views
{
  /// <summary>
  /// Interaction logic for TagCheckerView.xaml
  /// </summary>
  public partial class TagCheckerView : UserControl
  {
    public TagCheckerView()
    {
      InitializeComponent();
      var vm = (TagCheckerViewModel)DataContext;
      vm.ItemsGrid = this.ItemsGrid;
    }
  }
}
