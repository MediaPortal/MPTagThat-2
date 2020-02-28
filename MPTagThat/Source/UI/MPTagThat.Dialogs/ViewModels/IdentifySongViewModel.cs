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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using CommonServiceLocator;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Dialogs.Models;
using Prism.Services.Dialogs;
using Syncfusion.UI.Xaml.Utility;
using WPFLocalizeExtension.Engine;

#endregion

namespace MPTagThat.Dialogs.ViewModels
{
  public class IdentifySongViewModel : DialogViewModelBase
  {
    #region Variables

    private readonly NLogLogger log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger)?.GetLogger;
    private readonly Options _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;
    private MusicBrainzRecording _selectedRecording = new MusicBrainzRecording();

    #endregion

    #region Properties

    public Brush Background => (Brush)new BrushConverter().ConvertFromString(_options.MainSettings.BackGround);

    /// <summary>
    /// Binding for the Recordings
    /// </summary>
    private ObservableCollection<MusicBrainzRecording> _recordings;
    public ObservableCollection<MusicBrainzRecording> Recordings
    {
      get => _recordings;
      set => SetProperty(ref _recordings, value);
    }

    #endregion

    #region ctor

    public IdentifySongViewModel()
    {
      Title = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "identifySong_Title",
        LocalizeDictionary.Instance.Culture).ToString();

      ApplyRecordingCommand = new BaseCommand(ApplyRecording);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Apply the Lyrics to the Song
    /// </summary>
    public ICommand ApplyRecordingCommand { get; set; }

    private void ApplyRecording(object param)
    {
      log.Trace(">>>");
      _selectedRecording = (MusicBrainzRecording) param;
      CloseDialog("true");
      log.Trace("<<<");
    }

    #endregion

    #region Overrides

    public override void OnDialogOpened(IDialogParameters parameters)
    {
      var recordings = parameters.GetValue<List<MusicBrainzRecording>>("recordings");
      Recordings = new ObservableCollection<MusicBrainzRecording>(recordings.Select(r => r));
    }


    public override void CloseDialog(string parameter)
    {
      ButtonResult result = ButtonResult.None;

      if (parameter?.ToLower() == "true")
        result = ButtonResult.OK;
      else if (parameter?.ToLower() == "false")
        result = ButtonResult.Cancel;

      var parameters = new DialogParameters();
      parameters.Add("selectedrecording", _selectedRecording);
      var dialogResult = new DialogResult(result, parameters);
      CloseDialogWindow(dialogResult);
    }

    #endregion


  }
}
