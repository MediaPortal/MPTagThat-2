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
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FreeImageAPI;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using Prism.Mvvm;
using Prism.Regions;

#endregion

namespace MPTagThat.TagEdit.ViewModels
{
  public class TagEditViewModel : BindableBase, INavigationAware
  {
    #region Variables

    private List<SongData> _songs = null;

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

    // Check Box Checked properties
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
    public bool CkTrackLengthIsChecked { get => _ckTrackLengthIsChecked; set => SetProperty(ref _ckTrackLengthIsChecked, value); }

    #endregion

    #region ctor

    public TagEditViewModel()
    {
      _applyEditCommand = new BaseCommand(ApplyEdit);
      _textChangedCommand = new BaseCommand(TextChaged);
    }

    #endregion

    #region Commands

    private ICommand _textChangedCommand;
    public ICommand TextChangedCommand => _textChangedCommand;

    /// <summary>
    /// Callback from the View when a Text is changed, it should check the checkbox
    /// </summary>
    /// <param name="param"></param>
    private void TextChaged(object param)
    {
      if (!MultiCheckBoxVisibility)
      {
        return;
      }

      var tb = (TextBox)(param as TextChangedEventArgs).Source;
      if (tb.Name.ToLower() == "tracknumber") CkTrackIsChecked = true;
      else if (tb.Name.ToLower() == "trackcount") CkTrackIsChecked = true;
      else if (tb.Name.ToLower() == "discnumber") CkDiscIsChecked = true;
      else if (tb.Name.ToLower() == "disccount") CkDiscIsChecked = true;
      else if (tb.Name.ToLower() == "artist") CkArtistIsChecked = true;
      else if (tb.Name.ToLower() == "albumartist") CkAlbumArtistIsChecked = true;

    }

    /// <summary>
    /// Invoked by the Apply Changes button in the View
    /// Loop through the the selected songs and apply the changes.
    /// </summary>
    private ICommand _applyEditCommand;
    public ICommand ApplyEditCommand => _applyEditCommand;

    private void ApplyEdit(object param)
    {
      var songEdit = (SongData)param;

      if (_songs == null)
      {
        return;
      }

      foreach (var song in _songs)
      {
        song.Changed = true;
        song.Artist = songEdit.Artist;
      }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Set the bindings for the Controls in the Tagedit Form
    /// </summary>
    /// <param name="songs"></param>
    private void SetFormBindings(ref List<SongData> songs)
    {
      if (songs.Count == 1)
      {
        UncheckCheckboxes();
        SongEdit = songs[0];
        GetFrontCover(SongEdit);
        return;
      }

      SongEdit = new SongData();
      var i = 0;
      FrontCover = null;
      byte[] picData = new byte[] { };
      foreach (var song in songs)
      {
        if (SongEdit.Artist != song.Artist)
        {
          SongEdit.Artist = i == 0 ? song.Artist : "";
        }

        if (song.Pictures.Count > 0)
        {
          if (!song.Pictures[0].Data.SequenceEqual(picData))
          {
            if (i == 0)
            {
              GetFrontCover(song);
              picData = song.Pictures[0].Data;
            }
            else
            {
              FrontCover = null;
            }
          }
        }


        i++;
      }

      // We have multiple Songs selected, so show the Checkboxes and
      // decide if they shoud be checked.
      MultiCheckBoxVisibility = true;
    }

    /// <summary>
    /// Get the Picture out of the file and set the FrontCover property for the Binding in the View
    /// </summary>
    /// <param name="song"></param>
    private void GetFrontCover(SongData song)
    {
      if (song.Pictures.Count > 0)
      {
        var data = song.Pictures[0].Data;
        FreeImageBitmap img = null;
        try
        {
          MemoryStream ms = new MemoryStream(data);
          img = new FreeImageBitmap(ms);
          
          var bitmapImage = new BitmapImage();
          using (var memory = new MemoryStream())
          {
            img.Save(memory, FREE_IMAGE_FORMAT.FIF_PNG);
            memory.Position = 0;


            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            img.Dispose();
            FrontCover = bitmapImage;
          }

        }
        catch (Exception)
        {
        }
      }
    }

    /// <summary>
    /// Changes the Checkboxes behind the input fields to visible / Invisble
    /// </summary>
    /// <param name="visible"></param>
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
    }

    #endregion


    #region Interface

    /// <summary>
    /// This metzhod is invoked, if a song is selected in the Songgrid
    /// </summary>
    /// <param name="navigationContext"></param>
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
      MultiCheckBoxVisibility = false;
      _songs = navigationContext.Parameters["songs"] as List<SongData>;

      if (_songs.Count > 0)
      {
        SetFormBindings(ref _songs); //Set the bindings so that the data is displayed in the View
      }
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
      return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
      // Clear the view
    }

    #endregion
  }
}
