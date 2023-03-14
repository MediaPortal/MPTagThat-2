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

using MPTagThat.Core.Lyrics.LyricsSites;
using System;
using System.Threading;
using Timer = System.Timers.Timer;

#endregion

namespace MPTagThat.Core.Lyrics
{
  /// <summary>
  /// Class emulates long process which runs in worker thread
  /// and makes synchronous user UI operations.
  /// </summary>
  public class LyricSearch : IDisposable
  {
    #region Members

    private const int TimeLimit = 60 * 1000;
    private const int TimeLimitForSite = 30 * 1000;

    public static string[] LyricsSites;

    // Reference to the lyric controller used to make syncronous user interface calls:
    private readonly LyricsController _mLyricsController;

    // Uses to inform the specified site searches to stop searching and exit
    private readonly ManualResetEvent _mEventStopSiteSearches;

    private readonly bool _mAllowAllToComplete;
    private readonly string _mArtist;

    private readonly string _mOriginalArtist;
    private readonly string _mOriginalTrack;
    private readonly string _mTitle;
    private readonly Timer _timer;
    private readonly int _mRow;

    private bool _lyricFound;
    private bool _mSearchHasEnded;
    private int _mSitesSearched;

    #endregion

    #region Functions

    internal LyricSearch(LyricsController lyricsController, string artist, string title, string strippedArtistName, int row, bool allowAllToComplete)
    {
      _mLyricsController = lyricsController;

      _mArtist = strippedArtistName;
      _mTitle = title;

      _mRow = row;

      _mOriginalArtist = artist;
      _mOriginalTrack = title;

      _mAllowAllToComplete = allowAllToComplete;

      _mEventStopSiteSearches = new ManualResetEvent(false);

      _timer = new Timer { Enabled = true, Interval = TimeLimit };
      _timer.Elapsed += StopDueToTimeLimit;
      _timer.Start();
    }

    public void Dispose()
    {
      _mSearchHasEnded = true;
      _mEventStopSiteSearches.Set();
      _timer.Enabled = false;
      _timer.Stop();
      _timer.Close();
      _timer.Dispose();
    }

    public void Run()
    {
      foreach (var lyricsSearchSite in LyricsSites)
      {
        RunSearchForSiteInThread(lyricsSearchSite);
      }

      while (!_mSearchHasEnded)
      {
        Thread.Sleep(300);
      }

      Thread.CurrentThread.Abort();
    }

    private void RunSearchForSiteInThread(string lyricsSearchSiteName)
    {
      // Moved the Create out of the Thread, since we sometimes got null returned
      var lyricsSearchSite = LyricsSiteFactory.Create(lyricsSearchSiteName, _mArtist, _mTitle, _mEventStopSiteSearches, TimeLimitForSite);
      ThreadStart job = delegate
          {
            if (lyricsSearchSite != null)
            {
              lyricsSearchSite.FindLyrics();
              if (_mAllowAllToComplete)
              {
                ValidateSearchOutputForAllowAllToComplete(lyricsSearchSite.Lyric, lyricsSearchSiteName);
              }
              else
              {
                ValidateSearchOutput(lyricsSearchSite.Lyric, lyricsSearchSiteName);
              }
            }
          };
      var searchThread = new Thread(job);
      searchThread.Start();
    }

    #endregion

    public bool ValidateSearchOutput(string lyric, string site)
    {
      if (_mSearchHasEnded == false)
      {
        Monitor.Enter(this);
        try
        {
          ++_mSitesSearched;

          // Parse the lyrics and find a suitable lyric, if any
          if (!lyric.Equals(AbstractSite.NotFound) && lyric.Length != 0)
          {
            // if the lyrics hasn't been found by another site, then we have found the lyrics to count!
            if (_lyricFound == false)
            {
              _lyricFound = true;
              _mLyricsController.LyricFound(lyric, _mOriginalArtist, _mOriginalTrack, site, _mRow);
              Dispose();
              return true;
            }
            // if another was quicker it is just too bad... return
            else
            {
              return false;
            }
          }
          // still other lyricsites to search
          else if (_mSitesSearched < LyricsSites.Length)
          {
            return false;
          }
          // the search got to end due to no more sites to search
          else
          {
            _mLyricsController.LyricNotFound(_mOriginalArtist, _mOriginalTrack, "A matching lyric could not be found!", site, _mRow);
            Dispose();
            return false;
          }
        }
        finally
        {
          Monitor.Exit(this);
        }
      }
      return false;
    }

    public bool ValidateSearchOutputForAllowAllToComplete(string lyric, string site)
    {
      if (_mSearchHasEnded == false)
      {
        Monitor.Enter(this);
        try
        {
          if (!lyric.Equals("Not found") && lyric.Length != 0)
          {
            _lyricFound = true;
            _mLyricsController.LyricFound(lyric, _mOriginalArtist, _mOriginalTrack, site, _mRow);
            if (++_mSitesSearched == LyricsSites.Length)
            {
              Dispose();
            }
            return true;
          }
          else
          {
            _mLyricsController.LyricNotFound(_mOriginalArtist, _mOriginalTrack, "A matching lyric could not be found!", site, _mRow);
            if (++_mSitesSearched == LyricsSites.Length)
            {
              Dispose();
            }
            return false;
          }
        }
        finally
        {
          Monitor.Exit(this);
        }
      }
      return false;
    }

    private void StopDueToTimeLimit(object sender, EventArgs e)
    {
      _mLyricsController.LyricNotFound(_mOriginalArtist, _mOriginalTrack, "A matching lyric could not be found!", "All (timed out)", _mRow);
      Dispose();
    }

    #region Properties

    public bool SearchHasEnded
    {
      get => _mSearchHasEnded;
      set => _mSearchHasEnded = value;
    }

    #endregion
  }
}
