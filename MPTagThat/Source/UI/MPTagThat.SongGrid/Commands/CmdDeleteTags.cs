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

using MPTagThat.Core.Common.Song;
using System.Threading.Tasks;
using TagLib;

#endregion

namespace MPTagThat.SongGrid.Commands
{
  /// <summary>
  /// This command implements the removing of Tags from a song
  /// </summary>
  [SupportedCommandType("DeleteAllTags")]
  [SupportedCommandType("DeleteV1Tags")]
  [SupportedCommandType("DeleteV2Tags")]
  public class CmdDeleteTags : Command
  {
    #region Variables

    private TagLib.TagTypes _tagType;

    #endregion

    #region ctor

    public CmdDeleteTags(object[] parameters)
    {
      switch ((string)parameters[0])
      {
        case "DeleteAllTags":
          _tagType = TagTypes.AllTags;
          break;
        case "DeleteV1Tags":
          _tagType = TagTypes.Id3v1;
          break;
        case "DeleteV2Tags":
          _tagType = TagTypes.Id3v1;
          break;
        default:
          _tagType = TagTypes.AllTags;
          break;
      }
    }

    #endregion

    #region Command Implementation

    public override Task<(bool Changed, SongData song)> Execute(SongData song)
    {
      song.TagsRemoved.Add(_tagType);
      song = Song.ClearTag(song);
      return Task.FromResult((true, song));
    }

    #endregion

  }
}
