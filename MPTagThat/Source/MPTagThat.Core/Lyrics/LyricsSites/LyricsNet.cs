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
// ReSharper disable StringIndexOfIsCultureSpecific.1

#endregion

namespace MPTagThat.Core.Lyrics.LyricsSites
{
  public class LyricsNet : AbstractSite
  {
    #region const

    // Name
    private const string SiteName = "LyricsNet";

    // Base url
    private const string SiteBaseUrl = "https://www.lyrics.com";

    private const string SearchPathQuery = "/artist/";

    #endregion

    #region patterns

    //////////////////////////
    // First phase patterns //
    //////////////////////////

    // RegEx to extract lyrics page out of a search on Artist
    private const string FindLyricsPagePatternPrefix = @".*<a href=""(.*?)"">";
    private const string FindLyricsPagePatternSuffix = "</a>";

    ///////////////////////////
    // Second phase patterns //
    ///////////////////////////

    // Lyrics RegEx
    // Lyrics start RegEx
    private const string LyricsSearchPattern = @"<pre id=""lyric-body-text"".*?>(.*?)<\/pre>";

    #endregion

    // step 1 output
    private string _lyricsIndex;
    private bool _firstStepComplete;


    public LyricsNet(string artist, string title, WaitHandle mEventStopSiteSearches, int timeLimit) : base(artist, title, mEventStopSiteSearches, timeLimit)
    {
    }

    #region interface implemetation

    protected override void FindLyricsWithTimer()
    {
      var artist = FixEscapeCharacters(Artist);

      // 1st step - find lyrics page
      var firstUrlString = BaseUrl + SearchPathQuery + artist;

      var findLyricsPageWebClient = new LyricsWebClient();
      findLyricsPageWebClient.OpenReadCompleted += FirstCallbackMethod;
      findLyricsPageWebClient.OpenReadAsync(new Uri(firstUrlString));

      while (_firstStepComplete == false)
      {
        if (MEventStopSiteSearches.WaitOne(1, true))
        {
          _firstStepComplete = true;
        }
        else
        {
          Thread.Sleep(100);
        }
      }

      if (_lyricsIndex == null)
      {
        LyricText = NotFound;
        return;
      }
      // 2nd step - find lyrics
      var secondUrlString = BaseUrl + _lyricsIndex;

      var findLyricsWebClient = new LyricsWebClient(firstUrlString);
      findLyricsWebClient.OpenReadCompleted += SecondCallbackMethod;
      findLyricsWebClient.OpenReadAsync(new Uri(secondUrlString));

      while (Complete == false)
      {
        if (MEventStopSiteSearches.WaitOne(1, true))
        {
          Complete = true;
        }
        else
        {
          Thread.Sleep(100);
        }
      }
    }

    public override string Name => SiteName;

    public override string BaseUrl => SiteBaseUrl;


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
      return SiteComplexity.TwoSteps;
    }

    public override SiteSpeed GetSiteSpeed()
    {
      return SiteSpeed.Slow;
    }

    public override bool SiteActive()
    {
      return true;
    }

    #endregion interface implemetation

    #region private methods

    // Finds lyrics page
    private void FirstCallbackMethod(object sender, OpenReadCompletedEventArgs e)
    {
      Stream reply = null;
      StreamReader reader = null;

      try
      {
        reply = e.Result;
        reader = new StreamReader(reply, Encoding.UTF8);

        var line = reader.ReadToEnd();
        var findLyricsPagePattern = FindLyricsPagePatternPrefix + Title + FindLyricsPagePatternSuffix;
        var match = Regex.Match(line, findLyricsPagePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
          _lyricsIndex = match.Groups[1].Value;
        }
        else
        {
          _lyricsIndex = null;
        }
      }
      catch
      {
        _lyricsIndex = null;
      }
      finally
      {
        reader?.Close();
        reply?.Close();
        _firstStepComplete = true;
      }
    }

    // Find lyrics
    private void SecondCallbackMethod(object sender, OpenReadCompletedEventArgs e)
    {
      Stream reply = null;
      StreamReader reader = null;

      try
      {
        reply = e.Result;
        reader = new StreamReader(reply, Encoding.UTF8);

        var line = reader.ReadToEnd();
        var match = Regex.Match(line, LyricsSearchPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
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

    private void CleanLyrics()
    {
      while (LyricText.IndexOf("<a ") > -1)
      {
        var startIndex = LyricText.IndexOf("<a ");
        var length = LyricText.IndexOf("\">") - startIndex + 2;
        LyricText = LyricText.Remove(startIndex, length);
      }

      LyricText = LyricText.Replace("</a>", "");
      LyricText = LyricText.Replace("<br>", "");
      LyricText = LyricText.Replace("<br/>", "");
      LyricText = LyricText.Replace("&quot;", "\"");

      LyricText = LyricText.Trim();
    }

    private static string FixEscapeCharacters(string text)
    {
      text = text.Replace("(", "");
      text = text.Replace(")", "");
      text = text.Replace("#", "");
      text = text.Replace("/", "");

      text = text.Replace("%", "%25");

      text = text.Replace(" ", "%20");
      text = text.Replace("$", "%24");
      text = text.Replace("&", "%26");
      text = text.Replace("'", "%27");
      text = text.Replace("+", "%2B");
      text = text.Replace(",", "%2C");
      text = text.Replace(":", "%3A");
      text = text.Replace(";", "%3B");
      text = text.Replace("=", "%3D");
      text = text.Replace("?", "%3F");
      text = text.Replace("@", "%40");
      text = text.Replace("&amp;", "&");

      return text;
    }

    #endregion private methods
  }
}
