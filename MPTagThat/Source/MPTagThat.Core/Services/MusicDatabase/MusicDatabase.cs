﻿#region Copyright (C) 2022 Team MediaPortal
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

using LiteDB;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Core.Utils;
using Prism.Ioc;
using Syncfusion.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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
    private SQLiteConnection _sqLiteConnection;

    private BackgroundWorker _bgwScanShare;
    private int _audioFiles;
    private DateTime _scanStartTime;

    private readonly Dictionary<string, LiteDatabase> _stores = new Dictionary<string, LiteDatabase>();
    private readonly StatusBarEvent _progressEvent = new StatusBarEvent { Type = StatusBarEvent.StatusTypes.CurrentFile };
    private readonly DatabaseScanEvent _databaseScanEvent = new DatabaseScanEvent();

    private readonly string[] _indexedTags = { "artist", "albumartist", "album", "composer", "genre", "title", "type", "fullfilename" };
    private readonly Regex _queryRegex = new Regex("\\w+?(\\s?\\:|\\s?=|\\s?!=)\\\"?.+?\\\"?(?=(\\sAND|\\sOR|\\s&|\\s\\||$))");

    #endregion

    #region ctor / dtor

    public MusicDatabase()
    {
      CurrentDatabase = _defaultMusicDatabaseName;
      if (_options.MainSettings.LastUsedMusicDatabase != "")
      {
        CurrentDatabase = _options.MainSettings.LastUsedMusicDatabase;
      }

      // Open Connection to the SQLite Database with the MusicBrainz artists
      MusicBrainzDatabaseActive = false;
      var zipfile = $"{_options.StartupSettings.DatabaseFolder}\\MusicBrainzArtists.zip";
      if (File.Exists(zipfile))
      {
        log.Info("Found a zipped MusicBrainz Artist Database");
        if (File.Exists($"{_options.StartupSettings.DatabaseFolder}\\MusicBrainzArtists.db3"))
        {
          File.Delete($"{_options.StartupSettings.DatabaseFolder}\\MusicBrainzArtists.db3");
        }
        System.IO.Compression.ZipFile.ExtractToDirectory(zipfile, _options.StartupSettings.DatabaseFolder);
        File.Delete(zipfile);
      }

      var database = $"{_options.StartupSettings.DatabaseFolder}\\MusicBrainzArtists.db3";
      OpenAutoCorrectDatabase(database);
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
        _options.MainSettings.LastUsedMusicDatabase = CurrentDatabase;

        // Notify the treeview about database change to refresh

        GenericEvent evt = new GenericEvent
        {
          Action = "toggledatabaseview"
        };
        EventSystem.Publish(evt);

        evt = new GenericEvent
        {
          Action = "currentfolderchanged"
        };
        EventSystem.Publish(evt);

        evt = new GenericEvent
        {
          Action = "activedatabasechanged"
        };
        evt.MessageData.Add("database", CurrentDatabase);
        EventSystem.Publish(evt);

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
      else if (_indexedTags.Any(query.ToLower().StartsWith))
      {
        sql = BuildWhere(query);
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
      result = resultSet.OrderBy(x => x.DiscNumber).ThenBy(x => x.TrackNumber).ThenBy(x => x.Artist).ToList();

      msg.CurrentProgress = 0;
      msg.NumberOfFiles = result.Count;
      EventSystem.Publish(msg);

      return result;
    }


    /// <summary>
    /// Build a valid select statement
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public string BuildWhere(string query)
    {
      var sql = "select $ from songs where ";

      var matches = _queryRegex.Matches(query);
      foreach (Match match in matches)
      {
        var fullString = match.Groups[0].Value;
        var comparator = match.Groups[1].Value.Trim();
        var op = match.Groups[2].Value.Trim();
        var token = new string[2];
        if ((comparator.Length > 1))
        {
          token[0] = fullString.Substring(0, fullString.IndexOf(comparator, StringComparison.Ordinal) - 1);
          token[1] = fullString.Substring(fullString.IndexOf(comparator, StringComparison.Ordinal) + 2).Trim();
        }
        else
        {
          token = fullString.Split(Convert.ToChar(comparator));
        }

        if (!_indexedTags.Any(token[0].ToLower().Contains))
        {
          log.Info($"Invalid Token {token} used. Query not executed");
          break;
        }

        sql += token[0];
        if (comparator == ":")
        {
          sql += " like " + "'%" + token[1] + "%'";
        }
        else
        {
          sql += " " + comparator + " " + "'" + token[1] + "'";
        }

        if (op != "")
        {
          sql += " " + op + " ";
        }
      }
      return sql;
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
        //originalFileName = Util.EscapeDatabaseQuery(originalFileName);
        // Lookup the song in the database
        var col = _store.GetCollection<SongData>("songs");
        var originalSong = col.FindOne(s => s.FullFileName.Equals(originalFileName));
        if (originalSong != null)
        {
          song.Status = -1;
          song.Changed = false;
          song = StoreCoverArt(song);
          song.Id = originalSong.Id;
          col.Upsert(song);
        }
      }
      catch (Exception ex)
      {
        log.Error("UpdateTrack: Error updating song in database {0}: {1}", ex.Message, ex.InnerException?.ToString());
      }
    }

    /// <summary>
    /// Get Distinct Artists to be shown in TreeView
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Get Albums for selected Artist to be shown in TreeView
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Get Distinct AlbumArtists to be shown in TreeView
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Get Albums for selected AlbumArtist to be shown in TreeView
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Get Distinct Albums to be shown in TreeView
    /// </summary>
    /// <returns></returns>
    public List<string> GetAlbums()
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

      log.Trace("Getting distinct albums");

      var albums = new List<string>();
      var reader = _store.Execute("select distinct(*.Album)  from songs");
      reader.Read();
      var result = reader.Current;
      foreach (var album in result["Album"].AsArray)
      {
        albums.Add(album.ToString().Trim('"'));
      }

      log.Debug($"Found {albums.Count} distinct albums");
      return albums;
    }

    /// <summary>
    /// Get Distinct Genres to be shown in TreeView
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Get Artists for selected Genre to be shown in TreeView
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Get Albums for selected Genre and Artist to be shown in TreeView
    /// </summary>
    /// <returns></returns>
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
    /// Get Database Counts of various Items
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public int GetCount(string key)
    {
      if (_store == null)
      {
        _store = GetDocumentStoreFor(CurrentDatabase);
        if (_store == null)
        {
          log.Error("Could not establish a session.");
          return 0;
        }
      }

      log.Trace($"Getting count of {key}");

      var meta = "Songs";
      switch (key.ToLower())
      {
        case "albumartist":
          meta = "AlbumArtist";
          break;

        case "artist":
          meta = "Artist";
          break;

        case "album":
          meta = "Album";
          break;

        case "genre":
          meta = "Genre";
          break;
      }


      var genres = new List<string>();
      var query = $"select count(distinct(*.{meta})) as Counter  from songs";
      if (meta == "Songs")
      {
        query = $"select count(*) as Counter  from songs";
      }

      var reader = _store.Execute(query);
      reader.Read();
      var result = reader.Current;
      return result["Counter"].AsInt32;
    }


    /// <summary>
    /// Get a list of all Items in the TagChecker Collection
    /// </summary>
    /// <returns></returns>
    public List<TagCheckerData> GetTagCheckerItems(string itemType)
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

      log.Trace("Getting Items for the TagChecker Collection");
      var col = _store.GetCollection<TagCheckerData>($"tagchecker_{itemType}");
      var result = col.FindAll();
      var items = result.ToList();

      return items;
    }

    /// <summary>
    /// Add the Items found to the TagChecker Database
    /// </summary>
    /// <param name="items"></param>
    /// <param name="itemType"></param>
    public void AddItemsToTagCheckerDatabase(ref ObservableCollection<TagCheckerData> items, string itemType)
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

      log.Trace("Adding Items to the TagChecker Collection");
      var col = _store.GetCollection<TagCheckerData>($"tagchecker_{itemType}");
      col.EnsureIndex("$.OriginalItem", false);
      col.EnsureIndex("$.Status", false);

      // Remove all entries from the collection
      col.DeleteAll();

      _store.BeginTrans();

      foreach (var item in items)
      {
        col.Upsert(item);
      }

      _store.Commit();
    }

    /// <summary>
    /// Add the Items found to the TagChecker Database
    /// </summary>
    /// <param name="items"></param>
    /// <param name="itemType"></param>
    public void UpdateTagCheckerDatabase(ref List<TagCheckerData> items, string itemType)
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

      log.Trace("Updating Items in the TagChecker Collection");
      var col = _store.GetCollection<TagCheckerData>($"tagchecker_{itemType}");
      _store.BeginTrans();

      foreach (var item in items)
      {
        var dbItem = col.FindOne(i => i.OriginalItem.Equals(item.OriginalItem));
        if (dbItem != null)
        {
          item.Id = dbItem.Id;
          item.Changed = false;
          col.Upsert(item);
        }
      }

      _store.Commit();
    }

    /// <summary>
    /// Update a single Item in the TagChecker Database
    /// </summary>
    /// <param name="item"></param>
    /// <param name="itemType"></param>
    public void UpdateTagCheckerItem(TagCheckerData item, string itemType)
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

      var col = _store.GetCollection<TagCheckerData>($"tagchecker_{itemType}");
      var dbItem = col.FindOne(i => i.OriginalItem.Equals(item.OriginalItem));
      if (dbItem != null)
      {
        item.Id = dbItem.Id;
        item.Changed = false;
        col.Upsert(item);
      }
    }

    /// <summary>
    /// Search for Artists to put into Autocompletion Combo
    /// </summary>
    /// <param name="artist"></param>
    /// <returns></returns>
    public List<string> SearchAutocompleteArtists(string artist)
    {
      var artists = new List<string>();

      if (!MusicBrainzDatabaseActive)
        return artists;

      artist = Util.RemoveInvalidChars(artist);
      if (_sqLiteConnection != null)
      {
        var sql = $"select name,sortname from artist where name like '{artist}%' or name like '% {artist}%'";
        var command = new SQLiteCommand(sql, _sqLiteConnection);
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
          artists.Add(reader["Name"].ToString());
          if (!reader["Name"].Equals(reader["SortName"]))
          {
            artists.Add(reader["SortName"].ToString());
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

    /// <summary>
    /// Close the AutoCorrect Database
    /// </summary>
    public void CloseAutoCorrectDatabase()
    {
      if (_sqLiteConnection != null)
      {
        _sqLiteConnection.Close();
        _sqLiteConnection.Dispose();
        _sqLiteConnection = null;
      }
    }

    public void OpenAutoCorrectDatabase(string database)
    {
      CloseAutoCorrectDatabase();
      if (File.Exists(database))
      {
        log.Info("Opening MusicBrainz Artist Database");
        _sqLiteConnection = new SQLiteConnection($"Data Source={database}");
        _sqLiteConnection?.Open();
        MusicBrainzDatabaseActive = true;
      }
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
        _databaseScanEvent.CurrentFolder = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "database_ScanAborted", LocalizeDictionary.Instance.Culture).ToString();
        _databaseScanEvent.CurrentFile = "";
        _databaseScanEvent.NumberOfFiles = _audioFiles;
        EventSystem.Publish(_databaseScanEvent);
        log.Info("Database Scan cancelled");
      }
      else if (e.Error != null)
      {
        log.Error("Database Scan failed with {0}", e.Error.Message);
      }
      else
      {
        TimeSpan ts = DateTime.Now - _scanStartTime;
        var msg = string.Format(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "database_ScanFinished", LocalizeDictionary.Instance.Culture).ToString()
          , _audioFiles, ts.Hours, ts.Minutes, ts.Seconds);
        _progressEvent.CurrentFile = msg;
        EventSystem.Publish(_progressEvent);
        _databaseScanEvent.CurrentFolder = msg;
        _databaseScanEvent.CurrentFile = "";
        _databaseScanEvent.NumberOfFiles = _audioFiles;
        EventSystem.Publish(_databaseScanEvent);
        log.Info($"Database Scan finished. Processed {_audioFiles} songs in {ts.Hours}:{ts.Minutes}:{ts.Seconds}");
      }
      bgw.Dispose();
      _options.IsDatabaseScanActive = false;
    }

    private void ScanShare_DoWork(object sender, DoWorkEventArgs e)
    {
      _options.IsDatabaseScanActive = true;
      _audioFiles = 0;
      _scanStartTime = DateTime.Now;
      var di = new DirectoryInfo((string)e.Argument);
      try
      {
        var colLastScan = _store.GetCollection("lastScan");
        var lastUpdate = colLastScan.FindOne("$._id = 1");
        var lastUpdateTime = DateTime.MinValue;
        if (lastUpdate != null)
        {
          lastUpdateTime = lastUpdate["LastScan"];
          log.Info($"Scanning share for songs changed after {lastUpdateTime.ToString()}");
        }

        var col = _store.GetCollection<SongData>("songs");
        col.EnsureIndex("$.FullFileName", false);
        col.EnsureIndex("$.Artist", false);
        col.EnsureIndex("$.AlbumArtist", false);
        col.EnsureIndex("$.Album", false);
        col.EnsureIndex("$.Genre", false);
        col.EnsureIndex("$.Title", false);
        col.EnsureIndex("$.Type", false);
        col.EnsureIndex("$.Composer", false);
        //col.EnsureIndex("ArtistAlbum", "[$.Artist, $.Album]", false);
        //col.EnsureIndex("AlbumArtistAlbum", "[$.AlbumArtist, $.Album]", false);

        var doFullScan = lastUpdateTime == DateTime.MinValue;

        _store.BeginTrans();

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
            _databaseScanEvent.CurrentFolder = fi.DirectoryName;
            _databaseScanEvent.CurrentFile = fi.Name;
            _databaseScanEvent.NumberOfFiles = _audioFiles;
            EventSystem.Publish(_databaseScanEvent);

            if (!doFullScan && !(fi.CreationTime > lastUpdateTime || fi.LastWriteTime > lastUpdateTime))
            {
              continue;
            }

            var track = Song.Create(fi.FullName);
            if (track != null)
            {
              track = StoreCoverArt(track);

              if (!doFullScan)
              {
                // Search, if there is already a file in the database
                var originalSong = col.FindOne(s => s.FullFileName.Equals(fi.FullName));
                if (originalSong != null)
                {
                  track.Status = -1;
                  track.Changed = false;
                  track.Id = originalSong.Id;
                }
              }
              col.Upsert(track);
              _audioFiles++;
              if (_audioFiles % 1000 == 0)
              {
                log.Info($"Number of processed files: {_audioFiles}");
                _store.Commit();
                _store.BeginTrans();
              }
            }
          }
          catch (PathTooLongException)
          {
            log.Error("Path too long for {0}", fi.FullName);
            continue;
          }
          catch (System.UnauthorizedAccessException)
          {
            continue;
          }
          catch (Exception ex)
          {
            log.Error("Error during Database BulkInsert {1} {0}", ex.Message, fi.FullName);
          }
        }

        colLastScan.Upsert(new BsonDocument { ["_id"] = 1, ["LastScan"] = _scanStartTime.ToDateTime() });
        _store.Commit();
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
              log.Trace(dir.FullName);
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
        catch (PathTooLongException exPath)
        {
          log.Error("Path too long: {0} {1}", exPath.Message, exPath.StackTrace);
          continue;
        }
        catch (UnauthorizedAccessException ex)
        {
          log.Error("Unauthorised access {0}", ex.Message);
        }
      }
    }

    #endregion
  }
}
