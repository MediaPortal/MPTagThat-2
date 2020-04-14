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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommonServiceLocator;
using Hqub.MusicBrainz.API;
using Hqub.MusicBrainz.API.Entities;
using MPTagThat.Core.AlbumCoverSearch;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;

#endregion

namespace MPTagThat.Core.AlbumSearch.AlbumSites
{
  class MusicBrainz : AbstractAlbumSite
  {
    #region Variables

    private readonly NLogLogger log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger).GetLogger;
    private Options _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;
    private Regex _switchedArtist = new Regex(@"^.*, .*$");

    #endregion

    #region Properties

    public override string SiteName => "MusicBrainz";

    public override bool SiteActive() => true;

    #endregion

    #region ctor

    public MusicBrainz(string artist, string album, WaitHandle mEventStopSiteSearches, int timeLimit) : base(artist, album, mEventStopSiteSearches, timeLimit)
    {
    }

    #endregion

    #region Methods

    protected override void GetAlbumInfoWithTimer()
    {
      log.Debug("MusicBrainz: Looking up Album on MusicBrainz");
      Albums.Clear();
      try
      {
        var album = GetAlbumQuery(ArtistName, AlbumName);
        if (album.Result != null)
        {
          Albums.Add(album.Result);
        }
        log.Debug($"MusicBrainz: Found {Albums.Count} albums");
      }
      catch (Exception ex)
      {
        log.Debug($"MusicBrainz: Exception receiving Album Information. {ex.Message} {ex.StackTrace}");
      }
    }

    private async Task<Album> GetAlbumQuery(string artistName, string albumName)
    {
      log.Debug($"MusicBrainz: Querying {artistName} - {albumName}");
      // If we have an artist in form "LastName, FirstName" change it to "FirstName LastName" to have both results
      var artistNameOriginal = _switchedArtist.IsMatch(artistName) ? $" OR {SwitchArtist(artistName)}" : "";

      var query = new QueryParameters<Release>
      {
        {"artist", $"{artistName} {artistNameOriginal}"}, {"release", albumName}
      };
      var albums = await Release.SearchAsync(query);

      // First look for Albums from the selected countries
      Release mbAlbum = null;
      foreach (var country in _options.MainSettings.PreferredMusicBrainzCountries)
      {
        mbAlbum = albums.Items.FirstOrDefault(r => r.Country == country);
        if (mbAlbum != null)
        {
          break;
        }
      }

      if (mbAlbum == null && albums.Items.Count > 0)
      {
        mbAlbum = albums.Items[0];
      }
      else
      {
        return null;
      }

      var release = await Release.GetAsync(mbAlbum.Id, new[] { "recordings", "media", "artists", "discids" });

      var album = new Album() { Site = "MusicBrainz" };
      album.LargeImageUrl = release.CoverArtArchive != null && release.CoverArtArchive.Front
        ? string.Format(@"http://coverartarchive.org/release/{0}/front.jpg", release.Id)
        : "";
      album.CoverHeight = "0";
      album.CoverWidth = "0";
      album.Artist = JoinArtists(release.Credits);
      album.Title = release.Title;
      album.Year = release.Date;
      album.DiscCount = release.Media.Count;

      // Get the Tracks
      var discs = new List<List<AlbumSong>>();
      foreach (var medium in release.Media)
      {
        var albumTracks = new List<AlbumSong>();
        foreach (var track in medium.Tracks)
        {
          var albumtrack = new AlbumSong();
          albumtrack.Number = track.Position;
          TimeSpan duration = TimeSpan.FromMilliseconds((double)track.Recording.Length);
          albumtrack.Duration = $"{duration:mm\\:ss}";
          albumtrack.Title = track.Recording.Title;
          albumTracks.Add(albumtrack);
        }
        discs.Add(albumTracks);
      }
      album.Discs = discs;
      log.Debug("MusicBrainz: Finished MusicBrainz Query");
      return album;
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

    private string JoinArtists(List<NameCredit> credits)
    {
      var joinedArtist = "";
      var firstElement = true;

      foreach (var credit in credits)
      {
        if (!firstElement)
        {
          joinedArtist += $"; {credit.Artist.Name}";
        }
        else
        {
          joinedArtist = credit.Artist.Name;
          firstElement = false;
        }
      }

      return joinedArtist;
    }

    #endregion
  }
}
