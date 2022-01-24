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

using MPTagThat.Core.AlbumSearch;
using Prism.Services.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MPTagThat.Core.AlbumCoverSearch;
using MPTagThat.Core.Annotations;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Utils;
using MPTagThat.Dialogs.Models;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Shared;
using TagLib;
using WPFLocalizeExtension.Engine;
using Picture = MPTagThat.Core.Common.Song.Picture;

#endregion
namespace MPTagThat.Dialogs.ViewModels
{
  /// <summary>
  /// View Model handling the Album Search 
  /// </summary>
  public class TagFromInternetViewModel : DialogViewModelBase, IAlbumSearch
  {
    #region Variables

    #region Delegates

    private delegate void DelegateAlbumFound(List<Album> albums, String site);
    private delegate void DelegateSearchFinished();

    #endregion

    private readonly DelegateAlbumFound _albumFound;
    private readonly DelegateSearchFinished _searchFinished;
    private object _lock = new object();
    private object _lockSites = new object();

    private List<SongData> _songs;
    private Album _selectedAlbum;

    private string _statusMsgTmp;
    private int _nrOfSitesSearched;

    #endregion

    #region Properties

    /// <summary>
    /// Reference to the matched song grid for drag and Drop support
    /// </summary>
    public SfDataGrid MatchedSongsGrid;

    /// <summary>
    /// Matched Songs with Selected Site
    /// </summary>
    private ObservableCollection<SongMatch> _matchedSongs = new ObservableCollection<SongMatch>();

    public ObservableCollection<SongMatch> MatchedSongs
    {
      get => _matchedSongs;
      set => SetProperty(ref _matchedSongs, value);
    }

    /// <summary>
    /// Binding for the Sites with Albums found
    /// </summary>
    private ObservableCollection<string> _sitesWithAlbums = new ObservableCollection<string>();
    public ObservableCollection<string> SitesWithAlbums
    {
      get => _sitesWithAlbums;
      set => SetProperty(ref _sitesWithAlbums, value);
    }

    /// <summary>
    /// The Selected Result Album Search sites
    /// </summary>
    private int _selectedAlbumSite;
    public int SelectedAlbumSite
    {
      get => _selectedAlbumSite;
      set => SetProperty(ref _selectedAlbumSite, value);
    }

    /// <summary>
    /// Binding for the Selected Album
    /// </summary>
    private ObservableCollection<AlbumSong> _selectedAlbumSongs = new ObservableCollection<AlbumSong>();
    public ObservableCollection<AlbumSong> SelectedAlbumSongs
    {
      get => _selectedAlbumSongs;
      set => SetProperty(ref _selectedAlbumSongs, value);
    }

    /// <summary>
    /// Binding for the Albums found
    /// </summary>
    private List<Album> Albums = new List<Album>();

    /// <summary>
    /// The Binding for the Album Search Sites
    /// </summary>
    private ObservableCollection<string> _albumSearchSites = new ObservableCollection<string>();
    public ObservableCollection<string> AlbumSearchSites
    {
      get => _albumSearchSites;
      set => SetProperty(ref _albumSearchSites, value);
    }

    /// <summary>
    /// The Selected Album Search sites
    /// </summary>
    private ObservableCollection<string> _selectedAlbumSearchSites = new ObservableCollection<string>();
    public ObservableCollection<string> SelectedAlbumSearchSites
    {
      get => _selectedAlbumSearchSites;
      set => SetProperty(ref _selectedAlbumSearchSites, value);
    }

    /// <summary>
    /// The binding for the selected Album
    /// </summary>
    private Album _selectedItem;

    public Album SelectedItem
    {
      get => _selectedItem;
      set => SetProperty(ref _selectedItem, value);
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
    /// Binding for Year Text Field
    /// </summary>
    private string _year;

    public string Year
    {
      get => _year;
      set => SetProperty(ref _year, value);
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

    /// <summary>
    /// Binding for Wait Cursor
    /// </summary>
    private bool _isBusy;
    public bool IsBusy
    {
      get => _isBusy;
      set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// The Binding for the Status Message
    /// </summary>
    private string _statusMsg;
    public string StatusMsg
    {
      get => _statusMsg;
      set => SetProperty(ref _statusMsg, value);
    }

    #endregion

    #region ctor

    public TagFromInternetViewModel()
    {
      Title = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "lookup_Title",
        LocalizeDictionary.Instance.Culture).ToString();
      _albumFound = AlbumFoundMethod;
      _searchFinished = SearchFinishedMethod;
      BindingOperations.EnableCollectionSynchronization(Albums, _lock);
      BindingOperations.EnableCollectionSynchronization(SitesWithAlbums, _lockSites);

      SearchCommand = new BaseCommand(SearchAlbums);
      ApplyTagsCommand = new BaseCommand(ApplyTags);
      SelectedAlbumSiteChangedCommand = new BaseCommand(AlbumSiteSelected);
      SelectedSongsDoubleClickCommand = new BaseCommand(SelectedSongsDoubleClick);
    }

    #endregion

    #region Commands

    /// <summary>
    /// The Apply Button has been pressed
    /// </summary>
    public ICommand ApplyTagsCommand { get; }

    private void ApplyTags(object param)
    {
      // Get the Album Cover
      Picture pic = new Picture();
      ByteVector vector = _selectedAlbum.ImageData;
      if (vector != null)
      {
        pic.MimeType = "image/jpg";
        pic.Description = "";
        pic.Type = PictureType.FrontCover;
        pic.Data = vector.Data;
      }

      foreach (var songMatch in MatchedSongs)
      {
        if (songMatch == null)
        {
          continue;
        }

        var song = _songs[songMatch.PositionInSongList];
        song.Artist = Artist;
        song.Album = Album;
        var strYear = Year;
        if (strYear != null && strYear.Length > 4)
        {
          strYear = strYear.Substring(0, 4);
        }

        try
        {
          var year = Convert.ToInt32(strYear);
          song.Year = year;
        }
        catch (Exception) { }

        song.TrackNumber =songMatch.TrackNumber;
        song.Title = songMatch.Title;
        song.Pictures.Add(pic);
      }
      CloseDialog("true");
    }

    /// <summary>
    /// Search Album with the Artist and Album Name from the Dialog Text fields
    /// </summary>
    public ICommand SearchCommand { get; }

    private void SearchAlbums(object param)
    {
      Albums.Clear();
      DoSearchAlbum();
    }

    /// <summary>
    /// A Album Site has been selected
    /// </summary>
    public ICommand SelectedAlbumSiteChangedCommand { get; }

    private void AlbumSiteSelected(object param)
    {
      if (SelectedAlbumSite == -1)
      {
        return;
      }

      MatchedSongs.Clear();
      SelectedAlbumSongs.Clear();

      _selectedAlbum = Albums[SelectedAlbumSite];
      Artist = _selectedAlbum.Artist;
      Album = _selectedAlbum.Title;
      Year = _selectedAlbum.Year;

      foreach (var discs in _selectedAlbum.Discs)
      {
        SelectedAlbumSongs.AddRange(discs);
      }

      var songsNotFound = new List<SongMatch>();
      var list = new SongMatch[_songs.Count];
      var songPos = 0;
      foreach (var song in _songs)
      {
        var albumSongsPos = 0;
        var songFound = false;
        foreach (var albumSong in SelectedAlbumSongs)
        {
          var strCompare = song.FileName;
          if (!string.IsNullOrWhiteSpace(song.Title))
          {
            strCompare = song.Title;
          }

          if (Util.LongestCommonSubstring(albumSong.Title, strCompare) > 0.75)
          {
            if (albumSongsPos > list.Length - 1)
            {
              break;
            }

            var match = new SongMatch() {FileName = song.FileName, Title = albumSong.Title, PositionInSongList = songPos, TrackNumber = (uint)albumSong.Number, Matched = true};
            list[albumSongsPos] = match;
            songFound = true;
            break;
          }
          albumSongsPos++;
        }

        if (!songFound)
        {
          var match = new SongMatch() {FileName = song.FileName, Title = song.Title, PositionInSongList = songPos, TrackNumber = song.TrackNumber, Matched = false};
          songsNotFound.Add(match);
        }

        songPos++;
      }

      var i = 0;
      foreach (var s in list)
      {
        if (s == null && i < songsNotFound.Count)
        {
          _matchedSongs.Add(songsNotFound[i]);
          i++;
        }
        else
        {
          _matchedSongs.Add(s);
        }
      }
    }

    /// <summary>
    /// When Double Clicking on a Album Song, update the SOng with the same index
    /// </summary>
    public ICommand SelectedSongsDoubleClickCommand { get; }
    private void SelectedSongsDoubleClick(object param)
    {
      var albumSong = param as AlbumSong;
      if (albumSong.Number < _matchedSongs.Count)
      {
        var song = MatchedSongs[albumSong.Number - 1];
        song.TrackNumber = (uint)albumSong.Number;
        song.Title = albumSong.Title;
        song.Matched = true;
      }
    }

    #endregion

    #region Private Methods

    private void DoSearchAlbum()
    {
      SitesWithAlbums.Clear();
      if (SelectedAlbumSearchSites.Count == 0)
      {
        MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "coverSearch_NoSites_Selected", LocalizeDictionary.Instance.Culture).ToString(),
          LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Error_Title", LocalizeDictionary.Instance.Culture).ToString(), MessageBoxButton.OK);
        return;
      }

      _statusMsgTmp = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "lookup_SearchStatus",
        LocalizeDictionary.Instance.Culture).ToString();

      StatusMsg = string.Format(_statusMsgTmp, SelectedAlbumSearchSites.Count, SelectedAlbumSearchSites.Count - _nrOfSitesSearched);

      IsBusy = true;
      IsSearchButtonEnabled = false;
      var albumSearch = new AlbumSearch(this, Artist, Album) { AlbumSites = SelectedAlbumSearchSites.ToList() };
      albumSearch.Run();
      _options.MainSettings.SelectedAlbumInfoSites = SelectedAlbumSearchSites.ToList();
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

    [NotNull]
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
      _nrOfSitesSearched++;
      StatusMsg = string.Format(_statusMsgTmp, _options.MainSettings.AlbumInfoSites.Count, _options.MainSettings.AlbumInfoSites.Count - _nrOfSitesSearched);

      // Only add Albums that have really Songs in the list
      foreach (var album in albums)
      {
        var songCount = 0;
        foreach (var songs in album.Discs)
        {
          songCount += songs.Count;
        }
        if (songCount > 0)
        {
          Albums.Add(album);
        }
      }
    }

    private void SearchFinishedMethod()
    {
      IsBusy = false;
      IsSearchButtonEnabled = true;
      StatusMsg = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "lookup_SearchFinished",
        LocalizeDictionary.Instance.Culture).ToString();

      foreach (var album in Albums)
      {
        SitesWithAlbums.Add(album.Site);
      }

      if (SitesWithAlbums.Count > 0)
      {
        SelectedAlbumSite = 0;
      }
    }

    /// <summary>
    /// A Row has been dropped in the Matched Songs Grid
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnRowDropped(object sender, GridRowDroppedEventArgs e)
    {
      if (!e.IsFromOutSideSource && e.DropPosition != DropPosition.None)
      {
        // The Drag & Drop operation was inside the Matched Songs

        // Get Dragging records
        ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;

        // Gets the TargetRecord from the underlying collection using record index of the TargetRecord (e.TargetRecord)
        var targetRecord = MatchedSongs[(int)e.TargetRecord];

        // Use Batch update to avoid data operations in SfDataGrid during records removing and inserting
        MatchedSongsGrid.BeginInit();

        // Removes the dragging records from the underlying collection
        foreach (var item in draggingRecords)
        {
          MatchedSongs.Remove(item as SongMatch);
        }

        // Find the target record index after removing the records
        int targetIndex = MatchedSongs.IndexOf(targetRecord);
        int insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex : targetIndex + 1;
        insertionIndex = insertionIndex < 0 ? 0 : insertionIndex;

        // Insert dragging records to the target position
        for (int i = draggingRecords.Count - 1; i >= 0; i--)
        {
          MatchedSongs.Insert(insertionIndex, draggingRecords[i] as SongMatch);
        }
        MatchedSongsGrid.EndInit();
      }
      else if (e.IsFromOutSideSource)
      {
        // The Drag and Drop happened from the Songs Looked up from the Internet Source

        // Get Dragging records
        ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;

        // Gets the TargetRecord from the underlying collection using record index of the TargetRecord (e.TargetRecord)
        var targetRecord = MatchedSongs[(int)e.TargetRecord];
        targetRecord.Title = (draggingRecords[0] as AlbumSong).Title;
        targetRecord.TrackNumber = (uint)(draggingRecords[0] as AlbumSong).Number;
        targetRecord.Matched = true;
      }
    }


    private void OnDragOver(object sender, GridRowDragOverEventArgs e)
    {
      e.ShowDragUI = false;
    }


    #endregion

    #region Overrides

    public override void OnDialogOpened(IDialogParameters parameters)
    {
      MatchedSongsGrid.RowDragDropController.Dropped += OnRowDropped;
      MatchedSongsGrid.RowDragDropController.DragOver += OnDragOver;

      // Add the Album Search Sites to the Combobox
      AlbumSearchSites.AddRange(_options.MainSettings.AlbumInfoSites);
      SelectedAlbumSearchSites.AddRange(_options.MainSettings.SelectedAlbumInfoSites);

      _songs = parameters.GetValue<List<SongData>>("songs");
      Artist = "";
      if (_songs.GroupBy(s => s.Artist).Count() == 1)
      {
        Artist = _songs[0].Artist;
      }

      Album = "";
      if (_songs.GroupBy(s => s.Album).Count() == 1)
      {
        Album = _songs[0].Album;
      }
      DoSearchAlbum();
    }

    #endregion
  }
}
