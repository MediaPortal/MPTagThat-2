using System.Collections.ObjectModel;

namespace MPTagThat.Treeview.Model
{
    public class Directory : Item
    {
      private ObservableCollection<IItem> _files;

      public Directory()
      {
        Files = new ObservableCollection<IItem>();
        Items = new ObservableCollection<IItem>();
      }

      public ObservableCollection<IItem> Files
      {
        get => _files;

        set
        {
          _files = value;
          OnPropertyChanged("Files");
        }
      }

      private ObservableCollection<IItem> _items;

      public ObservableCollection<IItem> Items
      {
        get => _items;

        set
        {
          _items = value;
          OnPropertyChanged("Items");
        }
      }
    }
}
