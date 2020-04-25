#region Copyright (C) 2020 Team MediaPortal
// Copyright (C) 2020 Team MediaPortal
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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using CommonServiceLocator;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Core.Utils;
using MPTagThat.Core.Events;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using GridViewColumn = MPTagThat.Core.Common.GridViewColumn;
using Syncfusion.UI.Xaml.Grid;
using WPFLocalizeExtension.Engine;
using System.Windows.Threading;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using MPTagThat.Core.Services.ScriptManager;
using MPTagThat.Dialogs.ViewModels;
using Action = MPTagThat.Core.Common.Action;
using Prism.Services.Dialogs;
using Application = System.Windows.Forms.Application;
using Cursors = System.Windows.Input.Cursors;
using MessageBox = System.Windows.MessageBox;
// ReSharper disable ForCanBeConvertedToForeach

#endregion

namespace MPTagThat.SongGrid.ViewModels
{
  class SongGridViewModel : BindableBase
  {
    #region Variables

    private object _lock = new object();
    private IRegionManager _regionManager;
    private IDialogService _dialogService;
    private readonly NLogLogger log;
    private Options _options;
    private readonly SongGridViewColumns _gridColumns;

    private string _selectedFolder;
    private string[] _filterFileExtensions;
    private string _filterFileMask = "*.*";

    private List<string> _nonMusicFiles = new List<string>();
    private bool _progressCancelled = false;
    private bool _folderScanInProgress = false;

    private readonly System.Windows.Input.Cursor _numberOnClickCursor = new System.Windows.Input.Cursor(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/MPTagThat;component/Resources/Images/CursorNumbering.cur")).Stream);

    private BackgroundWorker _bgWorker;

    #endregion

    #region ctor

    public SongGridViewModel(IRegionManager regionManager, IDialogService dialogService)
    {
      _regionManager = regionManager;
      _dialogService = dialogService;
      log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger)?.GetLogger;
      log.Trace(">>>");
      _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;

      // Load the Settings
      _gridColumns = new SongGridViewColumns();
      CreateColumns();

      _songs = new BindingList<SongData>();
      ItemsSourceDataCommand = new BaseCommand(SetItemsSource);
      SelectionChangedCommand = new BaseCommand(SelectionChanged);

      EventSystem.Subscribe<GenericEvent>(OnMessageReceived, ThreadOption.UIThread);
      BindingOperations.EnableCollectionSynchronization(Songs, _lock);
      log.Trace("<<<");
    }

    #endregion

    #region Properties

    private System.Windows.Input.Cursor _customCursor;

    /// <summary>
    /// Custom Cursor object
    /// </summary>
    public System.Windows.Input.Cursor CustomCursor
    {
      get => _customCursor;
      set => SetProperty(ref _customCursor, value);
    }

    /// <summary>
    /// Reference to SongGrid. Set from code behind
    /// Used e.g in Find / Replace
    /// </summary>
    public SfDataGrid SongGrid { get; set; }

    /// <summary>
    /// The columns in the Grid
    /// </summary>
    public Columns DataGridColumns { get; set; }

    /// <summary>
    /// The Songs in the Grid
    /// </summary>

    private BindingList<SongData> _songs;
    public BindingList<SongData> Songs
    {
      get => _songs;
      set => SetProperty(ref _songs, value);
    }

    private ObservableCollection<object> _selectedItems = new ObservableCollection<object>();
    public ObservableCollection<object> SelectedItems
    {
      get => _selectedItems;
      set
      {
        _selectedItems = value;
        var evt = new StatusBarEvent { NumberOfSelectedFiles = _selectedItems.Count, Type = StatusBarEvent.StatusTypes.SelectedFiles };
        EventSystem.Publish(evt);
        RaisePropertyChanged("SelectedItems");
      }
    }

    /// <summary>
    ///   Do we have any changes pending?
    /// </summary>
    public bool ChangesPending
    {
      get { return Songs.Any(s => s.Changed); }
    }

    /// <summary>
    /// Binding for Wait Cursor
    /// </summary>
    private bool _isBusy;
    public bool IsBusy
    {
      get => _isBusy;
      set => SetProperty(ref _isBusy, value);
    }

    #endregion

    #region Commands

    public BaseCommand ItemsSourceDataCommand { get; set; }

    public ICommand SelectionChangedCommand { get; }

    private void SelectionChanged(object param)
    {
      if (param != null)
      {
        SelectedItems = (ObservableCollection<object>)param;
        var songs = SelectedItems.Cast<SongData>().ToList();
        // Handle Numberonclicked
        if (_options.NumberOnclick && songs.Count == 1)
        {
          songs[0].TrackNumber = (uint) _options.AutoNumber;
          _options.AutoNumber++;
          GenericEvent evt = new GenericEvent
          {
            Action = "autonumberchanged"
          };
          EventSystem.Publish(evt);
        }
        var parameters = new NavigationParameters();
        parameters.Add("songs", songs);
        _regionManager.RequestNavigate("TagEdit", "TagEditView", parameters);
      }
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

    /// <summary>
    /// Check, if we have any changes pending, when changing folder or closing the Application
    /// </summary>
    private void CheckChangesPending()
    {
      if (ChangesPending)
      {
        var result = MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Save_Changes", LocalizeDictionary.Instance.Culture).ToString(),
          LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Save_Changes_Title", LocalizeDictionary.Instance.Culture).ToString(), MessageBoxButton.YesNo);

        if (result == MessageBoxResult.Yes)
        {
          object[] parm = { "true" };
          ExecuteCommand("SaveAll", parm, false);
        }
      }
    }

    #endregion

    #region Script Handling

    /// <summary>
    ///   Executes a script on all selected rows
    /// </summary>
    /// <param name = "scriptFile"></param>
    public void ExecuteScript(string scriptFile)
    {
      log.Trace(">>>");
      Assembly assembly = (ServiceLocator.Current.GetInstance(typeof(IScriptManager)) as IScriptManager)?.Load(scriptFile);

      var count = 0;
      var msg = new ProgressBarEvent { CurrentProgress = 0, MinValue = 0, MaxValue = SelectedItems.Count };
      EventSystem.Publish(msg);

      try
      {
        if (assembly != null)
        {
          log.Debug($"Invoking Script: {scriptFile}");

          IsBusy = true;
          var songs = SelectedItems.Cast<SongData>().ToList();
          IScript script = (IScript)assembly.CreateInstance("Script");

          foreach (var song in songs)
          {
            count++;
            try
            {
              Application.DoEvents();
              msg.CurrentFile = song.FileName;
              msg.CurrentProgress = count;
              EventSystem.Publish(msg);

              song.Status = -1;
              script?.Invoke(song);
            }
            catch (Exception ex)
            {
              song.Status = 2;
              song.StatusMsg = ex.Message;
            }

          }
        }
      }
      catch (Exception ex)
      {
        log.Error("Script Execution failed: {0}", ex.Message);
        MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Script_Compile_Failed", LocalizeDictionary.Instance.Culture).ToString(),
          LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Error_Title", LocalizeDictionary.Instance.Culture).ToString(), MessageBoxButton.OK);
      }

      msg.MinValue = 0;
      msg.MaxValue = 0;
      msg.CurrentProgress = 0;
      msg.CurrentFile = "";
      EventSystem.Publish(msg);

      IsBusy = false;

      log.Trace("<<<");
    }

    #endregion

    #region Folder Scanning

    private async void FolderScan()
    {
      await Task.Run(() =>
      {
        if (System.Windows.Application.Current.Dispatcher != null)
          System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
            (SendOrPostCallback)delegate
           {
             log.Trace(">>>");
             if (String.IsNullOrEmpty(_selectedFolder))
             {
               log.Info("FolderScan: No folder selected");
               return;
             }

             IsBusy = true;
             _folderScanInProgress = true;
             Songs.Clear();
             _nonMusicFiles = new List<string>();
             GC.Collect();

             _options.ScanFolderRecursive = false;
             if (!Directory.Exists(_selectedFolder))
               return;

             // Get File Filter Settings
             _filterFileExtensions = new string[] { "*.*" };
             //_filterFileExtensions = _main.TreeView.ActiveFilter.FileFilter.Split('|');
             //_filterFileMask = _main.TreeView.ActiveFilter.FileMask.Trim() == ""
             //                    ? " * "
             //                    : _main.TreeView.ActiveFilter.FileMask.Trim();

             int count = 1;
             int nonMusicCount = 0;
             StatusBarEvent msg = new StatusBarEvent { CurrentFolder = _selectedFolder, CurrentProgress = -1 };

             try
             {
               foreach (FileInfo fi in GetFiles(new DirectoryInfo(_selectedFolder), _options.ScanFolderRecursive))
               {
                 Application.DoEvents();
                 if (_progressCancelled)
                 {
                   IsBusy = false;
                   break;
                 }

                 try
                 {
                   if (Util.IsAudio(fi.FullName))
                   {
                     msg.CurrentFile = fi.FullName;
                     log.Trace($"Retrieving file: {fi.FullName}");
                     // Read the Tag
                     var song = Song.Create(fi.FullName);
                     if (song != null)
                     {
                       //if (ApplyTagFilter(track))
                       //{
                       if (_options.MainSettings.MP3Validate && song.IsMp3)
                       {
                         log.Info($"Validating file {song.FullFileName}");
                         song.MP3ValidationError = Mp3Val.ValidateMp3File(song.FullFileName, out var strError);
                         song.StatusMsg = strError;
                         song.Status = song.MP3ValidationError != Util.MP3Error.NoError ? 3 : -1;
                       }
                       Songs.Add(song);
                       count++;
                       msg.NumberOfFiles = count;
                       EventSystem.Publish(msg);
                       //}
                     }
                   }
                   else
                   {
                     _nonMusicFiles.Add(fi.FullName);
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
               MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_OutOfMemory", LocalizeDictionary.Instance.Culture).ToString(),
                 LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_ErrorTitle", LocalizeDictionary.Instance.Culture).ToString(), MessageBoxButton.OK);
               log.Error("Folderscan: Running out of memory. Scanning aborted.");
             }

             // Commit changes to SongTemp, in case we have switched to DB Mode
             _options.Songlist.CommitDatabaseChanges();

             msg.CurrentProgress = 0;
             msg.CurrentFile = "";
             EventSystem.Publish(msg);
             log.Info($"FolderScan: Scanned {nonMusicCount + count} files. Found {count} audio files");

             var evt = new GenericEvent
             {
               Action = "miscfileschanged"
             };
             evt.MessageData.Add("files", _nonMusicFiles);
             EventSystem.Publish(evt);

             IsBusy = false;

             // Display Status Information
             try
             {
               //_main.ToolStripStatusFiles.Text = string.Format(localisation.ToString("main", "toolStripLabelFiles"), count, 0);
             }
             catch (InvalidOperationException)
             {
             }

             _folderScanInProgress = false;
             log.Trace("<<<");
           }, null);
      });
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

    #region Command Execution

    public void ExecuteCommand(string command)
    {
      object[] parameter = { };
      ExecuteCommand(command, parameter, true);
    }

    public void ExecuteCommand(string command, object parameters, bool runAsync)
    {
      log.Trace(">>>");
      log.Debug($"Invoking Command: {command}");

      object[] parameter = { command, parameters };

      if (runAsync)
      {
        Thread commandThread = new Thread(ExecuteCommandThread);
        commandThread.SetApartmentState(ApartmentState.STA);
        commandThread.Start(parameter);
      }
      else
      {
        ExecuteCommandThread(parameter);
      }
      log.Trace("<<<");
    }

    private async void ExecuteCommandThread(object param)
    {
      log.Trace(">>>");

      // Get the command object
      object[] parameters = param as object[];
      Commands.Command commandObj = Commands.Command.Create(parameters);
      if (commandObj == null)
      {
        return;
      }

      // Just in case that the Command needs to display a Dialog pass the Dialog Service
      commandObj.DialogService = _dialogService;

      // Extract the command name, since we might need it for specific selections afterwards
      var command = (string)parameters[0];

      // Extract Parameters
      var commandParmObj = (object[])parameters[1];
      var commandParm = commandParmObj.GetLength(0) > 0 ? (string)commandParmObj[0] : "";

      int count = 0;
      var msg = new ProgressBarEvent { CurrentProgress = 0, MinValue = 0, MaxValue = SelectedItems.Count };
      EventSystem.Publish(msg);

      // Select all Items in case of a SaveAll Command
      var selectedSongs = command == "SaveAll" ? Songs.ToList() : SelectedItems.Cast<SongData>().ToList();

      IsBusy = true;

      // If the command needs Preprocessing, then first loop over all tracks
      if (commandObj.NeedsPreprocessing)
      {
        foreach (var song in Songs)
        {
          commandObj.PreProcess(song);
        }
      }

      // Iterate in a for loop, since we are passing
      // the song as reference, which is not allowed in a foreach
      for (var i = 0; i < Songs.Count; i++)
      {
        var song = Songs[i];

        if (!selectedSongs.Contains(song))
        {
          continue;
        }

        count++;
        try
        {
          Application.DoEvents();

          song.Status = -1;
          if (command != "SaveAll" || commandParm == "true")
          {
            msg.CurrentFile = song.FileName;
            msg.CurrentProgress = count;
            EventSystem.Publish(msg);
            if (_progressCancelled)
            {
              commandObj.ProgressCancelled = true;
              msg.MinValue = 0;
              msg.MaxValue = 0;
              msg.CurrentProgress = 0;
              msg.CurrentFile = "";
              EventSystem.Publish(msg);
              return;
            }
          }

          if (command == "SaveAll")
          {
            song.Status = -1;
          }

          var result = await commandObj.Execute(song);
          if (result.Changed)
          {
            song = result.song;
            song.Changed = true;
          }

          // Has the file be renamed during save?
          if (Songs[i].FullFileName != song.FullFileName)
          {
            Songs.RemoveAt(i);
            Songs.Insert(i, song);
          }

          if (commandObj.ProgressCancelled)
          {
            break;
          }
        }
        catch (Exception ex)
        {
          song.Status = 2;
          song.StatusMsg = ex.Message;
        }
      }

      // Do Command Post Processing
      if (commandObj.NeedsPostprocessing)
      {
        foreach (var song in Songs)
        {
          commandObj.PostProcess(song);
        }
      }

      if (commandObj.NeedsCallback)
      {
        commandObj.CmdCallback();
      }

      msg.MinValue = 0;
      msg.MaxValue = 0;
      msg.CurrentProgress = 0;
      msg.CurrentFile = "";
      EventSystem.Publish(msg);

      IsBusy = false;

      commandObj.Dispose();
      log.Trace("<<<");
    }

    #endregion

    #region Event Handling

    // List of Supported commands, that should be processed by ExecuteCommand
    // Commands that show a Dialog, like Lyrics, CoverARt, etc. should not specified here, they will be handled as dialogs
    private readonly List<Action.ActionType> _supportedCommands = new List<Action.ActionType>()
      { Action.ActionType.SAVE,
        Action.ActionType.SAVEALL,
        Action.ActionType.REMOVEPICTURE,
        Action.ActionType.CASECONVERSION_BATCH,
        Action.ActionType.DELETEALLTAGS,
        Action.ActionType.DELETEV1TAGS,
        Action.ActionType.DELETEV2TAGS,
        Action.ActionType.REMOVECOMMENT,
        Action.ActionType.BPM,
        Action.ActionType.REPLAYGAIN,
        Action.ActionType.IDENTIFYFILE,
        Action.ActionType.VALIDATEMP3,
        Action.ActionType.FIXMP3,
        Action.ActionType.AUTONUMBER,
        Action.ActionType.MusicBrainzInfo,
      };

    private void OnMessageReceived(GenericEvent msg)
    {
      switch (msg.Action.ToLower())
      {
        case "selectedfolderchanged":
          CheckChangesPending();
          if (msg.MessageData.ContainsKey("folder"))
          {
            SelectedItems.Clear();
            _selectedFolder = (string)msg.MessageData["folder"];
            FolderScan();
          }
          break;

        case "command":

          var command = (Action.ActionType)msg.MessageData["command"];

          if (command == Action.ActionType.NUMBERONCLICK)
          {
            if (_options.NumberOnclick)
            {
              CustomCursor = _numberOnClickCursor;
            }
            else
            {
              CustomCursor = Cursors.Arrow;
            }

            return;
          }

          // Refresh the current folder
          if (command == Action.ActionType.REFRESH)
          {
            CheckChangesPending();
            FolderScan();
            return;
          }

          // Select all songs, except for Find & Replace and Help
          if (SelectedItems.Count == 0 && (command != Action.ActionType.FIND && command != Action.ActionType.REPLACE && command != Action.ActionType.HELP))
          {
            Songs.ToList().ForEach(song => SelectedItems.Add(song));
          }

          // Run Commands, which don't display a dialog
          if (_supportedCommands.Contains(command))
          {
            msg.MessageData.TryGetValue("runasync", out var runAsyncParam);
            var runAsync = true;
            if (runAsyncParam != null)
            {
              runAsync = (bool)runAsyncParam;
            }

            // If we have Replaygain and mutiple songs selected, we should do Album Gain
            if (command == Action.ActionType.REPLAYGAIN && SelectedItems.Count > 1)
            {
              msg.MessageData.Add("param", new object[] { "AlbumGain" });
            }

            msg.MessageData.TryGetValue("param", out var param);
            object parameter;
            parameter = param ?? new object[] { };

            ExecuteCommand(Action.ActionToCommand(command), parameter, runAsync);
            return;
          }

          // Execute a script
          if (command == Action.ActionType.SCRIPTEXECUTE)
          {
            ExecuteScript(_options.MainSettings.ActiveScript);
            return;
          }

          // Add Commands needing parameters below

          var parameters = new DialogParameters();

          if (command == Action.ActionType.HELP)
          {
            _dialogService.ShowDialogInAnotherWindow("AboutView", "DialogWindowView", parameters, null);
            return;
          }

          if (command == Action.ActionType.FIND)
          {
            parameters.Add("method", "Find");
            parameters.Add("songgrid", SongGrid);
            _dialogService.ShowDialogInAnotherWindow("FindReplaceView", "DialogWindowView", parameters, null);
            return;
          }

          if (command == Action.ActionType.REPLACE)
          {
            parameters.Add("method", "Replace");
            parameters.Add("songgrid", SongGrid);
            _dialogService.ShowDialogInAnotherWindow("FindReplaceView", "DialogWindowView", parameters, null);
            return;
          }

          // If a command needs access to songs add it below

          var songs = SelectedItems.Cast<SongData>().ToList();
          parameters.Add("songs", songs);

          if (command == Action.ActionType.FILENAME2TAG)
          {
            _dialogService.ShowDialogInAnotherWindow("FileName2TagView", "DialogWindowView", parameters, null);
            return;
          }

          if (command == Action.ActionType.TAG2FILENAME)
          {
            _dialogService.ShowDialogInAnotherWindow("Tag2FileNameView", "DialogWindowView", parameters, null);
            return;
          }

          if (command == Action.ActionType.GETCOVERART)
          {
            if (_options.MainSettings.EmbedFolderThumb && !_options.MainSettings.OnlySaveFolderThumb
                                                       && File.Exists(Path.Combine(songs[0].FilePath, "folder.jpg")))
            {
              log.Debug("CoverArt: Using existing folder.jpg");
              Picture folderThumb = null;
              var savedFolder = "";
              foreach (var song in songs)
              {
                if (folderThumb == null || Path.GetDirectoryName(song.FullFileName) != savedFolder)
                {
                  savedFolder = Path.GetDirectoryName(song.FullFileName);
                  folderThumb = Util.GetFolderThumb(savedFolder);
                }

                if (folderThumb != null)
                {
                  // Only write a picture if we don't have a picture OR Overwrite Pictures is set
                  if (song.Pictures.Count == 0 || _options.MainSettings.OverwriteExistingCovers)
                  {
                    if (_options.MainSettings.ChangeCoverSize && Picture.ImageFromData(folderThumb.Data).Width > _options.MainSettings.MaxCoverWidth)
                    {
                      folderThumb.Resize(_options.MainSettings.MaxCoverWidth);
                    }
                    // First Clear all the existingPictures
                    song.Pictures.Clear();
                    song.Pictures.Add(folderThumb);
                  }
                }
              }
              return;
            }

            if (msg.MessageData.ContainsKey("removeexistingpictures"))
            {
              parameters.Add("removeexistingpictures", msg.MessageData["removeexistingpictures"]);
            }
            _dialogService.ShowDialogInAnotherWindow("AlbumCoverSearchView", "DialogWindowView", parameters, null);
            return;
          }

          if (command == Action.ActionType.GETLYRICS)
          {
            _dialogService.ShowDialogInAnotherWindow("LyricsSearchView", "DialogWindowView", parameters, null);
            return;
          }

          if (command == Action.ActionType.ORGANISE)
          {
            parameters.Add("songgridinstance", this);  // We need to reference to the SongGrid using Reflection for a Save all Command
            _dialogService.ShowDialogInAnotherWindow("OrganiseFilesView", "DialogWindowView", parameters, null);
            return;
          }

          if (command == Action.ActionType.CASECONVERSION)
          {
            _dialogService.ShowDialogInAnotherWindow("CaseConversionView", "DialogWindowView", parameters, null);
            return;
          }

          if (command == Action.ActionType.TAGFROMINTERNET)
          {
            _dialogService.ShowDialogInAnotherWindow("TagFromInternetView", "DialogWindowView", parameters, null);
            return;
          }

          break;

        case "applicationclosing":
          CheckChangesPending();
          break;
      }
    }

    #endregion
  }
}
