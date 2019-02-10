using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interactivity;
using Syncfusion.Windows.Tools.Controls;
using MPTagThat.Treeview.ViewModels;

namespace MPTagThat.Treeview.Actions
{
  class OnDemandLoading:TargetedTriggerAction<TreeViewItemAdv>
  {
    protected override void Invoke(object parameter)
    {
      TreeViewItemAdv item = (parameter as LoadonDemandEventArgs).TreeViewItem;
      TreeviewViewModel data = new TreeviewViewModel();
      data.OnDemandLoad(item);
    }    }
}
