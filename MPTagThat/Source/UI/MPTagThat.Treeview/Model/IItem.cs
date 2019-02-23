using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MPTagThat.Treeview.Model
{
    public interface IItem
    {
      string Name { get; set; }
      object Info { get; set; }
      bool IsSelected { get; set; }
    }
}
