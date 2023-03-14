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

#region MyRegion

using IF.Lastfm.Core.Api;
using MPTagThat.Core.AlbumCoverSearch;
using MPTagThat.Core.Services.Logging;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace MPTagThat.Core.AlbumSearch.AlbumSites
{
  public class LastFM : AbstractAlbumSite
  {
    #region Variables

    private readonly NLogLogger _log = ContainerLocator.Current.Resolve<ILogger>()?.GetLogger;
    private readonly LastfmClient _lastfmClient;

    #endregion

    #region Properties

    public override string SiteName => "LastFM";

    public override bool SiteActive() => true;

    #endregion

    #region ctor

    public LastFM(string artist, string album, WaitHandle mEventStopSiteSearches, int timeLimit) : base(artist, album, mEventStopSiteSearches, timeLimit)
    {
      var apiKeys = LicenseManager.LicenseManager.GetLastFmApiKey().Split(new char[] { ',' });
      _lastfmClient = new LastfmClient(apiKeys[0], apiKeys[1]);
    }

    #endregion

    #region Methods

    protected override void GetAlbumInfoWithTimer()
    {
      _log.Debug("LastFM: Looking up Album on LastFM");
      Albums.Clear();
      try
      {
        var task = GetAlbumQuery();
        var result = task.Result; // Ignoring the result here, since we have added the album already in the task. 

        _log.Debug($"LastFM: Found {Albums.Count} albums");
      }
      catch (Exception ex)
      {
        _log.Debug($"LastFM: Exception receiving Album Information. {ex.Message} {ex.StackTrace}");
      }
    }


    private async Task<Album> GetAlbumQuery()
    {
      var response = await _lastfmClient.Album.GetInfoAsync(SwitchArtist(ArtistName), AlbumName);
      if (response.Success)
      {
        var lastfmAlbum = response.Content;

        var album = new Album
        {
          Site = "LastFM",
          Artist = lastfmAlbum.ArtistName,
          Title = lastfmAlbum.Name,
          SmallImageUrl = lastfmAlbum.Images?.Large?.AbsoluteUri,
          MediumImageUrl = lastfmAlbum.Images?.Largest?.AbsoluteUri,
          LargeImageUrl = lastfmAlbum.Images?.Mega?.AbsoluteUri
        };

        var discs = new List<List<AlbumSong>>();
        var albumTracks = new List<AlbumSong>();
        var i = 0;
        foreach (var track in lastfmAlbum.Tracks)
        {
          i++;
          var albumtrack = new AlbumSong();
          albumtrack.Title = track.Name;
          albumtrack.Duration = track.Duration.ToString();
          albumtrack.Number = i;
          albumTracks.Add(albumtrack);
        }

        discs.Add(albumTracks);
        album.Discs = discs;
        Albums.Add(album);
      }

      // We don't need to return really anything. Just to satisfy that a Task can't return void
      return null;
    }

    private string SwitchArtist(string artist)
    {
      int iPos = artist.IndexOf(',');
      if (iPos > 0)
      {
        artist = $"{artist.Substring(iPos + 2)} {artist.Substring(0, iPos)}";
      }
      return artist;
    }

    #endregion
  }
}
