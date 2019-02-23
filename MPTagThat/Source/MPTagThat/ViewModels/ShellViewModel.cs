using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using MPTagThat.Core;
using MPTagThat.Core.Events;
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

    #endregion

    #region Properties

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
    }
    #endregion

    #region Private Methods

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
