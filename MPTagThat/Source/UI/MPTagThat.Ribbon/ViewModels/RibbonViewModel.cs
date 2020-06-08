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
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Navigation;
using CommonServiceLocator;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.MusicDatabase;
using MPTagThat.Core.Services.ScriptManager;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using Newtonsoft.Json;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools;
using Syncfusion.Windows.Tools.Controls;
using Action = MPTagThat.Core.Common.Action;
using Application = System.Windows.Application;

#endregion

namespace MPTagThat.Ribbon.ViewModels
{
  public class RibbonViewModel : BindableBase
  {
    #region Variables

    private IRegionManager _regionManager;
    private readonly NLogLogger log;
    private Options _options;

    #endregion

    #region ctor

    public RibbonViewModel(IRegionManager regionManager)
    {
      _regionManager = regionManager;
      log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger)?.GetLogger;
      _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;

      ResetLayoutCommand = new BaseCommand(ResetLayout);
      DeleteLayoutCommand = new BaseCommand(DeleteLayout);
      ExecuteRibbonCommand = new BaseCommand(RibbonCommand);
      ExitCommand = new BaseCommand(Exit);
      ApplyKeyChangeCommand = new BaseCommand(ApplyKeyChange);
      AddGenreCommand = new BaseCommand(AddGenre);
      DeleteGenreCommand = new BaseCommand(DeleteGenre);
      SaveGenreCommand = new BaseCommand(SaveGenre);

      EventSystem.Subscribe<GenericEvent>(OnMessageReceived, ThreadOption.UIThread);

      Initialise();
    }

    #endregion

    #region Properties

    /// <summary>
    /// A Ribbon Tab has been selected
    /// Invoke the respective view
    /// </summary>
    private object _selectedRibbonTab;

    public object SelectedRibbonTab
    {
      get => _selectedRibbonTab;
      set
      {
        SetProperty(ref _selectedRibbonTab, value);
        var tab = (value as RibbonTab)?.Name;
        GenericEvent evt = new GenericEvent
        {
          Action = "ribbontabselected"
        };
        evt.MessageData.Add("ribbontab", tab);
        EventSystem.Publish(evt);
      }
    }

    /// <summary>
    /// The Binding for the Scripts
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
    /// A Script has been selected
    /// </summary>
    private int _scriptsSelectedIndex;

    public int ScriptsSelectedIndex
    {
      get => _scriptsSelectedIndex;
      set
      {
        _options.MainSettings.ActiveScript = Scripts[value].Name;
        SetProperty(ref _scriptsSelectedIndex, value);
      }
    }

    /// <summary>
    /// The Number On Click Button has been selected
    /// </summary>
    private bool _toggleNumberOnClick;

    public bool ToggleNumberOnClick
    {
      get => _toggleNumberOnClick;
      set => SetProperty(ref _toggleNumberOnClick, value);
    }

    /// <summary>
    /// The Auto Number button has been selected
    /// </summary>
    private int _autoNumberValue;

    public int AutoNumberValue
    {
      get => _autoNumberValue;
      set
      {
        SetProperty(ref _autoNumberValue, value);
        _options.AutoNumber = value;
      }
    }

    /// <summary>
    /// Selected Music Shares
    /// </summary>
    private ObservableCollection<string> _databaseMusicFolders = new ObservableCollection<string>();

    public ObservableCollection<string> DatabaseMusicFolders
    {
      get => _databaseMusicFolders;
      set => SetProperty(ref _databaseMusicFolders, value);
    }

    /// <summary>
    /// The Selected Music Folder
    /// </summary>
    private int _selectedMusicFolder;

    public int SelectedMusicFolder
    {
      get => _selectedMusicFolder;
      set => SetProperty(ref _selectedMusicFolder, value);
    }

    /// <summary>
    /// Should the database be cleared before scanning
    /// </summary>
    private bool _databaseClearChecked;

    public bool DatabaseClearChecked
    {
      get => _databaseClearChecked;
      set => SetProperty(ref _databaseClearChecked, value);
    }

    /// <summary>
    /// The list of the last queries
    /// </summary>
    private ObservableCollection<string> _queries = new ObservableCollection<string>();

    public ObservableCollection<string> Queries
    {
      get => _queries;
      set => SetProperty(ref _queries, value);
    }

    private string _queriesSelectedText;

    public string QueriesSelectedText
    {
      get => _queriesSelectedText;
      set => SetProperty(ref _queriesSelectedText, value);
    }

    private object _queriesSelectedItem;

    public object QueriesSelectedItem
    {
      get => _queriesSelectedItem;
      set => SetProperty(ref _queriesSelectedItem, value);
    }

    #region Settings related Properties in the Backstage

    #region General Settings

    /// <summary>
    /// Binding for available Languages
    /// </summary>
    private ObservableCollection<Item> _languages = new ObservableCollection<Item>();

    public ObservableCollection<Item> Languages
    {
      get => _languages;
      set
      {
        _languages = value;
        RaisePropertyChanged("Languages");
      }
    }

    /// <summary>
    /// Handling the selection of a language
    /// </summary>
    private Item _selectedLanguage;

    public Item SelectedLanguage
    {
      get => _selectedLanguage;
      set
      {
        SetProperty(ref _selectedLanguage, value);
        _options.MainSettings.Language = _selectedLanguage.Name;
        WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = new CultureInfo(_options.MainSettings.Language);
      }
    }

    /// <summary>
    /// The binding for the available themes
    /// </summary>
    private ObservableCollection<string> _themes = new ObservableCollection<string>();

    public ObservableCollection<string> Themes
    {
      get => _themes;
      set
      {
        _themes = value;
        RaisePropertyChanged("Themes");
      }
    }

    /// <summary>
    /// The selected Theme
    /// </summary>
    private string _selectedTheme;

    public string SelectedTheme
    {
      get => _selectedTheme;
      set
      {
        SetProperty(ref _selectedTheme, value);
        SfSkinManager.SetVisualStyle(Application.Current.MainWindow,
          (VisualStyles) Enum.Parse(typeof(VisualStyles), value));
        _options.MainSettings.Theme = value;
      }
    }

    /// <summary>
    /// Handling of color for changed rows
    /// </summary>
    private string _changedRowColor;

    public string ChangedRowColor
    {
      get => _changedRowColor;
      set
      {
        SetProperty(ref _changedRowColor, value);
        _options.MainSettings.ChangedRowColor = value;
      }
    }

    /// <summary>
    /// Handling of color for alternate rows
    /// </summary>
    private string _alternateRowColor;

    public string AlternateRowColor
    {
      get => _alternateRowColor;
      set
      {
        SetProperty(ref _alternateRowColor, value);
        _options.MainSettings.AlternateRowColor = value;
      }
    }

    /// <summary>
    /// Available Logging Levels
    /// </summary>
    private ObservableCollection<string> _debugLevel = new ObservableCollection<string>();

    public ObservableCollection<string> DebugLevel
    {
      get => _debugLevel;
      set
      {
        _debugLevel = value;
        RaisePropertyChanged("DebugLevel");
      }
    }

    /// <summary>
    /// Selected Log Levels
    /// </summary>
    private string _selectedLogLevel;

    public string SelectedLogLevel
    {
      get => _selectedLogLevel;
      set
      {
        SetProperty(ref _selectedLogLevel, value);
        log.Level = (LogLevel) Enum.Parse(typeof(LogLevel), value);
        _options.MainSettings.DebugLevel = value;
      }
    }

    #endregion

    #region Key Mapping

    private ObservableCollection<KeyDef> _keymap = new ObservableCollection<KeyDef>();

    public ObservableCollection<KeyDef> KeyMap
    {
      get => _keymap;
      set
      {
        _keymap = value;
        RaisePropertyChanged("KeyMap");
      }
    }

    private KeyDef _selectedKeyMap;
    public KeyDef SelectedKeyMap
    {
      get => _selectedKeyMap;
      set
      {
        KeyChangeGroupBoxEnabled = true;
        SetProperty(ref _selectedKeyMap, value);
        KeyMapDescription = value.Description;

        AltKey = CtrlKey = ShiftKey = false;
        var buttons = value.Key.Split('-');
        for (var i = 0; i < buttons.Length - 1; i++)
        {
          if (buttons[i] == "Alt")
            AltKey = true;
          else if (buttons[i] == "Ctrl")
            CtrlKey = true;
          else if (buttons[i] == "Shift")
            ShiftKey = true;
        }
        KeyValue =  buttons[buttons.Length - 1];
      }
    }

    private string _keyMapDescription;
    public string KeyMapDescription
    {
      get => _keyMapDescription;
      set => SetProperty(ref _keyMapDescription, value);
    }

    private bool _altKey;
    public bool AltKey
    {
      get => _altKey;
      set => SetProperty(ref _altKey, value);
    }

    private bool _ctrlKey;
    public bool CtrlKey
    {
      get => _ctrlKey;
      set => SetProperty(ref _ctrlKey, value);
    }

    private bool _shiftKey;
    public bool ShiftKey
    {
      get => _shiftKey;
      set => SetProperty(ref _shiftKey, value);
    }

    private string _keyValue;
    public string KeyValue
    {
      get => _keyValue;
      set => SetProperty(ref _keyValue, value);
    }

    private bool _keyChangeGroupBoxEnabled;
    public bool KeyChangeGroupBoxEnabled
    {
      get => _keyChangeGroupBoxEnabled;
      set => SetProperty(ref _keyChangeGroupBoxEnabled, value);
    }

    #endregion

    #region Tags General

    private bool _copyArtist;
    public bool CopyArtist
    {
      get => _copyArtist;
      set
      {
        SetProperty(ref _copyArtist, value);
        _options.MainSettings.CopyArtist = value;
      }
    }

    private bool _autoFillNumTracks;
    public bool AutoFillNumberOfTracks
    {
      get => _autoFillNumTracks;
      set
      {
        SetProperty(ref _autoFillNumTracks, value);
        _options.MainSettings.AutoFillNumberOfTracks = value;
      }
    }

    private bool _useCaseConversion;
    public bool UseCaseConversion
    {
      get => _useCaseConversion;
      set
      {
        SetProperty(ref _useCaseConversion, value);
        _options.MainSettings.UseCaseConversion = value;
      }
    }

    private bool _changeReadonlyAttribute;
    public bool ChangeReadOnlyAttribute
    {
      get => _changeReadonlyAttribute;
      set
      {
        SetProperty(ref _changeReadonlyAttribute, value);
        _options.MainSettings.ChangeReadOnlyAttribute = value;
      }
    }

    private string _preferredMusicBrainzCountries;
    public string PreferredMusicBrainzCountries
    {
      get => _preferredMusicBrainzCountries;
      set
      {
        SetProperty(ref _preferredMusicBrainzCountries, value);
        var countries = value.Split(',');
        _options.MainSettings.PreferredMusicBrainzCountries.Clear();
        _options.MainSettings.PreferredMusicBrainzCountries = countries.ToList();
      }
    }


    #endregion

    #region Tags ID3

    private ObservableCollection<string> _id3Encoding = new ObservableCollection<string>();

    public ObservableCollection<string> Id3Encoding
    {
      get => _id3Encoding;
      set
      {
        _id3Encoding = value;
        RaisePropertyChanged("Id3Encoding");
      }
    }

    private string _selectedEncoding;

    public string SelectedEncoding
    {
      get => _selectedEncoding;
      set
      {
        SetProperty(ref _selectedEncoding, value);
        _options.MainSettings.CharacterEncoding = value;
      }
    }

    private bool _id3UseV3;
    public bool ID3UseV3
    {
      get => _id3UseV3;
      set
      {
        SetProperty(ref _id3UseV3, value);
        if (value)
        {
          _options.MainSettings.ID3V2Version = 3;
        }
      }
    }

    private bool _id3UseV4;
    public bool ID3UseV4
    {
      get => _id3UseV4;
      set
      {
        SetProperty(ref _id3UseV4, value);
        if (value)
        {
          _options.MainSettings.ID3V2Version = 4;
        }
      }
    }

    private bool _id3UseApe;
    public bool ID3UseApe
    {
      get => _id3UseApe;
      set
      {
        SetProperty(ref _id3UseApe, value);
        if (value)
        {
          _options.MainSettings.ID3V2Version = 0;
        }
      }
    }

    private bool _id3UpdateV1;
    public bool ID3UpdateV1
    {
      get => _id3UpdateV1;
      set
      {
        SetProperty(ref _id3UpdateV1, value);
        if (value)
        {
          _options.MainSettings.ID3Version = 1;
          ID3RemoveV1Enabled = false;
          ID3RemoveV2Enabled = true;
          ID3RemoveV1 = false;
        }
      }
    }

    private bool _id3UpdateV2;
    public bool ID3UpdateV2
    {
      get => _id3UpdateV2;
      set
      {
        SetProperty(ref _id3UpdateV2, value);
        if (value)
        {
          _options.MainSettings.ID3Version = 2;
          ID3RemoveV1Enabled = true;
          ID3RemoveV2Enabled = false;
          ID3RemoveV2 = false;
        }
      }
    }

    private bool _id3UpdateV1V2;
    public bool ID3UpdateV1V2
    {
      get => _id3UpdateV1V2;
      set
      {
        SetProperty(ref _id3UpdateV1V2, value);
        if (value)
        {
          _options.MainSettings.ID3Version = 3;
          ID3RemoveV1Enabled = true;
          ID3RemoveV2Enabled = true;
        }
      }
    }

    private bool _id3RemoveV2;
    public bool ID3RemoveV2
    {
      get => _id3RemoveV2;
      set
      {
        SetProperty(ref _id3RemoveV2, value);
        _options.MainSettings.RemoveID3V2 = value;
      }
    }

    private bool _id3RemoveV1;
    public bool ID3RemoveV1
    {
      get => _id3RemoveV1;
      set
      {
        SetProperty(ref _id3RemoveV1, value);
        _options.MainSettings.RemoveID3V1 = value;
      }
    }

    private bool _id3RemoveV1Enabled;
    public bool ID3RemoveV1Enabled
    {
      get => _id3RemoveV1Enabled;
      set
      {
        SetProperty(ref _id3RemoveV1Enabled, value);
      }
    }

    private bool _id3RemoveV2Enabled;
    public bool ID3RemoveV2Enabled
    {
      get => _id3RemoveV2Enabled;
      set
      {
        SetProperty(ref _id3RemoveV2Enabled, value);
      }
    }

    private bool _clearUserFrames;
    public bool ClearUserFrames
    {
      get => _clearUserFrames;
      set
      {
        SetProperty(ref _clearUserFrames, value);
        _options.MainSettings.ClearUserFrames = value;
      }
    }

    private bool _validateMp3;
    public bool ValidateMp3
    {
      get => _validateMp3;
      set
      {
        SetProperty(ref _validateMp3, value);
        _options.MainSettings.MP3Validate = value;
      }
    }

    private bool _fixMp3;
    public bool FixMp3
    {
      get => _fixMp3;
      set
      {
        SetProperty(ref _fixMp3, value);
        _options.MainSettings.MP3AutoFix = value;
      }
    }

    private ObservableCollection<Item> _customGenres = new ObservableCollection<Item>();

    public ObservableCollection<Item> CustomGenres
    {
      get => _customGenres;
      set
      {
        _customGenres = value;
        RaisePropertyChanged("CustomGenres");
      }
    }

    private ObservableCollection<object> _selectedGenres = new ObservableCollection<object>();

    public ObservableCollection<object> SelectedGenres
    {
      get => _selectedGenres;
      set
      {
        SetProperty(ref _selectedGenres, value);
      }
    }


    #endregion

    #endregion

    #endregion

    #region Command Handling

    public ICommand ResetLayoutCommand { get; }
    /// <summary>
    /// Reset the Layout to the Default Layout
    /// </summary>
    /// <param name="param"></param>
    private void ResetLayout(object param)
    {
      GenericEvent evt = new GenericEvent
      {
        Action = "resetdockstate"
      };
      EventSystem.Publish(evt);
    }


    public ICommand DeleteLayoutCommand { get; }
    /// <summary>
    /// Delete the current Layout
    /// </summary>
    /// <param name="param"></param>
    private void DeleteLayout(object param)
    {
      GenericEvent evt = new GenericEvent
      {
        Action = "deletedockstate"
      };
      EventSystem.Publish(evt);
    }

    public ICommand ExitCommand { get; }

    /// <summary>
    /// Exit the Application
    /// </summary>
    /// <param name="parm"></param>
    private void Exit(object parm)
    {
      Application.Current.Shutdown();
    }

    public ICommand AddGenreCommand { get; }

    private void AddGenre(object param)
    {
      CustomGenres.Add(new Item("",""));
    }

    public ICommand DeleteGenreCommand { get; }

    private void DeleteGenre(object param)
    {
      // Can't use a foreach here, since it modifies the collection
      while(SelectedGenres.Count > 0)
      {
        SelectedGenres.Remove((string)SelectedGenres[0]);
      }
    }
    
    public ICommand SaveGenreCommand { get; }

    private void SaveGenre(object param)
    {
      _options.MainSettings.CustomGenres.Clear();
      foreach (var item in CustomGenres)
      {
        _options.MainSettings.CustomGenres.Add(item.Name);
      }
    }

    public ICommand ApplyKeyChangeCommand { get; }
    /// <summary>
    /// A Keyboard Layout has changed, reset the layout
    /// </summary>
    /// <param name="param"></param>
    private void ApplyKeyChange(object param)
    {
      var currentKeyDef = SelectedKeyMap;
      currentKeyDef.Description = KeyMapDescription;
      if (KeyValue.IsNullOrWhiteSpace())
      {
        return;
      }

      var key = KeyValue;
      if (ShiftKey)
      {
        key = "Shift-" + key;
      }

      if (AltKey)
      {
        key = "Alt-" + key;
      }

      if (CtrlKey)
      {
        key = "Ctrl-" + key;
      }
      currentKeyDef.Key = key;

      try
      {
        _options.KeyMap.KeyMap = KeyMap.ToList();
        var strFilename = $@"{_options.ConfigDir}\\keymap.json";
        var json = JsonConvert.SerializeObject(_options.KeyMap, Formatting.Indented);
        File.WriteAllText(strFilename, json, Encoding.UTF8);
      }
      catch (Exception)
      {
        log.Error("Settings: Error saving keymap file");
      }
    }

    public ICommand ExecuteRibbonCommand { get; }
    /// <summary>
    /// A Ribbon Command should be executed
    /// </summary>
    /// <param name="param"></param>
    private void RibbonCommand(object param)
    {
      if (param == null)
      {
        return;
      }

      var elementName = (string)param;
      var type = Action.ActionType.INVALID;
      var runAsync = true;
      var eventParameter = "";
      switch (elementName)
      {
        case "ButtonAbout":
          type = Action.ActionType.HELP;
          break;

        case "ButtonSave":
        case "ButtonSaveBackStage":
          type = Action.ActionType.SAVE;
          break;

        case "ButtonRefresh":
        case "ButtonRefreshBackStage":
          type = Action.ActionType.REFRESH;
          break;

        case "ButtonCaseConversion":
          type = Action.ActionType.CASECONVERSION_BATCH;
          break;

        case "ButtonCaseConversionOptions":
          type = Action.ActionType.CASECONVERSION;
          break;

        case "ButtonDeleteTags":
        case "ButtonDeleteAllTags":
          type = Action.ActionType.DELETEALLTAGS;
          break;

        case "ButtonDeleteV1Tags":
          type = Action.ActionType.DELETEV1TAGS;
          break;

        case "ButtonDeleteV2Tags":
          type = Action.ActionType.DELETEV2TAGS;
          break;

        case "ButtonExecuteScripts":
          type = Action.ActionType.SCRIPTEXECUTE;
          break;

        case "ButtonTagFromFile":
          type = Action.ActionType.FILENAME2TAG;
          break;

        case "ButtonTagFromInternet":
          type = Action.ActionType.TAGFROMINTERNET;
          break;

        case "ButtonGetCoverArt":
          type = Action.ActionType.GETCOVERART;
          break;

        case "ButtonGetLyrics":
          type = Action.ActionType.GETLYRICS;
          break;

        case "ButtonRenameFiles":
          type = Action.ActionType.TAG2FILENAME;
          break;

        case "ButtonOrganiseFiles":
          type = Action.ActionType.ORGANISE;
          break;

        case "ButtonRemoveComments":
          type = Action.ActionType.REMOVECOMMENT;
          break;

        case "ButtonBpm":
          type = Action.ActionType.BPM;
          break;

        case "ButtonGain":
          type = Action.ActionType.REPLAYGAIN;
          break;

        case "ButtonIdentifySong":
          type = Action.ActionType.IDENTIFYFILE;
          runAsync = false;
          break;

        case "ButtonValidateSong":
          type = Action.ActionType.VALIDATEMP3;
          runAsync = false;
          break;

        case "ButtonFixSong":
          type = Action.ActionType.FIXMP3;
          runAsync = false;
          break;

        case "ButtonFind":
          type = Action.ActionType.FIND;
          runAsync = false;
          break;

        case "ButtonReplace":
          type = Action.ActionType.REPLACE;
          runAsync = false;
          break;

        case "ButtonAutoNumber":
          type = Action.ActionType.AUTONUMBER;
          _options.AutoNumber = AutoNumberValue;
          runAsync = false;
          break;

        case "ButtonNumberOnClick":
          _options.NumberOnclick = ToggleNumberOnClick;
          _options.AutoNumber = AutoNumberValue;
          type = Action.ActionType.NUMBERONCLICK;
          break;

        case "ButtonConvertStart":
          type = Action.ActionType.CONVERT;
          runAsync = false;
          break;

        case "ButtonConvertCancel":
          type = Action.ActionType.CONVERTCANCEL;
          runAsync = false;
          break;

        case "ButtonAddConversion":
          type = Action.ActionType.ADDCONVERSION;
          runAsync = false;
          break;

        case "ButtonDatabaseScanStart":
          (ServiceLocator.Current.GetInstance(typeof(IMusicDatabase)) as IMusicDatabase)?.BuildDatabase(DatabaseMusicFolders[SelectedMusicFolder], DatabaseClearChecked);
          break;

        case "ButtonDatabaseScanAbort":
          (ServiceLocator.Current.GetInstance(typeof(IMusicDatabase)) as IMusicDatabase)?.AbortDatabaseScan();
          break;

        case "ButtonDatabaseDelete":
          (ServiceLocator.Current.GetInstance(typeof(IMusicDatabase)) as IMusicDatabase)?.DeleteDatabase((ServiceLocator.Current.GetInstance(typeof(IMusicDatabase)) as IMusicDatabase)?.CurrentDatabase);
          break;

        case "ButtonDatabaseQuery":
          type = Action.ActionType.DATABASEQUERY;
          eventParameter = QueriesSelectedText;

          // Maintain the last 10 used queries
          var currentQuery = QueriesSelectedText;
          Queries.Remove(currentQuery);
          Queries.Insert(0, currentQuery);
          _options.MainSettings.MusicDatabaseQueries.Clear();
          var i = 0;
          foreach (var q in Queries)
          {
            _options.MainSettings.MusicDatabaseQueries.Add(q);
            if (i++ > 10)
            {
              break;
            }
          }
          break;
      }

      if (type != Action.ActionType.INVALID)
      {
        // Send out the Event with the action
        var evt = new GenericEvent
        {
          Action = "Command"
        };
        evt.MessageData.Add("command", type);
        evt.MessageData.Add("runasync", runAsync);
        if (!eventParameter.IsNullOrWhiteSpace())
        {
          evt.MessageData.Add("parameter", eventParameter);
        }
        EventSystem.Publish(evt);
      }

      // We end up here, when a Ribbon Command is executed which doesn't require an event/action
      if (elementName == "DatabaseMusicFolderOpen")
      {
        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        {
          System.Windows.Forms.DialogResult result = dialog.ShowDialog();
          if (result == DialogResult.OK)
          {
            DatabaseMusicFolders.Insert(0, dialog.SelectedPath);
            SelectedMusicFolder = 0;
          }
        }
      }

    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initialise Values needed in the Ribbon
    /// </summary>
    private void Initialise()
    {
      log.Trace("<<<");

      // Load the available Scripts
      int i = 0;
      Scripts.Clear();
      ArrayList scripts = null;

      if (_options.MainSettings.ActiveScript == "")
      {
        _options.MainSettings.ActiveScript = "SwitchArtist.sct";
      }

      scripts = (ServiceLocator.Current.GetInstance(typeof(IScriptManager)) as IScriptManager)?.GetScripts();
      i = 0;
      if (scripts != null)
      {
        foreach (string[] item in scripts)
        {
          Scripts.Add(new Item(item[0], item[1], item[2]));
          if (item[0] == _options.MainSettings.ActiveScript)
          {
            ScriptsSelectedIndex = i;
            break;
          }
          i++;
        }
      }

      // Load the Database Queries
      foreach (var q in _options.MainSettings.MusicDatabaseQueries)
      {
        Queries.Add(q);
      }

      // Fill the Settings
      var logLevels = Enum.GetNames(typeof(LogLevel)).ToList();
      logLevels.ForEach(l => DebugLevel.Add(l));
      SelectedLogLevel = _options.MainSettings.DebugLevel;

      // Languages
      Languages.Add(new Item("de","Deutsch"));
      Languages.Add(new Item("en","English"));
      SelectedLanguage = _languages.First(item => item.Name == _options.MainSettings.Language);

      // Themes
      _themes.Add("Office365");
      _themes.Add("Office2016Colorful");
      _themes.Add("Office2016White");
      _themes.Add("Office2016DarkGray");
      SelectedTheme = _options.MainSettings.Theme;

      ChangedRowColor = _options.MainSettings.ChangedRowColor;
      AlternateRowColor = _options.MainSettings.AlternateRowColor;

      KeyMap.AddRange(_options.KeyMap.KeyMap);

      // Tags
      CopyArtist = _options.MainSettings.CopyArtist;
      AutoFillNumberOfTracks = _options.MainSettings.AutoFillNumberOfTracks;
      UseCaseConversion = _options.MainSettings.UseCaseConversion;
      ChangeReadOnlyAttribute = _options.MainSettings.ChangeReadOnlyAttribute;
      PreferredMusicBrainzCountries = string.Join(",", _options.MainSettings.PreferredMusicBrainzCountries);

      Id3Encoding.Add("Latin1");
      Id3Encoding.Add("UTF8");
      Id3Encoding.Add("UTF16");
      Id3Encoding.Add("UTF16-BE");
      Id3Encoding.Add("UTF16-LE");
      SelectedEncoding = _options.MainSettings.CharacterEncoding;

      switch (_options.MainSettings.ID3V2Version)
      {
        case 0: // APE Tags embedded in mp3
          ID3UseApe = true;
          break;

        case 3:
          ID3UseV3 = true;
          break;

        case 4:
          ID3UseV4 = true;
          break;
      }

      switch (_options.MainSettings.ID3Version)
      {
        case 1: 
          ID3UpdateV1 = true;
          break;

        case 2:
          ID3UpdateV2 = true;
          break;

        case 3:
          ID3UpdateV1V2 = true;
          break;
      }

      ID3RemoveV1 = _options.MainSettings.RemoveID3V1;
      ID3RemoveV2 = _options.MainSettings.RemoveID3V2;

      ClearUserFrames = _options.MainSettings.ClearUserFrames;

      ValidateMp3 = _options.MainSettings.MP3Validate;
      FixMp3 = _options.MainSettings.MP3AutoFix;

      foreach(var genre in _options.MainSettings.CustomGenres)
      {
        CustomGenres.Add(new Item(genre, ""));
      }

      log.Trace(">>>");
    }

    #endregion

    #region Event Handling

    private void OnMessageReceived(GenericEvent msg)
    {
      switch (msg.Action.ToLower())
      {
        case "autonumberchanged":
          AutoNumberValue = _options.AutoNumber;
          break;
      }
    }

    #endregion
  }
}
