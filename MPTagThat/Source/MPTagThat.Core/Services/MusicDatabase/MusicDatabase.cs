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
using System.Threading.Tasks;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.MusicDatabase.Indexes;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Core.Utils;
using Prism.Ioc;
using Raven.Client.Documents;
using Raven.Client.Documents.BulkInsert;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;
using Raven.Embedded;
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
    private IDocumentStore _store;
    private IDocumentSession _session;

    private BackgroundWorker _bgwScanShare;
    private int _audioFiles;
    private DateTime _scanStartTime;

    private readonly Dictionary<string, IDocumentStore> _stores = new Dictionary<string, IDocumentStore>();
    private readonly StatusBarEvent _progressEvent = new StatusBarEvent { Type = StatusBarEvent.StatusTypes.CurrentFile };
    private readonly SQLiteConnection _sqLiteConnection;

    private string[] _indexedTags = {"Artist:", "AlbumArtist:", "Album:", "Composer:", "Genre:", "Title:", "Type:", "FullFileName:"};
    #endregion

    #region ctor / dtor

    public MusicDatabase()
    {
      CurrentDatabase = _defaultMusicDatabaseName;

      // Open Connection to the SQLite Database with the MusicBrainz artists
      MusicBrainzDatabaseActive = false;
      if (File.Exists(@"bin\\MusicBrainzArtists.db3"))
      {
        log.Info("OPening MusicBrainz Artist Database");
        _sqLiteConnection = new SQLiteConnection("Data Source=bin\\MusicBrainzArtists.db3");
        _sqLiteConnection?.Open();
        MusicBrainzDatabaseActive = true;
      }
    }

    ~MusicDatabase()
    {
      if (_store != null && !_store.WasDisposed)
      {
        _session?.Dispose();
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
    public IDocumentStore GetDocumentStoreFor(string databaseName)
    {
      log.Trace($"Getting database store for {databaseName}");
      if (_stores.ContainsKey(databaseName))
      {
        return _stores[databaseName];
      }

      _stores.Add(databaseName, CreateDocumentStore(databaseName).Result);
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

      if (_store == null && !CreateDbConnection())
      {
        log.Error("Database Scan aborted.");
        return;
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
      log.Trace($"Deleteing database {databaseName}");
      _session?.Advanced.Clear();
      _session = null;
      _store?.Dispose();
      _store = null;
      RemoveStore(databaseName);
      var dbPath = $"{_options.StartupSettings.DatabaseFolder}Databases\\{databaseName}";
      Util.DeleteFolder(dbPath);
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
      _session?.Advanced.Clear();
      _session = null;
      _store?.Dispose();
      _store = null;
      RemoveStore(CurrentDatabase);
      CurrentDatabase = dbName;
      if (CreateDbConnection())
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

      if (_store == null && !CreateDbConnection())
      {
        log.Error("Could not establish a session.");
        return null;
      }

      StatusBarEvent msg = new StatusBarEvent { CurrentFolder = "", CurrentProgress = -1 };
      msg.NumberOfFiles = 0;
      EventSystem.Publish(msg);

      List<SongData> result = null;

      query = FormatQuery(query);

      if (query.Contains(":"))
      {
        var resultSet = _session.Advanced.DocumentQuery<SongData, DefaultSearchIndex>()
          .WhereLucene("Artist",query)
          .Take(int.MaxValue)
          .ToList();

        log.Info($"Query returned {resultSet.Count} results");

        // need to do our own ordering
        result = resultSet.OrderBy(x => x.Artist).ThenBy(x => x.Album).ThenBy(x => x.Track).ToList();
      }
      else
      {
        var searchText = new List<object>();
        searchText.AddRange(query.Split(new char[] { ' ' }));
        var resultSet = _session.Advanced.DocumentQuery<SongData, DefaultSearchIndex>()
          .ContainsAll("Query", searchText)
          .Take(int.MaxValue)
          .ToList();

        log.Info($"Query returned {resultSet.Count} results");

        // need to do our own ordering
        result = resultSet.OrderBy(x => x.Artist).ThenBy(x => x.Album).ThenBy(x => x.Track).ToList();
      }

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
      if (_store == null && !CreateDbConnection())
      {
        log.Error("Could not establish a session.");
        return;
      }

      try
      {
        originalFileName = Util.EscapeDatabaseQuery(originalFileName);
        // Lookup the song in the database
        var dbTracks = _session.Advanced.DocumentQuery<SongData, DefaultSearchIndex>().WhereEquals("FullFileName", originalFileName).ToList();
        if (dbTracks.Count > 0)
        {
          song.Id = dbTracks[0].Id;
          _session.Advanced.Evict(dbTracks[0]); // Release reference
          // Reset status
          song.Status = -1;
          song.Changed = false;
          song = StoreCoverArt(song);

          _session.Store(song);
          _session.SaveChanges();
        }
      }
      catch (Exception ex)
      {
        log.Error("UpdateTrack: Error updating song in database {0}: {1}", ex.Message, ex.InnerException?.ToString());
      }
    }


    public List<DistinctResult> GetArtists()
    {
      if (_store == null && !CreateDbConnection())
      {
        log.Error("Could not establish a session.");
        return null;
      }

      log.Trace("Getting distinct artists");
      var artists = _session.Query<DistinctResult, DistinctArtistIndex>()
        .Take(int.MaxValue)
        .OrderBy(x => x.Name)
        .ToList();

      log.Debug($"Found {artists.Count} distinct artists");
      return artists;
    }

    public List<DistinctResult> GetArtistAlbums(string query)
    {
      if (_store == null && !CreateDbConnection())
      {
        log.Error("Could not establish a session.");
        return null;
      }

      log.Trace($"Getting distinct artist albums for {query}");
      var artistalbums = _session.Query<DistinctResult, DistinctArtistAlbumIndex>()
        .Where(x => x.Name == Util.EscapeDatabaseQuery(query))
        .Take(int.MaxValue)
        .OrderBy(x => x.Name)
        .ThenBy(x => x.Album)
        .ToList();

      log.Debug($"Found {artistalbums.Count} distinct Artist Albums");
      return artistalbums;
    }

    public List<DistinctResult> GetAlbumArtists()
    {
      if (_store == null && !CreateDbConnection())
      {
        log.Error("Could not establish a session.");
        return null;
      }

      log.Trace("Getting distinct album artists");
      var albumartists = _session.Query<DistinctResult, DistinctAlbumArtistIndex>()
        .Take(int.MaxValue)
        .OrderBy(x => x.Name)
        .ToList();

      log.Debug($"Found {albumartists.Count} distinct album artists");
      return albumartists;
    }

    public List<DistinctResult> GetAlbumArtistAlbums(string query)
    {
      if (_store == null && !CreateDbConnection())
      {
        log.Error("Could not establish a session.");
        return null;
      }

      log.Trace($"Getting distinct albumartist  albums for {query}");
      var artistalbums = _session.Query<DistinctResult, DistinctAlbumArtistAlbumIndex>()
        .Where(x => x.Name == Util.EscapeDatabaseQuery(query))
        .Take(int.MaxValue)
        .OrderBy(x => x.Name)
        .ThenBy(x => x.Album)
        .ToList();

      log.Debug($"Found {artistalbums.Count} distinct AlbumArtist Albums");
      return artistalbums;
    }

    public List<DistinctResult> GetGenres()
    {
      if (_store == null && !CreateDbConnection())
      {
        log.Error("Could not establish a session.");
        return null;
      }

      log.Trace("Getting distinct genres");
      var genres = _session.Query<DistinctResult, DistinctGenreIndex>()
        .Take(int.MaxValue)
        .OrderBy(x => x.Genre)
        .ToList();

      log.Debug($"Found {genres.Count} distinct Genres");
      return genres;
    }

    public List<DistinctResult> GetGenreArtists(string query)
    {
      if (_store == null && !CreateDbConnection())
      {
        log.Error("Could not establish a session.");
        return null;
      }

      log.Trace($"Getting distinct genre artists for {query}");
      var genreartists = _session.Query<DistinctResult, DistinctGenreArtistIndex>()
        .Where(x => x.Genre == Util.EscapeDatabaseQuery(query))
        .Take(int.MaxValue)
        .OrderBy(x => x.Genre)
        .ThenBy(x => x.Name)
        .ToList();

      log.Debug($"Found {genreartists.Count} distinct Genre Artists");
      return genreartists;
    }

    public List<DistinctResult> GetGenreArtistAlbums(string genre, string artist)
    {
      if (_store == null && !CreateDbConnection())
      {
        log.Error("Could not establish a session.");
        return null;
      }

      log.Trace($"Getting distinct genre artists albums for {genre} and {artist}");
      var genreartists = _session.Query<DistinctResult, DistinctGenreArtistAlbumIndex>()
        .Where(x => x.Genre == Util.EscapeDatabaseQuery(genre) && x.Name == Util.EscapeDatabaseQuery(artist))
        .Take(int.MaxValue)
        .OrderBy(x => x.Genre)
        .ThenBy(x => x.Name)
        .ThenBy(x => x.Album)
        .ToList();

      log.Debug($"Found {genreartists.Count} distinct Genre Artist Albums");
      return genreartists;
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

    /// <summary>
    /// Creates a Database Connection and establishes a session
    /// </summary>
    /// <returns></returns>
    private bool CreateDbConnection()
    {
      if (_store != null)
      {
        return true;
      }

      try
      {
        _store = GetDocumentStoreFor(CurrentDatabase);
        log.Trace("Opening database session");
        _session = _store.OpenSession();

        log.Trace("Creating indices");
        IndexCreation.CreateIndexes(typeof(DefaultSearchIndex).Assembly, _store);
        IndexCreation.CreateIndexes(typeof(DistinctArtistIndex).Assembly, _store);
        IndexCreation.CreateIndexes(typeof(DistinctArtistAlbumIndex).Assembly, _store);
        IndexCreation.CreateIndexes(typeof(DistinctAlbumArtistIndex).Assembly, _store);
        IndexCreation.CreateIndexes(typeof(DistinctAlbumArtistAlbumIndex).Assembly, _store);
        IndexCreation.CreateIndexes(typeof(DistinctGenreIndex).Assembly, _store);
        IndexCreation.CreateIndexes(typeof(DistinctGenreArtistIndex).Assembly, _store);
        IndexCreation.CreateIndexes(typeof(DistinctGenreArtistAlbumIndex).Assembly, _store);
        log.Trace("Finished creating indices");

        return true;
      }
      catch (Exception ex)
      {
        log.Error("Error creating DB Connection. {0}", ex.Message);
      }

      return false;
    }

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
                ,_audioFiles, ts.Hours, ts.Minutes, ts.Seconds);
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
        using (BulkInsertOperation bulkInsert = _store.BulkInsert())
        {
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
                bulkInsert.Store(track);
                _audioFiles++;
                if (_audioFiles % 1000 == 0)
                {
                  log.Info($"Number of processed files: {_audioFiles}");
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
        }
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

    /// <summary>
    /// Starts the Raven Database Server
    /// </summary>
    private void StartDatabaseServer()
    {
      log.Info($"Starting database server in folder {_options.StartupSettings.DatabaseFolder}");
      EmbeddedServer.Instance.StartServer(new ServerOptions
      {
        DataDirectory = $"{_options.StartupSettings.DatabaseFolder}",
        ServerUrl = "http://127.0.0.1:8080"
      });
      DatabaseEngineStarted = true;
    }

    /// <summary>
    /// Creates a Raven Document Store
    /// </summary>
    /// <param name="databaseName"></param>
    /// <returns></returns>
    private async Task<IDocumentStore> CreateDocumentStore(string databaseName)
    {
      if (!DatabaseEngineStarted)
      {
        StartDatabaseServer();
      }

      var docStore = EmbeddedServer.Instance.GetDocumentStore(databaseName);
        
      log.Trace("Initializing database store");
      docStore.Initialize();
      return docStore;
    }

    /// <summary>
    /// Format the query for Lucene syntax
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    private string FormatQuery(string query)
    {
      // And / Or operators need to be all uppercase
      query = Regex.Replace(query, " and ", " AND ", RegexOptions.IgnoreCase);
      query = Regex.Replace(query, " or ", " OR ", RegexOptions.IgnoreCase);

      // Change the indexed Tags to first letter Upper case
      foreach (var tag in _indexedTags)
      {
        query = Regex.Replace(query, tag, tag, RegexOptions.IgnoreCase);
      }

      return query;
    }

    #endregion
  }
}
