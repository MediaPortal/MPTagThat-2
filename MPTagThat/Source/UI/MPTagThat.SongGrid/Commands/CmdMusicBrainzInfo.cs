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

#endregion

using System.Linq;
using System.Threading.Tasks;
using MPTagThat.Core.Common.Song;
using Hqub.MusicBrainz.API.Entities;

namespace MPTagThat.SongGrid.Commands
{
  /// <summary>
  /// Get Song Information from MusicBrainz
  /// </summary>
  [SupportedCommandType("MusicBrainzInfo")]
  public class CmdMusicBrainzInfo : Command
  {
    #region Variables

    public object[] Parameters { get; private set; }

    #endregion

    #region ctor

    public CmdMusicBrainzInfo(object[] parameters)
    {
      Parameters = parameters;
    }

    #endregion

    #region Command Implementation

    public override async Task<(bool Changed, SongData song)> Execute(SongData song)
    {
      log.Info($"MusicBrainz Info: Processing file: {song.FullFileName}");
      var query = $"artist:\"{SwitchArtist(song.Artist)}\" AND release:\"{song.Album}\" AND recording:\"{song.Title}\"";
      var recordings = await Recording.SearchAsync(query);
      if (recordings.Count == 0)
      {
        log.Info("MusicBrainz Info: Couldn't find information for song");
        return (false, song);
      }

      var recording = recordings.Items[0];
      song.MusicBrainzTrackId = recording.Id;
      if (recording.Credits.Count > 0)
      {
        song.MusicBrainzArtistId = recording.Credits[0].Artist.Id;
      }

      Release release = null;
      foreach (var country in options.MainSettings.PreferredMusicBrainzCountries)
      {
        release = recording.Releases.FirstOrDefault(r => r.Country == country);
        if (release != null)
        {
          break;
        }
      }
      
      if (release == null && recording.Releases.Count > 0)
      {
        release = recording.Releases[0];
      }

      song.MusicBrainzReleaseCountry = release.Country;
      song.MusicBrainzReleaseId = release.Id;
      song.MusicBrainzReleaseStatus = release.Status;

      if (release.Credits.Count > 0)
      {
        song.MusicBrainzReleaseArtistId = release.Credits[0].Artist.Id;
      }

      if (release.ReleaseGroup != null)
      {
        song.MusicBrainzReleaseGroupId = release.ReleaseGroup.Id;
        song.MusicBrainzReleaseType = release.ReleaseGroup.PrimaryType;
      }

      return (true, song);
    }

    /// <summary>
    /// Switches the Artist, if it is separated with a "colon"
    /// </summary>
    /// <param name="artist"></param>
    /// <returns></returns>
    private string SwitchArtist(string artist)
    {
      int iPos = artist.IndexOf(',');
      if (iPos > 0)
      {
        artist = $"{artist.Substring(iPos + 2)} {artist.Substring(0, iPos)}";
      }
      return artist;
    }

    #endregion
  }
}
