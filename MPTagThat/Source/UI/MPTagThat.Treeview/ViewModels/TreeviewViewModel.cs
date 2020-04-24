#region Copyright (C) 2017 Team MediaPortal
// Copyright (C) 2017 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MPTagThat is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPTagThat is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPTagThat. If not, see <http://www.gnu.org/licenses/>.
#endregion

#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommonServiceLocator;
using MPTagThat.Core;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Treeview.Model;
using MPTagThat.Treeview.Model.Win32;
using Prism.Events;
using Prism.Mvvm;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;

#endregion

namespace MPTagThat.Treeview.ViewModels
{
  // This is the Viewmodel to handle Treeview related display of Folders
  public class TreeviewViewModel : BindableBase
  {
    #region Variables

    private Options _options;
    private object _selectedItem;
    private DispatcherTimer _timer;
    private readonly TreeViewHelper _helper;
    private ITreeviewDataProvider _dataProvider;

    #endregion

    #region Properies

    /// <summary>
    /// A new Item has been selected. Send a notification to list the content of the folder
    /// </summary>
    public object SelectedItem
    {
      get => _selectedItem;
      set
      {
        SetProperty(ref _selectedItem, value);

        // Prepare Event to be published
        var selecteditem = (_selectedItem as TreeItem)?.Path;
        if (!string.IsNullOrEmpty(selecteditem))
        {
          _options.MainSettings.LastFolderUsed = selecteditem;

          GenericEvent evt = new GenericEvent
          {
            Action = "selectedfolderchanged"
          };
          evt.MessageData.Add("folder", selecteditem);
          EventSystem.Publish(evt);
        }
      }
    }

    public ICommand SelectedItemChangedCommand { get; set; }
    /// <summary>
    /// The Selected Item has Changed. 
    /// </summary>
    /// <param name="param"></param>
    public void SelectedItemChanged(object param)
    {
      var args = (RoutedPropertyChangedEventArgs<object>)param;
      if (args.NewValue is TreeItem item)
      {
        this.SelectedItem = item;
      }
    }

    public ICommand LoadFolderOnDemandCommand { get; set; }
    /// <summary>
    /// Expand the node. This is needed to use the Load On demand feature from syncfuion
    /// without that, we would have a spinning wheel and the expand of the node would never finish
    /// </summary>
    /// <param name="parameter"></param>
    private void LoadFolderOnDemand(object parameter)
    {
      TreeViewItemAdv treeitem = (parameter as LoadonDemandEventArgs)?.TreeViewItem;
      if (treeitem != null && treeitem.DataContext is TreeItem node)
      {
        if (node.Nodes.Count == 0)
        {
          _dataProvider.RequestSubDirs(_helper, node);
        }
        treeitem.IsLoadOnDemand = false;
      }
    }

    private Environment.SpecialFolder _rootFolder;
    public Environment.SpecialFolder RootFolder
    {
      get => _rootFolder;
      set { SetProperty(ref _rootFolder, value); }
    }

    // Nodes are used to bind to TreeView
    private ObservableCollection<TreeItem> _nodes = new ObservableCollection<TreeItem>();
    public ObservableCollection<TreeItem> Nodes
    {
      get => _nodes;
      set { SetProperty(ref _nodes, value); }
    }

    private Enums.DriveTypes _driveTypes;
    public Enums.DriveTypes DriveTypes
    {
      get => _driveTypes;
      set { SetProperty(ref _driveTypes, value); }
    }
    #endregion

    #region ctor

    public TreeviewViewModel()
    {
      _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;

      EventSystem.Subscribe<GenericEvent>(OnMessageReceived, ThreadOption.UIThread);

      _helper = new TreeViewHelper(this);
      DriveTypes = Enums.DriveTypes.LocalDisk | Enums.DriveTypes.NetworkDrive | Enums.DriveTypes.RemovableDisk;
      RootFolder = Environment.SpecialFolder.Desktop;
      _dataProvider = new TreeViewDataProvider();

      SelectedItemChangedCommand = new DelegateCommand<object>(SelectedItemChanged);
      LoadFolderOnDemandCommand = new DelegateCommand<object>(LoadFolderOnDemand);

      RefreshTreeview();

      // Work around the problem with Load On Demand being called from the Constructor.
      _timer = new DispatcherTimer();
      _timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
      _timer.Tick += SetCurrentFolder;
      _timer.Tag = RootFolder;
      _timer.Start();
    }

    #endregion

    #region Private Methods

    private void RefreshTreeview()
    {
      _dataProvider.RequestRoot(_helper);
    }


    /// <summary>
    /// Expand the tree to list the current folder.
    /// This cannot be used in the constructor, so it is invoked ny a Timer
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SetCurrentFolder(object sender, EventArgs e)
    {
      if (_timer != null && _timer.IsEnabled)
      {
        _timer.Stop();
      }
      var currentFolder = _options.MainSettings.LastFolderUsed;
      if (!Directory.Exists(currentFolder))
      {
        return;
      }

      _helper.TreeView.Nodes[0].IsExpanded = true;
      _helper.TreeView.Nodes[0].Nodes[0].IsExpanded = true;
      var requestNetwork = currentFolder.StartsWith(@"\\");
      var nodeCol = _dataProvider.RequestDriveCollection(_helper, requestNetwork);

      var dirInfo = new DirectoryInfo(currentFolder);
      
      // get path tokens
      var dirs = new List<string> {dirInfo.FullName};

      while (dirInfo.Parent != null)
      {
        dirs.Add(dirInfo.Parent.FullName);
        dirInfo = dirInfo.Parent;
      }
      // For network we should add also the server to the dir list
      if (requestNetwork)
      {
        dirs.Add(dirInfo.FullName.Substring(0, dirInfo.FullName.LastIndexOf(@"\", StringComparison.Ordinal)));
      }
      for (var i = dirs.Count - 1; i >= 0; i--)
      {
        foreach (var n in nodeCol)
        {
          if (string.Compare(n.Path.ToLower(), dirs[i].ToLower(), StringComparison.Ordinal) == 0)
          {
            if (i == 0)
            {
              n.IsSelected = true;
              // Set the Selected Item, because we will get a null value from the XAML Event
              SelectedItem = n;
            }
            else
            {
              n.IsExpanded = true;
              _dataProvider.RequestSubDirs(_helper, n);
              nodeCol = n.Nodes;
            }
            break;
          }
        }
      }
    }

    #endregion

    #region Events

    private void OnMessageReceived(GenericEvent msg)
    {
      switch (msg.Action.ToLower())
      {
        case "currentfolderchanged":
          RefreshTreeview();
          SetCurrentFolder(null, new EventArgs());
          break;
      }
    }

    #endregion
  }
}
