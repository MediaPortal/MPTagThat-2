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

using System;
using System.Threading.Tasks;
using MPTagThat.Core.Common.Song;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

#endregion


namespace MPTagThat.SongGrid.Commands
{
  [SupportedCommandType("Bpm")]
  public class CmdBpm : Command
  {
    #region Variables

    private BPMPROGRESSPROC _bpmProc;

    #endregion

    #region ctor

    public CmdBpm(object[] parameters)
    {

    }

    #endregion

    #region Command Implementation

    public override Task<(bool Changed, SongData song)> Execute(SongData song)
    {
      int stream = Bass.BASS_StreamCreateFile(song.FullFileName, 0, 0, BASSFlag.BASS_STREAM_DECODE);
      if (stream == 0)
      {
        log.Error("BPM: Could not create stream for {0}. {1}", song.FullFileName, Bass.BASS_ErrorGetCode().ToString());
        return Task.FromResult((false, song));
      }

      _bpmProc = BpmProgressProc;

      double len = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream));
      float bpm = BassFx.BASS_FX_BPM_DecodeGet(stream, 0.0, len, 0, BASSFXBpm.BASS_FX_BPM_BKGRND | BASSFXBpm.BASS_FX_FREESOURCE | BASSFXBpm.BASS_FX_BPM_MULT2,
        _bpmProc, IntPtr.Zero);

      song.BPM = Convert.ToInt32(bpm);
      BassFx.BASS_FX_BPM_Free(stream);
      return Task.FromResult((true, song));
    }

    private void BpmProgressProc(int channel, float percent, IntPtr userData)
    {
    }

    #endregion
  }
}
