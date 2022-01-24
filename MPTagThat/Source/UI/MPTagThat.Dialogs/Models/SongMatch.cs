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

using Prism.Mvvm;

#endregion

namespace MPTagThat.Dialogs.Models
{
  /// <summary>
  /// Song Matches when Tagging from Internet
  /// </summary>
  public class SongMatch : BindableBase
  {
    #region Properties

    public int PositionInSongList { get; set; }

    private uint _trackNumber;

    public uint TrackNumber
    {
      get => _trackNumber; 
      set => SetProperty(ref _trackNumber, value);
    }

    private string _title;

    public string Title
    {
      get => _title;
      set => SetProperty(ref _title, value);
    }

    public string FileName { get; set; }

    public string Duration { get; set; }

    /// <summary>
    /// Did we have a match on Tag From Internet
    /// </summary>
    private bool _matched = false;

    public bool Matched
    {
      get => _matched;
      set => SetProperty(ref _matched, value);
    }


    #endregion
  }
}
