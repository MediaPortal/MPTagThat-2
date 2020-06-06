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

using System.Linq;
using MPTagThat.Core.Common.Song;
using Raven.Client.Documents.Indexes;

#endregion

namespace MPTagThat.Core.Services.MusicDatabase.Indexes
{
  /// <summary>
  /// Default Search index on most common fields
  /// </summary>
  public class DefaultSearchIndex : AbstractIndexCreationTask<SongData, DefaultSearchIndex.Result>
  {
    public class Result
    {
      public string Query { get; set; }
      public string FullFileName { get; set; }
      public string Title { get; set; }
      public string Album { get; set; }
      public string Genre { get; set; }
      public string Artist { get; set; }
      public string AlbumArtist { get; set; }
      public string Type { get; set; }
      public string Composer { get; set; }
    }

    public DefaultSearchIndex()
    {
      Map = tracks => from track in tracks
                     select new
                     {
                       FullFileName = track.FullFileName,
                       Title = track.Title,
                       Album = track.Album,
                       Genre = track.Genre,
                       Artist = track.Artist,
                       AlbumArtist = track.AlbumArtist,
                       Type = track.TagType,
                       Composer = track.Composer,
                       Query = new object[]
                         {
                                track.Title,
                                track.Album,
                                track.Genre,
                                track.Artist,
                                track.AlbumArtist,
                                track.Lyrics
                         }
                     };

      Indexes.Add(x => x.Query, FieldIndexing.Search);
      Indexes.Add(x => x.FullFileName, FieldIndexing.Search);
      Indexes.Add(x => x.Title, FieldIndexing.Search);
      Indexes.Add(x => x.Album, FieldIndexing.Search);
      Indexes.Add(x => x.Genre, FieldIndexing.Search);
      Indexes.Add(x => x.Artist, FieldIndexing.Search);
      Indexes.Add(x => x.AlbumArtist, FieldIndexing.Search);
      Indexes.Add(x => x.Type, FieldIndexing.Search);
      Indexes.Add(x => x.Composer, FieldIndexing.Search);
    }
  }
}
