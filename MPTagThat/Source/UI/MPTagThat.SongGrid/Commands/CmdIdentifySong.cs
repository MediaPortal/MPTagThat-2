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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Documents;
using AcoustID;
using AcoustID.Web;
using MPTagThat.Core.Common.Song;
using MPTagThat.Dialogs.ViewModels;
using Prism.Services.Dialogs;
using Un4seen.Bass;
using Release = Hqub.MusicBrainz.API.Entities.Release;
using Recording = Hqub.MusicBrainz.API.Entities.Recording;
using System.Windows.Threading;
using System.Threading;

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
    public Release _release;

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
      var trackIds = await GetTrackIds(song.FullFileName);
      if (trackIds.Count == 0)
      {
        log.Info("Auto Tag: Couldn't identify file");
        return (false, song);
      }

      DialogService.ShowDialogInAnotherWindow("IdentifySongView", "DialogWindowView", new DialogParameters(), null);

      if (trackIds.Count > 0)
      {
        if (trackIds.Count == 1)
        {
          var releases = new List<Release>();
          foreach (var recording in trackIds[0].Recordings)
          {
            releases.AddRange(await GetReleases(recording));
          }

          var release = releases.FirstOrDefault(r => (r.Country != null && r.Country.ToLower() == "de"));
          if (release == null)
          {
            // Look for European wide release
            release = releases.FirstOrDefault(r => (r.Country != null && r.Country.ToLower() == "xe"));
            if (release == null)
            {
              // Look for US release
              release = releases.FirstOrDefault(r => (r.Country != null && r.Country.ToLower() == "us"));
              if (release == null)
              {
                release = releases.Count > 0 ? releases[0] : null;
              }
            }
          }

          if (release != null)
          {
            var album = await GetAlbum(release.Id);
            if (album != null)
            {
            }
          }


        }
      }


      return (false, song);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Create a Fingerprint and lookup 
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private async Task<List<LookupResult>> GetTrackIds(string file)
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

      var result = await lookupSvc.GetAsync(fingerPrint, Convert.ToInt32(time), new[] { "recordingids" });

      return result.Results;
    }

    /// <summary>
    /// Get the releases for the Recording
    /// </summary>
    /// <param name="recordingid"></param>
    /// <returns></returns>
    private async Task<List<Release>> GetReleases(AcoustID.Web.Recording recordingid)
    {
      var recording = await Recording.GetAsync(recordingid.Id, new[] { "artists", "releases", "media", "discids" });
      return recording.Releases;
    }

    private async Task<Release> GetAlbum(string releaseID)
    {
      var release = await Release.GetAsync(releaseID, new[] { "recordings" });
      return release;
    }

    #endregion
  }
}
