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

using System;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace MPTagThat.Core.Lyrics
{
  public class LyricUtil
  {
    private static readonly string[] CharsToDelete =
       {
        ".", ",", "&", "'", "!", "\"", "&", "?", "(", ")", "+"
        /*, "ä", "ö", "ü", "Ä", "Ö", "Ü", "ß" */
    };

    private static readonly string[] ParenthesesAndAlike = { "(", "[", "{" };

    // Capitalize string and make ready for XML
    public static string CapitalizeString(string s)
    {
      s = s.Replace("\"", "");

      char[] space = { ' ' };
      var words = s.Split(space, StringSplitOptions.RemoveEmptyEntries);

      var result = new StringBuilder();
      for (var i = 0; i < words.Length; i++)
      {
        result.Append(words[i].Substring(0, 1).ToUpper() +
                      (words[i].Length > 1 ? words[i].Substring(1, words[i].Length - 1).ToLower() : "") + " ");
      }
      return result.ToString().Trim();
    }

    public static string RemoveFeatComment(string str)
    {
      var index = str.IndexOf("(Feat", StringComparison.Ordinal);
      if (index != -1)
        str = str.Substring(0, index).Trim();
      return str;
    }

    public static string TrimForParenthesis(string str)
    {
      for (var i = 0; i < ParenthesesAndAlike.Length; i++)
      {
        var index = str.IndexOf(ParenthesesAndAlike[i], StringComparison.OrdinalIgnoreCase);
        if (index != -1)
        {
          str = str.Substring(0, index).Trim();
        }
      }
      return str;
    }

    public static string DeleteSpecificChars(string str)
    {
      for (var i = 0; i < CharsToDelete.Length; i++)
      {
        str = str.Replace(CharsToDelete[i], "");
      }
      return str;
    }

    public static string ChangeAnds(string str)
    {
      var strTemp = str;
      if (str.Contains("&"))
      {
        strTemp = str.Replace("&", "And");
      }
      return strTemp;
    }

    public static string ReturnEnvironmentNewLine(string str)
    {
      const string justNewLine = "\n";

      if (str.Split(justNewLine.ToCharArray()).Length == str.Split(Environment.NewLine.ToCharArray()).Length)
      {
        str = str.Replace(justNewLine, Environment.NewLine);
      }

      return str;
    }

    public static string FixLyrics(string lyrics)
    {
      lyrics = Regex.Replace(lyrics, "('){2,}", "'");
      return lyrics;
    }

    public static string FixLyrics(string lyrics, string[] find, string[] replace)
    {
      lyrics = FixLyrics(lyrics);

      if (find != null)
      {
        var valueIndex = 0;

        foreach (var findValue in find)
        {
          if (findValue != "")
          {
            lyrics = lyrics.Replace(findValue, replace[valueIndex]);
            valueIndex++;
          }
        }
      }

      return lyrics;
    }
  }
}
