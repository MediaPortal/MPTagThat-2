using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Input;
using System.Xml;
using CommonServiceLocator;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using Prism.Mvvm;
using Prism.Regions;
using Syncfusion.SfSkinManager;
using WPFLocalizeExtension.Engine;
using Action = MPTagThat.Core.Common.Action;

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
    public List<InputBinding> InputBindings { get; } = new List<InputBinding>();

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

      EventSystem.Subscribe<StatusBarEvent>(UpdateStatusBar);
      EventSystem.Subscribe<ProgressBarEvent>(UpdateProgressBar);
      SfSkinManager.ApplyStylesOnApplication = true;

      WindowCloseCommand = new BaseCommand(WindowClose);
      KeyPressedCommand = new BaseCommand(Keypressed);

      _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;

      LoadKeyMap();

      // Set Initial Window Size and Location
      WindowWidth = _options.MainSettings.FormSize.Width;
      WindowHeight = _options.MainSettings.FormSize.Height;
      WindowLeft = _options.MainSettings.FormLocation.X;
      WindowTop = _options.MainSettings.FormLocation.Y;
    }
    #endregion

    #region Commands

    /// <summary>
    /// Handle the Close of the Window
    /// </summary>
    public ICommand WindowCloseCommand { get; }

    private void WindowClose(object param)
    {
      _options.MainSettings.FormSize = new Size(WindowWidth, WindowHeight);
      _options.MainSettings.FormLocation = new Point(WindowLeft, WindowTop);

      log.Info("Saving Settings");
      _options.SaveAllSettings();
      log.Info("Terminating application");
    }

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

    #endregion

    #region Private Methods

    /// <summary>
    ///   Loads the keymap file and creates the mapping.
    /// </summary>
    /// <returns>True if the load was successfull, false if it failed.</returns>
    private void LoadKeyMap()
    {
      var strFilename = $@"{_options.ConfigDir}\\keymap.xml";
      if (!File.Exists(strFilename))
      {
        strFilename = $@"{AppDomain.CurrentDomain.BaseDirectory}\bin\keymap.xml";
      }
      log.Info($"Load key mapping from {strFilename}");
      try
      {
        // Load the XML file
        var doc = new XmlDocument();
        doc.Load(strFilename);
        // Check if it is a keymap
        if (doc.DocumentElement == null)
        {
          log.Error($"Exception loading keymap. No Root Element found");
          return;
        }
        var strRoot = doc.DocumentElement.Name;
        if (strRoot != "keymap")
        {
          log.Error($"Exception loading keymap. Root Element is not a Keymap");
          return;
        }

        // For each window
        XmlNodeList listWindows = doc.DocumentElement.SelectNodes("/keymap/window");
        foreach (XmlNode nodeWindow in listWindows)
        {
          var windowId = nodeWindow.SelectSingleNode("id");
          if (windowId != null && windowId.InnerText == "0")
          {          
            var actionNodes = nodeWindow.SelectNodes("action");
            // Create a list of key/actiontype mappings
            foreach (XmlNode node in actionNodes)
            {
              var nodeActionId = node.SelectSingleNode("id");
              var nodeKey = node.SelectSingleNode("key");
              var nodeRibbonKey = node.SelectSingleNode("ribbon");
              MapAction(nodeActionId, nodeKey, nodeRibbonKey);
            }
          }
        }
      }
      catch (Exception ex)
      {
        log.Error($"Exception loading keymap {strFilename} err:{ex.Message} stack:{ex.StackTrace}");
      }
    }

    /// <summary>
    ///   Map an action in a windowmap based on the id and key xml nodes.
    /// </summary>
    /// <param name = "nodeId">The id of the action</param>
    /// <param name = "nodeKey">The key corresponding to the mapping.</param>
    private void MapAction(XmlNode nodeId, XmlNode nodeKey, XmlNode nodeRibbonKey)
    {
      if (nodeId == null) return;
      var kb = new KeyBinding();
      kb.Command = KeyPressedCommand;
      kb.CommandParameter = (Action.ActionType)Int32.Parse(nodeId.InnerText);

      if (nodeRibbonKey != null)
      {
        // Define later
      }

      if (nodeKey != null)
      {
        var keys = nodeKey.InnerText.Split('-');
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
          log.Error($"Invalid buttons for action {nodeId.InnerText}");
        }
      }
    }

    #endregion

    #region Event Handling

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
