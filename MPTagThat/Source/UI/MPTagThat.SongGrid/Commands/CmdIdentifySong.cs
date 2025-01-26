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

using AcoustID;
using AcoustID.Web;
using Hqub.MusicBrainz;
using Hqub.MusicBrainz.Entities;
using MPTagThat.Core.AlbumSearch;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Dialogs.Models;
using MPTagThat.Dialogs.ViewModels;
using Prism.Ioc;
using Prism.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Lifetime;
using System.Threading.Tasks;
using Un4seen.Bass;
using Recording = Hqub.MusicBrainz.Entities.Recording;
using Release = Hqub.MusicBrainz.Entities.Release;

#endregion

namespace MPTagThat.SongGrid.Commands
{
  /// <summary>
  /// Fingerprint the song and do a lookup at MusicBrainz
  /// </summary>
  [SupportedCommandType("IdentifySong")]
  public class CmdIdentifySong : Command
  {
    #region Variables

    public object[] Parameters { get; private set; }
    private Release _album;
    private MusicBrainzRecording _mbalbum; // The condensed album info from acoustid lookup
    private Picture _pic;
    private Options _options = ContainerLocator.Current.Resolve<ISettingsManager>().GetOptions;

    #endregion

    #region ctor
    public CmdIdentifySong(object[] parameters)
    {
      Parameters = parameters;
    }

    #endregion

    #region Command Implementation

    public override async Task<(bool Changed, SongData song)> Execute(SongData song)
    {
      log.Info($"Auto Tag: Processing file: {song.FullFileName}");
      var releases = await GetRecordings(song.FullFileName);
      if (releases.Count == 0)
      {
        log.Info("Auto Tag: Couldn't identify song");
        return (false, song);
      }

      var selectedRecording = new MusicBrainzRecording();
      var albumFound = false;
      // We have already a Album from a previous search. Check,is this is found in the
      // releases from this song
      if (_mbalbum != null)
      {
        var release = releases.FirstOrDefault(r => r.AlbumId == _mbalbum.AlbumId);
        if (release != null && release.Id != String.Empty)
        {
          selectedRecording = releases.First(r => r.AlbumId == release.AlbumId);
          _mbalbum = release;
          albumFound = true;
        }
      }

      if (!albumFound)
      {
        // And now we remove duplicate Recordings and Countries
        var condensedRecordings = releases
          .GroupBy(r => new { r.AlbumTitle, r.Country })
          .Select(g => g.First())
          .ToList();

        var dialogResult = ButtonResult.None;
        var parameters = new DialogParameters { { "recordings", condensedRecordings } };
        DialogService.ShowDialogInAnotherWindow("IdentifySongView", "DialogWindowView", parameters, r =>
        {
          dialogResult = r.Result;
          if (dialogResult == ButtonResult.OK)
          {
            r.Parameters.TryGetValue("selectedrecording", out selectedRecording);
          }
        });

        if (dialogResult == ButtonResult.Cancel)
        {
          return (false, song);
        }
        else if (dialogResult == ButtonResult.Abort)
        {
          return (false, null);
        }

      }

      if (selectedRecording.Id != string.Empty)
      {
        _mbalbum = selectedRecording;
        if (!albumFound)
        {
          _album = await GetAlbum(selectedRecording.AlbumId);
        }

        song.Title = selectedRecording.Title;
        song.Artist = selectedRecording.Artist;
        song.AlbumArtist = _album.Credits != null ? string.Join(";", _album.Credits.Select(n => n.Artist.Name)) : "";
        song.Album = _album.Title;
        if (_album.Date != null && _album.Date.Length >= 4)
        {
          song.Year = Convert.ToInt32(_album.Date.Substring(0, 4));
        }

        if (_album.Media != null && _album.Media.Count > 0)
        {
          song.DiscNumber = (uint)_album.Media[0].Position;
          song.TrackCount = (uint)_album.Media[0].TrackCount;
          var track = _album.Media[0].Tracks.FirstOrDefault(t => t.Id == selectedRecording.TrackId);
          song.TrackNumber = track == null ? (uint)0 : (uint)track.Position;
          song.MusicBrainzDiscId = (_album.Media[0].Discs != null && _album.Media[0].Discs.Count > 0) ? _album.Media[0].Discs[0].Id : "";
        }

        // MusicBrainz Properties
        song.MusicBrainzArtistId = selectedRecording.ArtistId;
        song.MusicBrainzReleaseId = selectedRecording.AlbumId;
        song.MusicBrainzTrackId = selectedRecording.TrackId;
        song.MusicBrainzReleaseCountry = selectedRecording.Country;
      }

      var coverArtUrl = _album.CoverArtArchive != null && _album.CoverArtArchive.Front
        ? string.Format(@"http://coverartarchive.org/release/{0}/front.jpg", _album.Id)
        : null;

      if (coverArtUrl != null)
      {
        _pic = new Picture();
        if (_pic.ImageFromUrl(coverArtUrl))
        {
          song.Pictures.Clear();
          song.Pictures.Add(_pic);
        }
      }

      return (true, song);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Create a Fingerprint and lookup the Recordings
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private async Task<List<MusicBrainzRecording>> GetRecordings(string file)
    {
      var stream = Bass.BASS_StreamCreateFile(file, 0, 0, BASSFlag.BASS_STREAM_DECODE);
      var chInfo = Bass.BASS_ChannelGetInfo(stream);

      var bufLen = (int)Bass.BASS_ChannelSeconds2Bytes(stream, 120.0);
      var buf = new short[bufLen];

      var chromaContext = new ChromaContext();
      chromaContext.Start(chInfo.freq, chInfo.chans);

      var length = Bass.BASS_ChannelGetData(stream, buf, bufLen);
      chromaContext.Feed(buf, length / 2);

      chromaContext.Finish();

      var fingerPrint = chromaContext.GetFingerprint();

      Configuration.ClientKey = "mfbgmu2P";
      var lookupSvc = new LookupService();

      var len = Bass.BASS_ChannelGetLength(stream, BASSMode.BASS_POS_BYTE);
      var time = Bass.BASS_ChannelBytes2Seconds(stream, len);
      Bass.BASS_StreamFree(stream);


      var releases = new List<MusicBrainzRecording>();
      
      var lookup = await lookupSvc.GetAsync(fingerPrint, Convert.ToInt32(time), new[] { "recordings", "releases", "tracks", "compress" });
      if (lookup.StatusCode != HttpStatusCode.OK)
      {
        return releases; // return an empty recording on failure
      }

      foreach (var trackId in lookup.Results)
      {
        foreach (var recording in trackId.Recordings)
        {
          if (recording.Title == "")
          {
            continue; // Ignore empty recordings returned
          }

          foreach (var release in recording.Releases)
          {
            // Create a Recording item and set the default values for the Releases
            var mbRecording = new MusicBrainzRecording
            {
              Id = recording.Id,
              Title = recording.Title,
              Duration = recording.Duration.ToString(),
              ArtistId = (recording.Artists != null && recording.Artists.Count > 0) ? recording.Artists[0].Id : "",
              Artist = string.Join(";", recording.Artists),
            };
            mbRecording.Country = release.Country;
            mbRecording.Date = release.Date.ToString();
            mbRecording.AlbumId = release.Id;
            mbRecording.AlbumTitle = release.Title;
            mbRecording.TrackCount = release.TrackCount;
            releases.Add(mbRecording);
          }
        }
      }
      return releases;
    }

    private async Task<Release> GetAlbum(string releaseID)
    {
      // Create a MusicBrainz client with TLS 1.2
      ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
      var client = new MusicBrainzClient()
      {
        Cache = new FileRequestCache(System.IO.Path.Combine(_options.ConfigDir, "cache"))
      };

      var release = await client.Releases.GetAsync(releaseID, new[] { "recordings" });
      return release;
    }

    #endregion
  }
}
