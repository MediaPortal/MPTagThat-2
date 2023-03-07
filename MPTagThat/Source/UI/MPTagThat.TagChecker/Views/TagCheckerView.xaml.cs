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
using MPTagThat.TagChecker.ViewModels;

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
