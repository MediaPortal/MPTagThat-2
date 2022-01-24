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

using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

#endregion

namespace MPTagThat.Core.AlbumSearch.AlbumSites
{
  public abstract class AbstractAlbumSite
  {
    #region Variables

    public const string NotFound = "Not found";

    #endregion

    #region Properties

    // Artist
    protected readonly string ArtistName;
    // Album
    protected readonly string AlbumName;
    // Stop event
    protected WaitHandle MEventStopSiteSearches;
    // Time Limit
    protected readonly int TimeLimit;

    // Album
    protected List<Album> Albums = new List<Album>();

    // Complete
    protected bool Complete;
    private Timer _searchTimer;

    // Album Information
    public List<Album> AlbumInfo => Albums;

    // The AlbumSiteName
    public abstract string SiteName { get; }

    public abstract bool SiteActive();

    #endregion

    #region ctor

    protected AbstractAlbumSite(string artist, string album, WaitHandle mEventStopSiteSearches, int timeLimit)
    {
      // Artist
      ArtistName = artist;
      // AlbumName
      AlbumName = album;
      // Stop search event
      MEventStopSiteSearches = mEventStopSiteSearches;
      // Time Limit
      TimeLimit = timeLimit;
    }

    #endregion

    #region Methods

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
      _searchTimer.Stop();
      _searchTimer.Close();
      _searchTimer.Dispose();

      Albums.Clear();
      Complete = true;
      Thread.CurrentThread.Abort();
    }

    protected abstract void GetAlbumInfoWithTimer();

    public void GetAlbumInfo()
    {
      try
      {
        _searchTimer = new Timer { Enabled = false, Interval = TimeLimit };
        _searchTimer.Elapsed += TimerElapsed;
        _searchTimer.Start();

        GetAlbumInfoWithTimer();
      }
      finally
      {
        if (_searchTimer != null)
        {
          _searchTimer.Stop();
          _searchTimer.Close();
        }
      }
    }

    #endregion
  }
}
