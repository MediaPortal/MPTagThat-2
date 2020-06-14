#region Copyright (C) 2017 Team MediaPortal
// Copyright (C) 2017 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.MusicDatabase;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Utils;
using Prism.Ioc;
using Raven.Client.Documents;
using Raven.Client.Documents.BulkInsert;
using Raven.Client.Documents.Session;

#endregion

namespace MPTagThat.Core.Common.Song
{
  /// <summary>
  /// This class is used to store a list of Songs, represented as <see cref="MPTagThat.Core.Common.Song.SongData" />.
  /// Based on the amount of songs retrieved, it either stores them in a BindingList or uses
  /// a temporary database created on the fly to prevent Out of Memory issues, when processing a large collection of songs.
  /// </summary>
  public class SongList<T> : IList<T>, INotifyCollectionChanged
  {
    #region Variables

    private readonly IList<T> _list;
    private IDocumentStore _store;
    private IDocumentSession _session;

    private bool _databaseModeEnabled;
    private readonly string _databaseName = "SongsTemp";
    private string _databaseFolder;

    private int _songId;
    private List<string> _dbIdList = new List<string>();

    private int _lastRetrievedSongIndex = -1;
    private T _lastRetrievedSong = default(T);

    private readonly ISettingsManager _settings = ContainerLocator.Current.Resolve<ISettingsManager>();
    private readonly ILogger log = ContainerLocator.Current.Resolve<ILogger>()?.GetLogger;

    #endregion

    #region ctor

    public SongList()
    {
      _list = new List<T>();
    }

    public SongList(IEnumerable<T> collection)
    {
      _list = new List<T>(collection);
    }

    public SongList(int capacity)
    {
      _list = new List<T>(capacity);
    }

    #endregion

    #region IList Implementation

    /// <summary>
    /// Indexer
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public T this[int index]
    {
      get
      {
        if (_databaseModeEnabled)
        {
          if (index == _lastRetrievedSongIndex)
          {
            return _lastRetrievedSong;
          }

          _lastRetrievedSongIndex = index;

          var result = _session.Load<T>(_dbIdList[index]);

          _lastRetrievedSong = result;
          return _lastRetrievedSong;
        }

        return _list[index];
      }
      set
      {
        T originalItem = this[index];
        if (_databaseModeEnabled)
        {
          var result = _session.Load<T>(_dbIdList[index]);

          var song = result;
          song = value;
          _session.Store(song);
          _session.SaveChanges();
        }
        else
        {
          _list[index] = value;
        }

        OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, value, index);
      }
    }

    /// <summary>
    /// Enumerates the items in the list / database
    /// </summary>
    /// <returns></returns>
    public IEnumerator<T> GetEnumerator()
    {
      if (_databaseModeEnabled)
      {
        return _session.Query<T>().GetEnumerator();
      }
      return _list.GetEnumerator();
    }

    /// <summary>
    /// Enumerates the items in the list / database
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// Add a new Item
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item)
    {
      var index = 0;
      if (!_databaseModeEnabled && _list.Count > _settings.GetOptions.StartupSettings.MaxSongs)
      {
        CopyListToDatabase();
      }

      if (_databaseModeEnabled)
      {
        index = _session.Query<SongData>().Count();
        (item as SongData).Id = $"S/{_songId++}";
        _session.Store(item);
        _dbIdList.Add((item as SongData).Id);
      }
      else
      {
        index = _list.Count;
        _list.Add(item);
      }
      OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
    }

    /// <summary>
    /// Clear the list
    /// </summary>
    public void Clear()
    {
      if (_databaseModeEnabled)
      {
        _songId = 0;
        _databaseModeEnabled = false;
        _session?.Advanced.Clear();
        _session = null;
        _store.Dispose();
        _store = null;
        ContainerLocator.Current.Resolve<IMusicDatabase>().RemoveStore(_databaseName);
        _dbIdList.Clear();
      }
      else
      {
        _list.Clear();  
      }
      OnCollectionReset();
    }

    /// <summary>
    /// Check if Item is contained in list
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(T item)
    {
      return _list.Contains(item);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      if (_databaseModeEnabled)
      {
        _session.Query<T>().ToList().CopyTo(array, arrayIndex);
        return;
      }

      _list.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Remove item from list
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Remove(T item)
    {
      var index = _list.IndexOf(item);
      if (index > -1)
      {
        RemoveAt(index);
        return true;
      }

      return false;
    }

    /// <summary>
    /// Return the count of Items in list/database
    /// </summary>
    public int Count
    {
      get
      {
        if (_databaseModeEnabled)
        {
          return _session.Query<T>().Count();
        }

        return _list.Count;
      }
    }

    /// <summary>
    /// List is not readonly
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Lookup the index of the item 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int IndexOf(T item)
    {
      return _list.IndexOf(item);
    }

    /// <summary>
    /// Insert an Item into specific position
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    public void Insert(int index, T item)
    {
      _list.Insert(index, item);
      OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
    }

    /// <summary>
    /// Remove item at specific position
    /// </summary>
    /// <param name="index"></param>
    public void RemoveAt(int index)
    {
      if (_databaseModeEnabled)
      {
        var song = _session.Load<T>(_dbIdList[index]);
        _session.Delete(song);
        _session.SaveChanges();
        _dbIdList.RemoveAt(index);
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, song, index);
      }
      else
      {
        var item = _list[index];
        _list.RemoveAt(index);
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
      }
    }

    #endregion

    #region Database releated methods

    private bool CreateDbConnection()
    {
      if (_session != null)
      {
        return true;
      }

      try
      {
        _databaseFolder = $"{_settings?.GetOptions.StartupSettings.DatabaseFolder}\\{_databaseName}";
        Util.DeleteFolder(_databaseFolder);
        _store = ContainerLocator.Current.Resolve<IMusicDatabase>()?.GetDocumentStoreFor(_databaseName);
        _session = _store?.OpenSession();
        _session.Advanced.MaxNumberOfRequestsPerSession = 10000;

        return true;
      }
      catch (Exception ex)
      {
        log.Error("Error creating DB Connection. Database Mode disabled. {0}", ex.Message);
      }
      return false;
    }


    /// <summary>
    /// The number of allowed objects in the BindingList has been exceeded
    /// Copy all the data to the database
    /// </summary>
    private void CopyListToDatabase()
    {
      log.Debug("Number of Songs in list exceeded the limit. Database mode enabled");

      if (!CreateDbConnection())
      {
        return;
      }

      _dbIdList.Clear();
      _songId = 0;


      _databaseModeEnabled = true;

      using (BulkInsertOperation bulkinsert = _store.BulkInsert())
      {
        foreach (var item in _list)
        {
          var index = _session.Query<SongData>().Count();
          (item as SongData).Id = $"S/{_songId++}";
          bulkinsert.Store(item);
          _dbIdList.Add((item as SongData).Id);
        }
      }

      OnCollectionReset();
      _list.Clear();

      log.Debug("Finished enabling database mode.");
    }

    /// <summary>
    /// Commit the Changes to the Database
    /// </summary>
    public void CommitDatabaseChanges()
    {
      if (_databaseModeEnabled)
      {
        _session?.SaveChanges();
      }
    }
    #endregion

    #region CollectionChanged Implementation 

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    /// <summary>
    /// Helper to raise CollectionChanged event to any listeners
    /// </summary>
    private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
    {
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
    }

    /// <summary>
    /// Helper to raise CollectionChanged event to any listeners
    /// </summary>
    private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
    {
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
    }

    /// <summary>
    /// Helper to raise CollectionChanged event to any listeners
    /// </summary>
    private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
    {
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
    }

    /// <summary>
    /// Helper to raise CollectionChanged event with action == Reset to any listeners
    /// </summary>
    private void OnCollectionReset()
    {
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Raise CollectionChanged event to any listeners.
    /// Properties/methods modifying this ObservableCollection will raise
    /// a collection changed event through this virtual method.
    /// </summary>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
      if (CollectionChanged != null)
      {
        CollectionChanged(this, e);
      }
    }

    #endregion
  }
}
