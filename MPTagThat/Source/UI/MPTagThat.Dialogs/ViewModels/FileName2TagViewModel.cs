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
// ReSharper disable StringLiteralTypo

#endregion

namespace MPTagThat.Dialogs.ViewModels
{
  public class FileName2TagViewModel : DialogViewModelBase
  {
    #region Variables

    private readonly Options _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;
    private readonly NLogLogger log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger)?.GetLogger;
    private List<SongData> _songs;

    #endregion

    #region Properties

    public Brush Background => (Brush)new BrushConverter().ConvertFromString(_options.MainSettings.BackGround);

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

    #endregion

    #region ctor

    public FileName2TagViewModel()
    {
      Title = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "tagAndRename_Header",
        LocalizeDictionary.Instance.Culture).ToString();
      CancelChangesCommand = new BaseCommand(CancelChanges);
      LabelClickedCommand = new BaseCommand(LabelClicked);
      FileNameToTagCommand = new BaseCommand(FileNameToTag);
      PreviewChangesCommand = new BaseCommand(PreviewChanges);
      AddFormatCommand = new BaseCommand(AddFormat);
      RemoveFormatCommand = new BaseCommand(RemoveFormat);
      SelectionChangingCommand = new BaseCommand(SelectionChanging);
    }

    #endregion

    #region Commands

    public ICommand CancelChangesCommand { get; }

    private void CancelChanges(object parameters)
    {
      CloseDialog("false"); 
    }

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

    public ICommand FileNameToTagCommand { get; }

    private void FileNameToTag(object param)
    {
      log.Trace(">>>");

      if (!Util.CheckParameterFormat(SelectedItemText, Options.ParameterFormat.FileNameToTag))
      {
        MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "tagAndRename_InvalidParm",LocalizeDictionary.Instance.Culture).ToString(),
          LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Error_Title",LocalizeDictionary.Instance.Culture).ToString(), MessageBoxButton.OK);
        return;
      }

      var tagFormat = new TagFormat(SelectedItemText);
      var parts = tagFormat.ParameterParts;


      // Use a For loop instead foreach because we want to modify the song
      for (var i = 0; i < _songs.Count; i++)
      {
        try
        {
          ReplaceParametersWithValues(_songs[i], parts);
          _songs[i].Changed = true;
        }
        catch (Exception ex)
        {
          log.Error($"Error applying changes from Filename To Tag: {ex.Message} stack: {ex.StackTrace}");
          _songs[i].Status = 2;
          _songs[i].StatusMsg = ex.Message;
        }
      }

      // Did we get a new Format in the list, then store it temporarily
      bool newFormat = true;
      foreach (string format in _options.FileNameToTagSettingsTemp)
      {
        if (format == SelectedItemText)
        {
          newFormat = false;
          break;
        }
      }

      if (newFormat)
      {
        _options.FileNameToTagSettingsTemp.Add(SelectedItemText);
      }
      _options.FileNameToTagSettings.LastUsedFormat = SelectedIndex;

      log.Trace("<<<");
      CloseDialog("true");
    }

    private void ReplaceParametersWithValues(SongData song, List<ParameterPart> parameters)
    {
      var splittedFileValues = new List<string>();

      // Split up the file name
      // We use already the FileName from the Track instance, which might be already modified by the user.
      var file = $@"{Path.GetDirectoryName(song.FullFileName)}\{Path.GetFileNameWithoutExtension(song.FileName)}";

      var fileArray = file.Split(new[] { '\\' });

      // Now set Upper Bound depending on the length of parameters and file
      int upperBound;
      if (parameters.Count >= fileArray.Length)
        upperBound = fileArray.Length - 1;
      else
        upperBound = parameters.Count - 1;

      // Now loop through the delimiters and assign files
      for (var i = 0; i <= upperBound; i++)
      {
        var parameterpart = parameters[i];
        var delims = parameterpart.Delimiters;
        var parms = parameterpart.Parameters;

        // Set the part of the File to Process
        var filePart = fileArray[fileArray.GetUpperBound(0) - i];
        splittedFileValues.Clear();

        var upperBoundDelims = delims.GetUpperBound(0);
        for (var j = 0; j <= upperBoundDelims; j++)
        {
          if ((j == upperBoundDelims) | (delims[j] != ""))
          {
            if (filePart.IndexOf(delims[j], StringComparison.Ordinal) == 0 && j == upperBoundDelims)
            {
              splittedFileValues.Add(filePart);
              break;
            }

            var delimIndex = filePart.IndexOf(delims[j], StringComparison.Ordinal);
            if (delimIndex > -1)
            {
              splittedFileValues.Add(filePart.Substring(0, filePart.IndexOf(delims[j], StringComparison.Ordinal)));
              filePart = filePart.Substring(filePart.IndexOf(delims[j], StringComparison.Ordinal) + delims[j].Length);
            }
          }
        }

        int index = -1;
        // Now we need to Update the Tag Values
        foreach (string param in parms)
        {
          index++;
          switch (param.ToLower())
          {
            case "%artist%":
              song.Artist = splittedFileValues[index];
              break;

            case "%title%":
              song.Title = splittedFileValues[index];
              break;

            case "%album%":
              song.Album = splittedFileValues[index];
              break;

            case "%year%":
              song.Year = Convert.ToInt32(splittedFileValues[index]);
              break;

            case "%track%":
              song.TrackNumber = Convert.ToUInt32(splittedFileValues[index]);
              break;

            case "%tracktotal%":
              song.TrackCount = Convert.ToUInt32(splittedFileValues[index]);
              break;

            case "%disc%":
              song.DiscNumber = Convert.ToUInt32(splittedFileValues[index]);
              break;

            case "%disctotal%":
              song.DiscCount = Convert.ToUInt32(splittedFileValues[index]);
              break;

            case "%genre%":
              song.Genre = splittedFileValues[index];
              break;

            case "%albumartist%":
              song.AlbumArtist = splittedFileValues[index];
              break;

            case "%Comment%":
              song.Comment = splittedFileValues[index];
              break;

            case "%conductor%":
              song.Conductor = splittedFileValues[index];
              break;

            case "%composer%":
              song.Composer = splittedFileValues[index];
              break;

            case "%group%":
              song.Grouping = splittedFileValues[index];
              break;

            case "%subtitle%":
              song.SubTitle = splittedFileValues[index];
              break;

            case "%remixed%":
              song.Interpreter = splittedFileValues[index];
              break;

            case "%bpm%":
              song.BPM = Convert.ToInt32(splittedFileValues[index]);
              break;

            case "%x%":
              // ignore it
              break;
          }
        }
      }
    }

    public ICommand SelectionChangingCommand { get; set; }

    private void SelectionChanging(object param)
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

      if (!Util.CheckParameterFormat(SelectedItemText, Options.ParameterFormat.FileNameToTag))
      {
        MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "tagAndRename_InvalidParm", LocalizeDictionary.Instance.Culture).ToString(),
          LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Error_Title", LocalizeDictionary.Instance.Culture).ToString(), MessageBoxButton.OK);
        return;
      }

      var tagFormat = new TagFormat(SelectedItemText);
      var parts = tagFormat.ParameterParts;

      // Add Columns to Preview Grid
      var column = new GridTextColumn();
      column.HeaderText = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "songHeader_FileName", LocalizeDictionary.Instance.Culture).ToString();
      column.MappingName = "FullFileName";
      DataGridColumns.Add(column);

      foreach (var part in parts)
      {
        foreach (var parameter in part.Parameters)
        {
          column = new GridTextColumn();
          column.HeaderText = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", $"songHeader_{Util.ParameterToLabel(parameter)}",
            LocalizeDictionary.Instance.Culture).ToString();
          column.MappingName = Util.ParameterToLabel(parameter);
          DataGridColumns.Add(column);
        }
      }

      foreach (var song in _songs)
      {
        var songPreview = song.Clone();
        songPreview.Changed = false;
        ReplaceParametersWithValues(songPreview, parts);
        SongsPreview.Add(songPreview);
      }

      log.Trace("<<<");
    }

    /// <summary>
    /// Adds the current Format to the list
    /// </summary>
    public ICommand AddFormatCommand { get; set; }

    private void AddFormat(object param)
    {
      if (!_options.FileNameToTagSettings.FormatValues.Contains(SelectedItemText))
      {
        _options.FileNameToTagSettings.FormatValues.Add(SelectedItemText);
        _options.FileNameToTagSettings.Save();

        _options.FileNameToTagSettingsTemp.Add(SelectedItemText);
        Parameters.Add(SelectedItemText);
      }
    }

    /// <summary>
    /// Removes the selected Item from the List
    /// </summary>
    public ICommand RemoveFormatCommand { get; set; }

    private void RemoveFormat(object param)
    {
      _options.FileNameToTagSettings.FormatValues.Remove(SelectedItemText);
      _options.FileNameToTagSettings.Save();
      _options.FileNameToTagSettingsTemp.Remove(SelectedItemText);
      Parameters.Remove(SelectedItemText);
    }

    #endregion

    #region Private Methods

    private void LoadParameters()
    {
      log.Trace(">>>");
      foreach (string item in _options.FileNameToTagSettingsTemp)
      {
        Parameters.Add(item);
      }

      if (_options.FileNameToTagSettings.LastUsedFormat > Parameters.Count - 1)
        SelectedIndex = 0;
      else
        SelectedIndex = _options.FileNameToTagSettings.LastUsedFormat;
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
          FileNameToTag("");
          break;
      }
      base.OnMessageReceived(msg);
    }

    #endregion
  }
}
