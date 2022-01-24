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

using System;

namespace MPTagThat.Core.Common.Song
{
  [Serializable]
  public class Comment
  {
    #region Ctor

    public Comment() { }

    public Comment(Comment copyFrom)
    {
      this.Description = copyFrom.Description;
      this.Language = copyFrom.Language;
      this.Text = copyFrom.Text;
    }

    public Comment(string desc, string lang, string text)
    {
      Description = desc;
      if (lang.Length > 3)
        Language = lang.Substring(0, 3);
      else
        Language = lang;

      Text = text;
    }

    #endregion

    #region Properties

    public string Description { get; set; }

    public string Language { get; set; }

    public string Text { get; set; }

    #endregion
  }
}
