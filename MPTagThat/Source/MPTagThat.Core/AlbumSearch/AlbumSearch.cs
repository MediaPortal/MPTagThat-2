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
using MPTagThat.Core.AlbumSearch.AlbumSites;
using MPTagThat.Core.Services.Logging;
using Prism.Ioc;
using Timer = System.Timers.Timer;

#endregion

namespace MPTagThat.Core.AlbumSearch
{
  public class AlbumSearch : IDisposable
  {
    #region Variables

    private readonly NLogLogger log;

    private const int TimeLimit = 30 * 1000;
    private const int TimeLimitForSite = 15 * 1000;

    public List<string> AlbumSites = new List<string>();

    // Uses to inform the specified site searches to stop searching and exit
    private readonly ManualResetEvent _mEventStopSiteSearches;

    private readonly string _artist = "";
    private readonly string _albumTitle = "";
    private readonly IAlbumSearch _controller;

    private readonly Timer _timer;

    private int _mSitesSearched;


    #endregion

    #region Properties

    public bool SearchHasEnded { get; set; }

    #endregion

    #region Methods

    public AlbumSearch(IAlbumSearch controller, string artist, string album)
    {
      log = ContainerLocator.Current.Resolve<ILogger>().GetLogger;
      _artist = artist;
      _albumTitle = album;
      _controller = controller;

      _mEventStopSiteSearches = new ManualResetEvent(false);

      _timer = new Timer();
      _timer.Enabled = true;
      _timer.Interval = TimeLimit;
      _timer.Elapsed += StopDueToTimeLimit;
      _timer.Start();
    }

    public void Dispose()
    {
      SearchHasEnded = true;
      _mEventStopSiteSearches.Set();
      _timer.Enabled = false;
      _timer.Stop();
      _timer.Close();
      _timer.Dispose();
      _controller.SearchFinished = new object[] { };
    }

    public void Run()
    {
      foreach (var albumInfoSite in AlbumSites)
      {
        RunSearchForSiteInThread(albumInfoSite);
      }
    }

    private void RunSearchForSiteInThread(string albumInfoSite)
    {
      // Moved outside of thread on purpose, since we could get conflicts creating the delegate
      var albumSearchSite = AlbumSiteFactory.Create(albumInfoSite, _artist, _albumTitle, _mEventStopSiteSearches, TimeLimitForSite);
      ThreadStart job = delegate
      {
        if (albumSearchSite != null)
        {
          albumSearchSite.GetAlbumInfo();
          ValidateSearchOutput(albumSearchSite.AlbumInfo, albumInfoSite);
        }
      };
      var searchThread = new Thread(job);
      searchThread.Start();
    }

    private bool ValidateSearchOutput(List<Album> albums, string site)
    {
      if (SearchHasEnded == false)
      {
        Monitor.Enter(this);
        try
        {
          _mSitesSearched++;
          log.Debug($"{site} Albums: {albums.Count} Searched: {_mSitesSearched} Sites Count: {AlbumSites.Count}");
          if (albums.Count > 0)
          {
            _controller.AlbumFound = new object[] { albums, site };
            if (_mSitesSearched == AlbumSites.Count)
            {
              Dispose();
            }
            return true;
          }
          else
          {
            if (_mSitesSearched == AlbumSites.Count)
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
      Dispose();
    }

    #endregion
  }
}
