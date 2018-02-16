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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Forms;
using Microsoft.Practices.ServiceLocation;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Core.Utils;
using Prism.Mvvm;
using Syncfusion.Data.Extensions;
using GridViewColumn = MPTagThat.Core.Common.GridViewColumn;
using Syncfusion.UI.Xaml.Grid;

#endregion

namespace MPTagThat.SongGrid.ViewModels
{
  class SongGridViewModel : BindableBase
  {
    private readonly NLogLogger log;
    private Options _options;
    private readonly SongGridViewColumns _gridColumns;

    public SongGridViewModel()
    {
      log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger).GetLogger;
      _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;

      // Load the Settings
      _gridColumns = new SongGridViewColumns();
      CreateColumns();

      Folderscan();
    }

    public Columns DataGridColumns
    {
      get; set;
    }

    public IEnumerable<SongData> Songs
    {
      get { return _options.Songlist.ToList<SongData>(); }
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
      _options.Songlist.Add(song);
    }

    #endregion
  }
}
