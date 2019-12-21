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

using CommonServiceLocator;
using MPTagThat.Core.AlbumSearch;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;

#endregion

namespace MPTagThat.Dialogs.ViewModels
{
  /// <summary>
  /// This is the ViewModel handling AlbumCoverSearch
  /// </summary>
  public class AlbumCoverSearchViewModel : DialogViewModelBase, IAlbumSearch
  {
    #region Variables

    #region Delegates

    private delegate void DelegateAlbumFound(List<Album> albums, String site);
    private delegate void DelegateSearchFinished();

    #endregion

    private Options _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;
    private DelegateAlbumFound _albumFound;
    private DelegateSearchFinished _searchFinished;


    #endregion


    #region ctor

    public AlbumCoverSearchViewModel()
    {
      Title = "Album Cover Search";
      _albumFound = new DelegateAlbumFound(AlbumFoundMethod);
      _searchFinished = new DelegateSearchFinished(SearchFinishedMethod);
    }

    #endregion

    #region Private Methods

    private void DoSearchAlbum()
    {
      var albumSearch = new AlbumSearch(this, "Eagles", "Long Road Out Of Eden");
      //albumSearch.AlbumSites = _options.MainSettings.AlbumInfoSites;
      albumSearch.AlbumSites = new List<string>() { "MusicBrainz" };
      albumSearch.Run();
    }

    public object[] AlbumFound
    {
      set
      {
        try
        {
          _albumFound.Invoke((List<Album>)value[0], (string)value[1]);
        }
        catch (InvalidOperationException) { }
      }
    }

    public object[] SearchFinished
    {
      set
      {
        try
        {
          _searchFinished.Invoke();
        }
        catch (InvalidOperationException) { }
      }
    }


    private void AlbumFoundMethod(List<Album> albums, string siteName)
    {

    }

    private void SearchFinishedMethod()
    {

    }

    #endregion

    #region Overrides

    public override void OnDialogOpened(IDialogParameters parameters)
    {
      DoSearchAlbum();
    }

    public override void CloseDialog(string parameter)
    {
      ButtonResult result = ButtonResult.None;

      if (parameter?.ToLower() == "true")
        result = ButtonResult.OK;
      else if (parameter?.ToLower() == "false")
        result = ButtonResult.Cancel;

      CloseDialogWindow(new DialogResult(result));
    }


    #endregion
  }
}
