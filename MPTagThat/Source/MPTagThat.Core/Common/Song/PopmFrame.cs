﻿#region Copyright (C) 2022 Team MediaPortal
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

namespace MPTagThat.Core.Common
{
  public class PopmFrame
  {
    #region Variables

    public PopmFrame() { }

    public PopmFrame(PopmFrame copyFrom)
    {
      this.User = copyFrom.User;
      this.Rating = copyFrom.Rating;
      this.PlayCount = copyFrom.PlayCount;
    }

    public PopmFrame(string user, int rating, int playcount)
    {
      User = user;
      Rating = rating;
      PlayCount = playcount;
    }

    public string User { get; set; }

    public int Rating { get; set; }

    public int PlayCount { get; set; }

    #endregion
  }
}
