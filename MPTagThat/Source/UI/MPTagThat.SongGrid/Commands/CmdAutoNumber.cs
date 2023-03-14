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

using MPTagThat.Core;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
using System.Threading.Tasks;


namespace MPTagThat.SongGrid.Commands
{
  [SupportedCommandType("AutoNumber")]
  public class CmdAutoNumber : Command
  {
    #region ctor

    public CmdAutoNumber(object[] param)
    {
      NeedsCallback = true;
    }

    #endregion

    #region Command Implementation

    public override Task<(bool Changed, SongData song)> Execute(SongData song)
    {
      song.TrackNumber = (uint)options.AutoNumber;
      options.AutoNumber++;
      return Task.FromResult((true, song));
    }

    public override void CmdCallback()
    {
      GenericEvent evt = new GenericEvent
      {
        Action = "autonumberchanged"
      };
      EventSystem.Publish(evt);
    }

    #endregion
  }
}
