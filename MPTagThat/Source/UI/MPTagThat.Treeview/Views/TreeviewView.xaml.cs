using System.Windows.Controls;
using MPTagThat.Treeview.ViewModels;

namespace MPTagThat.Treeview.Views
{
  /// <summary>
  /// Interaction logic for TreeviewView.xaml
  /// </summary>
  public partial class TreeviewView : UserControl
  {
    public TreeviewView()
    {
      InitializeComponent();
      var vm = (TreeviewViewModel) DataContext;
      if (vm != null)
      {
        vm.TreeView = this.TreeView;
      }
    }
  }
}
