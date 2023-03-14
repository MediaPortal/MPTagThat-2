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

#region

using MPTagThat.Core.Common;
using MPTagThat.Core.Services.Settings;
using Prism.Ioc;
using System.Collections.Generic;

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
    private readonly GridViewColumn _musicBrainzArtistId;
    private readonly GridViewColumn _musicBrainzReleaseArtistId;
    private readonly GridViewColumn _musicBrainzReleaseId;
    private readonly GridViewColumn _musicBrainzReleaseGroupId;
    private readonly GridViewColumn _musicBrainzReleaseCountry;
    private readonly GridViewColumn _musicBrainzReleaseStatus;
    private readonly GridViewColumn _musicBrainzReleaseType;
    private readonly GridViewColumn _musicBrainzReleaseTrackId;
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
      _status = new GridViewColumn("Status", "image", 45, true, true, false, 0);
      _filename = new GridViewColumn("FileName", "text", 200, true, false, true, 1);
      _filepath = new GridViewColumn("FilePath", "text", 200, false, true, true, 2); // Initially hidden
      _track = new GridViewColumn("Track", "text", 40, true, false, false, 3);
      _artist = new GridViewColumn("Artist", "text", 150, true, false, true, 4);
      _albumartist = new GridViewColumn("AlbumArtist", "text", 150, true, false, true, 5);
      _album = new GridViewColumn("Album", "text", 150, true, false, true, 6);
      _title = new GridViewColumn("Title", "text", 250, true, false, true, 7);
      _year = new GridViewColumn("Year", "number", 40, true, false, true, 8);
      _genre = new GridViewColumn("Genre", "text", 100, true, false, true, 9);
      _disc = new GridViewColumn("Disc", "text", 45, true, false, true, 10);
      _bpm = new GridViewColumn("BPM", "number", 40, true, false, true, 12);
      _rating = new GridViewColumn("Rating", "rating", 90, true, false, true, 11);
      _replayGainTrack = new GridViewColumn("ReplayGainTrack", "text", 100, true, true, true, 17);
      _replayGainTrackPeak = new GridViewColumn("ReplayGainTrackPeak", "text", 100, true, true, true, 18);
      _replayGainAlbum = new GridViewColumn("ReplayGainAlbum", "text", 100, true, true, true, 19);
      _replayGainAlbumPeak = new GridViewColumn("ReplayGainAlbumPeak", "text", 100, true, true, true, 20);
      _comment = new GridViewColumn("Comment", "text", 200, true, false, true, 13);
      _composer = new GridViewColumn("Composer", "text", 150, true, false, true, 14);
      _conductor = new GridViewColumn("Conductor", "text", 150, true, false, true, 15);
      _numpics = new GridViewColumn("NumPics", "number", 40, true, false, false, 16);
      _tagtype = new GridViewColumn("TagType", "text", 100, true, true, true, 55);
      _duration = new GridViewColumn("Duration", "text", 100, true, true, false, 56);
      _filesize = new GridViewColumn("FileSize", "text", 80, true, true, true, 57);
      _bitrate = new GridViewColumn("BitRate", "text", 50, true, true, true, 58);
      _samplerate = new GridViewColumn("SampleRate", "text", 70, true, true, true, 59);
      _channels = new GridViewColumn("Channels", "text", 40, true, true, true, 60);
      _version = new GridViewColumn("Version", "text", 100, true, true, true, 61);
      _creationtime = new GridViewColumn("CreationTime", "text", 100, true, true, true, 62);
      _lastwritetime = new GridViewColumn("LastWriteTime", "text", 100, true, true, true, 63);

      // Initially Hidden Columns
      _artistSortName = new GridViewColumn("ArtistSortName", "text", 100, false, false, true, 21);
      _albumSortName = new GridViewColumn("AlbumSortName", "text", 100, false, false, true, 22);
      _commercialInformation = new GridViewColumn("CommercialInformation", "text", 100, false, false, true, 23);
      _copyright = new GridViewColumn("Copyright", "text", 100, false, false, true, 24);
      _copyrightInformation = new GridViewColumn("CopyrightInformation", "text", 100, false, false, true, 25);
      _encodedBy = new GridViewColumn("EncodedBy", "text", 100, false, false, true, 26);
      _interpreter = new GridViewColumn("Interpreter", "text", 100, false, false, true, 27);
      _grouping = new GridViewColumn("Grouping", "text", 100, false, false, true, 28);
      _lyrics = new GridViewColumn("Lyrics", "text", 100, false, false, true, 29);
      _mediaType = new GridViewColumn("MediaType", "text", 100, false, false, true, 30);
      _officialAudioFileInformation = new GridViewColumn("OfficialAudioFileInformation", "text", 100, false, false, true, 31);
      _officialArtistInformation = new GridViewColumn("OfficialArtistInformation", "text", 100, false, false, true, 32);
      _officialAudioSourceInformation = new GridViewColumn("OfficialAudioSourceInformation", "text", 100, false, false, true, 33);
      _officialInternetRadioInformation = new GridViewColumn("OfficialInternetRadioInformation", "text", 100, false, false, true, 34);
      _officialPaymentInformation = new GridViewColumn("OfficialPaymentInformation", "text", 100, false, false, true, 35);
      _officialPublisherInformation = new GridViewColumn("OfficialPublisherInformation", "text", 100, false, false, true, 36);
      _originalAlbum = new GridViewColumn("OriginalAlbum", "text", 100, false, false, true, 37);
      _originalFileName = new GridViewColumn("OriginalFileName", "text", 100, false, false, true, 38);
      _originalLyricsWriter = new GridViewColumn("OriginalLyricsWriter", "text", 100, false, false, true, 39);
      _originalArtist = new GridViewColumn("OriginalArtist", "text", 100, false, false, true, 40);
      _originalOwner = new GridViewColumn("OriginalOwner", "text", 100, false, false, true, 41);
      _originalRelease = new GridViewColumn("OriginalRelease", "text", 100, false, false, true, 42);
      _publisher = new GridViewColumn("Publisher", "text", 100, false, false, true, 43);
      _subTitle = new GridViewColumn("SubTitle", "text", 100, false, false, true, 44);
      _textWriter = new GridViewColumn("TextWriter", "text", 100, false, false, true, 45);
      _titleSortName = new GridViewColumn("TitleSortName", "text", 100, false, false, true, 46);
      _musicBrainzArtistId = new GridViewColumn("MusicBrainzArtistId", "text", 100, false, false, true, 47);
      _musicBrainzReleaseArtistId = new GridViewColumn("MusicBrainzReleaseArtistId", "text", 100, false, false, true, 48);
      _musicBrainzReleaseId = new GridViewColumn("MusicBrainzReleaseId", "text", 100, false, false, true, 49);
      _musicBrainzReleaseGroupId = new GridViewColumn("MusicBrainzReleaseGroupId", "text", 100, false, false, true, 50);
      _musicBrainzReleaseCountry = new GridViewColumn("MusicBrainzReleaseCountry", "text", 100, false, false, true, 51);
      _musicBrainzReleaseStatus = new GridViewColumn("MusicBrainzReleaseStatus", "text", 100, false, false, true, 52);
      _musicBrainzReleaseType = new GridViewColumn("MusicBrainzReleaseType", "text", 100, false, false, true, 53);
      _musicBrainzReleaseTrackId = new GridViewColumn("MusicBrainzTrackId", "text", 100, false, false, true, 54);

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
      ContainerLocator.Current.Resolve<ISettingsManager>().Load(_settings);
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
        ContainerLocator.Current.Resolve<ISettingsManager>().Save(_settings);
      }
    }

    public void SaveSettings()
    {
      _settings.Name = "Tracks";
      ContainerLocator.Current.Resolve<ISettingsManager>().Save(_settings);
    }


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
      columnList.Add(_musicBrainzArtistId);
      columnList.Add(_musicBrainzReleaseArtistId);
      columnList.Add(_musicBrainzReleaseId);
      columnList.Add(_musicBrainzReleaseGroupId);
      columnList.Add(_musicBrainzReleaseCountry);
      columnList.Add(_musicBrainzReleaseStatus);
      columnList.Add(_musicBrainzReleaseType);
      columnList.Add(_musicBrainzReleaseTrackId);
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
