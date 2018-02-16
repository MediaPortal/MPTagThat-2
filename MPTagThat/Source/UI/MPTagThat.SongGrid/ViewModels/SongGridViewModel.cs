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

using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Forms;
using Microsoft.Practices.ServiceLocation;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Utils;
using Prism.Mvvm;
using GridViewColumn = MPTagThat.Core.Common.GridViewColumn;
using Syncfusion.UI.Xaml.Grid;

#endregion

namespace MPTagThat.SongGrid.ViewModels
{
  class SongGridViewModel : BindableBase
  {
    private readonly NLogLogger log;
    private readonly SongGridViewColumns _gridColumns;
    private ObservableCollection<SongData> _songs;

    public SongGridViewModel()
    {
      log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger).GetLogger;
      
      // Load the Settings
      _gridColumns = new SongGridViewColumns();
      CreateColumns();

      _songs = new ObservableCollection<SongData>();
      Folderscan();
    }


    public Columns DataGridColumns
    {
      get; set;
    }

    public ObservableCollection<SongData> Songs
    {
      get
      {
        if (_songs == null)
          _songs = new ObservableCollection<SongData>();

        return _songs;
      }
      set
      {
        _songs = value;
        RaisePropertyChanged("Songs");
      }
    }

    #region Private Methods

    /// <summary>
    ///   Create the Columns of the Grid based on the users setting
    /// </summary>
    private void CreateColumns()
    {
      log.Trace(">>>");

      DataGridColumns = new Columns();
      // Now create the columns 
      foreach (GridViewColumn column in _gridColumns.Settings.Columns)
      {
        DataGridColumns.Add(Util.FormatGridColumn(column));
      }

      log.Trace("<<<");
    }

    #endregion

    #region Folder Scanning

    public void Folderscan()
    {
      SongData song = new SongData();
      TagLib.File file = null;

      TagLib.ByteVector.UseBrokenLatin1Behavior = true;
      file = TagLib.File.Create(@"d:\Music\Eagles, The\Long Road Out Of Eden\0107 - Waiting In The Weeds.mp3");
      song.AlbumArtist = file.Tag.AlbumArtists[0];
      song.Album = file.Tag.Album;
      Songs.Add(song);
    }

    #endregion
  }
}
