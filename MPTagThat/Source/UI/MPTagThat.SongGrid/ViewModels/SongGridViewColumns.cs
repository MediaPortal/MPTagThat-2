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
using CommonServiceLocator;
using MPTagThat.Core.Common;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;

#endregion

namespace MPTagThat.SongGrid.ViewModels
{
  public class SongGridViewColumns
  {
    #region Variables

    private readonly GridViewColumn _originalAlbum;
    private readonly GridViewColumn _album;
    private readonly GridViewColumn _albumSortName;
    private readonly GridViewColumn _albumartist;
    private readonly GridViewColumn _artist;
    private readonly GridViewColumn _artistSortName;
    private readonly GridViewColumn _bitrate;
    private readonly GridViewColumn _bpm;
    private readonly GridViewColumn _channels;
    private readonly GridViewColumn _comment;
    private readonly GridViewColumn _commercialInformation;
    private readonly GridViewColumn _composer;
    private readonly GridViewColumn _conductor;
    private readonly GridViewColumn _copyright;
    private readonly GridViewColumn _copyrightInformation;
    private readonly GridViewColumn _creationtime;
    private readonly GridViewColumn _disc;
    private readonly GridViewColumn _duration;
    private readonly GridViewColumn _encodedBy;
    private readonly GridViewColumn _filename;
    private readonly GridViewColumn _filepath;
    private readonly GridViewColumn _filesize;
    private readonly GridViewColumn _genre;
    private readonly GridViewColumn _grouping;
    private readonly GridViewColumn _interpreter;
    private readonly GridViewColumn _lastwritetime;
    private readonly GridViewColumn _lyrics;
    private readonly GridViewColumn _mediaType;
    private readonly GridViewColumn _numpics;
    private readonly GridViewColumn _officialArtistInformation;
    private readonly GridViewColumn _officialAudioFileInformation;
    private readonly GridViewColumn _officialAudioSourceInformation;
    private readonly GridViewColumn _officialInternetRadioInformation;
    private readonly GridViewColumn _officialPaymentInformation;
    private readonly GridViewColumn _officialPublisherInformation;
    private readonly GridViewColumn _originalArtist;
    private readonly GridViewColumn _originalFileName;
    private readonly GridViewColumn _originalLyricsWriter;
    private readonly GridViewColumn _originalOwner;
    private readonly GridViewColumn _originalRelease;
    private readonly GridViewColumn _publisher;
    private readonly GridViewColumn _rating;
    private readonly GridViewColumn _replayGainTrack;
    private readonly GridViewColumn _replayGainTrackPeak;
    private readonly GridViewColumn _replayGainAlbum;
    private readonly GridViewColumn _replayGainAlbumPeak;
    private readonly GridViewColumn _samplerate;
    private readonly GridViewColumn _status;
    private readonly GridViewColumn _subTitle;
    private readonly GridViewColumn _tagtype;
    private readonly GridViewColumn _textWriter;
    private readonly GridViewColumn _title;
    private readonly GridViewColumn _titleSortName;
    private readonly GridViewColumn _track;
    private readonly GridViewColumn _version;
    private readonly GridViewColumn _year;

    private SongGridViewColumnSettings _settings;

    #endregion

    #region Constructor

    public SongGridViewColumns()
    {
      _status = new GridViewColumn("Status", "image", 45, true, true, false);
      _filename = new GridViewColumn("FileName", "text", 200, true, false, true);
      _filepath = new GridViewColumn("FilePath", "text", 200, false, true, true); // Initially hidden
      _track = new GridViewColumn("Track", "text", 40, true, false, false);
      _artist = new GridViewColumn("Artist", "text", 150, true, false, true);
      _albumartist = new GridViewColumn("AlbumArtist", "text", 150, true, false, true);
      _album = new GridViewColumn("Album", "text", 150, true, false, true);
      _title = new GridViewColumn("Title", "text", 250, true, false, true);
      _year = new GridViewColumn("Year", "number", 40, true, false, true);
      _genre = new GridViewColumn("Genre", "text", 100, true, false, true);
      _creationtime = new GridViewColumn("CreationTime", "text", 100, true, true, true);
      _lastwritetime = new GridViewColumn("LastWriteTime", "text", 100, true, true, true);
      _tagtype = new GridViewColumn("TagType", "text", 100, true, true, true);
      _disc = new GridViewColumn("Disc", "text", 45, true, false, true);
      _bpm = new GridViewColumn("BPM", "number", 40, true, false, true);
      _rating = new GridViewColumn("Rating", "rating", 90, true, false, true);
      _replayGainTrack = new GridViewColumn("ReplayGainTrack", "text", 100, true, true, true);
      _replayGainTrackPeak = new GridViewColumn("ReplayGainTrackPeak", "text", 100, true, true, true);
      _replayGainAlbum = new GridViewColumn("ReplayGainAlbum", "text", 100, true, true, true);
      _replayGainAlbumPeak = new GridViewColumn("ReplayGainAlbumPeak", "text", 100, true, true, true);
      _comment = new GridViewColumn("Comment", "text", 200, true, false, true);
      _composer = new GridViewColumn("Composer", "text", 150, true, false, true);
      _conductor = new GridViewColumn("Conductor", "text", 150, true, false, true);
      _numpics = new GridViewColumn("NumPics", "number", 40, true, false, false);
      _duration = new GridViewColumn("Duration", "text", 100, true, true, false);
      _filesize = new GridViewColumn("FileSize", "text", 80, true, true, true);
      _bitrate = new GridViewColumn("BitRate", "text", 50, true, true, true);
      _samplerate = new GridViewColumn("SampleRate", "text", 70, true, true, true);
      _channels = new GridViewColumn("Channels", "text", 40, true, true, true);
      _version = new GridViewColumn("Version", "text", 100, true, true, true);

      // Initially Hidden Columns
      _artistSortName = new GridViewColumn("ArtistSortName", "text", 100, false, false, true);
      _albumSortName = new GridViewColumn("AlbumSortName", "text", 100, false, false, true);
      _commercialInformation = new GridViewColumn("CommercialInformation", "text", 100, false, false, true);
      _copyright = new GridViewColumn("Copyright", "text", 100, false, false, true);
      _copyrightInformation = new GridViewColumn("CopyrightInformation", "text", 100, false, false, true);
      _encodedBy = new GridViewColumn("EncodedBy", "text", 100, false, false, true);
      _interpreter = new GridViewColumn("Interpreter", "text", 100, false, false, true);
      _grouping = new GridViewColumn("Grouping", "text", 100, false, false, true);
      _lyrics = new GridViewColumn("Lyrics", "text", 100, false, false, true);
      _mediaType = new GridViewColumn("MediaType", "text", 100, false, false, true);
      _officialAudioFileInformation = new GridViewColumn("OfficialAudioFileInformation", "text", 100, false, false, true);
      _officialArtistInformation = new GridViewColumn("OfficialArtistInformation", "text", 100, false, false, true);
      _officialAudioSourceInformation = new GridViewColumn("OfficialAudioSourceInformation", "text", 100, false, false,
                                                           true);
      _officialInternetRadioInformation = new GridViewColumn("OfficialInternetRadioInformation", "text", 100, false,
                                                             false, true);
      _officialPaymentInformation = new GridViewColumn("OfficialPaymentInformation", "text", 100, false, false, true);
      _officialPublisherInformation = new GridViewColumn("OfficialPublisherInformation", "text", 100, false, false, true);
      _originalAlbum = new GridViewColumn("OriginalAlbum", "text", 100, false, false, true);
      _originalFileName = new GridViewColumn("OriginalFileName", "text", 100, false, false, true);
      _originalLyricsWriter = new GridViewColumn("OriginalLyricsWriter", "text", 100, false, false, true);
      _originalArtist = new GridViewColumn("OriginalArtist", "text", 100, false, false, true);
      _originalOwner = new GridViewColumn("OriginalOwner", "text", 100, false, false, true);
      _originalRelease = new GridViewColumn("OriginalRelease", "text", 100, false, false, true);
      _publisher = new GridViewColumn("Publisher", "text", 100, false, false, true);
      _subTitle = new GridViewColumn("SubTitle", "text", 100, false, false, true);
      _textWriter = new GridViewColumn("TextWriter", "text", 100, false, false, true);
      _titleSortName = new GridViewColumn("TitleSortName", "text", 100, false, false, true);

      LoadSettings();
    }

    #endregion

    #region Properties

    public SongGridViewColumnSettings Settings => _settings;

    #endregion

    #region Private Methods

    private void LoadSettings()
    {
      _settings = new SongGridViewColumnSettings();
      _settings.Name = "Tracks";
      (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).Load(_settings);
      if (_settings.Columns.Count == 0)
      {
        // Setup the Default Columns to display on first use of the program
        List<GridViewColumn> columnList = new List<GridViewColumn>();
        columnList = SetDefaultColumns();
        _settings.Columns.Clear();
        foreach (GridViewColumn column in columnList)
        {
          _settings.Columns.Add(column);
        }
        (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).Save(_settings);
      }
      else
      {
        // Add / Reorder Columns that have been added after Release, so that the settings don't need to be deleted

        // Reorder the Status field
        if (_settings.Columns[0].Name != "Status")
        {
          // We still have an old setting with Status at position 1
          _settings.Columns.RemoveAt(1);
          _settings.Columns.Insert(0, _status);
        }

        if (_settings.Columns[0].Name == "Status" && _settings.Columns[0].Type != "image")
        {
          _settings.Columns[0].Type = "image";
        }

        // FilePath should be column index #2
        if (_settings.Columns[2].Name != "FilePath")
        {
          _settings.Columns.Insert(2, _filepath);
        }

        // Replaygain Columns
        if (_settings.Columns[17].Name != "ReplayGainTrack")
        {
          _settings.Columns.Insert(17, _replayGainTrack);
          _settings.Columns.Insert(18, _replayGainTrackPeak);
          _settings.Columns.Insert(19, _replayGainAlbum);
          _settings.Columns.Insert(20, _replayGainAlbumPeak);
        }

      }
    }

    public void SaveSettings()
    {
      _settings.Name = "Tracks";
      (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).Save(_settings);
    }

    /*
    public void SaveColumnSettings(DataGridViewColumn column, int colIndex)
    {
      _settings.Columns[colIndex].Width = column.Width;
      _settings.Columns[colIndex].DisplayIndex = column.DisplayIndex;
      _settings.Columns[colIndex].Display = column.Visible;
    }
    */

    private List<GridViewColumn> SetDefaultColumns()
    {
      List<GridViewColumn> columnList = new List<GridViewColumn>();
      columnList.Add(_status);
      columnList.Add(_filename);
      columnList.Add(_filepath);
      columnList.Add(_track);
      columnList.Add(_artist);
      columnList.Add(_albumartist);
      columnList.Add(_album);
      columnList.Add(_title);
      columnList.Add(_year);
      columnList.Add(_genre);
      columnList.Add(_disc);
      columnList.Add(_rating);
      columnList.Add(_bpm);
      columnList.Add(_comment);
      columnList.Add(_composer);
      columnList.Add(_conductor);
      columnList.Add(_numpics);
      columnList.Add(_replayGainTrack);
      columnList.Add(_replayGainTrackPeak);
      columnList.Add(_replayGainAlbum);
      columnList.Add(_replayGainAlbumPeak);
      // Initially hidden columns
      columnList.Add(_artistSortName);
      columnList.Add(_albumSortName);
      columnList.Add(_commercialInformation);
      columnList.Add(_copyright);
      columnList.Add(_copyrightInformation);
      columnList.Add(_encodedBy);
      columnList.Add(_interpreter);
      columnList.Add(_grouping);
      columnList.Add(_lyrics);
      columnList.Add(_mediaType);
      columnList.Add(_officialAudioFileInformation);
      columnList.Add(_officialArtistInformation);
      columnList.Add(_officialAudioSourceInformation);
      columnList.Add(_officialInternetRadioInformation);
      columnList.Add(_officialPaymentInformation);
      columnList.Add(_officialPublisherInformation);
      columnList.Add(_originalAlbum);
      columnList.Add(_originalFileName);
      columnList.Add(_originalLyricsWriter);
      columnList.Add(_originalArtist);
      columnList.Add(_originalOwner);
      columnList.Add(_originalRelease);
      columnList.Add(_publisher);
      columnList.Add(_subTitle);
      columnList.Add(_textWriter);
      columnList.Add(_titleSortName);
      // end of initially hidden columns
      columnList.Add(_tagtype);
      columnList.Add(_duration);
      columnList.Add(_filesize);
      columnList.Add(_bitrate);
      columnList.Add(_samplerate);
      columnList.Add(_channels);
      columnList.Add(_version);
      columnList.Add(_creationtime);
      columnList.Add(_lastwritetime);

      return columnList;
    }

    #endregion
  }
}
