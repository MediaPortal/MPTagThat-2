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
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommonServiceLocator;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.ScriptManager;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using Prism.Mvvm;
using Prism.Regions;
using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools;
using Action = MPTagThat.Core.Common.Action;

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
      log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger)?.GetLogger;
      _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;

      ResetLayoutCommand = new BaseCommand(ResetLayout);
      DeleteLayoutCommand = new BaseCommand(DeleteLayout);
      ExecuteRibbonCommand = new BaseCommand(RibbonCommand);
      ExitCommand = new BaseCommand(Exit);

      Initialise();
    }

    #endregion

    #region Properties

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

    // Settings related Properties in the Backstage
    private ObservableCollection<string> _languages = new ObservableCollection<string>();

    public ObservableCollection<string> Languages
    {
      get => _languages;
      set
      {
        _languages = value;
        RaisePropertyChanged("Languages");
      }
    }

    private ObservableCollection<string> _themes = new ObservableCollection<string>();

    public ObservableCollection<string> Themes
    {
      get => _themes;
      set
      {
        _languages = value;
        RaisePropertyChanged("Themes");
      }
    }

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

    #region Command Handling

    public ICommand ResetLayoutCommand { get; }
    /// <summary>
    /// The Selected Item has Changed. 
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
    /// The Selected Item has Changed. 
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

    private void Exit(object parm)
    {
      Application.Current.Shutdown();
    }

    public ICommand ExecuteRibbonCommand { get; }

    private void RibbonCommand(object param)
    {
      if (param == null)
      {
        return;
      }

      var elementName = (string)param;
      var type = Action.ActionType.INVALID;
      var runAsync = true;
      switch (elementName)
      {
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
        EventSystem.Publish(evt);
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
          }

          i++;
        }
      }

      // Fill the Settings
      var logLevels = Enum.GetNames(typeof(LogLevel)).ToList();
      logLevels.ForEach(l => DebugLevel.Add(l));
      SelectedLogLevel = _options.MainSettings.DebugLevel;

      _themes.Add("Office365");
      _themes.Add("Office2016Colorful");
      _themes.Add("Office2016White");
      _themes.Add("Office2016DarkGray");
      SelectedTheme = _options.MainSettings.Theme;

      ChangedRowColor = _options.MainSettings.ChangedRowColor;
      AlternateRowColor = _options.MainSettings.AlternateRowColor;

      log.Trace(">>>");
    }

    #endregion
  }
}
