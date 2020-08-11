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
using System.ComponentModel;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using LiteDB;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Core.Utils;
using Prism.Ioc;
using WPFLocalizeExtension.Engine;

#endregion 

namespace MPTagThat.Core.Services.MusicDatabase
{
  /// <summary>
  /// This class handles all related RavenDB actions
  /// </summary>
  public class MusicDatabase : IMusicDatabase
  {
    #region Variables

    private readonly NLogLogger log = ContainerLocator.Current.Resolve<ILogger>()?.GetLogger;
    private readonly Options _options = ContainerLocator.Current.Resolve<ISettingsManager>()?.GetOptions;
    private readonly string _defaultMusicDatabaseName = "MusicDatabase";
    private LiteDatabase _store;

    private BackgroundWorker _bgwScanShare;
    private int _audioFiles;
    private DateTime _scanStartTime;

    private readonly Dictionary<string, LiteDatabase> _stores = new Dictionary<string, LiteDatabase>();
    private readonly StatusBarEvent _progressEvent = new StatusBarEvent { Type = StatusBarEvent.StatusTypes.CurrentFile };
    private readonly SQLiteConnection _sqLiteConnection;

    private string[] _indexedTags = { "Artist:", "AlbumArtist:", "Album:", "Composer:", "Genre:", "Title:", "Type:", "FullFileName:" };
    #endregion

    #region ctor / dtor

    public MusicDatabase()
    {
      CurrentDatabase = _defaultMusicDatabaseName;

      // Open Connection to the SQLite Database with the MusicBrainz artists
      MusicBrainzDatabaseActive = false;
      if (File.Exists(@"bin\\MusicBrainzArtists.db3"))
      {
        log.Info("Opening MusicBrainz Artist Database");
        _sqLiteConnection = new SQLiteConnection("Data Source=bin\\MusicBrainzArtists.db3");
        _sqLiteConnection?.Open();
        MusicBrainzDatabaseActive = true;
      }
    }

    #endregion

    #region Properties

    public bool ScanActive
    {
      get
      {
        if (_bgwScanShare == null)
          return false;

        if (_bgwScanShare.IsBusy)
          return true;

        return false;
      }
    }

    public string CurrentDatabase { get; set; }

    public bool MusicBrainzDatabaseActive { get; set; }

    public bool DatabaseEngineStarted { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Return a document store
    /// </summary>
    /// <param name="databaseName"></param>
    /// <returns></returns>
    public LiteDatabase GetDocumentStoreFor(string databaseName)
    {
      log.Trace($"Getting database store for {databaseName}");
      if (_stores.ContainsKey(databaseName))
      {
        return _stores[databaseName];
      }

      try
      {
        var db = new LiteDatabase($@"{_options.StartupSettings.DatabaseFolder}\{databaseName}.db");
        _stores.Add(databaseName, db);
      }
      catch (Exception ex)
      {
        log.Error("Error creating database connection: ", ex.Message);
        return null;
      }
      return _stores[databaseName];
    }

    /// <summary>
    /// Remove a store
    /// </summary>
    /// <param name="databasename"></param>
    public void RemoveStore(string databasename)
    {
      _stores.Remove(databasename);
    }

    /// <summary>
    /// Builds the database using the Music Share
    /// </summary>
    /// <param name="musicShare"></param>
    /// <param name="deleteDatabase"></param>
    public void BuildDatabase(string musicShare, bool deleteDatabase)
    {
      if (deleteDatabase)
      {
        DeleteDatabase(CurrentDatabase);
      }

      if (_store == null)
      {
        _store = GetDocumentStoreFor(CurrentDatabase);
        if (_store == null)
        {
          log.Error("Database Scan aborted.");
          return;
        }
      }

      _bgwScanShare = new BackgroundWorker
      {
        WorkerSupportsCancellation = true,
        WorkerReportsProgress = true
      };
      _bgwScanShare.DoWork += ScanShare_DoWork;
      _bgwScanShare.RunWorkerCompleted += ScanShare_Completed;
      _bgwScanShare.RunWorkerAsync(musicShare);
    }

    /// <summary>
    /// Aborts the scanning of the database
    /// </summary>
    public void AbortDatabaseScan()
    {
      if (_bgwScanShare != null && _bgwScanShare.IsBusy)
      {
        _bgwScanShare.CancelAsync();
      }
    }

    /// <summary>
    /// Deletes the Music Database
    /// </summary>
    public void DeleteDatabase(string databaseName)
    {
      log.Trace($"Deleting database {databaseName}");
      _store?.Dispose();
      _store = null;
      RemoveStore(databaseName);
      var dbPath = $"{_options.StartupSettings.DatabaseFolder}\\{databaseName}";
      File.Delete($"{dbPath}.db");
      File.Delete($"{dbPath}-log.db");
      if (CurrentDatabase == databaseName)
      {
        CurrentDatabase = _defaultMusicDatabaseName;
      }
    }

    /// <summary>
    /// Switch between databases
    /// </summary>
    public bool SwitchDatabase(string databaseName)
    {
      var dbName = Util.MakeValidFileName(databaseName);
      _store?.Dispose();
      _store = null;
      RemoveStore(CurrentDatabase);
      CurrentDatabase = dbName;
      _store = GetDocumentStoreFor(CurrentDatabase);
      if (_store != null)
      {
        log.Info($"Database has been switched to {CurrentDatabase}");
        return true;
      }
      return false;
    }

    /// <summary>
    /// Runs the query against the MusicDatabase
    /// </summary>
    /// <param name="query"></param>
    public List<SongData> ExecuteQuery(string query)
    {
      log.Info($"Executing database query: {query}");

      if (_store == null)
      {
        _store = GetDocumentStoreFor(CurrentDatabase);
        if (_store == null)
        {
          log.Error("Could not establish a session.");
          return null;
        }
      }

      StatusBarEvent msg = new StatusBarEvent { CurrentFolder = "", CurrentProgress = -1 };
      msg.NumberOfFiles = 0;
      EventSystem.Publish(msg);

      List<SongData> result = null;

      var sql = "";
      if (query.StartsWith("dbview:"))
      {
        sql = "select $ from songs where " + query.Substring(7);
      }
      else
      {
        sql = "select $ from songs " +
              $"where Artist like '%{query}%' or " +
              $"AlbumArtist like '%{query}%' or " +
              $"Album like '%{query}%' or " +
              $"Title like '%{query}%' or " +
              $"Genre like '%{query}%'";
      }

      var resultSet = _store.Execute(sql).ToEnumerable().Select(s => BsonMapper.Global.Deserialize<SongData>(s)).ToList();
      log.Info($"Query returned {resultSet.Count} results");

      // need to do our own ordering
      result = resultSet.OrderBy(x => x.Artist).ThenBy(x => x.Album).ThenBy(x => x.Track).ToList();

      msg.CurrentProgress = 0;
      msg.NumberOfFiles = result.Count;
      EventSystem.Publish(msg);

      return result;
    }

    /// <summary>
    /// Update a song in the database
    /// </summary>
    /// <param name="song"></param>
    /// <param name="originalFileName"></param>
    public void UpdateSong(SongData song, string originalFileName)
    {
      if (_store == null)
      {
        _store = GetDocumentStoreFor(CurrentDatabase);
        if (_store == null)
        {
          log.Error("Could not establish a session.");
          return;
        }
      }

      try
      {
        originalFileName = Util.EscapeDatabaseQuery(originalFileName);
        // Lookup the song in the database
        var col = _store.GetCollection<SongData>("songs");
        var originalSong = col.FindOne(s => s.FullFileName.Equals(originalFileName));
        if (originalSong != null)
        {
          originalSong.Status = -1;
          originalSong.Changed = false;
          originalSong = StoreCoverArt(originalSong);
          col.Update(originalSong.Id, originalSong);
        }
      }
      catch (Exception ex)
      {
        log.Error("UpdateTrack: Error updating song in database {0}: {1}", ex.Message, ex.InnerException?.ToString());
      }
    }


    public List<string> GetArtists()
    {
      if (_store == null)
      {
        _store = GetDocumentStoreFor(CurrentDatabase);
        if (_store == null)
        {
          log.Error("Could not establish a session.");
          return null;
        }
      }

      log.Trace("Getting distinct artists");

      var artists = new List<string>();
      var reader = _store.Execute("select distinct(*.Artist)  from songs");
      reader.Read();
      var result = reader.Current;
      foreach (var artist in result["Artist"].AsArray)
      {
        artists.Add(artist.ToString().Trim('"'));
      }

      log.Debug($"Found {artists.Count} distinct artists");
      return artists;
    }

    public List<string> GetArtistAlbums(string query)
    {
      if (_store == null)
      {
        _store = GetDocumentStoreFor(CurrentDatabase);
        if (_store == null)
        {
          log.Error("Could not establish a session.");
          return null;
        }
      }

      log.Trace($"Getting distinct artist albums for {query}");

      var artistalbums = new List<string>();
      var reader = _store.Execute($"select distinct(*.Album) from songs where Artist = \"{Util.EscapeDatabaseQuery(query)}\"");
      reader.Read();
      var result = reader.Current;
      foreach (var album in result["Album"].AsArray)
      {
        artistalbums.Add(album.ToString().Trim('"'));
      }

      log.Debug($"Found {artistalbums.Count} distinct Artist Albums");
      return artistalbums;
    }

    public List<string> GetAlbumArtists()
    {
      if (_store == null)
      {
        _store = GetDocumentStoreFor(CurrentDatabase);
        if (_store == null)
        {
          log.Error("Could not establish a session.");
          return null;
        }
      }

      log.Trace("Getting distinct album artists");

      var albumartists = new List<string>();
      var reader = _store.Execute("select distinct(*.AlbumArtist)  from songs");
      reader.Read();
      var result = reader.Current;
      foreach (var albumartist in result["AlbumArtist"].AsArray)
      {
        albumartists.Add(albumartist.ToString().Trim('"'));
      }

      log.Debug($"Found {albumartists.Count} distinct album artists");
      return albumartists;
    }

    public List<string> GetAlbumArtistAlbums(string query)
    {
      if (_store == null)
      {
        _store = GetDocumentStoreFor(CurrentDatabase);
        if (_store == null)
        {
          log.Error("Could not establish a session.");
          return null;
        }
      }

      log.Trace($"Getting distinct albumartist  albums for {query}");

      var artistalbums = new List<string>();
      var reader = _store.Execute($"select distinct(*.Album) from songs where AlbumArtist = \"{Util.EscapeDatabaseQuery(query)}\"");
      reader.Read();
      var result = reader.Current;
      foreach (var album in result["Album"].AsArray)
      {
        artistalbums.Add(album.ToString().Trim('"'));
      }

      log.Debug($"Found {artistalbums.Count} distinct AlbumArtist Albums");
      return artistalbums;
    }

    public List<string> GetGenres()
    {
      if (_store == null)
      {
        _store = GetDocumentStoreFor(CurrentDatabase);
        if (_store == null)
        {
          log.Error("Could not establish a session.");
          return null;
        }
      }

      log.Trace("Getting distinct genres");

      var genres = new List<string>();
      var reader = _store.Execute("select distinct(*.Genre)  from songs");
      reader.Read();
      var result = reader.Current;
      foreach (var genre in result["Genre"].AsArray)
      {
        genres.Add(genre.ToString().Trim('"'));
      }

      log.Debug($"Found {genres.Count} distinct Genres");
      return genres;
    }

    public List<string> GetGenreArtists(string query)
    {
      if (_store == null)
      {
        _store = GetDocumentStoreFor(CurrentDatabase);
        if (_store == null)
        {
          log.Error("Could not establish a session.");
          return null;
        }
      }

      log.Trace($"Getting distinct genre artists for {query}");
      var genreartists = new List<string>();
      var reader = _store.Execute($"select distinct(*.Artist) from songs where Genre = \"{Util.EscapeDatabaseQuery(query)}\"");
      reader.Read();
      var result = reader.Current;
      foreach (var artist in result["Artist"].AsArray)
      {
        genreartists.Add(artist.ToString().Trim('"'));
      }

      log.Debug($"Found {genreartists.Count} distinct Genre Artists");
      return genreartists;
    }

    public List<string> GetGenreArtistAlbums(string genre, string artist)
    {
      if (_store == null)
      {
        _store = GetDocumentStoreFor(CurrentDatabase);
        if (_store == null)
        {
          log.Error("Could not establish a session.");
          return null;
        }
      }

      log.Trace($"Getting distinct genre artists albums for {genre} and {artist}");
      var genreArtistAlbums = new List<string>();
      var reader = _store.Execute($"select distinct(*.Album) from songs where Genre = \"{Util.EscapeDatabaseQuery(genre)}\" and Artist = \"{Util.EscapeDatabaseQuery(artist)}\"");
      reader.Read();
      var result = reader.Current;
      foreach (var album in result["Album"].AsArray)
      {
        genreArtistAlbums.Add(album.ToString().Trim('"'));
      }

      log.Debug($"Found {genreArtistAlbums.Count} distinct Genre Artist Albums");
      return genreArtistAlbums;
    }

    /// <summary>
    /// Search for Artists to put into Autocompletion Combo
    /// </summary>
    /// <param name="artist"></param>
    /// <returns></returns>
    public List<object> SearchAutocompleteArtists(string artist)
    {
      var artists = new List<object>();

      if (!MusicBrainzDatabaseActive)
        return artists;

      artist = Util.RemoveInvalidChars(artist);
      if (_sqLiteConnection != null)
      {
        var sql = $"select artist,sortartist from artist where artist like '{artist}%' or artist like '% {artist}%'";
        var command = new SQLiteCommand(sql, _sqLiteConnection);
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
          artists.Add(reader["Artist"]);
          if (!reader["Artist"].Equals(reader["SortArtist"]))
          {
            artists.Add(reader["SortArtist"]);
          }
        }
      }
      return artists;
    }

    /// <summary>
    /// Search for the Artist to do auto correction
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public List<string> GetAutoCorrectArtists(string sql)
    {
      var artists = new List<string>();

      if (!MusicBrainzDatabaseActive)
        return artists;

      if (_sqLiteConnection != null)
      {
        var command = new SQLiteCommand(sql, _sqLiteConnection);
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
          artists.Add(reader[0].ToString());
        }
      }
      return artists;
    }

    #endregion

    #region Private Methods

    private void ScanShare_Completed(object sender, RunWorkerCompletedEventArgs e)
    {
      BackgroundWorker bgw = (BackgroundWorker)sender;
      if (e.Cancelled)
      {
        _progressEvent.CurrentFile = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "database_ScanAborted", LocalizeDictionary.Instance.Culture).ToString();
        EventSystem.Publish(_progressEvent);
        log.Info("Database Scan cancelled");
      }
      else if (e.Error != null)
      {
        log.Error("Database Scan failed with {0}", e.Error.Message);
      }
      else
      {
        TimeSpan ts = DateTime.Now - _scanStartTime;
        _progressEvent.CurrentFile = string.Format(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "database_ScanFinished", LocalizeDictionary.Instance.Culture).ToString()
                , _audioFiles, ts.Hours, ts.Minutes, ts.Seconds);
        EventSystem.Publish(_progressEvent);
        log.Info("Database Scan finished");
      }
      bgw.Dispose();
    }

    private void ScanShare_DoWork(object sender, DoWorkEventArgs e)
    {
      _audioFiles = 0;
      _scanStartTime = DateTime.Now;
      var di = new DirectoryInfo((string)e.Argument);
      try
      {
        var col = _store.GetCollection<SongData>("songs");
        col.EnsureIndex("$.FullFileName", true);
        col.EnsureIndex("$.Artist", false);
        col.EnsureIndex("$.AlbumArtist", false);
        col.EnsureIndex("$.Album", false);
        col.EnsureIndex("$.Genre", false);
        col.EnsureIndex("$.Title", false);
        col.EnsureIndex("$.Type", false);
        col.EnsureIndex("$.Composer", false);
        col.EnsureIndex("ArtistAlbum", "[$.Artist, $.Album]", false);
        col.EnsureIndex("AlbumArtistAlbum", "[$.AlbumArtist, $.Album]", false);

        var songList = new List<SongData>();

        foreach (FileInfo fi in GetFiles(di, true))
        {
          if (_bgwScanShare.CancellationPending)
          {
            e.Cancel = true;
            break;
          }

          try
          {
            if (!Util.IsAudio(fi.FullName))
            {
              continue;
            }
            _progressEvent.CurrentFile = $"Reading file {fi.FullName}";
            EventSystem.Publish(_progressEvent);
            var track = Song.Create(fi.FullName);
            if (track != null)
            {
              track = StoreCoverArt(track);
              songList.Add(track);
              _audioFiles++;
              if (_audioFiles % 1000 == 0)
              {
                log.Info($"Number of processed files: {_audioFiles}");
                col.InsertBulk(songList, 1000);
                songList.Clear();
              }
            }
          }
          catch (PathTooLongException)
          {
            continue;
          }
          catch (System.UnauthorizedAccessException)
          {
            continue;
          }
          catch (Exception ex)
          {
            log.Error("Error during Database BulkInsert {0}", ex.Message);
          }
        }
        col.InsertBulk(songList, 1000);
        songList.Clear();
      }
      catch (System.InvalidOperationException ex)
      {
        log.Error("Error during Database BulkInsert {0}", ex.Message);
      }
    }

    private SongData StoreCoverArt(SongData song)
    {
      HashAlgorithm sha = new SHA1CryptoServiceProvider();
      // Check for pictures in the song and add it to the hash list
      // For database objects, the pictures are hashed and stored in the cover art folder
      foreach (Picture picture in song.Pictures)
      {
        string hash = BitConverter.ToString(sha.ComputeHash(picture.Data)).Replace("-", string.Empty);
        song.PictureHashList.Add(hash);
        string fullFileName = $"{_options.StartupSettings.CoverArtFolder}{hash}.png";
        if (!File.Exists(fullFileName))
        {
          try
          {
            Image img = Picture.ImageFromData(picture.Data);
            if (img != null)
            {
              // Need to make a copy, otherwise we have a GDI+ Error
              Bitmap bCopy = new Bitmap(img);
              bCopy.Save(fullFileName, ImageFormat.Png);
            }
          }
          catch (Exception)
          {
            // ignored
          }
        }
      }
      // finally remove the pictures from the database object
      song.Pictures.Clear();
      return song;
    }

    /// <summary>
    ///   Read a Folder and return the files
    /// </summary>
    private IEnumerable<FileInfo> GetFiles(DirectoryInfo dirInfo, bool recursive)
    {
      Queue<DirectoryInfo> directories = new Queue<DirectoryInfo>();
      directories.Enqueue(dirInfo);
      Queue<FileInfo> files = new Queue<FileInfo>();
      while (files.Count > 0 || directories.Count > 0)
      {
        if (files.Count > 0)
        {
          yield return files.Dequeue();
        }
        try
        {
          if (directories.Count > 0)
          {
            DirectoryInfo dir = directories.Dequeue();

            if (recursive)
            {
              DirectoryInfo[] newDirectories = dir.GetDirectories();
              foreach (DirectoryInfo di in newDirectories)
              {
                directories.Enqueue(di);
              }
            }

            FileInfo[] newFiles = dir.GetFiles("*.*");
            foreach (FileInfo file in newFiles)
            {
              files.Enqueue(file);
            }
          }
        }
        catch (UnauthorizedAccessException ex)
        {
          Console.WriteLine(ex.Message, ex);
        }
      }
    }

    #endregion
  }
}
