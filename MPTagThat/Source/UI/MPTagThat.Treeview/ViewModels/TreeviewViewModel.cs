using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MPTagThat.Core;
using MPTagThat.Core.Events;
using MPTagThat.Treeview.Model;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
using Directory = MPTagThat.Treeview.Model.Directory;

namespace MPTagThat.Treeview.ViewModels
{
    public class TreeviewViewModel : NotificationObject
    {
      #region Variables

      private IItem _currentItem;
      private TreeViewItemAdv _treeItem;
      private ObservableCollection<IItem> _items;
      private object _selectedItem;

      #endregion

      #region Properies
      
      public ICommand SelectedItemChangedCommand { get; set; }

      public ObservableCollection<IItem> Items
      {
        get => _items;
        set
        {
          _items = value;
          this.RaisePropertyChanged(() => this.Items);
        }
      }

      public object SelectedItem
      {
        get => _selectedItem;
        set
        {
          _selectedItem = value;
          this.RaisePropertyChanged(() => this.SelectedItem);

          // Prepare Event to be published
          var selecteditem = "";
          if (_selectedItem is MPTagThat.Treeview.Model.Directory)
          {
            if ((_selectedItem as IItem).Info is DriveInfo)
            {
              selecteditem = ((_selectedItem as IItem).Info as DriveInfo).Name;
            }
            else
            {
              selecteditem = ((_selectedItem as IItem).Info as DirectoryInfo).FullName;
            }
          }

          GenericEvent evt = new GenericEvent();
          evt.Action = "selectedfolderchanged";
          evt.MessageData.Add("folder", selecteditem);
          EventSystem.Publish(evt);
        }
      }      

      public void SelectedItemChanged(object param)
      {
        if (param is IItem item)
        {
          this.SelectedItem = item;
        }
      }

      public void OnDemandLoad(TreeViewItemAdv treeitem)
      {
        if (treeitem != null)
        {
          this._treeItem = treeitem as TreeViewItemAdv;
          this._currentItem = _treeItem.DataContext as IItem;
          BuildDirectoryTree(_treeItem, _currentItem);
        }
      }

      #endregion

      #region ctor

      public TreeviewViewModel()
      {           
        _items = new ObservableCollection<IItem>();
        DriveInfo[] drives = DriveInfo.GetDrives();
        int count = 0;

        foreach (DriveInfo info in drives)
        {
          Directory directory = new Directory();
          directory.Name = "Local Disk " + info.Name;
          directory.Info = info;
          BitmapImage bmp = new BitmapImage(new Uri("CD_Drive.png", UriKind.RelativeOrAbsolute));
          directory.Icon = bmp;
          if (count == 0)
          {
            directory.IsSelected = true;
          }
          Items.Add(directory);
          count++;
        }

        SelectedItemChangedCommand = new DelegateCommand<object>(SelectedItemChanged);
      }

      #endregion

      #region Private Methods

      private void BuildDirectoryTree(TreeViewItemAdv item,IItem file)
      {
        var path = "";

        if (item!=null && file != null)
        {
          if (file.Info is DriveInfo)
          {
            path = ((DriveInfo)file.Info).Name;
          }
          else if (file.Info is DirectoryInfo)
          {
            path = ((DirectoryInfo)file.Info).FullName;
          }

          if (String.IsNullOrEmpty(path))
          {
            return;
          }
          DirectoryInfo info = new DirectoryInfo(path);
          if (info.Exists)
          {
            try
            {
              var infos = info.GetDirectories().OrderBy(x => x.Name);
              foreach (DirectoryInfo directory in infos)
              {
                Directory _file = new Directory();
                _file.Name = directory.Name;
                _file.Info = directory;
                //BitmapImage image = new BitmapImage(new Uri("folder.png", UriKind.RelativeOrAbsolute));
                //_file.Icon = image;
                ((Directory)file).Files.Add(_file);
                ((Directory)file).Items.Add(_file);
              }
            }
            catch (Exception e)
            {
              //MessageBox.Show(e.Message);
            }
          }
          else
          {
            //MessageBox.Show("The Device is not ready.", "Disk Error", MessageBoxButton.OK, MessageBoxImage.Error);
          }               
          item.IsLoadOnDemand = false;
        }
      }

      #endregion
    }
}
