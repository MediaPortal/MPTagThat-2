using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;
using CommonServiceLocator;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Settings;
using Prism.Mvvm;
using Prism.Regions;
using Syncfusion.SfSkinManager;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

namespace MPTagThat.ViewModels
{
  public class ShellViewModel : BindableBase
  {
    #region Variables
    IRegionManager _regionManager;

    private string _numberOfFiles = "0";
    private string _currentFolder = "";
    private string _currentFile = "";
    private string _filterActive = "false";
    private bool _progressBarIsIndeterminate = false;
    private bool _persistLayout = true;

    #endregion

    #region Properties

    /// <summary>
    /// The Window Height
    /// </summary>
    private int _windowHeight;
    public int WindowHeight
    {
      get => _windowHeight;
      set
      {
        if (!_windowHeight.Equals(value))
        {
          _windowHeight = value;
          RaisePropertyChanged("WindowHeight");
        }
      }
    }

    /// <summary>
    /// The Window Width
    /// </summary>
    private int _windowWidth;
    public int WindowWidth
    {
      get => _windowWidth;
      set
      {
        if (!_windowWidth.Equals(value))
        {
          _windowWidth = value;
          RaisePropertyChanged("WindowWidth");
        }
      }
    }

    /// <summary>
    /// The Window Position Left
    /// </summary>
    private int _windowLeft;
    public int WindowLeft
    {
      get => _windowLeft;
      set
      {
        if (!_windowLeft.Equals(value))
        {
          _windowLeft = value;
          RaisePropertyChanged("WindowLeft");
        }
      }
    }

    /// <summary>
    /// The Window Position Top
    /// </summary>
    private int _windowTop;
    public int WindowTop
    {
      get => _windowTop;
      set
      {
        if (!_windowTop.Equals(value))
        {
          _windowTop = value;
          RaisePropertyChanged("WindowTop");
        }
      }
    }


    /// <summary>
    /// Property to have the Progressbar show infinite
    /// </summary>
    public bool ProgressBarIsIndeterminate
    {
      get => _progressBarIsIndeterminate;
      set
      {
        if (value == _progressBarIsIndeterminate)
          return;

        _progressBarIsIndeterminate = value;
        RaisePropertyChanged();
      }
    }


    /// <summary>
    /// Property to show the number of files
    /// </summary>
    public string NumberOfFiles
    {
      get => string.Format(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "statusBar_NumberOfFiles",
        LocalizeDictionary.Instance.Culture).ToString(), _numberOfFiles);
      set
      {
        if (value == _numberOfFiles)
          return;

        _numberOfFiles = value;
        RaisePropertyChanged();
      }
    }

    /// <summary>
    /// Property to show the current Folder
    /// </summary>
    public string CurrentFolder
    {
      get => _currentFolder;
      set
      {
        if (value == _currentFolder)
          return;

        _currentFolder = value;
        RaisePropertyChanged();
      }
    }

    /// <summary>
    /// Property to show the current File
    /// </summary>
    public string CurrentFile
    {
      get => _currentFile;
      set
      {
        if (value == _currentFile)
          return;

        _currentFile = value;
        RaisePropertyChanged();
      }
    }
    
    /// <summary>
    /// Property to indicate if a TagFilter is active
    /// </summary>
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
      set
      {
        if (value == _filterActive)
          return;

        _filterActive = value;
        RaisePropertyChanged();
      }
    }

    #endregion

    #region ctor

    public ShellViewModel(IRegionManager regionManager)
    {
      _regionManager = regionManager;
      EventSystem.Subscribe<StatusBarEvent>(UpdateStatusBar);
      SfSkinManager.ApplyStylesOnApplication = true;

      _windowCloseCommand = new BaseCommand(WindowClose);

      var options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;

      // Set Initial Window Size and Location
      WindowWidth = options.MainSettings.FormSize.Width;
      WindowHeight = options.MainSettings.FormSize.Height;
      WindowLeft = options.MainSettings.FormLocation.X;
      WindowTop = options.MainSettings.FormLocation.Y;

    }
    #endregion

    #region Commands

    private ICommand _windowCloseCommand;
    public ICommand WindowCloseCommand => _windowCloseCommand;

    private void WindowClose(object param)
    {
      var options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;
      options.MainSettings.FormSize = new Size(WindowWidth, WindowHeight);
      options.MainSettings.FormLocation = new Point(WindowLeft, WindowTop);

      options.SaveAllSettings();
    }

    #endregion

    #region Event Handling

    private void UpdateStatusBar(StatusBarEvent msg)
    {
      NumberOfFiles = msg.NumberOfFiles.ToString();
      ProgressBarIsIndeterminate = msg.CurrentProgress == -1;
      CurrentFolder = msg.CurrentFolder;
      CurrentFile = msg.CurrentFile;
    }

    #endregion
  }


}
