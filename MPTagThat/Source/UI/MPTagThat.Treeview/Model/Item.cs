using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MPTagThat.Treeview.Model
{
    public class Item : IItem, INotifyPropertyChanged
    {
      #region Variables

      private bool isSelected;
      
      #endregion

      #region Interfaces
      
      public string Name { get; set; }
      public object Info { get; set; }
      
      public bool IsSelected
      {
        get 
        { 
          return isSelected; 
        }
        set 
        { 
          isSelected = value;
          OnPropertyChanged("IsSelected");
        }
      }
      
      public void OnPropertyChanged(string propertyname)
      {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
    }

      public event PropertyChangedEventHandler PropertyChanged;

      #endregion
    }
}
