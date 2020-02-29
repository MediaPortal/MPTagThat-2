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
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

#endregion

namespace MPTagThat.Core.Lyrics.LyricsSites
{
  public class Lyricsmode : AbstractSite
  {
    #region const

    // Name
    private const string SiteName = "Lyricsmode";

    // Base url
    private const string SiteBaseUrl = "https://www.lyricsmode.com";

    #endregion

    #region patterns

    // lyrics mark pattern 
    private const string LyricsMarkPattern = @".*<div id=""lyrics_text"" .*?"">(.*?)<div";

    #endregion patterns

    public Lyricsmode(string artist, string title, WaitHandle mEventStopSiteSearches, int timeLimit) : base(artist, title, mEventStopSiteSearches, timeLimit)
    {
    }

    #region interface implemetation

    protected override void FindLyricsWithTimer()
    {
      var artist = Artist.ToLower();
      artist = ClearName(artist);

      var title = Title.ToLower();
      title = ClearName(title);

      // Validation
      if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(title))
      {
        return;
      }

      var firstLetter = artist[0].ToString(CultureInfo.InvariantCulture);

      var urlString = SiteBaseUrl + "/lyrics/" + firstLetter + "/" + artist + "/" + title + ".html";

      var client = new LyricsWebClient();

      var uri = new Uri(urlString);
      client.OpenReadCompleted += CallbackMethod;
      client.OpenReadAsync(uri);

      while (Complete == false)
      {
        if (MEventStopSiteSearches.WaitOne(1, true))
        {
          Complete = true;
        }
        else
        {
          Thread.Sleep(300);
        }
      }
    }


    public override LyricType GetLyricType()
    {
      return LyricType.UnsyncedLyrics;
    }

    public override SiteType GetSiteType()
    {
      return SiteType.Scrapper;
    }

    public override SiteComplexity GetSiteComplexity()
    {
      return SiteComplexity.OneStep;
    }

    public override SiteSpeed GetSiteSpeed()
    {
      return SiteSpeed.Medium;
    }

    public override bool SiteActive()
    {
      return true;
    }

    public override string Name => SiteName;

    public override string BaseUrl => SiteBaseUrl;

    #endregion interface implemetation

    #region private methods

    private void CallbackMethod(object sender, OpenReadCompletedEventArgs e)
    {
      Stream reply = null;
      StreamReader reader = null;

      try
      {
        reply = e.Result;
        reader = new StreamReader(reply, Encoding.UTF8);

        var line = reader.ReadToEnd();
        var match = Regex.Match(line, LyricsMarkPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
          LyricText = match.Groups[1].Value;
        }

        if (LyricText.Length > 0)
        {
          CleanLyrics();
        }
        else
        {
          LyricText = NotFound;
        }
      }
      catch
      {
        LyricText = NotFound;
      }
      finally
      {
        reader?.Close();
        reply?.Close();
        Complete = true;
      }
    }

    // Cleans the lyrics
    private void CleanLyrics()
    {
      LyricText = LyricText.Replace("</span>", "");
      LyricText = LyricText.Replace("&quot;", "\"");
      LyricText = LyricText.Replace("<br>", " ");
      LyricText = LyricText.Replace("<br />", " ");
      LyricText = LyricText.Replace("<BR>", " ");
      LyricText = LyricText.Replace("&amp;", "&");
      LyricText = Regex.Replace(LyricText, ".*(<span class=.*>).*", MatchReplace, RegexOptions.Multiline);
      // Need to execute it twice for some reason the first run didn't clean the first occurence
      LyricText = Regex.Replace(LyricText, ".*(<span class=.*>).*", MatchReplace, RegexOptions.Multiline); 
      LyricText = LyricText.Trim();
    }

    private string MatchReplace(Match match)
    {
      return match.Value.Replace(match.Groups[1].Value, "");
    }


    private static string ClearName(string name)
    {
      // Spaces and special characters
      name = name.Replace(" ", "_");
      name = name.Replace("#", "_");
      name = name.Replace("%", "_");
      name = name.Replace("'", "");
      name = name.Replace("(", "%28");
      name = name.Replace(")", "%29");
      name = name.Replace("+", "%2B");
      name = name.Replace(",", "");
      name = name.Replace(".", "_");
      name = name.Replace(":", "_");
      name = name.Replace("=", "%3D");
      name = name.Replace("?", "_");

      // German letters
      name = name.Replace("ü", "%FC");
      name = name.Replace("Ü", "%DC");
      name = name.Replace("ä", "%E4");
      name = name.Replace("Ä", "%C4");
      name = name.Replace("ö", "%F6");
      name = name.Replace("Ö", "%D6");
      name = name.Replace("ß", "%DF");

      // Danish letters
      name = name.Replace("å", "%E5");
      name = name.Replace("Å", "%C5");
      name = name.Replace("æ", "%E6");
      name = name.Replace("ø", "%F8");

      // French letters
      name = name.Replace("é", "%E9");


      return name;
    }

    #endregion private methods
  }
}
