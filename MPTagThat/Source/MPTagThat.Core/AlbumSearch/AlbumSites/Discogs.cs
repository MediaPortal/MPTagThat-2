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

using DiscogsClient.Data.Query;
using DiscogsClient.Internal;
using MPTagThat.Core.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiscogsClient.Data.Result;
using MPTagThat.Core.AlbumCoverSearch;
using Prism.Ioc;

#endregion

namespace MPTagThat.Core.AlbumSearch.AlbumSites
{
  public class Discogs : AbstractAlbumSite
  {
    #region Variables

    private readonly NLogLogger log = ContainerLocator.Current.Resolve<ILogger>().GetLogger;
    private DiscogsClient.DiscogsClient _discogsClient;

    #endregion

    #region Properties

    public override string SiteName => "Discogs";

    public override bool SiteActive() => true;

    #endregion

    #region ctor

    public Discogs(string artist, string title, WaitHandle mEventStopSiteSearches, int timeLimit) : base(artist, title, mEventStopSiteSearches, timeLimit)
    {
      var tokenInformation = new TokenAuthenticationInformation(MPTagThat.LicenseManager.LicenseManager.GetDiscogsToken());
      _discogsClient = new DiscogsClient.DiscogsClient(tokenInformation, "MPTagThat 4.0");
    }

    #endregion

    #region Methods

    protected override void GetAlbumInfoWithTimer()
    {
      log.Debug("Discogs: Looking up Album on Discogs");
      Albums.Clear();
      try
      {
        var task = GetAlbumQuery();
        var result = task.Result; // Ignoring the result here, since we have added the album already in the task. 

        log.Debug($"Discogs: Found {Albums.Count} albums");
      }
      catch (Exception ex)
      {
        log.Debug($"Discogs: Exception receiving Album Information. {ex.Message} {ex.StackTrace}");
      }
    }

    private async Task<Album> GetAlbumQuery()
    {
      log.Trace($"Discogs: Querying {ArtistName} -  {AlbumName}");
      var query = new DiscogsSearch { artist = ArtistName, release_title = AlbumName, type = DiscogsEntityType.master };
      var searchresults = await _discogsClient.SearchAsync(query);
      if (searchresults != null)
      {
        // Look for the Master Release only
        foreach (var result in searchresults.GetResults())
        {
          var album = await GetRelease(result.id);
          if (album != null)
          {
            Albums.Add(album);
          }
        }
      }

      log.Trace("Discogs Query Ended");
      // We don't need to return really anything. Just to satisfy that a Task can't return void
      return null;
    }

    private async Task<Album> GetRelease(int releaseid)
    {
      log.Trace($"Discogs: Receiving Release {releaseid}");
      var release = await _discogsClient.GetMasterAsync(releaseid);
      if (release == null)
      {
        log.Trace("No release found. returning null");
        return null;
      }
      var album = new Album();
      album.Site = "Discogs";
      var discogsImage = release.images.FirstOrDefault(i => i.type == DiscogsImageType.primary);;

      if (discogsImage != null)
      {
        album.LargeImageUrl = discogsImage.uri;
        album.CoverHeight = discogsImage.height.ToString();
        album.CoverWidth = discogsImage.width.ToString();
      }

      album.Artist = string.Join(",", release.artists.Select(a => a.name));
      album.Title = release.title;
      album.Year = release.year.ToString();
      album.DiscCount = 1;

      // Get the Tracks
      var discs = new List<List<AlbumSong>>();
      var albumTracks = new List<AlbumSong>();
      var numDiscs = 1;
      var lastPosOnAlbumSideA = 0;

      foreach (var track in release.tracklist)
      {
        var pos = track.position;
        var albumTrack = new AlbumSong();

        if (string.IsNullOrEmpty(track.position) || string.IsNullOrEmpty(track.title))
        {
          continue;
        }

        // check for Multi Disc Album
        if (track.position.Contains("-"))
        {
          album.DiscCount = Convert.ToInt16(track.position.Substring(0, track.position.IndexOf("-", StringComparison.Ordinal)));
          // Has the number of Discs changed?
          if (album.DiscCount != numDiscs)
          {
            numDiscs = album.DiscCount;
            discs.Add(new List<AlbumSong>(albumTracks));
            albumTracks.Clear();
          }
          pos = track.position.Substring(track.position.IndexOf("-", StringComparison.Ordinal) + 1);
        }
        else if (!track.position.Substring(0, 1).All(Char.IsDigit))
        {
          // The Master Release returned was a Vinyl Album with side A and B. So we have tracks as "A1", "A2", ... "B1",..
          pos = track.position.Substring(1);
          if (track.position.Substring(0, 1) == "A")
          {
            lastPosOnAlbumSideA = Convert.ToInt16(pos);
          }
          else
          {
            pos = (lastPosOnAlbumSideA + Convert.ToInt16(pos)).ToString();
          }
        }
        albumTrack.Number = Convert.ToInt16(pos);
        albumTrack.Title = track.title;
        albumTrack.Duration = track.duration.ToString();
        albumTracks.Add(albumTrack);
      }
      discs.Add(albumTracks);
      album.Discs = discs;

      log.Trace("Discogs: Finished receiving Release");
      return album;
    }

    #endregion

  }
}
