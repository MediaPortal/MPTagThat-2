#region Copyright (C) 2022 Team MediaPortal
// Copyright (C) 2022 Team MediaPortal
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

using System.Windows.Forms;
using MPTagThat.Core;
using MPTagThat.Core.Events;
using Prism.Events;
using WPFLocalizeExtension.Engine;

#endregion

namespace MPTagThat.Dialogs.ViewModels
{
  public class ProgressViewModel : DialogViewModelBase
  {

    #region Properties

    private int _progress;
    public int Progress
    {
      get => _progress;
      set => SetProperty(ref _progress, value);
    }

    #endregion

    #region ctor

    public ProgressViewModel()
    {
      Title = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "settings_Tags_DownloadMusicBrainzDatabase_Downloading",
        LocalizeDictionary.Instance.Culture).ToString();

      EventSystem.Subscribe<GenericEvent>(ProgressReceived, ThreadOption.UIThread);
    }

    #endregion

    #region Event Handling

    public virtual void ProgressReceived(GenericEvent msg)
    {
      switch (msg.Action.ToLower())
      {
        case "progressnotification":
          Progress = (int)msg.MessageData["progress"];
          break;
      }
    }

    #endregion

  }
}
