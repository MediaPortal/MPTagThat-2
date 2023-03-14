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

namespace MPTagThat.Core.GnuDB
{
  /// <summary>
  ///   Contains Information about Tracks
  /// </summary>
  public class CDTrackDetail
  {
    private int _duration;
    private string _durationString;

    public CDTrackDetail() { }

    public CDTrackDetail(string songTitle, string artist, string extt,
                         int trackNumber, int offset, int duration)
    {
      Title = songTitle;
      Artist = artist;
      EXTT = extt;
      Track = trackNumber;
      Offset = offset;
      _duration = duration;
      _durationString = $"{(_duration / 60).ToString().PadLeft(2, '0')}:{(_duration % 60).ToString().PadLeft(2, '0')}";
    }

    public string Title { get; set; }

    // can be null if the artist is the same as the main
    // album
    public string Artist { get; set; }


    public int Track { get; set; }

    public int DurationInt
    {
      get => _duration;
      set => _duration = value;
    }

    public string Duration => $"{(_duration / 60).ToString().PadLeft(2, '0')}:{(_duration % 60).ToString().PadLeft(2, '0')}";

    public int Offset { get; set; }

    public string EXTT { get; set; }
  }
}
