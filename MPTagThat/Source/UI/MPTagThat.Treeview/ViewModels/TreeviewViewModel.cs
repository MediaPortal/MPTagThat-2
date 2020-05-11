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
using System.Security.Policy;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommonServiceLocator;
using MPTagThat.Core;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Treeview.Model;
using MPTagThat.Treeview.Model.Win32;
using Prism.Events;
using Prism.Mvvm;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
using WPFLocalizeExtension.Engine;

#endregion

namespace MPTagThat.Treeview.ViewModels
{
  // This is the Viewmodel to handle Treeview related display of Folders
  public class TreeviewViewModel : BindableBase
  {
    #region Variables

    private Options _options;
    private readonly NLogLogger log;

    private DispatcherTimer _timer;
    private readonly TreeViewHelper _helper;
    private ITreeviewDataProvider _dataProvider;

    private bool _init; 

    #endregion

    #region Properies

    /// <summary>
    /// A new Item has been selected. Send a notification to list the content of the folder
    /// </summary>
    private object _selectedItem;
    public object SelectedItem
    {
      get => _selectedItem;
      set
      {
        log.Trace(">>>");
        SetProperty(ref _selectedItem, value);

        // Prepare Event to be published
        var selecteditem = (_selectedItem as TreeItem)?.Path;
        if (!string.IsNullOrEmpty(selecteditem))
        {
          SetRecentFolder(selecteditem);
          _options.MainSettings.LastFolderUsed = selecteditem;

          GenericEvent evt = new GenericEvent
          {
            Action = "selectedfolderchanged"
          };
          evt.MessageData.Add("folder", selecteditem);
          evt.MessageData.Add("scansubfolders", ScanSubFolders);
          EventSystem.Publish(evt);
        }
        log.Trace("<<<");
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
        log.Trace($"Expanding node {node.Name}");
        if (node.Nodes.Count == 0)
        {
          _dataProvider.RequestSubDirs(_helper, node);
        }
        treeitem.IsLoadOnDemand = false;
      }
    }

    /// <summary>
    /// Refresh the Treeview
    /// </summary>
    public ICommand RefreshTreeViewCommand { get; }

    private void RefreshTreeview(object param)
    {
      RefreshTreeview();
    }


    private Environment.SpecialFolder _rootFolder;
    /// <summary>
    /// The RootFolder to start display from
    /// </summary>
    public Environment.SpecialFolder RootFolder
    {
      get => _rootFolder;
      set { SetProperty(ref _rootFolder, value); }
    }

    private ObservableCollection<TreeItem> _nodes = new ObservableCollection<TreeItem>();
    /// <summary>
    /// Nodes bound to TreeView
    /// </summary>
    public ObservableCollection<TreeItem> Nodes
    {
      get => _nodes;
      set { SetProperty(ref _nodes, value); }
    }

    private Enums.DriveTypes _driveTypes;
    /// <summary>
    /// Available Drive Types to be shown
    /// </summary>
    public Enums.DriveTypes DriveTypes
    {
      get => _driveTypes;
      set { SetProperty(ref _driveTypes, value); }
    }

    private Cursor _cursor = Cursors.Arrow;
    /// <summary>
    /// The State of the cursor
    /// </summary>
    public Cursor Cursor
    {
      get => _cursor;
      set => SetProperty(ref _cursor, value);
    }

    /// <summary>
    /// The Binding for the Recent Folders
    /// </summary>
    private ObservableCollection<string> _recentFolders = new ObservableCollection<string>();
    public ObservableCollection<string> RecentFolders
    {
      get => _recentFolders;
      set { SetProperty(ref _recentFolders, value); }
    }

    /// <summary>
    /// The Binding for the Selected Recent Folder
    /// </summary>
    private string _selectedRecentFolder;

    public string SelectedRecentFolder
    {
      get => _selectedRecentFolder;
      set
      {
        SetProperty(ref _selectedRecentFolder, value);
        _options.MainSettings.LastFolderUsed = _selectedRecentFolder;
        RefreshTreeview();
        SetCurrentFolder(null, new EventArgs());
      }
    }

    /// <summary>
    /// The Binding for the View Modes
    /// </summary>
    private ObservableCollection<string> _viewModes = new ObservableCollection<string>();
    public ObservableCollection<string> ViewModes
    {
      get => _viewModes;
      set { SetProperty(ref _viewModes, value); }
    }


    private int _selectedViewMode;

    public int SelectedViewMode
    {
      get => _selectedViewMode;
      set
      {
        SetProperty(ref _selectedViewMode, value);
        _options.MainSettings.DataProvider = _selectedViewMode;
        if (_selectedViewMode == 0)
        {
          _dataProvider = new TreeViewDataProvider();
        }
        else
        {
          // Database Data Provider
        }

      }
    }

    /// <summary>
    /// The Binding for scanning subfolders
    /// </summary>
    private bool _scanSubFolders;
    public bool ScanSubFolders
    {
      get => _scanSubFolders;
      set
      {
        SetProperty(ref _scanSubFolders, value);
        _options.MainSettings.ScanSubFolders = value;
      }
    }

    #endregion

    #region ctor

    public TreeviewViewModel()
    {
      log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger)?.GetLogger;
      log.Trace(">>>");
      _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;

      EventSystem.Subscribe<GenericEvent>(OnMessageReceived, ThreadOption.UIThread);

      _helper = new TreeViewHelper(this);
      DriveTypes = Enums.DriveTypes.LocalDisk | Enums.DriveTypes.NetworkDrive | Enums.DriveTypes.RemovableDisk;
      RootFolder = Environment.SpecialFolder.Desktop;
      _dataProvider = new TreeViewDataProvider();

      ViewModes.Add(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "treeView_Mode_Folder", LocalizeDictionary.Instance.Culture).ToString());
      ViewModes.Add(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "treeView_Mode_Database", LocalizeDictionary.Instance.Culture).ToString());

      SelectedViewMode = _options.MainSettings.DataProvider;

      ScanSubFolders = _options.MainSettings.ScanSubFolders;

      SelectedItemChangedCommand = new DelegateCommand<object>(SelectedItemChanged);
      LoadFolderOnDemandCommand = new DelegateCommand<object>(LoadFolderOnDemand);
      RefreshTreeViewCommand = new DelegateCommand<object>(RefreshTreeview);

      _init = true;
      RefreshTreeview();
      _init = false;

      // Work around the problem with Load On Demand being called from the Constructor.
      _timer = new DispatcherTimer();
      _timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
      _timer.Tick += SetCurrentFolder;
      _timer.Tag = RootFolder;
      _timer.Start();
      log.Trace("<<<");
    }

    #endregion

    #region Private Methods

    private void RefreshTreeview()
    {
      log.Trace(">>>");
      Nodes.Clear();
      _dataProvider.RequestRoot(_helper);
      
      // Don't call set current folder when called from Constructor
      if (!_init)
      {
        SetCurrentFolder(null, new EventArgs());
      }
      log.Trace("<<<");
    }


    /// <summary>
    /// Expand the tree to list the current folder.
    /// This cannot be used in the constructor, so it is invoked ny a Timer
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SetCurrentFolder(object sender, EventArgs e)
    {
      log.Trace(">>>");
      if (_timer != null && _timer.IsEnabled)
      {
        _timer.Stop();
      }

      var currentFolder = _options.MainSettings.LastFolderUsed;
      if (!Directory.Exists(currentFolder))
      {
        // Try to get 1 level up to find the parent
        var i = 0;
        while (i < 10)
        {
          if (Directory.Exists(currentFolder))
            break;

          currentFolder = currentFolder.Substring(0, currentFolder.LastIndexOf("\\"));
          i++; // Max of 10 levels, to avoid possible infinity loop
        }

        if (i == 10)
        {
          return;
        }
      }

      log.Info($"Set current folder to {currentFolder}");

      Cursor = Cursors.Wait;
      _helper.TreeView.Nodes[0].IsExpanded = true;
      _helper.TreeView.Nodes[0].Nodes[0].IsExpanded = true;
      var requestNetwork = currentFolder.StartsWith(@"\\");
      var nodeCol = _dataProvider.RequestDriveCollection(_helper, requestNetwork);

      var dirInfo = new DirectoryInfo(currentFolder);

      // get path tokens
      var dirs = new List<string> { dirInfo.FullName };

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
      Cursor = Cursors.Arrow;
      log.Trace("<<<");
    }

    /// <summary>
    /// Update the Recent Folder List, when a folder has been selected
    /// </summary>
    /// <param name="folder"></param>
    private void SetRecentFolder(string folder)
    {
      _options.MainSettings.RecentFolders.Remove(folder);
      _options.MainSettings.RecentFolders.Insert(0, folder);
      if (_options.MainSettings.RecentFolders.Count > 20)
      {
        _options.MainSettings.RecentFolders.RemoveAt(20);
      }

      // Set the Binding
      RecentFolders.Clear();
      RecentFolders.AddRange(_options.MainSettings.RecentFolders);
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
