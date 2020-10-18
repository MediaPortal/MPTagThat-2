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

using MPTagThat.Core.AlbumSearch;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MPTagThat.Core;
using MPTagThat.Core.Annotations;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
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

    private readonly DelegateAlbumFound _albumFound;
    private readonly DelegateSearchFinished _searchFinished;
    private object _lock = new object();

    private List<SongData> _songs;
    private string _statusMsgTmp;
    private int _nrOfSitesSearched;
    private bool _removeExistingPictures;

    #endregion

    #region Properties

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

    private bool _createFolderThumb;

    public bool CreateFolderThumb
    {
      get => _createFolderThumb;
      set => SetProperty(ref _createFolderThumb, value);
    }

    private bool _embedFolderThumb;

    public bool EmbedFolderThumb
    {
      get => _embedFolderThumb;
      set => SetProperty(ref _embedFolderThumb, value);
    }

    private bool _overwriteExistingCovers;

    public bool OverwriteExistingCovers
    {
      get => _overwriteExistingCovers;
      set => SetProperty(ref _overwriteExistingCovers, value);
    }

    private bool _onlySaveFolderThumb;

    public bool OnlySaveFolderThumb
    {
      get => _onlySaveFolderThumb;
      set => SetProperty(ref _onlySaveFolderThumb, value);
    }

    private bool _changeCoverSize;

    public bool ChangeCoverSize
    {
      get => _changeCoverSize;
      set => SetProperty(ref _changeCoverSize, value);
    }

    private int _maxCoverWidth;

    public int MaxCoverWidth
    {
      get => _maxCoverWidth;
      set => SetProperty(ref _maxCoverWidth, value);
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
      ApplyCoverCommand = new BaseCommand(ApplyCover);
    }

    #endregion

    #region Commands

    /// <summary>
    /// The Apply Button has been pressed
    /// </summary>
    public ICommand ApplyCoverCommand { get; }

    private void ApplyCover(object param)
    {
      CoverSelected(param);
    }

    /// <summary>
    /// A Cover has been selected by double clicking on it
    /// </summary>
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

        if (_options.MainSettings.ChangeCoverSize &&
            Picture.ImageFromData(pic.Data).Width > _options.MainSettings.MaxCoverWidth)
        {
          pic.Resize(_options.MainSettings.MaxCoverWidth);
        }

        if (_options.MainSettings.OnlySaveFolderThumb)
        {
          var fileName = Path.Combine(Path.GetDirectoryName(_songs[0].FullFileName), "folder.jpg");
          try
          {
            Image img = Picture.ImageFromData(pic.Data);

            // Need to make a copy, otherwise we have a GDI+ Error
            Bitmap bmp = new Bitmap(img);
            bmp.Save(fileName, ImageFormat.Jpeg);

            var miscfileevt = new GenericEvent
            {
              Action = "miscfileschanged"
            };
            EventSystem.Publish(miscfileevt);
          }
          catch (Exception ex)
          {
            log.Error("Exception Saving picture: {0} {1}", fileName, ex.Message);
          }
        }
        else
        {
          foreach (var song in _songs)
          {
            if (_removeExistingPictures || _options.MainSettings.OverwriteExistingCovers)
            {
              song.Pictures.Clear();
            }
            song.Pictures.Add(pic);
            song.Changed = true;
          }
        }

        GenericEvent evt = new GenericEvent
        {
          Action = "coverschanged"
        };
        EventSystem.Publish(evt);

        // Save Settings
        _options.MainSettings.CreateFolderThumb = CreateFolderThumb;
        _options.MainSettings.EmbedFolderThumb = EmbedFolderThumb;
        _options.MainSettings.OverwriteExistingCovers = OverwriteExistingCovers;
        _options.MainSettings.OnlySaveFolderThumb = OnlySaveFolderThumb;
        _options.MainSettings.ChangeCoverSize = ChangeCoverSize;
        _options.MainSettings.MaxCoverWidth = MaxCoverWidth;

        CloseDialog("true");
      }
    }


    /// <summary>
    /// Search Album with the Artist and Album Name from the Dialog Text fields
    /// </summary>
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
      if (SelectedAlbumSearchSites.Count == 0)
      {
        MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "coverSearch_NoSites_Selected", LocalizeDictionary.Instance.Culture).ToString(),
          LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Error_Title", LocalizeDictionary.Instance.Culture).ToString(), MessageBoxButton.OK);
        return;
      }

      _statusMsgTmp = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "coverSearch_Status",
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

      Albums.AddRange(albums);
    }

    private void SearchFinishedMethod()
    {
      IsBusy = false;
      IsSearchButtonEnabled = true;
      StatusMsg = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "coverSearch_Finished",
        LocalizeDictionary.Instance.Culture).ToString();
    }

    #endregion

    #region Overrides

    public override void OnDialogOpened(IDialogParameters parameters)
    {
      // Get Settings
      CreateFolderThumb = _options.MainSettings.CreateFolderThumb;
      EmbedFolderThumb = _options.MainSettings.EmbedFolderThumb;
      OverwriteExistingCovers = _options.MainSettings.OverwriteExistingCovers;
      OnlySaveFolderThumb = _options.MainSettings.OnlySaveFolderThumb;
      ChangeCoverSize = _options.MainSettings.ChangeCoverSize;
      MaxCoverWidth = _options.MainSettings.MaxCoverWidth;

      // Add the Album Search Sites to the Combobox
      AlbumSearchSites.AddRange(_options.MainSettings.AlbumInfoSites);
      SelectedAlbumSearchSites.AddRange(_options.MainSettings.SelectedAlbumInfoSites);

      if (parameters.TryGetValue("removeexistingpictures", out string param))
      {
        if (param == "true")
        {
          _removeExistingPictures = true;
        }
      }

      Artist = "";
      _songs = parameters.GetValue<List<SongData>>("songs");
      if (_songs.GroupBy(s => s.AlbumArtist).Count() == 1)
      {
        Artist = _songs[0].AlbumArtist;
      }

      if (Artist.Trim() == "" && _songs.GroupBy(s => s.Artist).Count() == 1)
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

    #region Event Handling

    public override void OnMessageReceived(GenericEvent msg)
    {
      switch (msg.Action.ToLower())
      {
        case "applychangesrequested":
          ApplyCover(SelectedItem);
          break;
      }
      base.OnMessageReceived(msg);
    }

    #endregion
  }
}
