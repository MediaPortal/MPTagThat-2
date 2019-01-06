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
using System.Linq;
using System.Linq.Expressions;
using Raven.Client.Embedded;
using Raven.Client;
using Raven.Client.Document;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using MPTagThat.Core.Common;
//using MPTagThat.Core.Services.MusicDatabase;
using Raven.Abstractions.Data;
using Raven.Client.Connection;
using Microsoft.Practices.ServiceLocation;
using MPTagThat.Core.Annotations;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Core.Utils;

#endregion

namespace MPTagThat.Core.Common.Song
{
  /// <summary>
  /// This class is used to store a list of Songs, respresented as <see cref="MPTagThat.Core.Common.Song.SongData" />.
  /// Based on the amount of songs reatrieved, it either stores them in a BindingList or uses
  /// a temporary db4o database created on the fly to prevent Out of Memory issues, when processing a large collection of songs.
  /// </summary>
  public class SongList : BindingList<SongData>, IDisposable
  {
    #region Variables

    private readonly string _databaseName = "SongsTemp";
    private string _databaseFolder;

    private bool _databaseModeEnabled = false;

    private IDocumentStore _store;
    private IDocumentSession _session;
    private List<string> _dbIdList = new List<string>();
    private BindingList<SongData> _songList = new BindingList<SongData>();
    private int _trackId = 0;

    private int _lastRetrievedTrackIndex = -1;
    private SongData _lastRetrievedTrack = null;
    private int _countCache = 0;

    private ISettingsManager settings;
    private ILogger log;
    #endregion

    #region ctor / dtor

    public SongList()
    {
      log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger).GetLogger;
      settings = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager);
      _databaseModeEnabled = false;
      _databaseFolder = $"{settings.GetOptions.StartupSettings.DatabaseFolder}{_databaseName}";
    }

    #endregion

    #region Properties

    /// <summary>
    /// Return the count of songs in the list
    /// </summary>
    /// <returns></returns>
    public new int Count
    {
      get
      {
        if (_databaseModeEnabled)
        {
          if (_countCache == 0)
          {
            _countCache = _session.Query<SongData>().Count();
          }
          return _countCache;
        }

        return _songList.Count;
      }
    }

    #endregion

    #region Indexer

    /// <summary>
    /// Implementation of indexer
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public new object this[int i]
    {
      get
      {
        if (_databaseModeEnabled)
        {
          if (i == _lastRetrievedTrackIndex)
          {
            return _lastRetrievedTrack;
          }

          _lastRetrievedTrackIndex = i;

          var result = _session.Load<SongData>(_dbIdList[i]);

          _lastRetrievedTrack = result;
          return _lastRetrievedTrack;
        }

        return _songList[i];
      }
      set
      {
        if (_databaseModeEnabled)
        {
          var result = _session.Load<SongData>(_dbIdList[i]);

          var track = result;
          track = (SongData)value;
          _session.Store(track);
          _session.SaveChanges();
        }
        else
        {
          _songList[i] = (SongData)value;
        }
      }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adding of new songs to the list
    /// </summary>
    /// <param name="track"></param>
    public int Add(object track)
    {
      if (!_databaseModeEnabled && _songList.Count > settings.GetOptions.StartupSettings.MaxSongs)
      {
        CopyLIstToDatabase();
      }

      if (_databaseModeEnabled)
      {
        (track as SongData).Id = $"SongDatas/{_trackId++.ToString()}";
        _session.Store(track);
        _dbIdList.Add((track as SongData).Id);
      }
      else
      {
        _songList.Add((SongData)track);
      }

      return 1;
    }


    public void CommitDatabaseChanges()
    {
      if (_databaseModeEnabled)
      {
        _session.SaveChanges();
      }
    }

    /// <summary>
    /// Removes the object at the specified index
    /// </summary>
    /// <param name="index"></param>
    public new void RemoveAt(int index)
    {
      if (_databaseModeEnabled)
      {
        var track = _session.Load<SongData>(_dbIdList[index]);
        _session.Delete(track);
        _session.SaveChanges();
        _dbIdList.RemoveAt(index);
      }
      else
      {
        _songList.RemoveAt(index);
      }
    }

    public bool Contains(object value)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Clear the list
    /// </summary>
    public new void Clear()
    {
      if (_databaseModeEnabled)
      {
        _trackId = 0;
        _databaseModeEnabled = false;
        _session?.Advanced.Clear();
        _session = null;
        _store.Dispose();
        _store = null;
        //ServiceScope.Get<IMusicDatabase>().RemoveStore(_databaseName);
        _dbIdList.Clear();
      }
      else
      {
        _songList.Clear();
      }
    }

    /// <summary>
    /// Apply Sorting
    /// </summary>
    /// <param name="property"></param>
    /// <param name="direction"></param>
    public void Sort(PropertyDescriptor property, ListSortDirection direction)
    {
      if (_databaseModeEnabled)
      {
        // Build the Linq Expression tree for sorting
        string sortFieldName = property.Name;
        string sortMethod = "OrderBy";
        if (direction.ToString() == "Descending")
        {
          sortMethod = "OrderbyDescending";
        }

        var queryableData = _session.Query<SongData>();
        var type = typeof(SongData);
        var prop = type.GetProperty(sortFieldName);
        var parameter = Expression.Parameter(type, "p");
        var propertyAccess = Expression.MakeMemberAccess(parameter, prop);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var queryExpr = Expression.Call(typeof(Queryable), sortMethod,
                                                new[] { type, property.PropertyType },
                                                queryableData.Expression, Expression.Quote(orderByExpression));


        var result = queryableData.Provider.CreateQuery<SongData>(queryExpr);

        _dbIdList.Clear();
        foreach (SongData dataObject in result)
        {
          _dbIdList.Add(dataObject.Id);
        }
      }
    }

    #endregion

    #region Private Methods

    private bool CreateDbConnection()
    {
      if (_store != null)
      {
        return true;
      }

      try
      {
        Util.DeleteFolder(_databaseFolder);
        //_store = ServiceScope.Get<IMusicDatabase>().GetDocumentStoreFor(_databaseName);
        _session = _store.OpenSession();
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
    private void CopyLIstToDatabase()
    {
      log.Debug("Number of Songs in list exceeded the limit. Database mode enabled");

      if (!CreateDbConnection())
      {
        return;
      }

      _dbIdList.Clear();
      _trackId = 0;

      BulkInsertOptions bulkInsertOptions = new BulkInsertOptions
      {
        BatchSize = 1000,
        OverwriteExisting = true
      };

      using (BulkInsertOperation bulkInsert = _store.BulkInsert(null, bulkInsertOptions))
      {
        foreach (SongData track in _songList)
        {
          track.Id = $"SongDatas/{_trackId++.ToString()}";
          bulkInsert.Store(track);
          _dbIdList.Add(track.Id);
        }
      }

      _songList.Clear();
      _databaseModeEnabled = true;
      log.Debug("Finished enabling database mode.");
    }

    #endregion

    #region Interfaces

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          if (_store != null && !_store.WasDisposed)
          {
            _session?.Dispose();
            _store.Dispose();
            Util.DeleteFolder(_databaseName);
          }
        }
        disposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
    }
    #endregion

    #endregion

    public PropertyDescriptor SortProperty { get; }
    public ListSortDirection SortDirection { get; }
  }
}
