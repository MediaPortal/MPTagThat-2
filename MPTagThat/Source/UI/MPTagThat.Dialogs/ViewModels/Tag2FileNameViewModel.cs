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
using Prism.Services.Dialogs;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Core.Utils;
using Syncfusion.UI.Xaml.Grid;
using WPFLocalizeExtension.Engine;
// ReSharper disable IdentifierTypo
// ReSharper disable StringIndexOfIsCultureSpecific.1

#endregion

namespace MPTagThat.Dialogs.ViewModels
{
  public class Tag2FileNameViewModel : DialogViewModelBase
  {
    #region Variables

    private List<SongData> _songs;

    #endregion

    #region Properties

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
    /// The Selected Text in the Combobox
    /// </summary>
    private string _selectedItemText;

    public string SelectedItemText
    {
      get => _selectedItemText;
      set => SetProperty(ref _selectedItemText, value);
    }

    /// <summary>
    /// The Binding for the Selected Index in the Combobox
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
    /// The Cursor Position, while Editing the Parameter Combobox
    /// </summary>
    private int _enumerateStartAt;
    public int EnumerateStartAt
    {
      get => _enumerateStartAt;
      set => SetProperty(ref _enumerateStartAt, value);
    }

    /// <summary>
    /// The Cursor Position, while Editing the Parameter Combobox
    /// </summary>
    private int _enumerateNumberDigits;
    public int EnumerateNumberDigits
    {
      get => _enumerateNumberDigits;
      set => SetProperty(ref _enumerateNumberDigits, value);
    }

    #endregion

    #region ctor

    public Tag2FileNameViewModel()
    {
      Title = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "tagAndRename_Header_Rename",
        LocalizeDictionary.Instance.Culture).ToString();
      LabelClickedCommand = new BaseCommand(LabelClicked);
      Tag2FileNameCommand = new BaseCommand(Tag2FileNameApply);
      PreviewChangesCommand = new BaseCommand(PreviewChanges);
      AddFormatCommand = new BaseCommand(AddFormat);
      RemoveFormatCommand = new BaseCommand(RemoveFormat);
      SelectionChangingCommand = new BaseCommand(SelectionChanging);
    }

    #endregion

    #region Commands

    /// <summary>
    /// The Apply Button has been pressed
    /// </summary>
    public ICommand Tag2FileNameCommand { get; }

    private void Tag2FileNameApply(object param)
    {
      log.Trace(">>>");

      if (!Util.CheckParameterFormat(SelectedItemText, Options.ParameterFormat.TagToFileName))
      {
        MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "tagAndRename_InvalidParm",LocalizeDictionary.Instance.Culture).ToString(),
          LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Error_Title",LocalizeDictionary.Instance.Culture).ToString(), MessageBoxButton.OK);
        return;
      }

      Tag2FileName(SelectedItemText);

      // Did we get a new Format in the list, then store it temporarily
      bool newFormat = true;
      foreach (string format in _options.TagToFileNameSettingsTemp)
      {
        if (format == SelectedItemText)
        {
          newFormat = false;
          break;
        }
      }

      if (newFormat)
      {
        _options.TagToFileNameSettingsTemp.Add(SelectedItemText);
      }
      _options.TagToFileNameSettings.LastUsedFormat = SelectedIndex;

      log.Trace("<<<");
      CloseDialog("true");
    }
    
    /// <summary>
    /// Rename the Filename either via Command Button or Batch Mode
    /// </summary>
    /// <param name="parameter"></param>
    private void Tag2FileName(string parameter)
    {
      // Use a For loop instead foreach because we want to modify the song
      for (var i = 0; i < _songs.Count; i++)
      {
        try
        {
          var fileName = ReplaceParametersWithValues(_songs[i], parameter);

          // Now check the length of the filename
          if (fileName.Length > 255)
          {
            log.Debug($"Filename too long: {fileName}");
            _songs[i].Status = 2;
            _songs[i].StatusMsg = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings",
              "tagAndRename_NameTooLong", LocalizeDictionary.Instance.Culture).ToString();
            continue; // Process next song
          }

          string ext = Path.GetExtension(_songs[i].FileName);

          // Now that we have a correct Filename and no duplicates accept the changes
          _songs[i].FileName = $"{fileName}{ext}";
        }
        catch (Exception ex)
        {
          log.Error($"Error applying changes from Filename To Tag: {ex.Message} stack: {ex.StackTrace}");
          _songs[i].Status = 2;
          _songs[i].StatusMsg = ex.Message;
        }
      }
    }

    private string ReplaceParametersWithValues(SongData song, string parameter)
    {
      string fileName = "";
      try
      {
        // FilenameToTag Special Variables
        if (parameter.IndexOf("%filename%") > -1)
          parameter = parameter.Replace("<F>", Path.GetFileNameWithoutExtension(song.FileName));

        if (parameter.IndexOf("<#>") > -1)
        {
          parameter = parameter.Replace("<#>", EnumerateStartAt.ToString().PadLeft(EnumerateStartAt, '0'));
          EnumerateStartAt++;
        }

        fileName = Util.ReplaceParametersWithTrackValues(parameter, song);
      }
      catch (Exception ex)
      {
        log.Error("Error Replacing parameters in file: {0} stack: {1}", ex.Message, ex.StackTrace);
      }
      return fileName;
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

      if (!Util.CheckParameterFormat(SelectedItemText, Options.ParameterFormat.TagToFileName))
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
        songPreview.FileName = ReplaceParametersWithValues(songPreview, parameter);
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
      if (!_options.TagToFileNameSettings.FormatValues.Contains(SelectedItemText))
      {
        _options.TagToFileNameSettings.FormatValues.Add(SelectedItemText);
        _options.TagToFileNameSettings.Save();

        _options.TagToFileNameSettingsTemp.Add(SelectedItemText);
        Parameters.Add(SelectedItemText);
      }
    }

    /// <summary>
    /// Removes the selected Item from the List
    /// </summary>
    public ICommand RemoveFormatCommand { get; set; }

    private void RemoveFormat(object parm)
    {
      _options.TagToFileNameSettings.FormatValues.Remove(SelectedItemText);
      _options.TagToFileNameSettings.Save();
      _options.TagToFileNameSettingsTemp.Remove(SelectedItemText);
      Parameters.Remove(SelectedItemText);
    }


    #endregion

    #region Private Methods

    private void LoadParameters()
    {
      log.Trace(">>>");
      foreach (string item in _options.TagToFileNameSettingsTemp)
      {
        Parameters.Add(item);
      }

      SelectedIndex = _options.TagToFileNameSettings.LastUsedFormat > Parameters.Count - 1 ? 0 : _options.TagToFileNameSettings.LastUsedFormat;

      log.Trace("<<<");
    }

    #endregion

    #region Overrides

    public override void OnDialogOpened(IDialogParameters parameters)
    {
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
          Tag2FileNameApply("");
          break;
      }
      base.OnMessageReceived(msg);
    }

    #endregion
  }
}
