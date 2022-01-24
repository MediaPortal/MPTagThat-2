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

using System.Threading.Tasks;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Utils;

#endregion

namespace MPTagThat.SongGrid.Commands
{
  [SupportedCommandType("ValidateMP3File")]
  public class CmdValidateMp3File : Command
  {

    #region ctor
    public CmdValidateMp3File(object[] parameters)
    {
    }

    #endregion


    #region Command Implementation

    public override async Task<(bool Changed, SongData song)> Execute(SongData song)
    {
      if (song.IsMp3)
      {
        Util.StatusCurrentFile($"Validating file {song.FullFileName}");
        log.Debug($"ValidateMp3: Validating file {song.FullFileName}");

        string strError;
        song.MP3ValidationError = Mp3Val.ValidateMp3File(song.FullFileName, out strError);
        song.StatusMsg = strError;
        song.Status = song.MP3ValidationError != Util.MP3Error.NoError ? 3 : 0;
      }

      return (false, song);
    }

    #endregion
  }
}
