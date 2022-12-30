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

using System;
using System.Windows.Input;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.MusicDatabase;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using Prism.Ioc;
using WPFLocalizeExtension.Engine;

namespace MPTagThat.Dialogs.ViewModels
{
  public class DatabaseStatusViewModel : DialogViewModelBase
  {
    #region Variables

    private readonly IMusicDatabase _musicDb = ContainerLocator.Current.Resolve<IMusicDatabase>();

    #endregion

    #region Properties

    /// <summary>
    /// Property to show the number of files
    /// </summary>
    private string _numberOfFiles = "0";
    public string NumberOfFiles
    {
      get => _numberOfFiles;
      set => SetProperty(ref _numberOfFiles, value);
    }

    /// <summary>
    /// Property to show the current Folder
    /// </summary>
    private string _currentFolder = "";
    public string CurrentFolder
    {
      get => _currentFolder;
      set => SetProperty(ref _currentFolder, value);
    }

    /// <summary>
    /// Property to show the current File
    /// </summary>
    private string _currentFile = "";
    public string CurrentFile
    {
      get => _currentFile;
      set => SetProperty(ref _currentFile, value);
    }

    /// <summary>
    /// Property to show the number of Artists
    /// </summary>
    private int _numArtists =0;
    public int NumArtists
    {
      get => _numArtists;
      set => SetProperty(ref _numArtists, value);
    }

    /// <summary>
    /// Property to show the number of Album Artists
    /// </summary>
    private int _numAlbumArtists =0;
    public int NumAlbumArtists
    {
      get => _numAlbumArtists;
      set => SetProperty(ref _numAlbumArtists, value);
    }

    /// <summary>
    /// Property to show the number of Albums
    /// </summary>
    private int _numAlbums =0;
    public int NumAlbums
    {
      get => _numAlbums;
      set => SetProperty(ref _numAlbums, value);
    }

    /// <summary>
    /// Property to show the number of Genres
    /// </summary>
    private int _numGenres =0;
    public int NumGenres
    {
      get => _numGenres;
      set => SetProperty(ref _numGenres, value);
    }

    /// <summary>
    /// Property to show the total number of Songs
    /// </summary>
    private int _numSongss =0;
    public int NumSongs
    {
      get => _numSongss;
      set => SetProperty(ref _numSongss, value);
    }

    /// <summary>
    /// Indicates if database Scan is active
    /// </summary>
    public bool IsDatabaseScanActive
    {
      get => _options.IsDatabaseScanActive;
    }

    #endregion

    #region ctor

    public DatabaseStatusViewModel()
    {
      Title = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "databaseStatus_Header",
        LocalizeDictionary.Instance.Culture).ToString();
      EventSystem.Subscribe<DatabaseScanEvent>(UpdateScanStatus);
      
      NumAlbumArtists = _musicDb.GetCount("AlbumArtist");
      NumArtists = _musicDb.GetCount("Artist");
      NumAlbums = _musicDb.GetCount("Album");
      NumGenres = _musicDb.GetCount("Genre");
      NumSongs = _musicDb.GetCount("Songs");

      UpdateStatsCommand = new BaseCommand(GetDatabaseCounts);
    }

    #endregion

    #region Private Methods

    public ICommand UpdateStatsCommand { get; }

    /// <summary>
    /// Get the Database counts
    /// </summary>
    /// <param name="parms"></param>
    private void GetDatabaseCounts(object parns)
    {
      NumAlbumArtists = _musicDb.GetCount("AlbumArtist");
      NumArtists = _musicDb.GetCount("Artist");
      NumAlbums = _musicDb.GetCount("Album");
      NumGenres = _musicDb.GetCount("Genre");
      NumSongs = _musicDb.GetCount("Songs");
    }
    
    #endregion

    #region Event Handling

    /// <summary>
    /// Update the status bar with information from the StatusBar Event
    /// </summary>
    /// <param name="msg"></param>
    private void UpdateScanStatus(DatabaseScanEvent msg)
    {
      if (msg.CurrentFolder.Length > 60)
      {
        CurrentFolder = $"... {msg.CurrentFolder.Substring(msg.CurrentFolder.Length - 60)}";
        /*
        var firstPart = msg.CurrentFolder.Length - 25;
        if (firstPart > 40)
        {
          firstPart = 40;
        }
        CurrentFolder = $"{msg.CurrentFolder.Substring(0, firstPart - 1)} ... {msg.CurrentFolder.Substring(msg.CurrentFolder.Length - 25)}";
        */
      }
      else
      {
        CurrentFolder = msg.CurrentFolder;  
      }

      NumberOfFiles = msg.NumberOfFiles.ToString();
      
      CurrentFile = msg.CurrentFile;
    }

    #endregion

  }
}
