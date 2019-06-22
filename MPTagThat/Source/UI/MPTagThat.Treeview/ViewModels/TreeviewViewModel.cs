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
using CommonServiceLocator;
using MPTagThat.Core;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Core.Utils;
using MPTagThat.Treeview.Model;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
using Directory = MPTagThat.Treeview.Model.Directory;

namespace MPTagThat.Treeview.ViewModels
{
    public class TreeviewViewModel : NotificationObject
    {
      #region Variables

      private Options _options;

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

          _options.MainSettings.LastFolderUsed = selecteditem;

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

      public ICommand LoadFolderOnDemandCommand { get; set; }
      private void LoadFolderOnDemand(object parameter)
      {
        TreeViewItemAdv treeitem = (parameter as LoadonDemandEventArgs).TreeViewItem;
        if (treeitem != null)
        {
          BuildDirectoryTree(treeitem, treeitem.DataContext as IItem);
        }
      }

      #endregion

      #region ctor

      public TreeviewViewModel()
      {
        _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;

        _items = new ObservableCollection<IItem>();
        DriveInfo[] drives = DriveInfo.GetDrives();
        int count = 0;

        foreach (DriveInfo info in drives)
        {
          Directory directory = new Directory();
          directory.Name = "Local Disk " + info.Name;
          directory.Info = info;
          if (count == 0)
          {
            directory.IsSelected = true;
          }
          Items.Add(directory);
          count++;
        }

        SelectedItemChangedCommand = new DelegateCommand<object>(SelectedItemChanged);
        LoadFolderOnDemandCommand = new DelegateCommand<object>(LoadFolderOnDemand);

        SetCurrentFolder(_options.MainSettings.LastFolderUsed);
      }

      #endregion

      #region Private Methods

      private void SetCurrentFolder(string folderName)
      {
        if (!System.IO.Directory.Exists(folderName))
        {
          return;
        }

        var folderstructure = Util.SplitPath(folderName);
        foreach (var item in Items)
        {
          if (item.Info.ToString().Trim('\\') == folderstructure[0])
          {

          }
        }
      }


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
                // ignore Recycle Bin
                if (directory.Name.ToLower().Equals("$recycle.bin") || directory.Name.Equals("Config.Msi") || directory.Name.Equals("System Volume Information"))
                {
                  continue;
                }
                Directory _file = new Directory();
                _file.Name = directory.Name;
                _file.Info = directory;
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
