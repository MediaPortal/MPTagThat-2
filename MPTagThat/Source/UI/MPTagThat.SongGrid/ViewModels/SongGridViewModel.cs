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
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using CommonServiceLocator;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Core.Utils;
using MPTagThat.Core.Events;
using Prism.Mvvm;
using Syncfusion.Data.Extensions;
using GridViewColumn = MPTagThat.Core.Common.GridViewColumn;
using Syncfusion.UI.Xaml.Grid;
using WPFLocalizeExtension.Engine;

#endregion

namespace MPTagThat.SongGrid.ViewModels
{
  class SongGridViewModel : BindableBase, INotifyPropertyChanged
  {
    #region Variables

    private readonly NLogLogger log;
    private Options _options;
    private readonly SongGridViewColumns _gridColumns;

    private string _selectedFolder;
    private string[] _filterFileExtensions;
    private string _filterFileMask = "*.*";

    private List<FileInfo> _nonMusicFiles = new List<FileInfo>();
    private bool _progressCancelled = false;
    private bool _folderScanInProgress = false;

    #endregion

    public SongGridViewModel()
    {
      log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger).GetLogger;
      _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;

      // Load the Settings
      _gridColumns = new SongGridViewColumns();
      CreateColumns();

      Songs = _options.Songlist;
      ItemsSourceDataCommand = new RelayCommand(SetItemsSource);

      EventSystem.Subscribe<GenericEvent>(OnMessageReceived);
    }

    public RelayCommand ItemsSourceDataCommand { get; set; }

    public Columns DataGridColumns { get; set; }

    public BindingList<SongData> Songs
    {
      get => _options.Songlist;
      set
      { 
        _options.Songlist = (SongList)value;
        OnPropertyChanged("Songs");
      }
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region Private Methods

    public void SetItemsSource(object grid)
    {
      if (grid is SfDataGrid dataGrid) dataGrid.ItemsSource = Songs;
    }

    /// <summary>
    ///   Create the Columns of the Grid based on the users setting
    /// </summary>
    private void CreateColumns()
    {
      log.Trace(">>>");

      DataGridColumns = new Columns();
      // Now create the columns 
      foreach (GridViewColumn column in _gridColumns.Settings.Columns)
      {
        DataGridColumns.Add(Util.FormatGridColumn(column));
      }

      log.Trace("<<<");
    }

    #endregion

    #region Folder Scanning

    public void Folderscan()
    {
      log.Trace(">>>");
      if (_folderScanInProgress)
      {
        return;
      }
      if (String.IsNullOrEmpty(_selectedFolder))
      {
        log.Info("FolderScan: No folder selected");
        return;
      }
      _folderScanInProgress = true;
      //tracksGrid.Rows.Clear();
      Songs.Clear();
      //_nonMusicFiles = new List<FileInfo>();
      //_main.MiscInfoPanel.ClearNonMusicFiles();
      //_main.MiscInfoPanel.ActivateNonMusicTab();
      GC.Collect();

      _options.ScanFolderRecursive = false;
      if (!Directory.Exists(_selectedFolder))
        return;

      //_main.FolderScanning = true;

      // Get File Filter Settings
      _filterFileExtensions = new string[] { "*.*" };
      //_filterFileExtensions = _main.TreeView.ActiveFilter.FileFilter.Split('|');
      //_filterFileMask = _main.TreeView.ActiveFilter.FileMask.Trim() == ""
      //                    ? " * "
      //                    : _main.TreeView.ActiveFilter.FileMask.Trim();

      //SetProgressBar(1);

      // Change the style to Marquee, since we really don't know how much files we will get
      //_main.progressBar1.Style = ProgressBarStyle.Marquee;
      //_main.progressBar1.MarqueeAnimationSpeed = 10;

      int count = 1;
      int nonMusicCount = 0;
      StatusBarEvent msg = new StatusBarEvent {CurrentFolder = _selectedFolder, CurrentProgress = -1 };

      try
      {
        foreach (FileInfo fi in GetFiles(new DirectoryInfo(_selectedFolder), _options.ScanFolderRecursive))
        {
          if (_progressCancelled)
          {
            break;
          }
          try
          {
            if (Util.IsAudio(fi.FullName))
            {
              msg.CurrentFile = fi.FullName;
              log.Trace($"Retrieving file: {fi.FullName}");
              // Read the Tag
              SongData track = Song.Create(fi.FullName);
              if (track != null)
              {
                //if (ApplyTagFilter(track))
                //{
                  Songs.Add(track);
                  count++;
                  msg.NumberOfFiles = count;
                  EventSystem.Publish(msg);
                //}
              }
            }
            else
            {
              _nonMusicFiles.Add(fi);
              nonMusicCount++;
            }
          }
          catch (PathTooLongException)
          {
            log.Warn($"FolderScan: Ignoring track {fi.FullName} - path too long!");
            continue;
          }
          catch (System.UnauthorizedAccessException exUna)
          {
            log.Warn($"FolderScan: Could not access file or folder: {exUna.Message}. {fi.FullName}");
          }
          catch (Exception ex)
          {
            log.Error($"FolderScan: Caught error processing files: {ex.Message} {fi.FullName}");
          }
        }
      }
      catch (OutOfMemoryException)
      {
        GC.Collect();
        MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_OutOfMemory", 
            LocalizeDictionary.Instance.Culture).ToString(),
          LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_ErrorTitle",
            LocalizeDictionary.Instance.Culture).ToString(),
                        MessageBoxButtons.OK);
        log.Error("Folderscan: Running out of memory. Scanning aborted.");
      }

      // Commit changes to SongTemp, in case we have switched to DB Mode
      _options.Songlist.CommitDatabaseChanges();

      msg.CurrentProgress = 100;
      EventSystem.Publish(msg);
      log.Info($"FolderScan: Scanned {nonMusicCount + count} files. Found {count} audio files");

      //_main.MiscInfoPanel.AddNonMusicFiles(_nonMusicFiles);

      //_main.ToolStripStatusScan.Text = "";


      //ResetProgressBar();
      //_main.progressBar1.Style = ProgressBarStyle.Continuous;

      // Display Status Information
      try
      {
        //_main.ToolStripStatusFiles.Text = string.Format(localisation.ToString("main", "toolStripLabelFiles"), count, 0);
      }
      catch (InvalidOperationException) { }

      /*
      // unselect the first row, which would be selected automatically by the grid
      // And set the background color of the rating cell, as it isn't reset by the grid
      try
      {
        if (tracksGrid.Rows.Count > 0)
        {
          _main.TagEditForm.ClearForm();
          tracksGrid.Rows[0].Selected = false;
        }
      }
      catch (ArgumentOutOfRangeException) { }

      // If MP3 Validation is turned on, set the color
      if (Options.MainSettings.MP3Validate)
      {
        ChangeErrorRowColor();
      }
      
      _main.FolderScanning = false;
      */
    _folderScanInProgress = false;
    log.Trace("<<<");
  }

    /// <summary>
    ///   Read a Folder and return the files
    /// </summary>
    /// <param name = "folder"></param>
    /// <param name = "foundFiles"></param>
    private IEnumerable<FileInfo> GetFiles(DirectoryInfo dirInfo, bool recursive)
    {
      Queue<DirectoryInfo> directories = new Queue<DirectoryInfo>();
      directories.Enqueue(dirInfo);
      Queue<FileInfo> files = new Queue<FileInfo>();
      while (files.Count > 0 || directories.Count > 0)
      {
        if (files.Count > 0)
        {
          yield return files.Dequeue();
        }
        try
        {
          if (directories.Count > 0)
          {
            DirectoryInfo dir = directories.Dequeue();

            if (recursive)
            {
              DirectoryInfo[] newDirectories = dir.GetDirectories();
              foreach (DirectoryInfo di in newDirectories)
              {
                directories.Enqueue(di);
              }
            }

            foreach (string extension in _filterFileExtensions)
            {
              string searchFilter = string.Format("{0}.{1}", _filterFileMask, extension);
              FileInfo[] newFiles = dir.GetFiles(searchFilter);
              foreach (FileInfo file in newFiles)
              {
                files.Enqueue(file);
              }
            }
          }
        }
        catch (UnauthorizedAccessException ex)
        {
          log.Error($"{ex.Message}, {ex}");
        }
      }
    }

    #endregion

    #region Event Handling

    private void OnMessageReceived(GenericEvent msg)
    {
      switch (msg.Action.ToLower())
      {
        case "selectedfolderchanged":
          if (msg.MessageData.ContainsKey("folder"))
          {
            _selectedFolder = (string)msg.MessageData["folder"];
            Folderscan();
          }
          break;
      }
    }
    
    #endregion
  }
}
