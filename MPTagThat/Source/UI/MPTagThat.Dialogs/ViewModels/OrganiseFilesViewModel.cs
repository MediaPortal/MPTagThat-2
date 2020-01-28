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
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Prism.Services.Dialogs;
using CommonServiceLocator;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Core.Utils;
using Syncfusion.UI.Xaml.Grid;
using WPFLocalizeExtension.Engine;
using System.Collections;
using System.Reflection;
using MPTagThat.Core;
using MPTagThat.Core.Services.ScriptManager;
using Action = MPTagThat.Core.Common.Action;
using DialogResult = System.Windows.Forms.DialogResult;
using Microsoft.VisualBasic.FileIO;
// ReSharper disable CommentTypo

// ReSharper disable IdentifierTypo
// ReSharper disable StringIndexOfIsCultureSpecific.1

#endregion

namespace MPTagThat.Dialogs.ViewModels
{
  public class OrganiseFilesViewModel : DialogViewModelBase
  {
    #region Variables

    private readonly Options _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;
    private readonly NLogLogger log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger)?.GetLogger;
    private List<SongData> _songs;
    private Assembly _scriptAssembly;
    private object _instance;

    #endregion

    #region Properties

    public Brush Background => (Brush)new BrushConverter().ConvertFromString(_options.MainSettings.BackGround);

    /// <summary>
    /// The Binding for the TargetPath ComboBox
    /// </summary>
    private ObservableCollection<string> _targetPath = new ObservableCollection<string>();
    public ObservableCollection<string> TargetPath
    {
      get => _targetPath;
      set
      {
        _targetPath = value;
        RaisePropertyChanged("TargetPath");
      }
    }

    /// <summary>
    /// The Binding for the Selected Index in the TargetPath Combobox
    /// </summary>
    private int _selectedIndexTargetPath;

    public int SelectedIndexTargetPath
    {
      get => _selectedIndexTargetPath;
      set => SetProperty(ref _selectedIndexTargetPath, value);
    }

    /// <summary>
    /// The Binding for the Parameters in the ComboBox
    /// </summary>
    private ObservableCollection<string> _parameters = new ObservableCollection<string>();
    public ObservableCollection<string> Parameters
    {
      get => _parameters;
      set
      {
        _parameters = value;
        RaisePropertyChanged("Parameters");
      }
    }

    /// <summary>
    /// The Selected Text in the Parameter Combobox
    /// </summary>
    private string _selectedItemText;

    public string SelectedItemText
    {
      get => _selectedItemText;
      set => SetProperty(ref _selectedItemText, value);
    }

    /// <summary>
    /// The Binding for the Selected Index in the Parameter Combobox
    /// </summary>
    private int _selectedIndex;

    public int SelectedIndex
    {
      get => _selectedIndex;
      set => SetProperty(ref _selectedIndex, value);
    }

    /// <summary>
    /// The Cursor Position, while Editing the Parameter Combobox
    /// </summary>
    private int _cursorPositionCombo;
    public int CursorPositionCombo
    {
      get => _cursorPositionCombo;
      set => SetProperty(ref _cursorPositionCombo, value);
    }

    /// <summary>
    /// The Binding for the Songs in the Preview Grid
    /// </summary>
    private ObservableCollection<SongData> _songsPreview = new ObservableCollection<SongData>();
    public ObservableCollection<SongData> SongsPreview
    {
      get => _songsPreview;
      set => SetProperty(ref _songsPreview, value);
    }

    /// <summary>
    /// The columns in the Preview Grid
    /// </summary>
    public Columns DataGridColumns { get; set; } = new Columns();

    /// <summary>
    /// The Selected Tab Index in the NavigationTab
    /// </summary>
    private int _selectedTabIndex;

    public int SelectedTabIndex
    {
      get => _selectedTabIndex;
      set => SetProperty(ref _selectedTabIndex, value);
    }

    /// <summary>
    /// The Binding for the Script ComboBox
    /// </summary>
    private ObservableCollection<Item> _scripts = new ObservableCollection<Item>();
    public ObservableCollection<Item> Scripts
    {
      get => _scripts;
      set
      {
        _scripts = value;
        RaisePropertyChanged("Scripts");
      }
    }

    /// <summary>
    /// The Binding for the Selected Index in the Scripts Combobox
    /// </summary>
    private int _selectedIndexScripts;

    public int SelectedIndexScripts
    {
      get => _selectedIndexScripts;
      set => SetProperty(ref _selectedIndexScripts, value);
    }

    // Check Box Checked properties
    private bool _ckOverWriteFiles;
    public bool CkOverWriteFiles { get => _ckOverWriteFiles; set => SetProperty(ref _ckOverWriteFiles, value); }

    private bool _ckCopyFiles;
    public bool CkCopyFiles { get => _ckCopyFiles; set => SetProperty(ref _ckCopyFiles, value); }

    private bool _ckCopyNonMusicFiles;
    public bool CkCopyNonMusicFiles { get => _ckCopyNonMusicFiles; set => SetProperty(ref _ckCopyNonMusicFiles, value); }

    #endregion

    #region ctor

    public OrganiseFilesViewModel()
    {
      Title = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "organise_Header",
        LocalizeDictionary.Instance.Culture).ToString();
      CancelChangesCommand = new BaseCommand(CancelChanges);
      LabelClickedCommand = new BaseCommand(LabelClicked);
      OrganiseFilesCommand = new BaseCommand(OrganiseFilesApply);
      PreviewChangesCommand = new BaseCommand(PreviewChanges);
      AddFormatCommand = new BaseCommand(AddFormat);
      RemoveFormatCommand = new BaseCommand(RemoveFormat);
      SelectionChangingCommand = new BaseCommand(SelectionChanging);
      BrowseFolderCommand = new BaseCommand(BrowseFolder);
    }

    #endregion

    #region Commands

    /// <summary>
    /// The Apply Button has been pressed
    /// </summary>
    public ICommand OrganiseFilesCommand { get; }

    private void OrganiseFilesApply(object param)
    {
      log.Trace(">>>");
      if (!Util.CheckParameterFormat(SelectedItemText, Options.ParameterFormat.Organise))
      {
        MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "tagAndRename_InvalidParm", LocalizeDictionary.Instance.Culture).ToString(),
          LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Error_Title", LocalizeDictionary.Instance.Culture).ToString(), MessageBoxButton.OK);
        return;
      }

      // See if we have a script selected
      if (SelectedIndexScripts > 0)
      {
        var scriptName = Scripts[SelectedIndexScripts].Name;
        _scriptAssembly =
          (ServiceLocator.Current.GetInstance(typeof(IScriptManager)) as IScriptManager)?.Load(scriptName);
      }

      OrganiseFiles(SelectedItemText);

      // Did we get a new Format in the list, then store it temporarily
      if (!_options.OrganiseSettingsTemp.Contains(SelectedItemText))
      {
        _options.OrganiseSettingsTemp.Add(SelectedItemText);
      }

      _options.OrganiseSettings.LastUsedFormat = SelectedIndex;
      _options.OrganiseSettings.LastUsedFolderIndex = SelectedIndexTargetPath;
      _options.OrganiseSettings.CopyFiles = CkCopyFiles;
      _options.OrganiseSettings.OverWriteFiles = CkOverWriteFiles;
      _options.OrganiseSettings.CopyNonMusicFiles = CkCopyNonMusicFiles;
      _options.OrganiseSettings.LastUsedScript = SelectedIndexScripts > -1 ? Scripts[SelectedIndexScripts].Name : "";
      CloseDialog("true");
      log.Trace("<<<");
    }

    /// <summary>
    /// Runs the Organise
    /// </summary>
    private void OrganiseFiles(string parameter)
    {
      log.Trace(">>>");

      // Use Reflection. We cannot use the Event Aggregator since this doesn't run synced
      if (_instance != null)
      {
        log.Debug("Saving All Pending changes first");
        var method = _instance.GetType().GetMethod("ExecuteCommand", new[] { typeof(string), typeof(object), typeof(bool) });
        method?.Invoke(_instance, new object[] { "SaveAll", new object[] { "true" }, false });
        log.Debug("Finished Saving All Pending changes");
      }

      var directories = new Dictionary<string, string>();
      var targetFolder = TargetPath[SelectedIndexTargetPath];
      bool bError = false;

      foreach (var song in _songs)
      {
        // Replace the Parameter Value with the Values from the song
        var resolvedParmString = Util.ReplaceParametersWithTrackValues(parameter, song);

        if (_scriptAssembly != null) // Do we have a script selected
        {
          try
          {
            IScript script = (IScript)_scriptAssembly.CreateInstance("Script");
            targetFolder = script?.Invoke(song);
            if (targetFolder == "")
            {
              // Fall back to standard folder, if something happened in the script
              targetFolder = TargetPath[SelectedIndexTargetPath];
            }
          }
          catch (Exception ex)
          {
            log.Error("Script Execution failed: {0}", ex.Message);
          }
        }

        var directoryName = "";
        if (resolvedParmString.Contains(@"\"))
        {
          directoryName = Path.GetDirectoryName(resolvedParmString);
        }

        directoryName = Path.Combine(targetFolder, directoryName);

        try
        {
          // Now check the validity of the directory
          if (!Directory.Exists(directoryName))
          {
            try
            {
              Directory.CreateDirectory(directoryName);
            }
            catch (Exception e1)
            {
              bError = true;
              log.Debug($"Error creating folder: {directoryName} {e1.Message}");
              song.Status = 2;
              song.StatusMsg = string.Format("{0}: {1} {2}", LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Error_Title",
                LocalizeDictionary.Instance.Culture).ToString(), directoryName, e1.Message);
              continue; // Process next row
            }
          }

          // Store the directory of the current file in the dictionary, so that we may copy later all non-music files
          var dir = Path.GetDirectoryName(song.FullFileName);
          if (!directories.ContainsKey(dir))
          {
            directories.Add(dir, directoryName);
          }

          // Now construct the new File Name
          var newFilename = resolvedParmString;
          var lastBackSlash = resolvedParmString.LastIndexOf(@"\");
          if (lastBackSlash > -1)
          {
            newFilename = resolvedParmString.Substring(lastBackSlash + 1);
          }
          newFilename += song.FileExt;
          newFilename = Path.Combine(directoryName, newFilename);

          try
          {
            if (!CkOverWriteFiles)
            {
              if (File.Exists(newFilename))
              {
                bError = true;
                log.Debug($"File exists: {newFilename}");
                song.Status = 2;
                song.StatusMsg = string.Format("{0}: {1}", LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "organise_FileExists",
                  LocalizeDictionary.Instance.Culture).ToString(), directoryName);
                continue;
              }
            }

            // If new file name validates to be the same as the old file, i.e. it goes into the source folder
            // then continue, as this would lead in the source file to be deleted first and then there's nothing, which could be copied
            if (newFilename.ToLowerInvariant() == song.FullFileName.ToLowerInvariant())
            {
              bError = true;
              log.Debug($"Old File and New File same: {newFilename}");
              song.Status = 2;
              song.StatusMsg = string.Format("{0}: {1}", LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "organise_SameFile",
                LocalizeDictionary.Instance.Culture).ToString(), directoryName);
              continue;
            }

            if (CkCopyFiles)
            {
              FileSystem.CopyFile(song.FullFileName, newFilename, CkOverWriteFiles);
              song.Status = 0;
            }
            else
            {
              FileSystem.MoveFile(song.FullFileName, newFilename, CkOverWriteFiles);
              song.Status = 0;
            }

            // TODO: Update the Music Database
            //var originalFileName = song.FullFileName;
            //song.FullFileName = newFilename;
            //ServiceScope.Get<IMusicDatabase>().UpdateTrack(song, originalFileName);
          }
          catch (Exception e2)
          {
            bError = true;
            log.Error("Error Copy/Move File: {0} {1}", song.FullFileName, e2.Message);
            song.Status = 2;
            song.StatusMsg = string.Format("{0}: {1}", LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Error_Title",
              LocalizeDictionary.Instance.Culture).ToString(), e2.Message);
          }
        }
        catch (Exception ex)
        {
          bError = true;
          log.Error("Error Organising Files: {0} stack: {1}", ex.Message, ex.StackTrace);
          song.Status = 2;
          song.StatusMsg = string.Format("{0}: {1}", LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Error_Title",
            LocalizeDictionary.Instance.Culture).ToString(), ex.Message);
        }
      }

      // Now that we have Moved/Copied the individual Files, we will copy / move the Pictures, etc.
      if (CkCopyNonMusicFiles)
      {
        foreach (var dir in directories.Keys)
        {
          var files = Directory.GetFiles(dir);
          foreach (var file in files)
          {
            // ignore audio files, we've processed them before
            // and might have got an error
            if (Util.IsAudio(file))
              continue;

            var newFilename = Path.Combine(directories[dir], Path.GetFileName(file));
            try
            {
              if (!CkOverWriteFiles)
              {
                if (File.Exists(newFilename))
                {
                  log.Debug($"File exists: {newFilename}");
                  continue;
                }
              }

              if (CkCopyFiles)
              {
                FileSystem.CopyFile(file, newFilename, UIOption.AllDialogs, UICancelOption.DoNothing);
              }
              else
              {
                FileSystem.MoveFile(file, newFilename, UIOption.AllDialogs, UICancelOption.DoNothing);
              }
            }
            catch (Exception ex)
            {
              log.Error("Error Copy/Move File: {0} {1}", file, ex.Message);
            }
          }
        }
      }

      // Delete empty folders,if we didn't get any error
      if (!CkCopyFiles && !bError)
      {
        foreach (string dir in directories.Keys)
        {
          DeleteSubFolders(dir);
          DeleteParentFolders(dir);
        }

        string currentSelectedFolder = _options.MainSettings.LastFolderUsed;
        // Go up 1 level in the directory structure to find an existing folder
        int i = 0;
        while (i < 10)
        {
          if (Directory.Exists(currentSelectedFolder))
            break;

          currentSelectedFolder = currentSelectedFolder.Substring(0, currentSelectedFolder.LastIndexOf("\\"));
          i++; // Max of 10 levels, to avoid possible infinity loop
        }
        _options.MainSettings.LastFolderUsed = currentSelectedFolder;
        GenericEvent evt = new GenericEvent
        {
          Action = "currentfolderchanged"
        };
        EventSystem.Publish(evt);
      }
      
      log.Trace("<<<");
    }

    private void DeleteSubFolders(string folder)
    {
      foreach (var f in Directory.GetDirectories(folder))
      {
        DeleteSubFolders(f);
      }
      string[] files = Directory.GetFiles(folder);
      string[] subDirs = Directory.GetDirectories(folder);
      // Do we still have files or folders then skip the delete
      if (files.Length == 0 && subDirs.Length == 0)
        DeleteFolder(folder);
    }

    private void DeleteParentFolders(string folder)
    {
      int lastSlash = folder.LastIndexOf("\\");

      string parentFolder = folder;
      while (lastSlash > -1)
      {
        parentFolder = parentFolder.Substring(0, lastSlash);
        string[] files = Directory.GetFiles(parentFolder);
        string[] subDirs = Directory.GetDirectories(parentFolder);
        // Do we still have files or folders then skip the delete
        if (files.Length == 0 && subDirs.Length == 0)
          DeleteFolder(parentFolder);

        lastSlash = parentFolder.LastIndexOf("\\");
      }
    }

    private void DeleteFolder(string folder)
    {
      try
      {
        Directory.Delete(folder);
      }
      catch (Exception ex)
      {
        log.Error("Error Deleting Folder: {0} {1}", folder, ex.Message);
      }
    }

    /// <summary>
    /// Cancel Button has been clicked
    /// </summary>
    public ICommand CancelChangesCommand { get; }

    private void CancelChanges(object parameters)
    {
      CloseDialog("false");
    }

    /// <summary>
    /// One of the labels has been clicked.
    /// </summary>
    public ICommand LabelClickedCommand { get; }

    private void LabelClicked(object param)
    {
      if (param == null)
      {
        return;
      }
      var label = (param as MouseButtonEventArgs)?.Source as System.Windows.Controls.Label;
      var parameter = Util.LabelToParameter(label?.Name);
      if (parameter != String.Empty)
      {
        var currentCursorPos = CursorPositionCombo;
        SelectedItemText = SelectedItemText.Insert(CursorPositionCombo, parameter);
        CursorPositionCombo = currentCursorPos + parameter.Length;
      }
    }

    /// <summary>
    /// The Preview has been requested, by scrolling the navigator
    /// </summary>
    public ICommand SelectionChangingCommand { get; set; }

    private void SelectionChanging(object parm)
    {
      SongsPreview.Clear();
      DataGridColumns = new Columns();
      FillPreview();
    }

    /// <summary>
    /// The Preview Button has been pressed
    /// </summary>
    public ICommand PreviewChangesCommand { get; set; }

    private void PreviewChanges(object parm)
    {
      SelectedTabIndex = 1;
    }

    /// <summary>
    /// Fill the Preview Grid
    /// </summary>
    private void FillPreview()
    {
      log.Trace(">>>");

      if (!Util.CheckParameterFormat(SelectedItemText, Options.ParameterFormat.Organise))
      {
        MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "tagAndRename_InvalidParm", LocalizeDictionary.Instance.Culture).ToString(),
          LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Error_Title", LocalizeDictionary.Instance.Culture).ToString(), MessageBoxButton.OK);
        return;
      }

      var parameter = SelectedItemText;

      // Add Columns to Preview Grid
      var column = new GridTextColumn();
      column.HeaderText = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "songHeader_FileName", LocalizeDictionary.Instance.Culture).ToString();
      column.MappingName = "FullFileName";
      DataGridColumns.Add(column);

      column = new GridTextColumn();
      column.HeaderText = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "songHeader_NewFileName", LocalizeDictionary.Instance.Culture).ToString();
      column.MappingName = "FileName";
      DataGridColumns.Add(column);

      foreach (var song in _songs)
      {
        var songPreview = song.Clone();
        songPreview.Changed = false;
        songPreview.FileName = Util.ReplaceParametersWithTrackValues(parameter, songPreview);
        SongsPreview.Add(songPreview);
      }

      log.Trace("<<<");
    }

    /// <summary>
    /// Adds the current Format to the list
    /// </summary>
    public ICommand AddFormatCommand { get; set; }

    private void AddFormat(object parm)
    {
      if (!_options.OrganiseSettings.FormatValues.Contains(SelectedItemText))
      {
        _options.OrganiseSettings.FormatValues.Add(SelectedItemText);
        _options.OrganiseSettings.Save();

        _options.OrganiseSettingsTemp.Add(SelectedItemText);
        Parameters.Add(SelectedItemText);
      }
    }

    /// <summary>
    /// Removes the selected Item from the List
    /// </summary>
    public ICommand RemoveFormatCommand { get; set; }

    private void RemoveFormat(object parm)
    {
      _options.OrganiseSettings.FormatValues.Remove(SelectedItemText);
      _options.OrganiseSettings.Save();
      _options.OrganiseSettingsTemp.Remove(SelectedItemText);
      Parameters.Remove(SelectedItemText);
    }

    /// <summary>
    /// Handle te Browse for the Target Folder
    /// </summary>
    public ICommand BrowseFolderCommand { get; set; }

    private void BrowseFolder(object param)
    {
      var oFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      oFolderBrowserDialog.ShowNewFolderButton = true;
      if (oFolderBrowserDialog.ShowDialog() == DialogResult.OK)
      {
        _options.OrganiseSettings.LastUsedFolders.Insert(0, oFolderBrowserDialog.SelectedPath);
        TargetPath.Insert(0, oFolderBrowserDialog.SelectedPath);
        SelectedIndexTargetPath = 0;
      }
    }

    #endregion

    #region Private Methods

    private void LoadParameters()
    {
      log.Trace(">>>");

      CkCopyFiles = _options.OrganiseSettings.CopyFiles;
      CkOverWriteFiles = _options.OrganiseSettings.OverWriteFiles;
      CkCopyNonMusicFiles = _options.OrganiseSettings.CopyNonMusicFiles;

      // Get Last Used Folders
      foreach (var folder in _options.OrganiseSettings.LastUsedFolders)
      {
        TargetPath.Add(folder);
      }

      if (_options.OrganiseSettings.LastUsedFolders.Count == 0)
      {
        var musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        _options.OrganiseSettings.LastUsedFolders.Insert(0, musicFolder);
        TargetPath.Add(musicFolder);
        SelectedIndexTargetPath = 0;
      }
      else
      {
        SelectedIndexTargetPath = _options.OrganiseSettings.LastUsedFolderIndex == -1 ? 0 : _options.OrganiseSettings.LastUsedFolderIndex;
      }

      // Get Format Settings
      foreach (var parameter in _options.OrganiseSettings.FormatValues)
      {
        Parameters.Add(parameter);
      }

      SelectedIndex = _options.OrganiseSettings.LastUsedFormat > Parameters.Count - 1 ? 0 : _options.OrganiseSettings.LastUsedFormat;

      // Get Scripts
      ArrayList scripts = (ServiceLocator.Current.GetInstance(typeof(IScriptManager)) as IScriptManager)?.GetOrganiseScripts();

      var i = 1;
      SelectedIndexScripts = 0;
      Scripts.Add(new Item("", "", "")); // Add an empty value to allow de-selection
      if (scripts != null)
        foreach (string[] script in scripts)
        {
          Scripts.Add(new Item(script[0], script[1], script[2]));
          if (script[0] == _options.OrganiseSettings.LastUsedScript)
          {
            SelectedIndexScripts = i;
          }

          i++;
        }

      log.Trace("<<<");
    }

    #endregion

    #region Overrides

    public override void OnDialogOpened(IDialogParameters parameters)
    {
      _instance = parameters.GetValue<object>("songgridinstance");
      _songs = parameters.GetValue<List<SongData>>("songs");
      LoadParameters();
    }

    #endregion

    #region Event Handling

    public override void OnMessageReceived(GenericEvent msg)
    {
      switch (msg.Action.ToLower())
      {
        case "applychangesrequested":
          OrganiseFilesApply("");
          break;
      }
      base.OnMessageReceived(msg);
    }

    #endregion

  }
}
