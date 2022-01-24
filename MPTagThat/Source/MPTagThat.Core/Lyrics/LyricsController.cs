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

using System;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace MPTagThat.Core.Lyrics
{
  public class LyricsController : IDisposable
  {
    private readonly string[] _lyricsSites;

    private readonly bool _allowAllToComplete;

    // Main thread sets this event to stop LyricController
    private readonly ManualResetEvent _eventStopLyricController;

    // The LyricController sets this when all lyricSearch threads have been aborted
    private readonly ManualResetEvent _eventStoppedLyricController;

    private readonly string[] _findArray;
    private readonly ILyricsSearch _lyricsDialog;

    private readonly string[] _replaceArray;

    private readonly List<Thread> _threadList = new List<Thread>();

    private int _nrOfLyricsFound;
    private int _nrOfLyricsNotFound;
    private int _nrOfLyricsSearched;


    public LyricsController(ILyricsSearch dialog,
                            ManualResetEvent eventStopThread,
                            string[] lyricSites,
                            bool allowAllToComplete,
                            string find, string replace)
    {
      _lyricsDialog = dialog;
      _allowAllToComplete = allowAllToComplete;

      NrOfLyricsToSearch = 1;
      _nrOfLyricsSearched = 0;
      _nrOfLyricsFound = 0;
      _nrOfLyricsNotFound = 0;
      NrOfCurrentSearches = 0;

      _lyricsSites = lyricSites;

      LyricSearch.LyricsSites = _lyricsSites;

      _eventStopLyricController = eventStopThread;
      _eventStoppedLyricController = new ManualResetEvent(false);

      if (!string.IsNullOrEmpty(find) && !string.IsNullOrEmpty(replace))
      {
        if (find != "")
        {
          _findArray = find.Split(',');
          _replaceArray = replace.Split(',');
        }
      }
    }

    public bool StopSearches { get; set; }

    public int NrOfLyricsToSearch { get; set; }

    public int NrOfCurrentSearches { get; private set; }

    #region IDisposable Members

    public void Dispose()
    {
      // clean-up operations may be placed here
      foreach (var thread in _threadList)
      {
        thread.Abort();
      }

      StopSearches = true;

      var stillThreadsAlive = _threadList.Count > 0;
      while (stillThreadsAlive)
      {
        stillThreadsAlive = false;
        foreach (var thread in _threadList)
        {
          if (thread.IsAlive)
          {
            stillThreadsAlive = true;
          }
        }
      }

      FinishThread("", "", "The search has ended.", "");
    }

    #endregion

    public void Run()
    {
      // check if thread is cancelled
      while (true)
      {
        Thread.Sleep(100);

        // check if thread is cancelled
        if (_eventStopLyricController.WaitOne())
        {
          // clean-up operations may be placed here
          foreach (var thread in _threadList)
          {
            thread.Abort();
          }

          StopSearches = true;

          var stillThreadsAlive = _threadList.Count > 0;
          while (stillThreadsAlive)
          {
            stillThreadsAlive = false;
            foreach (var thread in _threadList)
            {
              if (thread.IsAlive)
              {
                stillThreadsAlive = true;
              }
            }
          }

          _eventStoppedLyricController.Set();
          break;
        }
      }
    }


    public void AddNewLyricSearch(string artist, string title, string strippedArtistName)
    {
      AddNewLyricSearch(artist, title, strippedArtistName, -1);
    }

    public void AddNewLyricSearch(string artist, string title, string strippedArtistName, int row)
    {
      if (_lyricsSites.Length > 0 && !string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(title))
      {
        ++NrOfCurrentSearches;

        // create worker thread instance
        ThreadStart threadInstance = delegate
            {
              var lyricSearch = new LyricSearch(this, artist, title, strippedArtistName, row, _allowAllToComplete);
              lyricSearch.Run();
            };

        var lyricSearchThread = new Thread(threadInstance);
        lyricSearchThread.Name = "BasicSearch for " + artist + " - " + title; // looks nice in Output window
        lyricSearchThread.IsBackground = true;
        lyricSearchThread.Start();
        _threadList.Add(lyricSearchThread);
      }
    }

    internal void StatusUpdate(string artist, string title, string site, bool lyricFound)
    {
      if (lyricFound)
      {
        ++_nrOfLyricsFound;
      }
      else
      {
        ++_nrOfLyricsNotFound;
      }

      ++_nrOfLyricsSearched;

      _lyricsDialog.UpdateStatus = new Object[]
          {
                    NrOfLyricsToSearch, _nrOfLyricsSearched, _nrOfLyricsFound,
                    _nrOfLyricsNotFound
          };

      if ((_nrOfLyricsSearched >= NrOfLyricsToSearch * _lyricsSites.Length))
      {
        FinishThread(artist, title, "All songs have been searched!", site);
      }
    }

    internal void LyricFound(String lyricStrings, String artist, String title, String site, int row)
    {
      var cleanLyric = LyricUtil.FixLyrics(lyricStrings, _findArray, _replaceArray);

      --NrOfCurrentSearches;

      if (_allowAllToComplete || StopSearches == false)
      {
        _lyricsDialog.LyricFound = new Object[] { artist, title, site, row, cleanLyric };
        StatusUpdate(artist, title, site, true);
      }
    }

    internal void LyricNotFound(String artist, String title, String message, String site, int row)
    {
      --NrOfCurrentSearches;

      if (_allowAllToComplete || StopSearches == false)
      {
        _lyricsDialog.LyricNotFound = new Object[] { artist, title, site, row, message };
        StatusUpdate(artist, title, site, false);
      }
    }

    public void FinishThread(String artist, String title, String message, String site)
    {
      StopSearches = true;
      _eventStopLyricController.Set();

      while (!_eventStoppedLyricController.WaitOne(Timeout.Infinite, true))
      {
        Thread.Sleep(50);
      }
      _lyricsDialog.ThreadFinished = new Object[] { artist, title, message, site };
    }

    internal void ThreadException(String s)
    {
      _lyricsDialog.ThreadException = s;
    }
  }
}
