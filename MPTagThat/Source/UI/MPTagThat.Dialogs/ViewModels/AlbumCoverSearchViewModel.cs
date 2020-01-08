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
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using Prism.Events;
using WPFLocalizeExtension.Engine;

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

    private readonly Options _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;
    private readonly DelegateAlbumFound _albumFound;
    private readonly DelegateSearchFinished _searchFinished;
    private object _lock = new object();

    private List<SongData> _songs;

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

    /// <summary>
    /// Binding for Artist Text Field
    /// </summary>
    private string _artist;

    public string Artist
    {
      get => _artist;
      set => SetProperty(ref _artist, value);
    }

    /// <summary>
    /// Binding for Album Text Field
    /// </summary>
    private string _album;

    public string Album
    {
      get => _album;
      set => SetProperty(ref _album, value);
    }

    /// <summary>
    /// Binding for Search Button enabled
    /// </summary>
    private bool _isSearchButtonEnabled;
    public bool IsSearchButtonEnabled
    {
      get => _isSearchButtonEnabled;
      set => SetProperty(ref _isSearchButtonEnabled, value);
    }

    #endregion


    #region ctor

    public AlbumCoverSearchViewModel()
    {
      Title = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "coverSearch_Title",
        LocalizeDictionary.Instance.Culture).ToString();
      _albumFound = AlbumFoundMethod;
      _searchFinished = SearchFinishedMethod;
      _albums = new ObservableCollection<Album>();
      BindingOperations.EnableCollectionSynchronization(Albums, _lock);

      CoverSelectedCommand = new BaseCommand(CoverSelected);
      SearchCommand = new BaseCommand(SearchCovers);
    }

    #endregion

    #region Commands

    public ICommand CoverSelectedCommand { get; }

    /// <summary>
    /// A cover has been selected. Set the picture in the selected Songs
    /// </summary>
    /// <param name="param"></param>
    private void CoverSelected(object param)
    {
      var vector = (param as Album)?.ImageData;
      if (vector != null)
      {
        var pic = new Picture();
        pic.MimeType = "image/jpg";
        pic.Description = "";
        pic.Type = TagLib.PictureType.FrontCover;
        pic.Data = vector.Data;

        
        foreach (var song in _songs)
        {
          song.Pictures.Clear();
          song.Pictures.Add(pic);
          song.Changed = true;
        }

        CloseDialog("true");
      }
    }

    public ICommand SearchCommand { get; }

    private void SearchCovers(object param)
    {
      Albums.Clear();
      DoSearchAlbum();
    }

    #endregion

    #region Private Methods

    private void DoSearchAlbum()
    {
      IsSearchButtonEnabled = false;
      var albumSearch = new AlbumSearch(this, Artist, Album) {AlbumSites = _options.MainSettings.AlbumInfoSites};
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
      IsSearchButtonEnabled = true;
    }

    #endregion

    #region Overrides

    public override void OnDialogOpened(IDialogParameters parameters)
    {
      _songs = parameters.GetValue<List<SongData>>("songs");
      if (_songs.GroupBy(s => s.Artist).Count() == 1)
      {
        Artist = _songs[0].Artist;
      }
      if (_songs.GroupBy(s => s.Album).Count() == 1)
      {
        Album = _songs[0].Album;
      }
      DoSearchAlbum();
    }

    #endregion
  }
}
