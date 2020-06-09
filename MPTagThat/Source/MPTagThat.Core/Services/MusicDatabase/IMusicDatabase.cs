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

using System.Collections.Generic;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Services.MusicDatabase.Indexes;
using Raven.Client.Documents;

#endregion

namespace MPTagThat.Core.Services.MusicDatabase
{
  public interface IMusicDatabase
  {
    /// <summary>
    /// Get A Document Store
    /// </summary>
    /// <param name="databaseName"></param>
    /// <returns></returns>
    IDocumentStore GetDocumentStoreFor(string databaseName);

    /// <summary>
    /// Remove the Store
    /// </summary>
    /// <param name="databasename"></param>
    void RemoveStore(string databasename);

    /// <summary>
    /// Returns, if a Database Scan is Active
    /// </summary>
    bool ScanActive { get; }

    /// <summary>
    /// Returns, if the MusicBrainzDatabase search option has been enabled
    /// </summary>
    bool MusicBrainzDatabaseActive { get; set; }

    /// <summary>
    /// Returns, if the DatabaseEngine has been started
    /// </summary>
    bool DatabaseEngineStarted { get; set; }

    /// <summary>
    /// Returns the active database
    /// </summary>
    string CurrentDatabase { get; }

    /// <summary>
    /// Start a Database Build out of a given Share
    /// </summary>
    /// <param name="musicShare"></param>
    /// <param name="deleteDatabase"></param>
    void BuildDatabase(string musicShare, bool deleteDatabase);

    /// <summary>
    /// Aborts the scanning of the database
    /// </summary>
    void AbortDatabaseScan();
    
    /// <summary>
    /// Deletes the Music Database
    /// </summary>
    void DeleteDatabase(string databaseName);

    /// <summary>
    /// Switch between Work and Productive database
    /// </summary>
    bool SwitchDatabase(string databaseName);

    /// <summary>
    /// Runs the query against the MusicDatabase
    /// </summary>
    /// <param name="query"></param>
    List<SongData> ExecuteQuery(string query);

    /// <summary>
    /// Update a track in the Music Database
    /// </summary>
    /// <param name="track"></param>
    /// <param name="originalFileName"></param>
    void UpdateTrack(SongData track, string originalFileName);

    /// <summary>
    /// Retrieves Distinct Artists
    /// </summary>
    /// <returns></returns>
    List<DistinctResult> GetArtists();

    /// <summary>
    /// Retrieves Distinct Artists and their Distinct Albums based on the query
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    List<DistinctResult> GetArtistAlbums(string query);

    /// <summary>
    /// Retrieves Distinct AlbumArtists 
    /// </summary>
    /// <returns></returns>
    List<DistinctResult> GetAlbumArtists();

    /// <summary>
    /// Retrieves Distinct AlbumArtists and their Distinct Albums based on the query
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    List<DistinctResult> GetAlbumArtistAlbums(string query);

    /// <summary>
    /// Retrieves Distinct Genres
    /// </summary>
    /// <returns></returns>
    List<DistinctResult> GetGenres();

    /// <summary>
    /// Retrieves Distinct Genres and their Distinct Artists
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    List<DistinctResult> GetGenreArtists(string query);

    /// <summary>
    /// Retrieves Distinct Genres and their Distinct Artists and Albums
    /// </summary>
    /// <param name="genre"></param>
    /// <param name="album"></param>
    /// <returns></returns>
    List<DistinctResult> GetGenreArtistAlbums(string genre, string album);

    /// <summary>
    /// Search for Artists to put into Autocompletion Combo
    /// </summary>
    /// <param name="artist"></param>
    /// <returns></returns>
    List<object> SearchAutocompleteArtists(string artist);

    /// <summary>
    /// Used by the AutoCorrect script
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    List<string> GetAutoCorrectArtists(string sql);
  }
}
