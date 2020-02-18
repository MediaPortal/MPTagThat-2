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

      if (trackIds.Count > 0)
      {
        var releases = new List<AcoustID.Web.Release>();
        foreach (var trackId in trackIds)
        {
          foreach (var recording in trackId.Recordings)
          {
            releases.AddRange(recording.Releases);
          }
        }

        var distinctReleases = releases
          .GroupBy(r => new {r.Title, r.Country})
          .Select(g => g.First())
          .ToList();

        if (distinctReleases.Count > 0)
        {

          var parameters = new DialogParameters {{"releases", distinctReleases}};

          DialogService.ShowDialogInAnotherWindow("IdentifySongView", "DialogWindowView", parameters, null);

        }


        /*
        if (release != null)
        {
          var album = await GetAlbum(release.Id);
          if (album != null)
          {
          }
        }
        */

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

      var result = await lookupSvc.GetAsync(fingerPrint, Convert.ToInt32(time), new[] { "recordingids", "releases" });

      return result.Results;
    }

    private async Task<Release> GetAlbum(string releaseID)
    {
      var release = await Release.GetAsync(releaseID, new[] { "recordings" });
      return release;
    }

    #endregion
  }
}
