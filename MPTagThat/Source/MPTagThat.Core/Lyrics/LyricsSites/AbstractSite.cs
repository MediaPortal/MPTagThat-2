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

using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

#endregion

namespace MPTagThat.Core.Lyrics.LyricsSites
{
    public abstract class AbstractSite : ILyricSite
    {
        #region const

        // Not found
        public const string NotFound = "Not found";

        #endregion const

        #region members

        // Artist
        protected readonly string Artist;
        // Title
        protected readonly string Title;
        // Stop event
        protected WaitHandle MEventStopSiteSearches;
        // Time Limit
        protected readonly int TimeLimit;

        // Lyrics
        protected string LyricText = "";

        // Complete
        protected bool Complete;
        private Timer _searchTimer;

        #endregion members


        protected AbstractSite(string artist, string title, WaitHandle mEventStopSiteSearches, int timeLimit)
        {
            // Artist
            Artist = artist;
            // Title
            Title = title;
            // Stop search event
            MEventStopSiteSearches = mEventStopSiteSearches;
            // Time Limit
            TimeLimit = timeLimit;
        }


        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            _searchTimer.Stop();
            _searchTimer.Close();
            _searchTimer.Dispose();

            LyricText = NotFound;
            Complete = true;
            Thread.CurrentThread.Abort();
        }

        protected abstract void FindLyricsWithTimer();


        #region interface abstract methods

        public string Lyric
        {
            get { return LyricText; }
        }

        public abstract string Name { get; }
        public abstract string BaseUrl { get; }
        public void FindLyrics()
        {
            try
            {
                // timer
                _searchTimer = new Timer { Enabled = false, Interval = TimeLimit };
                _searchTimer.Elapsed += TimerElapsed;
                _searchTimer.Start();

                // Find Lyrics
                FindLyricsWithTimer();
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
        
        public abstract LyricType GetLyricType();
        public abstract SiteType GetSiteType();
        public abstract SiteComplexity GetSiteComplexity();
        public abstract SiteSpeed GetSiteSpeed();
        public abstract bool SiteActive();

        #endregion interface abstract methods
    }
}
