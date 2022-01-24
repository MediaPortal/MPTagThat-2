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

using System.Text;

#endregion

namespace MPTagThat.Core.GnuDB
{
  /// <summary>
  ///   Contains Information about a CD
  /// </summary>
  public class CDInfo
  {
    public CDInfo() {}

    public CDInfo(string discid, string category, string title)
    {
      DiscId = discid;
      Category = category;
      Title = title;
    }

    public string Category { get; set; }

    public string DiscId { get; set; }

    public string Title { get; set; }

    public override string ToString()
    {
      StringBuilder buff = new StringBuilder(100);
      buff.Append("DiscId: ");
      buff.Append(DiscId);
      buff.Append("; Category: ");
      buff.Append(Category);
      buff.Append("; Title: ");
      buff.Append(Title);
      return buff.ToString();
    }
  }
}
