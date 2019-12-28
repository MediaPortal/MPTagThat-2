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
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media;
using Prism.Events;

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

    private readonly Options _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;
    private readonly DelegateAlbumFound _albumFound;
    private readonly DelegateSearchFinished _searchFinished;
    private object _lock = new object();

    #endregion

    #region Properties

    public Brush Background => (Brush)new BrushConverter().ConvertFromString(_options.MainSettings.BackGround);
    
    /// <summary>
    /// Binding for the Albums found
    /// </summary>
    private ObservableCollection<Album> _albums;
    public ObservableCollection<Album> Albums
    {
      get => _albums;
      set => SetProperty(ref _albums, value);
    }


    #endregion


    #region ctor

    public AlbumCoverSearchViewModel()
    {
      Title = "Album Cover Search";
      _albumFound = new DelegateAlbumFound(AlbumFoundMethod);
      _searchFinished = new DelegateSearchFinished(SearchFinishedMethod);
      _albums = new ObservableCollection<Album>();
      BindingOperations.EnableCollectionSynchronization(Albums, _lock);
    }

    #endregion

    #region Private Methods

    private void DoSearchAlbum()
    {
      var albumSearch = new AlbumSearch(this, "Eagles", "Long Road Out Of Eden");
      //albumSearch.AlbumSites = _options.MainSettings.AlbumInfoSites;
      albumSearch.AlbumSites = new List<string>() { "MusicBrainz", "Discogs", "LastFM" };
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
      Albums.AddRange(albums);
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
