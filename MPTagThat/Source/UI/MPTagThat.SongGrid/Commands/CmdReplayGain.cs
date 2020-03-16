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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPTagThat.Core.Common.Song;
using NReplayGain;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;

#endregion

namespace MPTagThat.SongGrid.Commands
{
  /// <summary>
  /// This command calculates the replay gain of a song and album
  /// </summary>
  [SupportedCommandType("ReplayGain")]
  public class CmdReplayGain : Command
  {
    #region Variables

    public object[] Parameters { get; private set; }
    private AlbumGain _albumGain;

    #endregion

    #region ctor
    public CmdReplayGain(object[] parameters)
    {
      NeedsPostprocessing = true;
      Parameters = parameters;
      var commandParmObj = (object[])parameters[1];
      if (commandParmObj.Length > 0)
      {
        if ((string) commandParmObj[0] == "AlbumGain")
        {
          _albumGain = new AlbumGain();
        }
      }
    }

    #endregion

    #region Command Implementation

    public override async Task<(bool Changed, SongData song)> Execute(SongData song)
    {
      int stream = Bass.BASS_StreamCreateFile(song.FullFileName, 0, 0, BASSFlag.BASS_STREAM_DECODE);
      if (stream == 0)
      {
        log.Error("ReplayGain: Could not create stream for {0}. {1}", song.FullFileName, Bass.BASS_ErrorGetCode().ToString());
        return (false, song);
      }

      BASS_CHANNELINFO chInfo = Bass.BASS_ChannelGetInfo(stream);
      if (chInfo == null)
      {
        log.Error("ReplayGain: Could not get channel info for {0}. {1}", song.FullFileName, Bass.BASS_ErrorGetCode().ToString());
        return (false, song);
      }
      var trackGain = new TrackGain(chInfo.freq, 16);

      var leftSamples = new List<int>();
      var rightSamples = new List<int>();

      var bufLen = 1024;
      var buf = new short[bufLen];

      while (true)
      {
        int length = Bass.BASS_ChannelGetData(stream, buf, bufLen * sizeof(short));
        if (length == -1) break;

        for (int i = 0; i < length / sizeof(short); i += 2)
        {
          leftSamples.Add(Convert.ToInt32(buf[i]));
          rightSamples.Add(Convert.ToInt32(buf[i + 1]));
        }
      }

      Bass.BASS_StreamFree(stream);

      trackGain.AnalyzeSamples(leftSamples.ToArray(), rightSamples.ToArray());

      _albumGain?.AppendTrackData(trackGain);

      double gain = Math.Round(trackGain.GetGain(), 2, MidpointRounding.ToEven);
      double peak = Math.Round(trackGain.GetPeak(), 2, MidpointRounding.ToEven);
      song.ReplayGainTrack = gain.ToString(CultureInfo.InvariantCulture);
      song.ReplayGainTrackPeak = peak.ToString(CultureInfo.InvariantCulture);

      return (true, song);
    }

    public override bool PostProcess(SongData song)
    {
      if (_albumGain != null)
      {
        song.ReplayGainAlbum = Math.Round(_albumGain.GetGain(), 2, MidpointRounding.ToEven).ToString(CultureInfo.InvariantCulture);
        song.ReplayGainAlbumPeak = Math.Round(_albumGain.GetPeak(), 2, MidpointRounding.ToEven).ToString(CultureInfo.InvariantCulture);
        return true;
      }
      return false;
    }

    #endregion


  }
}
