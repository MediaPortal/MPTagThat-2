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
using System.Linq;
using System.Windows.Input;
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

#endregion

namespace MPTagThat.Treeview.ViewModels
{
  // This is the Viewmodel to handle Treeview related display of Folders
  public class TreeviewViewModel : NotificationObject
  {
    #region Variables

    private Options _options;
    private object _selectedItem;
    private DispatcherTimer _timer;
    
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
        _selectedItem = value;
        RaisePropertyChanged(() => SelectedItem);

        // Prepare Event to be published
        var selecteditem = "";
        if (_selectedItem is DriveItem)
        {
          selecteditem = (_selectedItem as DriveInfo).Name;
        }
        else if (_selectedItem is FolderItem)
        {
          selecteditem = (_selectedItem as FolderItem).FullPathName;
        }

        if (!string.IsNullOrEmpty(selecteditem))
        {
          _options.MainSettings.LastFolderUsed = selecteditem;

          GenericEvent evt = new GenericEvent();
          evt.Action = "selectedfolderchanged";
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
      if (param is INavTreeItem item)
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
      TreeViewItemAdv treeitem = (parameter as LoadonDemandEventArgs).TreeViewItem;
      if (treeitem != null)
      {
        treeitem.IsLoadOnDemand = false;
      }
    }


    private int _rootNr;
    public int RootNr
    {
      get => _rootNr;
      set
      {
        _rootNr = value;
        RaisePropertyChanged(() => RootNr);
      }
    }

    // RootChildren are used to bind to TreeView
    private ObservableCollection<INavTreeItem> _rootChildren = new ObservableCollection<INavTreeItem> { };
    public ObservableCollection<INavTreeItem> RootChildren
    {
      get => _rootChildren;
      set
      {
        _rootChildren = value;
        RaisePropertyChanged(() => RootChildren);
      }
    }
    #endregion

    #region ctor

    public TreeviewViewModel()
    {
      _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;

      // create a new RootItem given rootNumber using convention
      RootNr = 0;
      NavTreeItem treeRootItem = NavTreeUtils.ReturnRootItem(RootNr);

      // Delete RootChildren and init RootChildren ussing treeRootItem.Children
      foreach (INavTreeItem item in RootChildren) { 
        item.DeleteChildren(); 
      }
      RootChildren.Clear();

      SelectedItemChangedCommand = new DelegateCommand<object>(SelectedItemChanged);
      LoadFolderOnDemandCommand = new DelegateCommand<object>(LoadFolderOnDemand);

      foreach (INavTreeItem item in treeRootItem.Children) {
        RootChildren.Add(item); 
      }

      // Work around the problem with Load On Demand being called from the Constructor.
      _timer = new DispatcherTimer();
      _timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
      _timer.Tick += new EventHandler(SetCurrentFolder);
      _timer.Tag = treeRootItem;
      _timer.Start();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Expand the tree to list the current folder.
    /// This cannot be used in the constructor, so it is invoked ny a Timer
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SetCurrentFolder(object sender, EventArgs e)
    {
      _timer.Stop();
      var currentFolder = _options.MainSettings.LastFolderUsed;
      if (!System.IO.Directory.Exists(currentFolder))
      {
        return;
      }

      var treeRootItem = ((DispatcherTimer)sender).Tag as NavTreeItem;

      var splitFolder = currentFolder.Split(new char[] { '\\' });
      var currentDir = new List<string>();

      // We need to constrict something like this:
      // "D:", "D:[+]D:\\Music", "D:[+]D:\\Music[+]D:\\Music\\Eagles, The", "D:[+]D:\\Music[+]D:\\Music\\Eagles, The[+]D:\\Music\\Eagles, The\\Long Road Out Of Eden"
      for (int i = 0; i < splitFolder.Length; i++)
      {
        var tmpStr = splitFolder[0];
        if (i > 0)
        {
          tmpStr = currentDir[i - 1] + "[+]";
          for (int j = 0; j <= i; j++)
          {
            tmpStr += splitFolder[j] + "\\";
          }
          tmpStr = tmpStr.Substring(0, tmpStr.Length - 1);
        }
        currentDir.Add(tmpStr);
      }
      NavTreeUtils.ExpandCurrentFolder(currentDir, treeRootItem);
    }

    #endregion
  }
}
