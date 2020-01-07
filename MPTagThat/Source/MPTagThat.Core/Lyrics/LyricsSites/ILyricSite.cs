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

namespace MPTagThat.Core.Lyrics.LyricsSites
{
  public enum LyricType
  {
    Lrc = 0,
    UnsyncedLyrics = 1
  }

  public enum SiteType
  {
    Api = 0,
    Scrapper = 1
  }

  public enum SiteComplexity
  {
    OneStep = 1,
    TwoSteps = 2
  }

  public enum SiteSpeed
  {
    Fast = 0,
    Medium = 1,
    Slow = 2,
    VerySlow = 3
  }

  public interface ILyricSite
  {
    string Name { get; }

    void FindLyrics();

    string Lyric { get; }

    LyricType GetLyricType();

    SiteType GetSiteType();

    SiteComplexity GetSiteComplexity();

    bool SiteActive();
  }
}
