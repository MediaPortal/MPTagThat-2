#region Copyright (C) 2017 Team MediaPortal
// Copyright (C) 2017 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.MusicDatabase;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Core.Utils;
using MPTagThat.TagEdit.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Syncfusion.UI.Xaml.Grid;
using TagLib;
using Action = MPTagThat.Core.Common.Action;
using File = System.IO.File;
using Frame = MPTagThat.Core.Common.Song.Frame;
using Picture = MPTagThat.Core.Common.Song.Picture;
using TextBox = System.Windows.Controls.TextBox;

// ReSharper disable StringLiteralTypo

#endregion

namespace MPTagThat.TagEdit.ViewModels
{
  public class TagEditViewModel : BindableBase, INavigationAware, IDragDropTarget
  {
    #region Variables

    private readonly NLogLogger log = ContainerLocator.Current.Resolve<ILogger>()?.GetLogger;
    private readonly Options _options = ContainerLocator.Current.Resolve<ISettingsManager>()?.GetOptions;
    private List<SongData> _songs;
    private SongData _songBackup;
    private bool _isInitializing;

    #endregion

    #region Properties

    /// <summary>
    /// The Songdata object holding the Values edited
    /// </summary>
    private SongData _songEdit;
    public SongData SongEdit
    {
      get => _songEdit;
      set => SetProperty(ref _songEdit, value);
    }

    /// <summary>
    /// Binding for View Enablement
    /// </summary>
    private bool _isEnabled;
    public bool IsEnabled
    {
      get => _isEnabled;
      set => SetProperty(ref _isEnabled, value);
    }

    /// <summary>
    /// Binding for ApplyButton Enablement
    /// </summary>
    private bool _isApplyButtonEnabled;
    public bool IsApplyButtonEnabled
    {
      get => _isApplyButtonEnabled;
      set => SetProperty(ref _isApplyButtonEnabled, value);
    }

    /// <summary>
    /// The Binding for the Genres
    /// </summary>
    private ObservableCollection<string> _genres = new ObservableCollection<string>();
    public ObservableCollection<string> Genres
    {
      get => _genres;
      set => SetProperty(ref _genres, value);
    }

    /// <summary>
    /// The Selected Genres
    /// </summary>
    private ObservableCollection<object> _selectedGenres = new ObservableCollection<object>();
    public ObservableCollection<object> SelectedGenres
    {
      get => _selectedGenres;
      set => SetProperty(ref _selectedGenres, value);
    }

    /// <summary>
    /// Indicates if the checkboxes for Multiline edit should be shown
    /// </summary>
    private bool _multiCheckBoxVisibility;
    public bool MultiCheckBoxVisibility
    {
      get => _multiCheckBoxVisibility;
      set => SetProperty(ref _multiCheckBoxVisibility, value);
    }

    /// <summary>
    /// The Binding for the Front Cover Picture
    /// </summary>
    private BitmapImage _frontCover;
    public BitmapImage FrontCover
    {
      get => _frontCover;
      set => SetProperty(ref _frontCover, value);
    }

    /// <summary>
    /// The Binding for the Media Types
    /// </summary>
    private ObservableCollection<Item> _mediaTypes = new ObservableCollection<Item>();

    public ObservableCollection<Item> MediaTypes
    {
      get => _mediaTypes;
      set => SetProperty(ref _mediaTypes, value);
    }

    /// <summary>
    /// The Selected Text in the Media Types Combobox
    /// </summary>
    private int _selectedMediaTypeText;

    public int SelectedIndexMediaType
    {
      get => _selectedMediaTypeText;
      set
      {
        if (MultiCheckBoxVisibility)
        {
          CkMediaTypeIsChecked = true;
        }

        if (!_isInitializing)
        {
          SongEdit.MediaType = MediaTypes[value].Value;
          IsApplyButtonEnabled = true;
        }
        SetProperty(ref _selectedMediaTypeText, value);
      }
    }

    /// <summary>
    /// The Picture Types shown in the Picture Details Grid
    /// </summary>
    public List<string> PictureTypes => Enum.GetNames(typeof(PictureType)).ToList();

    /// <summary>
    /// The selected Picture in the Picture Details
    /// </summary>
    private ObservableCollection<object> _selectedPicture = new ObservableCollection<object>();
    public ObservableCollection<object> SelectedPicture
    {
      get => _selectedPicture;
      set
      {
        _selectedPicture = value;
        RaisePropertyChanged("SelectedPicture");
      }
    }

    /// <summary>
    /// The Binding for the Front Cover Picture
    /// </summary>
    private BitmapImage _pictureDetail;
    public BitmapImage PictureDetail
    {
      get => _pictureDetail;
      set => SetProperty(ref _pictureDetail, value);
    }

    /// <summary>
    /// The Selected Rating in the Ratings
    /// </summary>
    private ObservableCollection<object> _selectedRating = new ObservableCollection<object>();

    public ObservableCollection<object> SelectedRating
    {
      get => _selectedRating;
      set => SetProperty(ref _selectedRating, value);
    }

    /// <summary>
    /// A reference to the Ratings Grid to be used for Drag & Drop operation
    /// </summary>
    public SfDataGrid RatingsGrid;


    private ObservableCollection<Person> _involvedPersons = new ObservableCollection<Person>();

    public ObservableCollection<Person> InvolvedPersons
    {
      get => _involvedPersons;
      set => SetProperty(ref _involvedPersons, value);
    }

    /// <summary>
    /// The Selected Person in the Involved Person
    /// </summary>
    private ObservableCollection<object> _selectedPerson = new ObservableCollection<object>();

    public ObservableCollection<object> SelectedPerson
    {
      get => _selectedPerson;
      set => SetProperty(ref _selectedPerson, value);
    }

    private ObservableCollection<Person> _involvedMusicians = new ObservableCollection<Person>();

    public ObservableCollection<Person> InvolvedMusicians
    {
      get => _involvedMusicians;
      set => SetProperty(ref _involvedMusicians, value);
    }

    /// <summary>
    /// The Selected Person in the Involved Person
    /// </summary>
    private ObservableCollection<object> _selectedMusician = new ObservableCollection<object>();

    public ObservableCollection<object> SelectedMusician
    {
      get => _selectedMusician;
      set => SetProperty(ref _selectedMusician, value);
    }

    /// <summary>
    /// The Selected User Frame(s)
    /// </summary>
    private ObservableCollection<object> _selectedUserFrames = new ObservableCollection<object>();
    public ObservableCollection<object> SelectedUserFrames
    {
      get => _selectedUserFrames;
      set => SetProperty(ref _selectedUserFrames, value);
    }

    /// <summary>
    /// The Binding for the Artist Auto Complete
    /// </summary>
    private ObservableCollection<string> _autoCompleteArtists = new ObservableCollection<string>();
    public ObservableCollection<string> AutoCompleteArtists
    {
      get => _autoCompleteArtists;
      set => SetProperty(ref _autoCompleteArtists, value);
    }

    #region Multi Album, Artist, AlbumArtist

    /// <summary>
    /// When we have Multiple Albums show a Combo instead of Text Box
    /// </summary>
    private bool _multiAlbum;
    public bool MultiAlbum
    {
      get => _multiAlbum;
      set => SetProperty(ref _multiAlbum, value);
    }

    /// <summary>
    /// The Collection for Multiple Albums
    /// </summary>
    private ObservableCollection<string> _albums = new ObservableCollection<string>();
    public ObservableCollection<string> Albums
    {
      get => _albums;
      set => SetProperty(ref _albums, value);
    }

    /// <summary>
    /// The selected Album in the Album Combo
    /// </summary>
    private int _selectedAlbumsIndex;
    public int SelectedAlbumsIndex
    {
      get => _selectedAlbumsIndex;
      set => SetProperty(ref _selectedAlbumsIndex, value);
    }

    /// <summary>
    /// The Text of the Album in the Album Combo
    /// </summary>
    private string _albumComboText;
    public string AlbumComboText
    {
      get => _albumComboText;
      set => SetProperty(ref _albumComboText, value);
    }

    /// <summary>
    /// When we have Multiple Artists show a Combo instead of Text Box
    /// </summary>
    private bool _multiArtist;
    public bool MultiArtist
    {
      get => _multiArtist;
      set => SetProperty(ref _multiArtist, value);
    }

    /// <summary>
    /// The Collection for Multiple Artists
    /// </summary>
    private ObservableCollection<string> _artists = new ObservableCollection<string>();
    public ObservableCollection<string> Artists
    {
      get => _artists;
      set => SetProperty(ref _artists, value);
    }

    /// <summary>
    /// The selected Artist in the Artist Combo
    /// </summary>
    private int _selectedArtistsIndex;
    public int SelectedArtistsIndex
    {
      get => _selectedArtistsIndex;
      set => SetProperty(ref _selectedArtistsIndex, value);
    }

    /// <summary>
    /// The Text of the Artist in the Artist Combo
    /// </summary>
    private string _artistComboText;
    public string ArtistComboText
    {
      get => _artistComboText;
      set => SetProperty(ref _artistComboText, value);
    }

    /// <summary>
    /// When we have Multiple AlbumArtists show a Combo instead of Text Box
    /// </summary>
    private bool _multiAlbumArtist;
    public bool MultiAlbumArtist
    {
      get => _multiAlbumArtist;
      set => SetProperty(ref _multiAlbumArtist, value);
    }

    /// <summary>
    /// The Collection for Multiple AlbumArtists
    /// </summary>
    private ObservableCollection<string> _albumArtists = new ObservableCollection<string>();
    public ObservableCollection<string> AlbumArtists
    {
      get => _albumArtists;
      set => SetProperty(ref _albumArtists, value);
    }

    /// <summary>
    /// The selected AlbumArtist in the AlbumArtist Combo
    /// </summary>
    private int _selectedAlbumArtistsIndex;
    public int SelectedAlbumArtistsIndex
    {
      get => _selectedAlbumArtistsIndex;
      set => SetProperty(ref _selectedAlbumArtistsIndex, value);
    }

    /// <summary>
    /// The Text of the AlbumArtist in the AlbumArtist Combo
    /// </summary>
    private string _albumArtistComboText;
    public string AlbumArtistComboText
    {
      get => _albumArtistComboText;
      set => SetProperty(ref _albumArtistComboText, value);
    }

    #endregion

    #region Check Box Checked properties
    private bool _ckTrackIsChecked;
    public bool CkTrackIsChecked { get => _ckTrackIsChecked; set => SetProperty(ref _ckTrackIsChecked, value); }

    private bool _ckDiscIsChecked;
    public bool CkDiscIsChecked { get => _ckDiscIsChecked; set => SetProperty(ref _ckDiscIsChecked, value); }

    private bool _ckArtistIsChecked;
    public bool CkArtistIsChecked { get => _ckArtistIsChecked; set => SetProperty(ref _ckArtistIsChecked, value); }

    private bool _ckAlbumArtistIsChecked;
    public bool CkAlbumArtistIsChecked { get => _ckAlbumArtistIsChecked; set => SetProperty(ref _ckAlbumArtistIsChecked, value); }

    private bool _ckGenreIsChecked;
    public bool CkGenreIsChecked { get => _ckGenreIsChecked; set => SetProperty(ref _ckGenreIsChecked, value); }

    private bool _ckAlbumIsChecked;
    public bool CkAlbumIsChecked { get => _ckAlbumIsChecked; set => SetProperty(ref _ckAlbumIsChecked, value); }

    private bool _ckTitleIsChecked;
    public bool CkTitleIsChecked { get => _ckTitleIsChecked; set => SetProperty(ref _ckTitleIsChecked, value); }

    private bool _ckYearIsChecked;
    public bool CkYearIsChecked { get => _ckYearIsChecked; set => SetProperty(ref _ckYearIsChecked, value); }

    private bool _ckBPMIsChecked;
    public bool CkBPMIsChecked { get => _ckBPMIsChecked; set => SetProperty(ref _ckBPMIsChecked, value); }

    private bool _ckConductorIsChecked;
    public bool CkConductorIsChecked { get => _ckConductorIsChecked; set => SetProperty(ref _ckConductorIsChecked, value); }

    private bool _ckComposerIsChecked;
    public bool CkComposerIsChecked { get => _ckComposerIsChecked; set => SetProperty(ref _ckComposerIsChecked, value); }

    private bool _ckInterpretedByIsChecked;
    public bool CkInterpretedByIsChecked { get => _ckInterpretedByIsChecked; set => SetProperty(ref _ckInterpretedByIsChecked, value); }

    private bool _ckTextWriterIsChecked;
    public bool CkTextWriterIsChecked { get => _ckTextWriterIsChecked; set => SetProperty(ref _ckTextWriterIsChecked, value); }

    private bool _ckPublisherIsChecked;
    public bool CkPublisherIsChecked { get => _ckPublisherIsChecked; set => SetProperty(ref _ckPublisherIsChecked, value); }

    private bool _ckEncodedByIsChecked;
    public bool CkEncodedByIsChecked { get => _ckEncodedByIsChecked; set => SetProperty(ref _ckEncodedByIsChecked, value); }

    private bool _ckCopyrightIsChecked;
    public bool CkCopyrightIsChecked { get => _ckCopyrightIsChecked; set => SetProperty(ref _ckCopyrightIsChecked, value); }

    private bool _ckContentGroupIsChecked;
    public bool CkContentGroupIsChecked { get => _ckContentGroupIsChecked; set => SetProperty(ref _ckContentGroupIsChecked, value); }

    private bool _ckSubTitleIsChecked;
    public bool CkSubTitleIsChecked { get => _ckSubTitleIsChecked; set => SetProperty(ref _ckSubTitleIsChecked, value); }

    private bool _ckArtistSortIsChecked;
    public bool CkArtistSortIsChecked { get => _ckArtistSortIsChecked; set => SetProperty(ref _ckArtistSortIsChecked, value); }

    private bool _ckAlbumArtistSortIsChecked;
    public bool CkAlbumArtistSortIsChecked { get => _ckAlbumArtistSortIsChecked; set => SetProperty(ref _ckAlbumArtistSortIsChecked, value); }

    private bool _ckAlbumSortIsChecked;
    public bool CkAlbumSortIsChecked { get => _ckAlbumSortIsChecked; set => SetProperty(ref _ckAlbumSortIsChecked, value); }

    private bool _ckTitleSortIsChecked;
    public bool CkTitleSortIsChecked { get => _ckTitleSortIsChecked; set => SetProperty(ref _ckTitleSortIsChecked, value); }

    private bool _ckOriginalAlbumIsChecked;
    public bool CkOriginalAlbumIsChecked { get => _ckOriginalAlbumIsChecked; set => SetProperty(ref _ckOriginalAlbumIsChecked, value); }

    private bool _ckOriginalArtistIsChecked;
    public bool CkOriginalArtistIsChecked { get => _ckOriginalArtistIsChecked; set => SetProperty(ref _ckOriginalArtistIsChecked, value); }

    private bool _ckOriginalFileNameIsChecked;
    public bool CkOriginalFileNameIsChecked { get => _ckOriginalFileNameIsChecked; set => SetProperty(ref _ckOriginalFileNameIsChecked, value); }

    private bool _ckOriginalLyricsWriterIsChecked;
    public bool CkOriginalLyricsWriterIsChecked { get => _ckOriginalLyricsWriterIsChecked; set => SetProperty(ref _ckOriginalLyricsWriterIsChecked, value); }

    private bool _ckOriginalOwnerIsChecked;
    public bool CkOriginalOwnerIsChecked { get => _ckOriginalOwnerIsChecked; set => SetProperty(ref _ckOriginalOwnerIsChecked, value); }

    private bool _ckOriginalReleaseIsChecked;
    public bool CkOriginalReleaseIsChecked { get => _ckOriginalReleaseIsChecked; set => SetProperty(ref _ckOriginalReleaseIsChecked, value); }

    private bool _ckCopyrightUrlIsChecked;
    public bool CkCopyrightUrlIsChecked { get => _ckCopyrightUrlIsChecked; set => SetProperty(ref _ckCopyrightUrlIsChecked, value); }

    private bool _ckOfficialAudioFileUrlIsChecked;
    public bool CkOfficialAudioFileUrlIsChecked { get => _ckOfficialAudioFileUrlIsChecked; set => SetProperty(ref _ckOfficialAudioFileUrlIsChecked, value); }

    private bool _ckOfficialArtistUrlIsChecked;
    public bool CkOfficialArtistUrlIsChecked { get => _ckOfficialArtistUrlIsChecked; set => SetProperty(ref _ckOfficialArtistUrlIsChecked, value); }

    private bool _ckOfficialAudioSourceUrlIsChecked;
    public bool CkOfficialAudioSourceUrlIsChecked { get => _ckOfficialAudioSourceUrlIsChecked; set => SetProperty(ref _ckOfficialAudioSourceUrlIsChecked, value); }

    private bool _ckOfficialInternetRadioUrlIsChecked;
    public bool CkOfficialInternetRadioUrlIsChecked { get => _ckOfficialInternetRadioUrlIsChecked; set => SetProperty(ref _ckOfficialInternetRadioUrlIsChecked, value); }

    private bool _ckOfficialPaymentUrlIsChecked;
    public bool CkOfficialPaymentUrlIsChecked { get => _ckOfficialPaymentUrlIsChecked; set => SetProperty(ref _ckOfficialPaymentUrlIsChecked, value); }

    private bool _ckCommercialInformationUrlIsChecked;
    public bool CkCommercialInformationUrlIsChecked { get => _ckCommercialInformationUrlIsChecked; set => SetProperty(ref _ckCommercialInformationUrlIsChecked, value); }

    private bool _ckOfficialPublisherUrlIsChecked;
    public bool CkOfficialPublisherUrlIsChecked { get => _ckOfficialPublisherUrlIsChecked; set => SetProperty(ref _ckOfficialPublisherUrlIsChecked, value); }

    private bool _ckInvolvedPersonIsChecked;
    public bool CkInvolvedPersonIsChecked { get => _ckInvolvedPersonIsChecked; set => SetProperty(ref _ckInvolvedPersonIsChecked, value); }

    private bool _ckInvolvedMusicianIsChecked;
    public bool CkInvolvedMusicianIsChecked { get => _ckInvolvedMusicianIsChecked; set => SetProperty(ref _ckInvolvedMusicianIsChecked, value); }

    private bool _ckRemoveExistingRatingsIsChecked;
    public bool CkRemoveExistingRatingsIsChecked { get => _ckRemoveExistingRatingsIsChecked; set => SetProperty(ref _ckRemoveExistingRatingsIsChecked, value); }

    private bool _ckRemoveLyricsIsChecked;
    public bool CkRemoveLyricsIsChecked { get => _ckRemoveLyricsIsChecked; set => SetProperty(ref _ckRemoveLyricsIsChecked, value); }

    private bool _ckMediaTypeIsChecked;
    public bool CkMediaTypeIsChecked { get => _ckMediaTypeIsChecked; set => SetProperty(ref _ckMediaTypeIsChecked, value); }

    private bool _ckTrackLengthIsChecked;

    public bool CkTrackLengthIsChecked
    {
      get => _ckTrackLengthIsChecked;
      set
      {
        IsApplyButtonEnabled = true;
        SetProperty(ref _ckTrackLengthIsChecked, value);
      }
    }

    private bool _ckCommentIsChecked;
    public bool CkCommentIsChecked { get => _ckCommentIsChecked; set => SetProperty(ref _ckCommentIsChecked, value); }

    private bool _ckPartOfCompilationIsChecked;
    public bool CkPartOfCompilationIsChecked { get => _ckPartOfCompilationIsChecked; set => SetProperty(ref _ckPartOfCompilationIsChecked, value); }

    private bool _ckRemovePicturesIsChecked;
    public bool CkRemovePicturesIsChecked { get => _ckRemovePicturesIsChecked; set => SetProperty(ref _ckRemovePicturesIsChecked, value); }

    private bool _ckPicturesIsChecked;
    public bool CkPicturesIsChecked { get => _ckPicturesIsChecked; set => SetProperty(ref _ckPicturesIsChecked, value); }

    private bool _ckMbArtistIdIsChecked;
    public bool CkMbArtistIdIsChecked { get => _ckMbArtistIdIsChecked; set => SetProperty(ref _ckMbArtistIdIsChecked, value); }

    private bool _ckMbReleaseArtistIdIsChecked;
    public bool CkMbReleaseArtistIdIsChecked { get => _ckMbReleaseArtistIdIsChecked; set => SetProperty(ref _ckMbReleaseArtistIdIsChecked, value); }

    private bool _ckMbReleaseIdIsChecked;
    public bool CkMbReleaseIdIsChecked { get => _ckMbReleaseIdIsChecked; set => SetProperty(ref _ckMbReleaseIdIsChecked, value); }

    private bool _ckMbReleaseGroupIdIsChecked;
    public bool CkMbReleaseGroupIdIsChecked { get => _ckMbReleaseGroupIdIsChecked; set => SetProperty(ref _ckMbReleaseGroupIdIsChecked, value); }

    private bool _ckMbReleaseCountryIsChecked;
    public bool CkMbReleaseCountryIsChecked { get => _ckMbReleaseCountryIsChecked; set => SetProperty(ref _ckMbReleaseCountryIsChecked, value); }

    private bool _ckMbDiscIdIsChecked;
    public bool CkMbDiscIdIsChecked { get => _ckMbDiscIdIsChecked; set => SetProperty(ref _ckMbDiscIdIsChecked, value); }

    private bool _ckMbReleaseStatusIsChecked;
    public bool CkMbReleaseStatusIsChecked { get => _ckMbReleaseStatusIsChecked; set => SetProperty(ref _ckMbReleaseStatusIsChecked, value); }

    private bool _ckMbReleaseTypeIsChecked;
    public bool CkMbReleaseTypeIsChecked { get => _ckMbReleaseTypeIsChecked; set => SetProperty(ref _ckMbReleaseTypeIsChecked, value); }

    private bool _ckMbTrackIdIsChecked;
    public bool CkMbTrackIdIsChecked { get => _ckMbTrackIdIsChecked; set => SetProperty(ref _ckMbTrackIdIsChecked, value); }

    #endregion

    #endregion

    #region ctor

    public TagEditViewModel()
    {
      KeyDownCommand = new DelegateCommand<KeyEventArgs>(KeyDown);
      ApplyEditCommand = new BaseCommand(ApplyEdit);
      CancelEditCommand = new BaseCommand(CancelEdit);
      TextChangedCommand = new BaseCommand(TextChanged);
      ApplyArtistToAlbumArtistCommand = new BaseCommand(ApplyArtistToAlbumArtist);
      SaveCoverCommand = new BaseCommand(SaveCover);
      RemoveCoverCommand = new BaseCommand(RemoveCover);
      GetCoverCommand = new BaseCommand(GetCover);
      GetCoverFromFileCommand = new BaseCommand(GetCoverFromFile);
      GetSongLengthCommand = new BaseCommand(GetSongLength);
      PictureSelectionChangedCommand = new BaseCommand(PictureSelectionChanged);
      RemoveDetailedCoverCommand = new BaseCommand(RemoveDetailCover);
      SaveDetailCoverCommand = new BaseCommand(SaveDetailCover);
      GetLyricsCommand = new BaseCommand(GetLyrics);
      GetLyricsFromFileCommand = new BaseCommand(GetLyricsFromFile);
      RemoveLyricsCommand = new BaseCommand(RemoveLyrics);
      AddRatingCommand = new BaseCommand(AddRating);
      DeleteRatingCommand = new BaseCommand(DeleteRating);
      AddPersonCommand = new BaseCommand(AddPerson);
      DeletePersonCommand = new BaseCommand(DeletePerson);
      AddMusicianCommand = new BaseCommand(AddMusician);
      DeleteMusicianCommand = new BaseCommand(DeleteMusician);
      MusicBrainzCommand = new BaseCommand(GetFromMusicBrainz);
      AddUserFrameCommand = new BaseCommand(AddUserFrame);
      DeleteUserFrameCommand = new BaseCommand(DeleteUserFrame);
      DeleteAllUserFramesCommand = new BaseCommand(DeleteAllUserFrames);

      SelectedGenres.CollectionChanged += SelectedGenres_CollectionChanged;
      MediaTypes.AddRange(_options.MediaTypes);

      EventSystem.Subscribe<GenericEvent>(OnMessageReceived, ThreadOption.UIThread);
    }

    #endregion

    #region Commands

    public ICommand KeyDownCommand { get; }

    /// <summary>
    /// Handle Enter and Escape Key
    /// </summary>
    /// <param name="param"></param>
    private void KeyDown(KeyEventArgs param)
    {
      if (param.Key == Key.Enter)
      {
        ApplyEdit(SongEdit);
      }

      if (param.Key == Key.Escape)
      {
        CancelEdit(SongEdit);
      }
    }

    public ICommand TextChangedCommand { get; }

    /// <summary>
    /// Callback from the View when a Text is changed, it should check the checkbox
    /// </summary>
    /// <param name="param"></param>
    private void TextChanged(object param)
    {
      IsApplyButtonEnabled = true;

      var tb = (TextBox)(param as TextChangedEventArgs)?.Source;

      // Auto Complete Support for Artist / AlbumArtist
      if (tb != null)
      {
        switch (tb.Name.ToLower())
        {
          case "artist":
          case "albumartist":
            if (tb.Text.Length > 3)
            {
              AutoCompleteArtists.Clear();
              AutoCompleteArtists.AddRange(ContainerLocator.Current.Resolve<IMusicDatabase>()?.SearchAutocompleteArtists(tb.Text));
            }
            break;
        }
      }

      if (!MultiCheckBoxVisibility)
      {
        return;
      }

      if (tb != null)
      {
        // Song Length left out on Purpose, it doesn't need a multi checkbox
        switch (tb.Name.ToLower())
        {
          case "tracknumber":
          case "trackcount":
            CkTrackIsChecked = true;
            break;
          case "discnumber":
          case "disccount":
            CkDiscIsChecked = true;
            break;
          case "title":
            CkTitleIsChecked = true;
            break;
          case "artist":
          case "artistcombotext":
            CkArtistIsChecked = true;
            break;
          case "albumartist":
          case "albumartistcombotext":
            CkAlbumArtistIsChecked = true;
            break;
          case "album":
          case "albumcombotext":
            CkAlbumIsChecked = true;
            break;
          case "year":
            CkYearIsChecked = true;
            break;
          case "comment":
            CkCommentIsChecked = true;
            break;
          case "composer":
            CkComposerIsChecked = true;
            break;
          case "conductor":
            CkConductorIsChecked = true;
            break;
          case "interpretedby":
            CkInterpretedByIsChecked = true;
            break;
          case "textwriter":
            CkTextWriterIsChecked = true;
            break;
          case "publisher":
            CkPublisherIsChecked = true;
            break;
          case "encodedby":
            CkEncodedByIsChecked = true;
            break;
          case "copyright":
            CkCopyrightIsChecked = true;
            break;
          case "grouping":
            CkContentGroupIsChecked = true;
            break;
          case "subtitle":
            CkSubTitleIsChecked = true;
            break;
          case "artistsort":
            CkArtistSortIsChecked = true;
            break;
          case "albumartistsort":
            CkAlbumArtistSortIsChecked = true;
            break;
          case "albumsort":
            CkAlbumSortIsChecked = true;
            break;
          case "titlesort":
            CkTitleSortIsChecked = true;
            break;
          case "origalbum":
            CkOriginalAlbumIsChecked = true;
            break;
          case "origfilename":
            CkOriginalFileNameIsChecked = true;
            break;
          case "origlyricswriter":
            CkOriginalLyricsWriterIsChecked = true;
            break;
          case "origowner":
            CkOriginalOwnerIsChecked = true;
            break;
          case "origrelease":
            CkOriginalReleaseIsChecked = true;
            break;
          case "copyrightinformation":
            CkCopyrightUrlIsChecked = true;
            break;
          case "audiofileurl":
            CkOfficialAudioFileUrlIsChecked = true;
            break;
          case "artisturl":
            CkOfficialArtistUrlIsChecked = true;
            break;
          case "audiosourceurl":
            CkOfficialAudioFileUrlIsChecked = true;
            break;
          case "radiourl":
            CkOfficialInternetRadioUrlIsChecked = true;
            break;
          case "paymenturl":
            CkOfficialPaymentUrlIsChecked = true;
            break;
          case "publisherurl":
            CkOfficialPublisherUrlIsChecked = true;
            break;
          case "commercialurl":
            CkCommercialInformationUrlIsChecked = true;
            break;
          case "mbartistid":
            CkMbArtistIdIsChecked = true;
            break;
          case "mbreleaseartistid":
            CkMbReleaseArtistIdIsChecked = true;
            break;
          case "mbreleaseid":
            CkMbReleaseIdIsChecked = true;
            break;
          case "mbreleasegroupid":
            CkMbReleaseGroupIdIsChecked = true;
            break;
          case "mbreleasecountry":
            CkMbReleaseCountryIsChecked = true;
            break;
          case "mbdiscid":
            CkMbDiscIdIsChecked = true;
            break;
          case "mbreleasestatus":
            CkMbReleaseStatusIsChecked = true;
            break;
          case "mbreleasetype":
            CkMbReleaseTypeIsChecked = true;
            break;
          case "mbtrackid":
            CkMbTrackIdIsChecked = true;
            break;
        }
      }
    }

    /// <summary>
    /// Invoked, when the Genre collection has been changed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SelectedGenres_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      IsApplyButtonEnabled = true;

      if (!_isInitializing)
      {
        SongEdit.Changed = true;
      }

      if (MultiCheckBoxVisibility)
      {
        CkGenreIsChecked = true;
      }

      if (SongEdit != null && (_songs != null && _songs.Count > 0) && !_isInitializing)
      {
        SongEdit.Genre = string.Join(";", SelectedGenres);
      }
    }

    /// <summary>
    /// Invoked by the Cancel Edit Button
    /// </summary>
    public ICommand CancelEditCommand { get; }

    private void CancelEdit(object param)
    {
      log.Trace(">>>");
      ClearForm();
      if (_songBackup != null && _songs.Count == 1)
      {
        UndoSongedits(_songs[0], _songBackup);
      }

      _songs.ForEach(s => s.Changed = false);
      SongEdit.Changed = false;
      SetFormBindings(ref _songs);
      log.Trace("<<<");
    }

    /// <summary>
    /// Use the content of the Album Field as AlbumArtist
    /// </summary>
    public ICommand ApplyArtistToAlbumArtistCommand { get; }

    private void ApplyArtistToAlbumArtist(object param)
    {
      if (MultiArtist && SongEdit != null)
      {
        if (MultiAlbumArtist)
        {
          AlbumArtistComboText = ArtistComboText;
        }
        else
        {
          SongEdit.AlbumArtist = ArtistComboText;
        }
      }
      else if (SongEdit != null)
      {
        SongEdit.AlbumArtist = SongEdit.Artist;
      }
    }

    #region Picture related Commands

    /// <summary>
    /// Retrieve Cover Art
    /// </summary>
    public ICommand GetCoverCommand { get; }

    private void GetCover(object param)
    {
      log.Trace(">>>");
      GenericEvent evt = new GenericEvent()
      {
        Action = "Command"
      };
      evt.MessageData.Add("command", Action.ActionType.GETCOVERART);
      evt.MessageData.Add("removeexistingpictures", "true");
      EventSystem.Publish(evt);
      log.Trace("<<<");
    }

    /// <summary>
    /// Get a Cover From a File
    /// </summary>
    public ICommand GetCoverFromFileCommand { get; }

    private void GetCoverFromFile(object param)
    {
      if (SongEdit == null)
      {
        return;
      }
      log.Trace(">>>");

      var oFd = new OpenFileDialog
      {
        Filter = "Pictures (Bmp, Jpg, Gif, Png)|*.jpg;*.jpeg;*.bmp;*.Gif;*.png|All Files|*.*",
        InitialDirectory = SongEdit.FullFileName != null ? SongEdit.FilePath : _songs[0].FilePath

      };
      if (oFd.ShowDialog() == true)
      {
        if (CkRemovePicturesIsChecked)
        {
          SongEdit.Pictures.Clear();
          FrontCover = null;
        }

        try
        {
          var pic = new Picture(oFd.FileName);
          pic.Type = PictureType.FrontCover;
          SongEdit.Pictures.Add(pic);
          FrontCover = SongEdit.FrontCover;
          SongEdit.Changed = true;

          if (MultiCheckBoxVisibility)
          {
            CkPicturesIsChecked = true;
          }
          IsApplyButtonEnabled = true;
        }
        catch (Exception ex)
        {
          log.Error("Exception Loading picture: {0} {1}", oFd.FileName, ex.Message);
        }
      }
      log.Trace("<<<");
    }

    /// <summary>
    /// Remove All covers
    /// </summary>
    public ICommand RemoveCoverCommand { get; }

    private void RemoveCover(object param)
    {
      log.Trace(">>>");
      GenericEvent evt = new GenericEvent()
      {
        Action = "Command"
      };
      evt.MessageData.Add("command", Action.ActionType.REMOVEPICTURE);
      EventSystem.Publish(evt);
      FrontCover = null;
      if (MultiCheckBoxVisibility)
      {
        CkPicturesIsChecked = true;
      }
      IsApplyButtonEnabled = true;
      log.Trace("<<<");
    }

    /// <summary>
    /// Remove a cover, selected in the details section
    /// </summary>
    public ICommand RemoveDetailedCoverCommand { get; }

    private void RemoveDetailCover(object param)
    {
      log.Trace(">>>");
      if (SelectedPicture.Count > 0)
      {
        SongEdit.Pictures.Remove((Picture)SelectedPicture[0]);
        IsApplyButtonEnabled = true;
      }
      log.Trace("<<<");
    }

    /// <summary>
    /// Save picture as Folder.jpg
    /// </summary>
    public ICommand SaveCoverCommand { get; }

    private void SaveCover(object param)
    {
      log.Trace(">>>");
      log.Info("Saving Folder Thumb");
      var song = (SongData)param;
      // Do we have multiple Songs selected and a Front Cover exists?
      if (_songs.Count > 1 && FrontCover != null)
      {
        song.FullFileName = _songs[0].FullFileName;
        var pic = new Picture { Data = Picture.ImageToByte(FrontCover) };
        pic.Type = PictureType.FrontCover;
        song.Pictures.Add(pic);
      }
      var fileName = Path.Combine(Path.GetDirectoryName(song.FullFileName), "folder.jpg");
      int indexFrontCover = song.Pictures
        .Select((pic, i) => new { Pic = pic, Position = i }).First(m => m.Pic.Type == PictureType.FrontCover).Position;
      if (indexFrontCover < 0)
      {
        indexFrontCover = 0;
      }

      Util.SavePicture(song.Pictures[indexFrontCover], fileName);
      var miscfileevt = new GenericEvent
      {
        Action = "miscfileschanged"
      };
      EventSystem.Publish(miscfileevt);
      log.Trace("<<<");
    }

    /// <summary>
    /// Save a detailed picture as specified filename
    /// </summary>
    public ICommand SaveDetailCoverCommand { get; }

    private void SaveDetailCover(object param)
    {
      log.Trace(">>>");
      log.Info("Saving Picture as file");
      if (SelectedPicture.Count > 0)
      {
        var sFd = new SaveFileDialog
        {
          Filter = "Pictures (Bmp, Jpg, Gif, Png)|*.jpg;*.jpeg;*.bmp;*.Gif;*.png|All Files|*.*",
          InitialDirectory = SongEdit.FullFileName != null ? SongEdit.FilePath : _songs[0].FilePath

        };
        if (sFd.ShowDialog() == true)
        {
          var fileName = Path.Combine(Path.GetDirectoryName(SongEdit.FullFileName), sFd.FileName);
          Util.SavePicture((Picture)SelectedPicture[0], fileName);
          var miscfileevt = new GenericEvent
          {
            Action = "miscfileschanged"
          };
          EventSystem.Publish(miscfileevt);
        }
      }
      log.Trace("<<<");
    }

    /// <summary>
    /// A Picture has been selected in the Picture Details
    /// </summary>
    public ICommand PictureSelectionChangedCommand { get; }
    private void PictureSelectionChanged(object param)
    {
      if (SelectedPicture.Count > 0)
      {
        var pic = (Picture)SelectedPicture[0];
        try
        {
          var bitmapImage = new BitmapImage();
          using (var stream = new MemoryStream(pic.Data))
          {
            stream.Seek(0, SeekOrigin.Begin);
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            PictureDetail = bitmapImage;
          }
        }
        catch
        {
          PictureDetail = null;
        }
      }
      else
      {
        PictureDetail = null;
      }
    }

    #endregion

    #region Lyrics related Commands

    /// <summary>
    /// Get Lyrics from Internet
    /// </summary>
    public ICommand GetLyricsCommand { get; }

    public void GetLyrics(object param)
    {
      log.Trace(">>>");
      var evt = new GenericEvent
      {
        Action = "Command"
      };
      evt.MessageData.Add("command", Action.ActionType.GETLYRICS);
      EventSystem.Publish(evt);
      IsApplyButtonEnabled = true;
      log.Trace("<<<");
    }

    /// <summary>
    /// Get Lyrics from File
    /// </summary>
    public ICommand GetLyricsFromFileCommand { get; }

    public void GetLyricsFromFile(object param)
    {
      log.Trace(">>>");
      var oFd = new OpenFileDialog
      {
        Filter = "Text Files|*.txt|All Files|*.*",
        InitialDirectory = SongEdit.FullFileName != null ? SongEdit.FilePath : _songs[0].FilePath
      };
      if (oFd.ShowDialog() == true)
      {
        try
        {
          SongEdit.Lyrics = File.ReadAllText(oFd.FileName, Encoding.UTF8);
          IsApplyButtonEnabled = true;
        }
        catch (Exception ex)
        {
          log.Error($"Error reading Lyrics text {ex.Message}");
        }
      }
      log.Trace("<<<");
    }

    /// <summary>
    /// RemoveLyrics
    /// </summary>
    public ICommand RemoveLyricsCommand { get; }

    public void RemoveLyrics(object param)
    {
      log.Trace(">>>");
      // Do we have multiple Songs selected
      if (_songs.Count > 1)
      {
        foreach (var song in _songs)
        {
          song.Lyrics = "";
        }
      }
      else
      {
        SongEdit.Lyrics = "";
      }
      IsApplyButtonEnabled = true;
      log.Trace("<<<");
    }

    #endregion

    #region Rating related Commands

    /// <summary>
    /// Add a Rating
    /// </summary>
    public ICommand AddRatingCommand { get; }

    private void AddRating(object param)
    {
      log.Trace(">>>");
      var mptagthatUser = SongEdit.Ratings.FirstOrDefault(r => r.User.ToLowerInvariant() == "mptagthat");
      var user = mptagthatUser?.User.ToLowerInvariant() == "mptagthat" ? "" : "MPTagThat";
      SongEdit.Ratings.Add(new PopmFrame(user, 0, 0));
      SongEdit.Changed = true;
      IsApplyButtonEnabled = true;
      log.Trace("<<<");
    }

    /// <summary>
    /// Delete a Rating
    /// </summary>
    public ICommand DeleteRatingCommand { get; }

    private void DeleteRating(object param)
    {
      log.Trace(">>>");
      if (SelectedRating.Count > 0)
      {
        SongEdit.Ratings.Remove((PopmFrame)SelectedRating[0]);
        SongEdit.Changed = true;
        IsApplyButtonEnabled = true;
      }
      log.Trace("<<<");
    }

    #endregion

    #region Involed People / Musicians Commands

    /// <summary>
    /// Add a involved Person
    /// </summary>
    public ICommand AddPersonCommand { get; }

    private void AddPerson(Object param)
    {
      InvolvedPersons.Add(new Person { Function = "", Name = "" });
      UpdateInvolvedPersons(SongEdit);
      IsApplyButtonEnabled = true;
    }

    /// <summary>
    /// Delete a Person
    /// </summary>
    public ICommand DeletePersonCommand { get; }

    private void DeletePerson(Object param)
    {
      if (SelectedPerson.Count > 0)
      {
        InvolvedPersons.Remove((Person)SelectedPerson[0]);
        UpdateInvolvedPersons(SongEdit);
        IsApplyButtonEnabled = true;
      }
    }

    /// <summary>
    /// Add a Musician
    /// </summary>
    public ICommand AddMusicianCommand { get; }

    private void AddMusician(Object param)
    {
      InvolvedMusicians.Add(new Person { Function = "", Name = "" });
      UpdateMusicians(SongEdit);
      IsApplyButtonEnabled = true;
    }

    /// <summary>
    /// Delete a Musician
    /// </summary>
    public ICommand DeleteMusicianCommand { get; }

    private void DeleteMusician(Object param)
    {
      if (SelectedMusician.Count > 0)
      {
        InvolvedMusicians.Remove((Person)SelectedMusician[0]);
        UpdateMusicians(SongEdit);
        IsApplyButtonEnabled = true;
      }
    }

    #endregion

    #region User Frame Commands

    public ICommand AddUserFrameCommand { get; }

    private void AddUserFrame(object param)
    {
      log.Trace(">>>");
      SongEdit.UserFrames.Add(new Frame("TXXX", "", ""));
      SongEdit.Changed = true;
      IsApplyButtonEnabled = true;
      log.Trace("<<<");
    }

    public ICommand DeleteUserFrameCommand { get; }

    private void DeleteUserFrame(object param)
    {
      log.Trace(">>>");
      // Can't use a foreach here, since it modifies the collection
      while (SelectedUserFrames.Count > 0)
      {
        SongEdit.UserFrames.Remove((Frame)SelectedUserFrames[0]);
      }
      SongEdit.Changed = true;
      IsApplyButtonEnabled = true;
      log.Trace("<<<");
    }

    public ICommand DeleteAllUserFramesCommand { get; }

    private void DeleteAllUserFrames(object param)
    {
      log.Trace(">>>");
      SongEdit.UserFrames.Clear();
      SongEdit.Changed = true;
      IsApplyButtonEnabled = true;
      log.Trace("<<<");
    }

    #endregion

    #region MusicBrainz Commands
    public ICommand MusicBrainzCommand { get; }

    /// <summary>
    /// Get Song Information from MusicBrainz
    /// </summary>
    /// <param name="param"></param>
    private void GetFromMusicBrainz(object param)
    {
      log.Trace(">>>");
      // Send out the Event with the action
      var evt = new GenericEvent
      {
        Action = "Command"
      };
      evt.MessageData.Add("command", Action.ActionType.MusicBrainzInfo);
      evt.MessageData.Add("runasync", false);
      EventSystem.Publish(evt);
      log.Trace("<<<");
    }

    #endregion

    /// <summary>
    /// Set the Song Length from file
    /// </summary>
    public ICommand GetSongLengthCommand { get; }

    private void GetSongLength(object param)
    {
      var song = (SongData)param;
      song.TrackLength = song.DurationTimespan.TotalMilliseconds.ToString();
      IsApplyButtonEnabled = true;
    }

    /// <summary>
    /// Invoked by the Apply Changes button in the View
    /// Loop through the the selected songs and apply the changes.
    /// </summary>
    public ICommand ApplyEditCommand { get; }

    private void ApplyEdit(object param)
    {
      log.Trace(">>>");
      var songEdit = (SongData)param;

      if (_songs == null)
      {
        return;
      }

      // If we got only one song, then the changes have been applied already through binding
      if (_songs.Count == 1)
      {
        UpdateInvolvedPersons(_songs[0]);
        UpdateMusicians(_songs[0]);
        log.Trace("<<<");
        return;
      }

      foreach (var song in _songs)
      {
        if (CkTrackIsChecked)
        {
          if (SongEdit.TrackNumber != 0 && SongEdit.TrackNumber != song.TrackNumber)
          {
            song.TrackNumber = songEdit.TrackNumber;
          }
          song.TrackCount = songEdit.TrackCount;
        }

        if (CkDiscIsChecked)
        {
          if (SongEdit.DiscNumber != 0 && SongEdit.DiscNumber != song.DiscNumber)
          {
            song.DiscNumber = songEdit.DiscNumber;
          }
          song.DiscCount = songEdit.DiscCount;
        }

        if (CkTitleIsChecked)
        {
          song.Title = songEdit.Title;
        }

        if (CkArtistIsChecked)
        {
          song.Artist = MultiArtist ? ArtistComboText : songEdit.Artist.Trim();
        }

        if (CkAlbumArtistIsChecked)
        {
          song.AlbumArtist = MultiAlbumArtist ? AlbumArtistComboText : songEdit.AlbumArtist.Trim();
        }

        if (CkAlbumIsChecked)
        {
          song.Album = MultiAlbum ? AlbumComboText : songEdit.Album.Trim();
        }

        song.Compilation = CkPartOfCompilationIsChecked;

        if (CkYearIsChecked)
        {
          song.Year = songEdit.Year;
        }

        if (CkGenreIsChecked)
        {
          song.Genre = songEdit.Genre;
        }

        if (CkCommentIsChecked)
        {
          song.Comment = songEdit.Comment;
        }

        if (CkPicturesIsChecked)
        {
          song.Pictures = songEdit.Pictures;
          song.Changed = true;
        }

        if (CkComposerIsChecked)
        {
          song.Composer = songEdit.Composer;
        }

        if (CkConductorIsChecked)
        {
          song.Conductor = songEdit.Conductor;
        }

        if (CkInterpretedByIsChecked)
        {
          song.Interpreter = songEdit.Interpreter;
        }

        if (CkTextWriterIsChecked)
        {
          song.TextWriter = songEdit.TextWriter;
        }

        if (CkPublisherIsChecked)
        {
          song.Publisher = songEdit.Publisher;
        }

        if (CkEncodedByIsChecked)
        {
          song.EncodedBy = songEdit.EncodedBy;
        }

        if (CkCopyrightIsChecked)
        {
          song.Copyright = songEdit.Copyright;
        }

        if (CkContentGroupIsChecked)
        {
          song.Grouping = songEdit.Grouping;
        }

        if (CkSubTitleIsChecked)
        {
          song.SubTitle = songEdit.SubTitle;
        }

        if (CkArtistSortIsChecked)
        {
          song.ArtistSortName = songEdit.ArtistSortName;
        }

        if (CkAlbumArtistSortIsChecked)
        {
          song.AlbumArtistSortName = songEdit.AlbumArtistSortName;
        }

        if (CkAlbumSortIsChecked)
        {
          song.AlbumSortName = songEdit.AlbumSortName;
        }

        if (CkTitleSortIsChecked)
        {
          song.TitleSortName = songEdit.TitleSortName;
        }

        if (CkMediaTypeIsChecked)
        {
          song.MediaType = MediaTypes[SelectedIndexMediaType].Value.ToString();
        }

        if (CkTrackLengthIsChecked)
        {
          song.TrackLength = song.DurationTimespan.TotalMilliseconds.ToString();
        }

        if (CkOriginalAlbumIsChecked)
        {
          song.OriginalAlbum = songEdit.OriginalAlbum;
        }

        if (CkOriginalFileNameIsChecked)
        {
          song.OriginalFileName = songEdit.OriginalFileName;
        }

        if (CkOriginalLyricsWriterIsChecked)
        {
          song.OriginalLyricsWriter = songEdit.OriginalLyricsWriter;
        }

        if (CkOriginalArtistIsChecked)
        {
          song.OriginalArtist = songEdit.OriginalArtist;
        }

        if (CkOriginalOwnerIsChecked)
        {
          song.OriginalOwner = songEdit.OriginalOwner;
        }

        if (CkOriginalReleaseIsChecked)
        {
          song.OriginalRelease = songEdit.OriginalRelease;
        }

        if (CkCopyrightUrlIsChecked)
        {
          song.CopyrightInformation = songEdit.CopyrightInformation;
        }

        if (CkOfficialAudioFileUrlIsChecked)
        {
          song.OfficialAudioFileInformation = songEdit.OfficialAudioFileInformation;
        }

        if (CkOfficialArtistUrlIsChecked)
        {
          song.OfficialArtistInformation = songEdit.OfficialArtistInformation;
        }

        if (CkOfficialAudioSourceUrlIsChecked)
        {
          song.OfficialAudioSourceInformation = songEdit.OfficialAudioSourceInformation;
        }

        if (CkOfficialInternetRadioUrlIsChecked)
        {
          song.OfficialInternetRadioInformation = songEdit.OfficialInternetRadioInformation;
        }

        if (CkOfficialPaymentUrlIsChecked)
        {
          song.OfficialPaymentInformation = songEdit.OfficialPaymentInformation;
        }

        if (CkOfficialPublisherUrlIsChecked)
        {
          song.OfficialPublisherInformation = songEdit.OfficialPublisherInformation;
        }

        if (CkCommercialInformationUrlIsChecked)
        {
          song.CommercialInformation = songEdit.CommercialInformation;
        }

        if (CkMbArtistIdIsChecked)
        {
          song.MusicBrainzArtistId = songEdit.MusicBrainzArtistId;
        }

        if (CkMbReleaseArtistIdIsChecked)
        {
          song.MusicBrainzReleaseArtistId = songEdit.MusicBrainzReleaseArtistId;
        }

        if (CkMbDiscIdIsChecked)
        {
          song.MusicBrainzDiscId = songEdit.MusicBrainzDiscId;
        }

        if (CkMbReleaseCountryIsChecked)
        {
          song.MusicBrainzReleaseCountry = songEdit.MusicBrainzReleaseCountry;
        }

        if (CkMbReleaseIdIsChecked)
        {
          song.MusicBrainzReleaseId = songEdit.MusicBrainzReleaseId;
        }

        if (CkMbReleaseStatusIsChecked)
        {
          song.MusicBrainzReleaseStatus = songEdit.MusicBrainzReleaseStatus;
        }

        if (CkMbTrackIdIsChecked)
        {
          song.MusicBrainzTrackId = songEdit.MusicBrainzTrackId;
        }

        if (CkMbReleaseTypeIsChecked)
        {
          song.MusicBrainzReleaseType = songEdit.MusicBrainzReleaseType;
        }

        if (CkMbReleaseGroupIdIsChecked)
        {
          song.MusicBrainzReleaseGroupId = songEdit.MusicBrainzReleaseGroupId;
        }
      }
      log.Trace("<<<");
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Set the bindings for the Controls in the Tagedit Form
    /// </summary>
    /// <param name="songs"></param>
    private void SetFormBindings(ref List<SongData> songs)
    {
      log.Trace(">>>");
      ClearForm();
      _isInitializing = true;
      MultiAlbum = false;
      Albums.Clear();
      MultiArtist = false;
      Artists.Clear();
      MultiAlbumArtist = false;
      AlbumArtists.Clear();

      if (songs.Count == 1)
      {
        try
        {

          log.Trace("Single song selected");
          UncheckCheckboxes();
          SongEdit = songs[0];
          _songBackup = SongEdit.Clone();
          UpdateGenres(SongEdit);
          SongEdit.UpdateChangedProperty = true;

          // Prevent a Collection Changed Exception
          SelectedGenres.CollectionChanged -= SelectedGenres_CollectionChanged;
          SelectedGenres?.AddRange(SongEdit.Genre.Split(';'));
          SelectedGenres.CollectionChanged += SelectedGenres_CollectionChanged;

          var j = 0;
          foreach (var mediatype in MediaTypes)
          {
            if (mediatype.Value.ToString() == SongEdit.MediaType)
            {
              SelectedIndexMediaType = j;
              break;
            }
            j++;
          }
          FillInvolvedPersonsGrid(SongEdit);
          FillMusicianGrid(SongEdit);
          FrontCover = SongEdit.FrontCover;
          _isInitializing = false;
          IsApplyButtonEnabled = false;
          log.Trace("<<<");
          return;
        }
        catch (Exception e)
        {
          log.Error($"Exception in TagEdit: {e.Message}. Trace: {e.StackTrace}");
        }
      }

      log.Trace($"{songs.Count} songs selected");
      var i = 0;
      byte[] picData = new byte[] { };
      var strGenreTemp = "";
      SelectedGenres?.Clear();

      try
      {
        foreach (var song in songs)
        {
          // Don't handle single track for Multitag Edit
          if (SongEdit.TrackCount != song.TrackCount)
          {
            if (i == 0 && song.TrackCount != 0)
            {
              SongEdit.TrackCount = song.TrackCount;
            }
            else
            {
              SongEdit.TrackCount = 0;
            }
          }

          if (SongEdit.DiscNumber != song.DiscNumber)
          {
            if (i == 0 && song.DiscNumber != 0)
            {
              SongEdit.DiscNumber = song.DiscNumber;
            }
            else
            {
              SongEdit.DiscNumber = 0;
            }
          }

          if (SongEdit.DiscCount != song.DiscCount)
          {
            if (i == 0 && song.DiscCount != 0)
            {
              SongEdit.DiscCount = song.DiscCount;
            }
            else
            {
              SongEdit.DiscCount = 0;
            }
          }

          if (SongEdit.Title != song.Title)
          {
            SongEdit.Title = i == 0 ? song.Title : "";
          }

          if (SongEdit.Artist != song.Artist)
          {
            SongEdit.Artist = i == 0 ? song.Artist : "";
            if (!Artists.Contains(song.Artist))
            {
              Artists.Add(song.Artist);
            }

            if (i > 0)
            {
              MultiArtist = true;
              SelectedArtistsIndex = 0;
              ArtistComboText = Artists[0];
            }
          }

          if (SongEdit.AlbumArtist != song.AlbumArtist)
          {
            SongEdit.AlbumArtist = i == 0 ? song.AlbumArtist : "";
            if (!AlbumArtists.Contains(song.AlbumArtist))
            {
              AlbumArtists.Add(song.AlbumArtist);
            }

            if (i > 0)
            {
              MultiAlbumArtist = true;
              SelectedAlbumArtistsIndex = 0;
              AlbumArtistComboText = AlbumArtists[0];
            }
          }

          if (SongEdit.Album != song.Album)
          {
            SongEdit.Album = i == 0 ? song.Album : "";
            if (!Albums.Contains(song.Album))
            {
              Albums.Add(song.Album);
            }
            if (i > 0)
            {
              MultiAlbum = true;
              SelectedAlbumsIndex = 0;
            }
          }

          if (CkPartOfCompilationIsChecked != song.Compilation)
          {
            SongEdit.Compilation = CkPartOfCompilationIsChecked = i == 0 && song.Compilation;
          }

          if (SongEdit.Year != song.Year)
          {
            SongEdit.Year = i == 0 ? song.Year : 0;
          }

          UpdateGenres(song);
          if (strGenreTemp != song.Genre)
          {
            if (i == 0)
            {
              SelectedGenres.AddRange(song.Genre.Split(';'));
              strGenreTemp = song.Genre;
            }
            else
            {
              SelectedGenres.Clear();
            }
          }

          if (SongEdit.Comment != song.Comment)
          {
            SongEdit.Comment = i == 0 ? song.Comment : "";
          }

          if (song.Pictures.Count > 0)
          {
            if (!song.Pictures[0].Data.SequenceEqual(picData))
            {
              if (i == 0)
              {
                FrontCover = song.FrontCover;
                picData = song.Pictures[0].Data;
              }
              else
              {
                FrontCover = null;
              }
            }
          }

          if (SongEdit.Composer != song.Composer)
          {
            SongEdit.Composer = i == 0 ? song.Composer : "";
          }

          if (SongEdit.Conductor != song.Conductor)
          {
            SongEdit.Conductor = i == 0 ? song.Conductor : "";
          }

          if (SongEdit.Interpreter != song.Interpreter)
          {
            SongEdit.Interpreter = i == 0 ? song.Interpreter : "";
          }

          if (SongEdit.TextWriter != song.TextWriter)
          {
            SongEdit.TextWriter = i == 0 ? song.TextWriter : "";
          }

          if (SongEdit.Publisher != song.Publisher)
          {
            SongEdit.Publisher = i == 0 ? song.Publisher : "";
          }

          if (SongEdit.EncodedBy != song.EncodedBy)
          {
            SongEdit.EncodedBy = i == 0 ? song.EncodedBy : "";
          }

          if (SongEdit.Copyright != song.Copyright)
          {
            SongEdit.Copyright = i == 0 ? song.Copyright : "";
          }

          if (SongEdit.Grouping != song.Grouping)
          {
            SongEdit.Grouping = i == 0 ? song.Grouping : "";
          }

          if (SongEdit.SubTitle != song.SubTitle)
          {
            SongEdit.SubTitle = i == 0 ? song.SubTitle : "";
          }

          if (SongEdit.ArtistSortName != song.ArtistSortName)
          {
            SongEdit.ArtistSortName = i == 0 ? song.ArtistSortName : "";
          }

          if (SongEdit.AlbumArtistSortName != song.AlbumArtistSortName)
          {
            SongEdit.AlbumArtistSortName = i == 0 ? song.AlbumArtistSortName : "";
          }

          if (SongEdit.AlbumSortName != song.AlbumSortName)
          {
            SongEdit.AlbumSortName = i == 0 ? song.AlbumSortName : "";
          }

          if (SongEdit.TitleSortName != song.TitleSortName)
          {
            SongEdit.TitleSortName = i == 0 ? song.TitleSortName : "";
          }

          if (SongEdit.MediaType != song.MediaType)
          {
            if (i == 0)
            {
              var j = 0;
              foreach (var mediatype in MediaTypes)
              {
                if (mediatype.Value.ToString() == song.MediaType)
                {
                  SelectedIndexMediaType = j;
                  break;
                }
              }

              j++;
            }
            else
            {
              SelectedIndexMediaType = 0;
            }
          }

          if (SongEdit.TrackLength != song.TrackLength)
          {
            SongEdit.TrackLength = i == 0 ? song.TrackLength : "";
          }

          if (SongEdit.OriginalAlbum != song.OriginalAlbum)
          {
            SongEdit.OriginalAlbum = i == 0 ? song.OriginalAlbum : "";
          }

          if (SongEdit.OriginalFileName != song.OriginalFileName)
          {
            SongEdit.OriginalFileName = i == 0 ? song.OriginalFileName : "";
          }

          if (SongEdit.OriginalLyricsWriter != song.OriginalLyricsWriter)
          {
            SongEdit.OriginalLyricsWriter = i == 0 ? song.OriginalLyricsWriter : "";
          }

          if (SongEdit.OriginalArtist != song.OriginalArtist)
          {
            SongEdit.OriginalArtist = i == 0 ? song.OriginalArtist : "";
          }

          if (SongEdit.OriginalOwner != song.OriginalOwner)
          {
            SongEdit.OriginalOwner = i == 0 ? song.OriginalOwner : "";
          }

          if (SongEdit.OriginalRelease != song.OriginalRelease)
          {
            SongEdit.OriginalRelease = i == 0 ? song.OriginalRelease : "";
          }

          if (SongEdit.CopyrightInformation != song.CopyrightInformation)
          {
            SongEdit.CopyrightInformation = i == 0 ? song.CopyrightInformation : "";
          }

          if (SongEdit.OfficialAudioFileInformation != song.OfficialAudioFileInformation)
          {
            SongEdit.OfficialAudioFileInformation = i == 0 ? song.OfficialAudioFileInformation : "";
          }

          if (SongEdit.OfficialArtistInformation != song.OfficialArtistInformation)
          {
            SongEdit.OfficialArtistInformation = i == 0 ? song.OfficialArtistInformation : "";
          }

          if (SongEdit.OfficialAudioSourceInformation != song.OfficialAudioSourceInformation)
          {
            SongEdit.OfficialAudioSourceInformation = i == 0 ? song.OfficialAudioSourceInformation : "";
          }

          if (SongEdit.OfficialInternetRadioInformation != song.OfficialInternetRadioInformation)
          {
            SongEdit.OfficialInternetRadioInformation = i == 0 ? song.OfficialInternetRadioInformation : "";
          }

          if (SongEdit.OfficialPaymentInformation != song.OfficialPaymentInformation)
          {
            SongEdit.OfficialPaymentInformation = i == 0 ? song.OfficialPaymentInformation : "";
          }

          if (SongEdit.OfficialPublisherInformation != song.OfficialPublisherInformation)
          {
            SongEdit.OfficialPublisherInformation = i == 0 ? song.OfficialPublisherInformation : "";
          }

          if (SongEdit.CommercialInformation != song.CommercialInformation)
          {
            SongEdit.CommercialInformation = i == 0 ? song.CommercialInformation : "";
          }

          if (SongEdit.MusicBrainzArtistId != song.MusicBrainzArtistId)
          {
            SongEdit.MusicBrainzArtistId = i == 0 ? song.MusicBrainzArtistId : "";
          }

          if (SongEdit.MusicBrainzReleaseArtistId != song.MusicBrainzReleaseArtistId)
          {
            SongEdit.MusicBrainzReleaseArtistId = i == 0 ? song.MusicBrainzReleaseArtistId : "";
          }

          if (SongEdit.MusicBrainzDiscId != song.MusicBrainzDiscId)
          {
            SongEdit.MusicBrainzDiscId = i == 0 ? song.MusicBrainzDiscId : "";
          }

          if (SongEdit.MusicBrainzReleaseCountry != song.MusicBrainzReleaseCountry)
          {
            SongEdit.MusicBrainzReleaseCountry = i == 0 ? song.MusicBrainzReleaseCountry : "";
          }

          if (SongEdit.MusicBrainzReleaseId != song.MusicBrainzReleaseId)
          {
            SongEdit.MusicBrainzReleaseId = i == 0 ? song.MusicBrainzReleaseId : "";
          }

          if (SongEdit.MusicBrainzReleaseStatus != song.MusicBrainzReleaseStatus)
          {
            SongEdit.MusicBrainzReleaseStatus = i == 0 ? song.MusicBrainzReleaseStatus : "";
          }

          if (SongEdit.MusicBrainzTrackId != song.MusicBrainzTrackId)
          {
            SongEdit.MusicBrainzTrackId = i == 0 ? song.MusicBrainzTrackId : "";
          }

          if (SongEdit.MusicBrainzReleaseType != song.MusicBrainzReleaseType)
          {
            SongEdit.MusicBrainzReleaseType = i == 0 ? song.MusicBrainzReleaseType : "";
          }

          if (SongEdit.MusicBrainzReleaseGroupId != song.MusicBrainzReleaseGroupId)
          {
            SongEdit.MusicBrainzReleaseGroupId = i == 0 ? song.MusicBrainzReleaseGroupId : "";
          }

          // Enable Update mode for Song
          song.UpdateChangedProperty = true;
          i++;
        }

        if (SongEdit.TrackCount == 0 && _options.MainSettings.AutoFillNumberOfTracks)
        {
          SongEdit.TrackCount = (uint)songs.Count;
          CkTrackIsChecked = true;
          IsApplyButtonEnabled = true;
        }

        _isInitializing = false;
        SongEdit.UpdateChangedProperty = true;
        IsApplyButtonEnabled = false;

        // We have multiple Songs selected, so show the Checkboxes and
        // decide if they should be checked.
        MultiCheckBoxVisibility = true;
      }
      catch (Exception e)
      {
        log.Error($"Exception in TagEdit: {e.Message}. Trace: {e.StackTrace}");
      }
      log.Trace("<<<");
    }

    /// <summary>
    /// Update the List of Genres
    /// </summary>
    /// <param name="song"></param>
    private void UpdateGenres(SongData song)
    {
      var newGenres = (from g in song.Genre.Split(';')
                       where !_genres.Contains(g)
                       select g).ToList<string>();
      foreach (var newGenre in newGenres)
      {
        Genres.Insert(0, newGenre);
      }
    }

    /// <summary>
    /// Fill the Grid with the Involved Persons
    /// </summary>
    /// <param name="song"></param>
    private void FillInvolvedPersonsGrid(SongData song)
    {
      // A IPLS is delimited with "\0"
      // A TIPL is delimited with ";"
      var ipls = song.InvolvedPeople?.Split(new[] { '\0', ';' });
      for (var j = 0; j < ipls?.Length - 1; j += 2)
      {
        InvolvedPersons.Add(new Person { Name = ipls[j].Trim(), Function = ipls[j + 1].Trim() });
      }
    }

    /// <summary>
    /// Update the Involved Persons Frame
    /// </summary>
    /// <param name="song"></param>
    private void UpdateInvolvedPersons(SongData song)
    {
      var delimiter = ";";
      if (song.ID3Version == 3)
      {
        delimiter = new string(new char[1] { '\0' });
      }

      var involvedpersons = "";
      foreach (var person in InvolvedPersons)
      {
        involvedpersons += $"{person.Name}{delimiter}{person.Function}{delimiter}";
      }

      involvedpersons = involvedpersons.Trim(new[] { ';', '\0' });
      song.InvolvedPeople = involvedpersons;
    }

    /// <summary>
    /// Fill the Grid with the Musicians
    /// </summary>
    /// <param name="song"></param>
    private void FillMusicianGrid(SongData song)
    {
      // A TMCL is delimited with ";"
      var tmcl = song.MusicCreditList?.Split(new[] { ';' });
      for (var j = 0; j < tmcl?.Length - 1; j += 2)
      {
        InvolvedMusicians.Add(new Person { Function = tmcl[j].Trim(), Name = tmcl[j + 1].Trim() });
      }
    }

    /// <summary>
    /// Update the MusicCreditList Frame
    /// </summary>
    /// <param name="song"></param>
    private void UpdateMusicians(SongData song)
    {
      if (song.ID3Version == 3)
      {
        return;
      }
      var delimiter = ";";
      var musicians = "";
      foreach (var person in InvolvedMusicians)
      {
        musicians += $"{person.Function}{delimiter}{person.Name}{delimiter}";
      }

      musicians = musicians.Trim(new[] { ';' });
      song.InvolvedPeople = musicians;
    }


    /// <summary>
    /// Clear the form
    /// </summary>
    private void ClearForm()
    {
      log.Trace(">>>");
      _isInitializing = true;
      SongEdit = new SongData();
      UncheckCheckboxes();
      FrontCover = null;
      PictureDetail = null;
      Genres.Clear();
      SelectedGenres?.Clear();
      SelectedPicture?.Clear();
      Genres.AddRange(_options.MainSettings.CustomGenres);
      Genres.AddRange(Util.Genres);
      InvolvedPersons?.Clear();
      SelectedIndexMediaType = 0;
      MultiCheckBoxVisibility = false;
      IsApplyButtonEnabled = false;
      _isInitializing = false;
      log.Trace("<<<");
    }

    /// <summary>
    /// Clears the Checkboxes behind the input fields
    /// </summary>
    private void UncheckCheckboxes()
    {
      CkTrackIsChecked = false;
      CkDiscIsChecked = false;
      CkArtistIsChecked = false;
      CkAlbumArtistIsChecked = false;
      CkGenreIsChecked = false;
      CkAlbumIsChecked = false;
      CkTitleIsChecked = false;
      CkYearIsChecked = false;
      CkBPMIsChecked = false;
      CkConductorIsChecked = false;
      CkComposerIsChecked = false;
      CkInterpretedByIsChecked = false;
      CkTextWriterIsChecked = false;
      CkPublisherIsChecked = false;
      CkEncodedByIsChecked = false;
      CkCopyrightIsChecked = false;
      CkContentGroupIsChecked = false;
      CkSubTitleIsChecked = false;
      CkArtistSortIsChecked = false;
      CkAlbumSortIsChecked = false;
      CkAlbumSortIsChecked = false;
      CkTitleSortIsChecked = false;
      CkOriginalAlbumIsChecked = false;
      CkOriginalArtistIsChecked = false;
      CkOriginalFileNameIsChecked = false;
      CkOriginalLyricsWriterIsChecked = false;
      CkOriginalOwnerIsChecked = false;
      CkOriginalReleaseIsChecked = false;
      CkCopyrightUrlIsChecked = false;
      CkOfficialAudioFileUrlIsChecked = false;
      CkOfficialArtistUrlIsChecked = false;
      CkOfficialAudioSourceUrlIsChecked = false;
      CkOfficialInternetRadioUrlIsChecked = false;
      CkOfficialPaymentUrlIsChecked = false;
      CkCommercialInformationUrlIsChecked = false;
      CkOfficialPublisherUrlIsChecked = false;
      CkInvolvedPersonIsChecked = false;
      CkInvolvedMusicianIsChecked = false;
      CkRemoveExistingRatingsIsChecked = false;
      CkRemoveLyricsIsChecked = false;
      CkMediaTypeIsChecked = false;
      CkTrackLengthIsChecked = false;
      CkPicturesIsChecked = false;
      CkMbArtistIdIsChecked = false;
      CkMbReleaseArtistIdIsChecked = false;
      CkMbDiscIdIsChecked = false;
      CkMbReleaseCountryIsChecked = false;
      CkMbReleaseIdIsChecked = false;
      CkMbReleaseStatusIsChecked = false;
      CkMbTrackIdIsChecked = false;
      CkMbReleaseTypeIsChecked = false;
      CkMbReleaseGroupIdIsChecked = false;
    }

    /// <summary>
    /// Undo Changes to the Song by resetting the values from the backup
    /// </summary>
    /// <param name="original"></param>
    /// <param name="backup"></param>
    private void UndoSongedits(SongData original, SongData backup)
    {
      original.UpdateChangedProperty = false;
      original.Status = -1;
      original.Changed = false;
      original.FullFileName = backup.FullFileName;
      original.FileName = backup.FileName;
      original.Artist = backup.Artist;
      original.ArtistSortName = backup.ArtistSortName;
      original.AlbumArtist = backup.AlbumArtist;
      original.AlbumArtistSortName = backup.AlbumArtistSortName;
      original.Album = backup.Album;
      original.AlbumSortName = backup.AlbumSortName;
      original.BPM = backup.BPM;
      original.Comment = backup.Comment;
      original.CommercialInformation = backup.CommercialInformation;
      original.Compilation = backup.Compilation;
      original.Composer = backup.Composer;
      original.Conductor = backup.Conductor;
      original.Copyright = backup.Copyright;
      original.CopyrightInformation = backup.CopyrightInformation;
      original.Disc = backup.Disc;
      original.EncodedBy = backup.EncodedBy;
      original.Interpreter = backup.Interpreter;
      original.Genre = backup.Genre;
      original.Grouping = backup.Grouping;
      original.InvolvedPeople = backup.InvolvedPeople;
      original.Lyrics = backup.Lyrics;
      original.MediaType = backup.MediaType;
      original.MusicBrainzArtistId = backup.MusicBrainzArtistId;
      original.MusicBrainzDiscId = backup.MusicBrainzDiscId;
      original.MusicBrainzReleaseArtistId = backup.MusicBrainzReleaseArtistId;
      original.MusicBrainzReleaseCountry = backup.MusicBrainzReleaseCountry;
      original.MusicBrainzReleaseGroupId = backup.MusicBrainzReleaseGroupId;
      original.MusicBrainzReleaseId = backup.MusicBrainzReleaseId;
      original.MusicBrainzReleaseStatus = backup.MusicBrainzReleaseStatus;
      original.MusicBrainzTrackId = backup.MusicBrainzTrackId;
      original.MusicBrainzReleaseType = backup.MusicBrainzReleaseType;
      original.MusicCreditList = backup.MusicCreditList;
      original.OfficialAudioFileInformation = backup.OfficialAudioFileInformation;
      original.OfficialArtistInformation = backup.OfficialArtistInformation;
      original.OfficialAudioSourceInformation = backup.OfficialAudioSourceInformation;
      original.OfficialInternetRadioInformation = backup.OfficialInternetRadioInformation;
      original.OfficialPaymentInformation = backup.OfficialPaymentInformation;
      original.OfficialPublisherInformation = backup.OfficialPublisherInformation;
      original.OriginalAlbum = backup.OriginalAlbum;
      original.OriginalFileName = backup.OriginalFileName;
      original.OriginalLyricsWriter = backup.OriginalLyricsWriter;
      original.OriginalArtist = backup.OriginalArtist;
      original.OriginalOwner = backup.OriginalOwner;
      original.OriginalRelease = backup.OriginalRelease;
      original.Publisher = backup.Publisher;
      original.Pictures = backup.Pictures;
      original.Rating = backup.Rating;
      original.Ratings = backup.Ratings;
      original.ReplayGainTrack = backup.ReplayGainTrack;
      original.ReplayGainTrackPeak = backup.ReplayGainTrackPeak;
      original.ReplayGainAlbum = backup.ReplayGainAlbum;
      original.ReplayGainAlbumPeak = backup.ReplayGainAlbumPeak;
      original.SubTitle = backup.SubTitle;
      original.TextWriter = backup.TextWriter;
      original.Title = backup.Title;
      original.TitleSortName = backup.TitleSortName;
      original.Track = backup.Track;
      original.TrackLength = backup.TrackLength;
      original.Year = backup.Year;
      original.UserFrames = backup.UserFrames;
    }

    /// <summary>
    /// Handle Drag & Drop on Ratings Grid
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RatingsGrid_OnDropped(object sender, GridRowDroppedEventArgs e)
    {
      if (e.DropPosition != DropPosition.None)
      {
        // Get Dragging records
        ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;

        // Gets the TargetRecord from the underlying collection using record index of the TargetRecord (e.TargetRecord)
        var targetRecord = SongEdit.Ratings[(int)e.TargetRecord];

        // Use Batch update to avoid data operations in SfDataGrid during records removing and inserting
        RatingsGrid.BeginInit();

        // Removes the dragging records from the underlying collection
        foreach (var item in draggingRecords)
        {
          SongEdit.Ratings.Remove(item as PopmFrame);
        }

        // Find the target record index after removing the records
        int targetIndex = SongEdit.Ratings.IndexOf(targetRecord);
        int insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex : targetIndex + 1;
        insertionIndex = insertionIndex < 0 ? 0 : insertionIndex;

        // Insert dragging records to the target position
        for (int i = draggingRecords.Count - 1; i >= 0; i--)
        {
          SongEdit.Ratings.Insert(insertionIndex, draggingRecords[i] as PopmFrame);
        }
        RatingsGrid.EndInit();
      }
    }


    #endregion

    #region Interface

    /// <summary>
    /// This method is invoked, if a song is selected in the Songgrid
    /// </summary>
    /// <param name="navigationContext"></param>
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
      log.Trace(">>>");
      _isInitializing = true;
      RatingsGrid.RowDragDropController.Dropped += RatingsGrid_OnDropped;

      MultiCheckBoxVisibility = false;
      _songs = navigationContext.Parameters["songs"] as List<SongData>;

      ClearForm();
      if (_songs.Count > 0)
      {
        SetFormBindings(ref _songs); //Set the bindings so that the data is displayed in the View
        IsEnabled = true;
        IsApplyButtonEnabled = false;
      }
      else
      {
        IsEnabled = false;
      }
      log.Trace("<<<");
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
      return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
      // Clear the view
      RatingsGrid.RowDragDropController.Dropped -= RatingsGrid_OnDropped;
    }

    public void OnFileDrop(string[] filepaths)
    {
      log.Trace(">>>");
      if (filepaths.Length > 0)
      {
        var pic = new Picture(filepaths[0]) { Type = PictureType.FrontCover };
        UpdatePictures(pic);
      }
      log.Trace("<<<");
    }

    public void OnHtmlDrop(object html)
    {
      log.Trace(">>>");
      var fragment = Util.ExtractHtmlFragmentFromClipboardData((string)html);
      if (fragment.StartsWith("<img"))
      {
        var start = fragment.IndexOf("src=\"", StringComparison.Ordinal);
        if (start > -1)
        {
          start = start + "src=\"".Length;
          var url = fragment.Substring(start, fragment.IndexOf('"', start) - start);
          var pic = new Picture { Type = PictureType.FrontCover };
          if (pic.ImageFromUrl(url))
          {
            UpdatePictures(pic);
          }
        }
      }
      log.Trace("<<<");
    }

    private void UpdatePictures(Picture pic)
    {
      log.Trace(">>>");
      FrontCover = pic.ImageFromPic();
      foreach (var song in _songs)
      {
        if (CkRemovePicturesIsChecked)
        {
          song.Pictures.Clear();
        }
        song.Pictures.Insert(0, pic);
      }
      log.Trace("<<<");
    }

    #endregion

    #region Event Handling

    private void OnMessageReceived(GenericEvent msg)
    {
      switch (msg.Action.ToLower())
      {
        // Clear the Tag Edit Panel on Folder change
        case "selectedfolderchanged":
          _isInitializing = true;
          ClearForm();
          IsEnabled = false;
          _isInitializing = false;
          break;

        // Covers have been changed because of a Cover Search
        // Update the FrontCover
        case "coverschanged":
          FrontCover = _songs[0].FrontCover;
          if (MultiCheckBoxVisibility)
          {
            CkPicturesIsChecked = true;
          }
          break;

        case "command":
          var command = (Action.ActionType)msg.MessageData["command"];
          if (command == Action.ActionType.REFRESH)
          {
            log.Trace($"Command {command}");
            _isInitializing = true;
            ClearForm();
            IsEnabled = false;
            _isInitializing = false;
          }
          break;
      }
    }

    #endregion
  }
}
