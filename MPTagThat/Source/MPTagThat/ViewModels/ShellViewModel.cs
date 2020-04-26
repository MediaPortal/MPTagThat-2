using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using CommonServiceLocator;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using Newtonsoft.Json;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Syncfusion.SfSkinManager;
using WPFLocalizeExtension.Engine;
using Action = MPTagThat.Core.Common.Action;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace MPTagThat.ViewModels
{
  public class ShellViewModel : BindableBase
  {
    #region Variables
    IRegionManager _regionManager;
   
    private readonly NLogLogger log;
    private Options _options;

    #endregion

    #region Properties

    /// <summary>
    /// The Binding for handling the Keypress
    /// </summary>
    public ObservableCollection<InputBinding> InputBindings { get; } = new ObservableCollection<InputBinding>();

    /// <summary>
    /// The State of the MainWindow
    /// </summary>
    private WindowState _windowState;
    public WindowState WindowState
    {
      get => _windowState;
      set => SetProperty(ref _windowState, value);
    }

    /// <summary>
    /// The Window Height
    /// </summary>
    private int _windowHeight;
    public int WindowHeight
    {
      get => _windowHeight;
      set => SetProperty(ref _windowHeight, value);
    }

    /// <summary>
    /// The Window Width
    /// </summary>
    private int _windowWidth;
    public int WindowWidth
    {
      get => _windowWidth;
      set => SetProperty(ref _windowWidth, value);
    }

    /// <summary>
    /// The Window Position Left
    /// </summary>
    private int _windowLeft;
    public int WindowLeft
    {
      get => _windowLeft;
      set => SetProperty(ref _windowLeft, value);
    }

    /// <summary>
    /// The Window Position Top
    /// </summary>
    private int _windowTop;
    public int WindowTop
    {
      get => _windowTop;
      set => SetProperty(ref _windowTop, value);
    }

    /// <summary>
    /// Property to have the Progressbar show infinite
    /// </summary>
    private bool _progressBarIsIndeterminate = false;
    public bool ProgressBarIsIndeterminate
    {
      get => _progressBarIsIndeterminate;
      set => SetProperty(ref _progressBarIsIndeterminate, value);
    }

    /// <summary>
    /// Property for the Minimum value of the ProgressBar
    /// </summary>
    private int _progressBarMinium;
    public int ProgressBarMinimum
    {
      get => _progressBarMinium;
      set => SetProperty(ref _progressBarMinium, value);
    }

    /// <summary>
    /// Property for the Maximum value of the ProgressBar
    /// </summary>
    private int _progressBarMaximum;
    public int ProgressBarMaximum
    {
      get => _progressBarMaximum;
      set => SetProperty(ref _progressBarMaximum, value);
    }

    /// <summary>
    /// Property for the Current value of the ProgressBar
    /// </summary>
    private int _progressBarValue;
    public int ProgressBarValue
    {
      get => _progressBarValue;
      set => SetProperty(ref _progressBarValue, value);
    }

    /// <summary>
    /// Property to show the number of files
    /// </summary>
    private string _numberOfFiles = "0";
    public string NumberOfFiles
    {
      get => string.Format(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "statusBar_NumberOfFiles",
        LocalizeDictionary.Instance.Culture).ToString(), _numberOfFiles);
      set => SetProperty(ref _numberOfFiles, value);
    }

    /// <summary>
    /// Property to Show the Number of Selected Files
    /// </summary>
    private string _selectedFiles = "0";
    public string NumberOfSelectedFiles
    {
      get => string.Format(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "statusBar_NumberOfSelectedFiles",
        LocalizeDictionary.Instance.Culture).ToString(), _selectedFiles);
      set => SetProperty(ref _selectedFiles, value);
    }

    /// <summary>
    /// Property to show the current Folder
    /// </summary>
    private string _currentFolder = "";
    public string CurrentFolder
    {
      get => _currentFolder;
      set => SetProperty(ref _currentFolder, value);
    }

    /// <summary>
    /// Property to show the current File
    /// </summary>
    private string _currentFile = "";
    public string CurrentFile
    {
      get => _currentFile;
      set => SetProperty(ref _currentFile, value);
    }

    /// <summary>
    /// Property to indicate if a TagFilter is active
    /// </summary>
    private string _filterActive = "false";
    public string FilterActive
    {
      get
      {
        if (_filterActive == "true")
        {
          return LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "statusBar_FilterActive",
            LocalizeDictionary.Instance.Culture).ToString();
        }

        return LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "statusBar_FilterInActive",
          LocalizeDictionary.Instance.Culture).ToString();
      }
      set => SetProperty(ref _filterActive, value);
    }

    #endregion

    #region ctor

    public ShellViewModel(IRegionManager regionManager)
    {
      _regionManager = regionManager;
      log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger).GetLogger;

      log.Trace(">>>");

      EventSystem.Subscribe<StatusBarEvent>(UpdateStatusBar);
      EventSystem.Subscribe<ProgressBarEvent>(UpdateProgressBar);
      SfSkinManager.ApplyStylesOnApplication = true;

      WindowCloseCommand = new BaseCommand(WindowClose);
      KeyPressedCommand = new BaseCommand(Keypressed);
      CancelFolderScanCommand = new BaseCommand(CancelFolderScan);

      _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;

      LoadKeyMap();

      // Set Initial Window Size and Location
      WindowWidth = _options.MainSettings.FormSize.Width;
      WindowHeight = _options.MainSettings.FormSize.Height;
      WindowLeft = _options.MainSettings.FormLocation.X;
      WindowTop = _options.MainSettings.FormLocation.Y;
      WindowState = _options.MainSettings.FormIsMaximized ? WindowState.Maximized : WindowState.Normal;
      log.Trace("<<<");
    }
    #endregion

    #region Commands

    /// <summary>
    /// Handle the Close of the Window
    /// </summary>
    public ICommand WindowCloseCommand { get; }

    private void WindowClose(object param)
    {
      log.Trace(">>>");
      _options.MainSettings.FormSize = new Size(WindowWidth, WindowHeight);
      _options.MainSettings.FormLocation = new Point(WindowLeft, WindowTop);
      _options.MainSettings.FormIsMaximized = WindowState == WindowState.Maximized ? true : false;

      log.Info("Saving Settings");
      _options.SaveAllSettings();
      log.Info("Terminating application");
      log.Trace("<<<");
    }

    /// <summary>
    /// Handle Keypress
    /// </summary>
    public ICommand KeyPressedCommand { get; }

    private void Keypressed(object param)
    {
      // Send out the Event with the action
      var evt = new GenericEvent
      {
        Action = "Command"
      };
      evt.MessageData.Add("command", (Action.ActionType)param);
      EventSystem.Publish(evt);
    }

    /// <summary>
    /// Handle the canceling of Folder Scan
    /// </summary>
    public ICommand CancelFolderScanCommand { get; }

    private void CancelFolderScan(object param)
    {
      // Send out the Event with the action
      var evt = new GenericEvent
      {
        Action = "CancelFolderScan"
      };
      EventSystem.Publish(evt);
    }

    #endregion

    #region Private Methods

    /// <summary>
    ///   Loads the keymap file and creates the mapping.
    /// </summary>
    private void LoadKeyMap()
    {
      var strFilename = $@"{_options.ConfigDir}\\keymap.json";
      if (!File.Exists(strFilename))
      {
        strFilename = $@"{AppDomain.CurrentDomain.BaseDirectory}\bin\keymap.json";
      }
      log.Info($"Load key mapping from {strFilename}");
      try
      {
        // Load and deserialize the Json File
        var json = File.ReadAllText(strFilename);
        var keyMap = JsonConvert.DeserializeObject<KeyMaps>(json);
        if (keyMap == null)
        {
          log.Error($"Exception loading keymap.");
          return;
        }

        foreach (var keyDef in keyMap.KeyMap)
        {
          MapAction(keyDef);
        }

        _options.KeyMap = keyMap;
      }
      catch (Exception ex)
      {
        log.Error($"Exception loading keymap {strFilename} err:{ex.Message} stack:{ex.StackTrace}");
      }
    }

    /// <summary>
    ///   Map an action based on the id and key.
    /// </summary>
    /// <param name = "keyDef">The key definition</param>
    private void MapAction(KeyDef keyDef)
    {
      var kb = new KeyBinding {Command = KeyPressedCommand, CommandParameter = (Action.ActionType) (keyDef.Id)};

      if (keyDef.Key != null)
      {
        var keys = keyDef.Key.Split('-');
        for (int i = 0; i < keys.Length - 1; i++)
        {
          if (keys[i] == "Alt")
            kb.Modifiers |= ModifierKeys.Alt;
          else if (keys[i] == "Ctrl")
            kb.Modifiers |= ModifierKeys.Control;
          else if (keys[i] == "Shift")
            kb.Modifiers |= ModifierKeys.Shift;
        }

        var key = keys[keys.Length - 1];

        try
        {
          if (key != "")
          {
            kb.Key = (Key)Enum.Parse(typeof(Key), key);
            InputBindings.Add(kb);
          }
        }
        catch (ArgumentException)
        {
          log.Error($"Invalid buttons for action {keyDef.Id}");
        }
      }
    }

    #endregion

    #region Event Handling

    /// <summary>
    /// Update the status bar with information from the StatusBar Event
    /// </summary>
    /// <param name="msg"></param>
    private void UpdateStatusBar(StatusBarEvent msg)
    {
      switch (msg.Type)
      {
        case StatusBarEvent.StatusTypes.CurrentFile:
          CurrentFile = msg.CurrentFile;
          break;

        case StatusBarEvent.StatusTypes.SelectedFiles:
          NumberOfSelectedFiles = msg.NumberOfSelectedFiles.ToString();
          break;

        default:
          NumberOfFiles = msg.NumberOfFiles.ToString();
          ProgressBarIsIndeterminate = msg.CurrentProgress == -1;
          CurrentFolder = msg.CurrentFolder;
          CurrentFile = msg.CurrentFile;
          break;
      }
    }

    /// <summary>
    /// Update the progress bar with information from ProgressBar Event
    /// </summary>
    /// <param name="msg"></param>
    private void UpdateProgressBar(ProgressBarEvent msg)
    {
      ProgressBarIsIndeterminate = false;
      if (msg.CurrentProgress == -1)
      {
        ProgressBarIsIndeterminate = true;
      }
      else
      {
        ProgressBarValue = msg.CurrentProgress;
      }
      ProgressBarMinimum = msg.MinValue;
      ProgressBarMaximum = msg.MaxValue;
      CurrentFile = msg.CurrentFile;
    }

    #endregion
  }
}
