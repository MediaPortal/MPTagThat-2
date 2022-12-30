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

#endregion

namespace MPTagThat.SongGrid.Commands
{
  [SupportedCommandType("CaseConversion")]
  public class CmdCaseConversion : Command
  {
    #region Variables

    public object[] Parameters { get; private set; }

    #endregion

    #region ctor

    public CmdCaseConversion(object[] parameters)
    {
      Parameters = parameters;
    }

    #endregion

    #region Command Implementation

    public override Task<(bool Changed, SongData song)> Execute(SongData song)
    {
      if (CaseConversion.CaseConvert(ref song))
      {
        return Task.FromResult((true, song));
      }

      return Task.FromResult((false, song));
    }

    #endregion
  }
}
