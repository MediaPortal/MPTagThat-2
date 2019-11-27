using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace MPTagThat.TagEdit.ViewModels
{
  public class TagEditViewModel : BindableBase, INavigationAware
  {
    #region Variables

    #endregion

    #region Properties

    private SongData _songEdit;
    public SongData SongEdit
    {
      get => _songEdit;
      set => SetProperty(ref _songEdit, value);
    }

    private bool _multiCheckBoxVisibility;
    public bool MultiCheckBoxVisibility
    {
      get => _multiCheckBoxVisibility;
      set => SetProperty(ref _multiCheckBoxVisibility, value);
    }

    // Check Box Checked properties
    public bool CkTrackIsChecked { get; set; }
    public bool CkDiskIsChecked { get; set; }
    public bool CkArtistIsChecked { get; set; }
    public bool CkAlbumArtistIsChecked { get; set; }
    public bool CkGenreIsChecked { get; set; }
    public bool CkAlbumIsChecked { get; set; }
    public bool CkTitleIsChecked { get; set; }
    public bool CkYearIsChecked { get; set; }
    public bool CkBPMIsChecked { get; set; }
    public bool CkConductorIsChecked { get; set; }
    public bool CkComposerIsChecked { get; set; }
    public bool CkInterpretedByIsChecked { get; set; }
    public bool CkTextWriterIsChecked { get; set; }
    public bool CkPublisherIsChecked { get; set; }
    public bool CkEncodedByIsChecked { get; set; }
    public bool CkCopyrightIsChecked { get; set; }
    public bool CkContentGroupIsChecked { get; set; }
    public bool CkSubTitleIsChecked { get; set; }
    public bool CkArtistSortIsChecked { get; set; }
    public bool CkAlbumSortIsChecked { get; set; }
    public bool CkTitleSortIsChecked { get; set; }
    public bool CkOriginalAlbumIsChecked { get; set; }
    public bool CkOriginalArtistIsChecked { get; set; }
    public bool CkOriginalFileNameIsChecked { get; set; }
    public bool CkOriginalLyricsWriterIsChecked { get; set; }
    public bool CkOriginalOwnerIsChecked { get; set; }
    public bool CkOriginalReleaseIsChecked { get; set; }
    public bool CkCopyrightUrlIsChecked { get; set; }
    public bool CkOfficialAudioFileUrlIsChecked { get; set; }
    public bool CkOfficialArtistUrlIsChecked { get; set; }
    public bool CkOfficialAudioSourceUrlIsChecked { get; set; }
    public bool CkOfficialInternetRadioUrlIsChecked { get; set; }
    public bool CkOfficialPaymentUrlIsChecked { get; set; }
    public bool CkCommercialInformationUrlIsChecked { get; set; }
    public bool CkOfficialPublisherUrlIsChecked { get; set; }
    public bool CkInvolvedPersonIsChecked { get; set; }
    public bool CkInvolvedMusicianIsChecked { get; set; }
    public bool CkRemoveExistingRatingsIsChecked { get; set; }
    public bool CkRemoveLyricsIsChecked { get; set; }
    public bool CkMediaTypeIsChecked { get; set; }
    public bool CkTrackLengthIsChecked { get; set; }

    #endregion

    #region ctor

    public TagEditViewModel()
    {
      _applyEditCommand = new BaseCommand(ApplyEdit);
    }

    #endregion

    #region Commands

    private ICommand _applyEditCommand;
    public ICommand ApplyEditCommand => _applyEditCommand;

    private void ApplyEdit(object param)
    {
      var evt = new GenericEvent
      {
        Action = "applytagedit"
      };
      evt.MessageData.Add("tags", (SongData)param);
      EventSystem.Publish(evt);
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
        SongEdit = songs[0];
        return;
      }

      // We have multiple Songs selected, so show the Checkboxes and
      // decide if they shoud be checked.
      MultiCheckBoxVisibility = true;

      SongEdit = new SongData();
      var i = 0;
      foreach (var song in songs)
      {
        if (SongEdit.Artist != song.Artist)
        {
          SongEdit.Artist = i == 0 ? song.Artist : "";
        }

        i++;
      }


    }

    #endregion


    #region Interface

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
      //MultiCheckBoxVisibility = false;
      var songs = navigationContext.Parameters["songs"] as List<SongData>;

      if (songs.Count > 0)
      {
        SetFormBindings(ref songs);
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
