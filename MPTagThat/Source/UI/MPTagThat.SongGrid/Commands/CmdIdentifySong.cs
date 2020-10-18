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

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using AcoustID;
using AcoustID.Web;
using Hqub.MusicBrainz.API.Entities;
using MPTagThat.Core.Common.Song;
using MPTagThat.Dialogs.ViewModels;
using Prism.Services.Dialogs;
using Un4seen.Bass;
using Release = Hqub.MusicBrainz.API.Entities.Release;
using Recording = Hqub.MusicBrainz.API.Entities.Recording;
using MPTagThat.Dialogs.Models;

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
    private Picture _pic;

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
      var recordings = await GetRecordings(song.FullFileName);
      if (recordings.Count == 0)
      {
        log.Info("Auto Tag: Couldn't identify song");
        return (false, song);
      }

      var releases = new List<Release>();
      // We might get back a lot of Recordings, so condense the list to recordings, which have
      // the same duration
      var tmpRecordings = new List<MusicBrainzRecording>();
      foreach (var recording in recordings)
      {
        if (recording.Length != null)
        {
          var timeDiff = Math.Abs(song.DurationTimespan.TotalMilliseconds / 1000 - (int)recording.Length / 1000);
          if (timeDiff <= 5)
          {
            foreach (var release in recording.Releases)
            {
              releases.Add(release);
              var mbRecording = new MusicBrainzRecording
              {
                Id = recording.Id,
                TrackId = release.Media[0].Tracks[0].Id,
                Title = recording.Title,
                Duration = $"{TimeSpan.FromMilliseconds((int) recording.Length).Hours:D2}:{TimeSpan.FromMilliseconds((int) recording.Length).Minutes:D2}:{TimeSpan.FromMilliseconds((int) recording.Length).Seconds:D2}",
                AlbumId = release.Id,
                ArtistId = (recording.Credits != null && recording.Credits.Count > 0) ? recording.Credits[0].Artist.Id : "",
                AlbumTitle = release.Title,
                Country = release.Country,
                Date = release.Date
              };
              mbRecording.Artist = JoinArtists(recording.Credits);
              tmpRecordings.Add(mbRecording);
            }
          }
        }
      }

      var selectedRecording = new MusicBrainzRecording();
      var albumFound = false;
      // We have already a Album from a previous search. Check,is this is found in the
      // releases from this song
      if (_album != null)
      {
        var release = releases.FirstOrDefault(r => r.Id == _album.Id);
        if (release != null && release.Id != String.Empty)
        {
          selectedRecording = tmpRecordings.First(r => r.AlbumId == release.Id);
          _album = release;
          albumFound = true;
        }
      }

      if (!albumFound)
      {
        // And now we remove duplicate Recordings and Countries
        var condensedRecordings = tmpRecordings
          .GroupBy(r => new {r.AlbumTitle, r.Country})
          .Select(g => g.First())
          .ToList();

        var dialogResult = ButtonResult.None;
        var parameters = new DialogParameters {{"recordings", condensedRecordings}};
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
        if (!albumFound)
        {
          _album = await GetAlbum(selectedRecording.AlbumId);
        }

        song.Title = selectedRecording.Title;
        song.Artist = selectedRecording.Artist;
        song.AlbumArtist = _album.Credits != null ? JoinArtists(_album.Credits) : "";
        song.Album = _album.Title;
        if (_album.Date != null && _album.Date.Length >= 4)
        {
          song.Year = Convert.ToInt32(_album.Date.Substring(0, 4));
        }

        if (_album.Media != null && _album.Media.Count > 0)
        {
          song.DiscNumber = (uint)_album.Media[0].Position;
          song.TrackCount = (uint)_album.Media[0].TrackCount;
          song.TrackNumber = (uint)_album.Media[0].Tracks.First(t => t.Id == selectedRecording.TrackId).Position;
          song.MusicBrainzDiscId = _album.Media[0].Discs != null ? _album.Media[0].Discs[0].Id : "";
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
    private async Task<List<Recording>> GetRecordings(string file)
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

      //var result = await lookupSvc.GetAsync(fingerPrint, Convert.ToInt32(time), new[] { "recordingids", "releases", "artists" });
      var trackIds = await lookupSvc.GetAsync(fingerPrint, Convert.ToInt32(time), new[] { "recordingids" });

      var recordings = new List<Recording>();
      foreach (var trackId in trackIds.Results)
      {
        foreach (var rec in trackId.Recordings)
        {
          System.Threading.Thread.Sleep(400);
          var recording = await Recording.GetAsync(rec.Id, new[] {"releases", "artists", "media", "discids"});
          recordings.Add(recording);
          
        }
      }
      return recordings;
    }

    private async Task<Release> GetAlbum(string releaseID)
    {
      var release = await Release.GetAsync(releaseID, new[] { "recordings" });
      return release;
    }

    private string JoinArtists(List<NameCredit> credits)
    {
      var joinedArtist = "";
      var firstElement = true;

      foreach (var credit in credits)
      {
        if (!firstElement)
        {
          joinedArtist += $"; {credit.Artist.Name}";
        }
        else
        {
          joinedArtist = credit.Artist.Name;
          firstElement = false;
        }
      }

      return joinedArtist;
    }

    #endregion
  }
}
