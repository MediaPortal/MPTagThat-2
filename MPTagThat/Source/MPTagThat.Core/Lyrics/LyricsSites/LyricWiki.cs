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
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

#endregion

namespace MPTagThat.Core.Lyrics.LyricsSites
{
  public class LyricWiki : AbstractSite
  {
    #region const

    // Name
    private const string SiteName = "LyricWiki";

    // Base url
    private const string SiteBaseUrl = "https://lyrics.fandom.com/wiki";

    #endregion const

    #region patterns

    // lyrics mark pattern 
    private const string LyricsMarkPattern = @".*<div class='lyricbox'>(.*?)<div";

    #endregion patterns

    public LyricWiki(string artist, string title, WaitHandle mEventStopSiteSearches, int timeLimit) : base(artist, title, mEventStopSiteSearches, timeLimit)
    {
    }

    #region interface implemetation

    protected override void FindLyricsWithTimer()
    {
      // Clean artist name
      var artist = LyricUtil.RemoveFeatComment(Artist);
      artist = LyricUtil.CapitalizeString(artist);
      artist = artist.Replace(" ", "_");

      // Clean title name
      var title = LyricUtil.TrimForParenthesis(Title);
      title = LyricUtil.CapitalizeString(title);
      title = title.Replace(" ", "_");
      title = title.Replace("?", "%3F");

      // Validate not empty
      if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(title))
      {
        return;
      }

      var urlString = SiteBaseUrl + "/" + artist + ":" + title;

      var uri = new Uri(urlString);
      var client = new LyricsWebClient();
      client.OpenReadCompleted += CallbackMethod;
      client.OpenReadAsync(uri);

      while (Complete == false)
      {
        if (MEventStopSiteSearches.WaitOne(500, true))
        {
          Complete = true;
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
          LyricText = HttpUtility.HtmlDecode(LyricText);
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
      LyricText = LyricText.Replace("%quot;", "\"");
      LyricText = LyricText.Replace("<br>", "\r\n");
      LyricText = LyricText.Replace("<br />", "\r\n");
      LyricText = LyricText.Replace("<BR>", "\r\n");
      LyricText = LyricText.Replace("&amp;", "&");
      LyricText = LyricText.Trim();
    }

    #endregion private methods
  }
}
