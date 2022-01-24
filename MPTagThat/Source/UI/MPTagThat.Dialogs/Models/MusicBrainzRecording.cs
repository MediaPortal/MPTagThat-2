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

namespace MPTagThat.Dialogs.Models
{
  public class MusicBrainzRecording
  {
    #region Properties

    // Recording Related Properties
    public string Id { get; set; }

    public string TrackId { get; set; }

    public string Title { get; set; }

    public string Artist { get; set; }

    public string ArtistId { get; set; }

    public string Duration { get; set; }


    // Release / Album related Properties

    public string AlbumId { get; set; }

    public string AlbumTitle { get; set; }

    public string Country { get; set; }

    public string Date { get; set; }

    #endregion
  }
}
