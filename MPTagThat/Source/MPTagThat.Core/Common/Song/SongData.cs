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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media.Imaging;
using FreeImageAPI;
using MPTagThat.Core.Utils;
using Prism.Mvvm;
using Raven.Abstractions.Extensions;
using TagLib;
using Picture = MPTagThat.Core.Common.Song.Picture;

#endregion

namespace MPTagThat.Core.Common.Song
{
  [Serializable]
  public class SongData : BindableBase
  {
    #region Variables

    private const int NumTrackDigits = 2;

    private string _artist;
    private string _artistSort;
    private string _albumArtist;
    private string _albumArtistSort;
    private string _album;
    private string _albumSort;
    private int _bpm;
    private bool _compilation;
    private string _composer;
    private string _conductor;
    private string _copyright;
    private UInt32 _discNumber;
    private UInt32 _discCount;
    private string _fileName;
    private string _fullFileName;
    private string _genre;
    private string _grouping;
    private string _title;
    private string _titleSort;
    private UInt32 _trackNumber;
    private UInt32 _trackCount;
    private string _replaygainTrack;
    private string _replaygainAlbum;
    private string _replayGainTrackPeak;
    private string _replayGainAlbumPeak;
    private int _year;

    private TimeSpan _durationTimeSpan;
    private string _fileSize;
    private string _bitRate;
    private string _sampleRate;
    private string _channels;
    private string _version;
    private string _creationTime;
    private string _lastWriteTime;

    private Util.MP3Error _mp3ValError;
    private string _mp3ValErrorText;
    private ObservableCollection<Picture> _pictures = new ObservableCollection<Picture>();
    private List<string> _pictureHashList = new List<string>();
    private List<Comment> _comments = new List<Comment>();
    private List<Lyric> _lyrics = new List<Lyric>();
    private ObservableCollection<PopmFrame> _popmframes = new ObservableCollection<PopmFrame>();
    private List<TagLib.TagTypes> _removedTagTypes = null;

    #endregion

    #region ctor

    public SongData()
    {
      _mp3ValError = Util.MP3Error.NoError;
      Frames = new List<Frame>();
      UserFrames = new List<Frame>();

      Pictures.CollectionChanged += Pictures_CollectionChanged;
    }

    #endregion

    #region Properties

    #region Common Properties

    /// <summary>
    /// Unique ID of the Track
    /// To be used in identifying cloned / changed tracks
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The ID3 Version
    /// </summary>
    public int ID3Version { get; set; } = 3;

    /// <summary>
    /// The Extended Frames included in the file
    /// </summary>
    public List<Common.Song.Frame> Frames { get; set; }

    /// <summary>
    /// The User Defined Frames included in the file
    /// </summary>
    public List<Common.Song.Frame> UserFrames { get; set; }

    /// <summary>
    /// The User Defined Frames that we have read before modification
    /// </summary>
    public List<Common.Song.Frame> SavedUserFrames { get; set; }

    /// <summary>
    /// Current Status of Track, as indicated in Column 0 of grid
    /// -1 = Not set
    /// 0  = Ok
    /// 1  = Changed
    /// 2  = Error
    /// 3  = Broken Song
    /// 4  = Fixed Song
    /// </summary>
    private int _status = -1;

    public int Status
    {
      get => _status;
      set
      {
        if (value < 1)
        {
          StatusMsg = "";
        }
        SetProperty(ref _status, value);
      }
    }

    /// <summary>
    /// Has the Track been changed
    /// </summary>
    private bool _changed = false;

    public bool Changed
    {
      get => _changed;
      set
      {
        if (value)
        {
          Status = 1;
        }
        SetProperty(ref _changed, value);
      }
    }

    /// <summary>
    /// A Status message, which we might want to display
    /// </summary>
    private string _statusMsg = "";
    public string StatusMsg
    {
      get => _statusMsg;
      set => SetProperty(ref _statusMsg, value);
    }

    /// <summary>
    /// Indicates that we are in the Init stage and
    /// should not set the Changed status
    /// </summary>
    public bool Init { get; set; }

    /// <summary>
    /// Indicates, if the Tags have been removed
    /// </summary>
    public List<TagLib.TagTypes> TagsRemoved
    {
      get
      {
        if (_removedTagTypes == null)
        {
          _removedTagTypes = new List<TagTypes>();
        }
        return _removedTagTypes;
      }
    }

    /// <summary>
    /// Is the File Readonly
    /// </summary>
    public bool Readonly { get; set; }

    /// <summary>
    /// The Full Filename including the path
    /// </summary>
    public string FullFileName { get => _fullFileName; set => SetProperty(ref _fullFileName, value); }

    /// <summary>
    /// Filename without Path
    /// </summary>
    public string FileName { get => _fileName; set => SetProperty(ref _fileName, value); }

    /// <summary>
    /// File Extension
    /// </summary>
    public string FileExt { get => Path.GetExtension(FullFileName); }

    /// <summary>
    /// Path of the File
    /// </summary>
    public string FilePath { get => Path.GetDirectoryName(FullFileName); }

    /// <summary>
    /// The Tag Type
    /// </summary>
    public string TagType { get; set; } = "";

    /// <summary>
    /// Do we have a MP3 File?
    /// </summary>
    public bool IsMp3 => TagType.ToLower() == "mp3";

    /// <summary>
    /// Do we have a V2.4 MP3 File?
    /// </summary>
    public bool IsMp3V4 => TagType.ToLower() == "mp3" && ID3Version == 4;

    /// <summary>
    /// Number of Pictures in File
    /// </summary>
    public int NumPics => Pictures.Count > 0 ? Pictures.Count : PictureHashList.Count;

    /// <summary>
    /// Has the Track fixable errors?
    /// </summary>
    public Util.MP3Error MP3ValidationError { get => _mp3ValError; set => SetProperty(ref _mp3ValError, value); }

    #endregion

    #region Tags

    /// <summary>
    /// Artist / Performer Tag
    /// ID3: TPE1
    /// </summary>
    public string Artist
    {
      get => _artist;
      set => SetProperty(ref _artist, value ?? "");
    }

    /// <summary>
    /// Artist Sortname Tag
    /// ID3: TSOP
    /// </summary>
    public string ArtistSortName
    {
      get => _artistSort;
      set => SetProperty(ref _artistSort, value ?? "");
    }

    /// <summary>
    /// Album Artist / Band  / Orchestra Tag
    /// ID3: TPE2
    /// </summary>
    public string AlbumArtist
    {
      get => _albumArtist;
      set => SetProperty(ref _albumArtist, value ?? "");
    }

    /// <summary>
    /// AlbumArtist Sortname Tag
    /// ID3: TSO2
    /// </summary>
    public string AlbumArtistSortName
    {
      get => _albumArtistSort;
      set => SetProperty(ref _albumArtistSort, value ?? "");
    }

    /// <summary>
    /// ALbum Tag
    /// ID3: TALB
    /// </summary>
    public string Album
    {
      get => _album;
      set => SetProperty(ref _album, value ?? "");
    }

    /// <summary>
    /// Album Sortname Tag
    /// ID3: TSOA
    /// </summary>
    public string AlbumSortName
    {
      get => _albumSort;
      set => SetProperty(ref _albumSort, value ?? "");
    }

    /// <summary>
    /// Beats Per Minute Tag
    /// ID3: TBPM
    /// </summary>
    public int BPM
    {
      get => _bpm;
      set => SetProperty(ref _bpm, value);
    }

    /// <summary>
    /// Comment Tag
    /// ID3: COMM
    /// </summary>
    public string Comment
    {
      get => _comments.Count > 0 ? _comments[0].Text : "";
      set
      {
        if (_comments.Count == 0)
        {
          _comments.Add(new Comment("", "", value));
          RaisePropertyChanged("Comment");
        }
        else
        {
          _comments[0].Text = value;
          RaisePropertyChanged("Comment");
        }
      }
    }

    /// <summary>
    /// Comment Tag
    /// ID3: COMM
    /// </summary>
    public List<Comment> ID3Comments
    {
      get => _comments;
    }

    /// <summary>
    /// Commercial Information Tag
    /// ID3: WCOM
    /// </summary>
    public string CommercialInformation
    {
      get => GetFrame("WCOM");
      set
      {
        SetText("WCOM", value);
        RaisePropertyChanged("CommercialInformation");
      }
    }

    public bool Compilation
    {
      get => _compilation;
      set => SetProperty(ref _compilation, value);
    }

    /// <summary>
    /// Composer Tag
    /// ID3: TCOM
    /// </summary>
    public string Composer
    {
      get => _composer;
      set => SetProperty(ref _composer, value ?? "");
    }

    /// <summary>
    /// Conductor Tag
    /// ID3: TPE3
    /// </summary>
    public string Conductor
    {
      get => _conductor;
      set => SetProperty(ref _conductor, value ?? "");
    }

    /// <summary>
    /// Copyright Tag
    /// ID3: TCOP
    /// </summary>
    public string Copyright
    {
      get => _copyright;
      set => SetProperty(ref _copyright, value ?? "");
    }

    /// <summary>
    /// Copyright Information Tag
    /// ID3: WCOP
    /// </summary>
    public string CopyrightInformation
    {
      get => GetFrame("WCOP");
      set
      {
        SetText("WCOP", value);
        RaisePropertyChanged("CopyrightInformation");
      }
    }

    /// <summary>
    /// Position in Mediaset Tag
    /// ID3: TPOS
    /// </summary>
    public string Disc
    {
      get
      {
        string disc = DiscNumber > 0 ? DiscNumber.ToString().PadLeft(NumTrackDigits, '0') : "";
        return DiscCount > 0
                 ? String.Format("{0}/{1}", disc, DiscCount.ToString().PadLeft(NumTrackDigits, '0'))
                 : disc;
      }

      set
      {
        string[] disc = null;
        try
        {
          disc = value.Split('/');
          if (disc[0] != "")
            DiscNumber = Convert.ToUInt32(disc[0]);
        }
        catch (Exception) { }

        try
        {
          if (disc[1] != "")
            DiscCount = Convert.ToUInt32(disc[1]);
        }
        catch (Exception) { }
        RaisePropertyChanged("Disc");
      }
    }

    /// <summary>
    /// The Disc Number part of TPOS
    /// </summary>
    public UInt32 DiscNumber
    {
      get => _discNumber;
      set
      {
        SetProperty(ref _discNumber, value);
        RaisePropertyChanged("Disc");
      }
    }

    /// <summary>
    /// The Disc Count part of TPOS
    /// </summary>
    public UInt32 DiscCount
    {
      get => _discCount;
      set
      {
        SetProperty(ref _discCount, value);
        RaisePropertyChanged("Disc");
      }
    }

    /// <summary>
    /// Encoded By
    /// ID3: TENC
    /// </summary>
    public string EncodedBy
    {
      get => GetFrame("TENC");
      set
      {
        SetText("TENC", value);
        RaisePropertyChanged("EncodedBy");
      }
    }

    /// <summary>
    /// Interpreted / Remixed / Modified by Tag
    /// ID3: TPE4
    /// </summary>
    public string Interpreter
    {
      get => GetFrame("TPE4");
      set
      {
        SetText("TPE4", value);
        RaisePropertyChanged("Interpreter");
      }
    }

    /// <summary>
    /// Genre Tag
    /// ID3: TCON
    /// </summary>
    public string Genre
    {
      get => _genre;
      set => SetProperty(ref _genre, value ?? "");
    }

    /// <summary>
    /// Content Group  Tag
    /// ID3: TIT1
    /// </summary>
    public string Grouping
    {
      get => _grouping;
      set => SetProperty(ref _grouping, value ?? "");
    }

    /// <summary>
    /// Involved People Tag
    /// ID3: IPLS (2.3) / TIPL (2.4)
    /// </summary>
    public string InvolvedPeople
    {
      get
      {
        if (TagType != "mp3")
          return "";

        if (ID3Version == 4)
          return GetFrame("TIPL");

        return GetFrame("IPLS");
      }
      set
      {
        if (TagType != "mp3")
          return;

        if (ID3Version == 4)
          SetText("TIPL", value);

        SetText("IPLS", value);
        RaisePropertyChanged("InvolvedPeople");
      }
    }

    /// <summary>
    /// Lyrics Tag
    /// ID3: USLT
    /// </summary>
    public string Lyrics
    {
      get
      {
        return _lyrics.Count > 0 ? _lyrics[0].Text : "";
      }
      set
      {
        if (_lyrics.Count == 0)
        {
          _lyrics.Add(new Lyric("", "", value));
        }
        else
        {
          _lyrics[0].Text = value;
        }
        RaisePropertyChanged("Lyrics");
      }
    }


    public List<Lyric> LyricsFrames
    {
      get => _lyrics;
    }

    /// <summary>
    /// MediaType Tag
    /// ID3: TMED
    /// </summary>
    public string MediaType
    {
      get => GetFrame("TMED");
      set
      {
        SetText("TMED", value);
        RaisePropertyChanged("MediaType");
      }
    }

    /// <summary>
    /// Music Credit List Tag
    /// ID3: TMCL
    /// </summary>
    public string MusicCreditList
    {
      get => GetFrame("TMCL");
      set
      {
        SetText("TMCL", value);
        RaisePropertyChanged("MusicCreditList");
      }
    }

    /// <summary>
    /// Official Audio File Information Tag
    /// ID3: WOAF
    /// </summary>
    public string OfficialAudioFileInformation
    {
      get => GetFrame("WOAF");
      set
      {
        SetText("WOAF", value);
        RaisePropertyChanged("OfficialAudioFileInformation");
      }
    }

    /// <summary>
    /// Official Artist Information Tag
    /// ID3: WOAR
    /// </summary>
    public string OfficialArtistInformation
    {
      get => GetFrame("WOAR");
      set
      {
        SetText("WOAR", value);
        RaisePropertyChanged("OfficialArtistInformation");
      }
    }

    /// <summary>
    /// Official Audio Source Information Tag
    /// ID3: WOAS
    /// </summary>
    public string OfficialAudioSourceInformation
    {
      get => GetFrame("WOAS");
      set
      {
        SetText("WOAS", value);
        RaisePropertyChanged("OfficialAudioSourceInformation");
      }
    }

    /// <summary>
    /// Official Internet Radio Station Information Tag
    /// ID3: WORS
    /// </summary>
    public string OfficialInternetRadioInformation
    {
      get => GetFrame("WORS");
      set
      {
        SetText("WORS", value);
        RaisePropertyChanged("OfficialInternetRadioInformation");
      }
    }

    /// <summary>
    /// Official Payment Information Tag
    /// ID3: WPAY
    /// </summary>
    public string OfficialPaymentInformation
    {
      get => GetFrame("WPAY");
      set
      {
        SetText("WPAY", value);
        RaisePropertyChanged("OfficialPaymentInformation");
      }
    }

    /// <summary>
    /// Official Publisher Information Tag
    /// ID3: WPUB
    /// </summary>
    public string OfficialPublisherInformation
    {
      get => GetFrame("WPUB");
      set
      {
        SetText("WPUB", value);
        RaisePropertyChanged("OfficialPublisherInformation");
      }
    }

    /// <summary>
    /// Original Album Title Tag
    /// ID3: TOAL
    /// </summary>
    public string OriginalAlbum
    {
      get => GetFrame("TOAL");
      set
      {
        SetText("TOAL", value);
        RaisePropertyChanged("OriginalAlbum");
      }
    }

    /// <summary>
    /// Original File Name Tag
    /// ID3: TOFN
    /// </summary>
    public string OriginalFileName
    {
      get => GetFrame("TOFN");
      set
      {
        SetText("TOFN", value);
        RaisePropertyChanged("OriginalFileName");
      }
    }

    /// <summary>
    /// Original LyricsWriter Tag
    /// ID3: TOLY
    /// </summary>
    public string OriginalLyricsWriter
    {
      get => GetFrame("TOLY");
      set
      {
        SetText("TOLY", value);
        RaisePropertyChanged("OriginalLyricsWriter");
      }
    }

    /// <summary>
    /// Original Artist / Performer Tag
    /// ID3: TOPE
    /// </summary>
    public string OriginalArtist
    {
      get => GetFrame("TOPE");
      set
      {
        SetText("TOPE", value);
        RaisePropertyChanged("OriginalArtist");
      }
    }

    /// <summary>
    /// Original Owner Title Tag
    /// ID3: TOWN
    /// </summary>
    public string OriginalOwner
    {
      get => GetFrame("TOWN");
      set
      {
        SetText("TOWN", value);
        RaisePropertyChanged("OriginalOwner");
      }
    }

    /// <summary>
    /// Original Release Time Tag
    /// ID3: TORY (2.3) / TDOR (2.4)
    /// Handled transparently by Taglib. We only need to look for TDOR
    /// </summary>
    public string OriginalRelease
    {
      get
      {
        if (TagType != "mp3")
          return "";

        return GetFrame("TDOR");
      }
      set
      {
        if (TagType != "mp3")
          return;

        SetText("TDOR", value);
        RaisePropertyChanged("OriginalRelease");
      }
    }

    /// <summary>
    /// Return the Front Cover
    /// </summary>
    public BitmapImage FrontCover
    {
      get
      {
        if (Pictures.Count > 0)
        {
          int indexFrontCover = _pictures
            .Select((pic, i) => new { Pic = pic, Position = i }).First(m => m.Pic.Type == PictureType.FrontCover).Position;
          if (indexFrontCover < 0)
          {
            indexFrontCover = 0;
          }
          return _pictures[indexFrontCover].ImageFromPic();
        }
        return null;
      }
    }

    /// <summary>
    /// Returns the stored Coverart for the Song
    /// </summary>
    public ObservableCollection<Picture> Pictures
    {
      get => _pictures;
      set => SetProperty(ref _pictures, value);
    }

    /// <summary>
    /// Returns the Hashlist for objects, which we have in the database
    /// </summary>
    public List<string> PictureHashList => _pictureHashList;

    /// <summary>
    /// Publisher Writer Tag
    /// ID3: TPUB
    /// </summary>
    public string Publisher
    {
      get => GetFrame("TPUB");
      set
      {
        SetText("TPUB", value);
        RaisePropertyChanged("Publisher");
      }
    }

    /// <summary>
    /// Rating Tag
    /// ID3: POPM
    /// </summary>
    public int Rating
    {
      get
      {
        if (_popmframes.Count > 0 && _popmframes[0].User.StartsWith("Windows"))
        {
          // Handle Windows Media Player Star Ratings
          // 1 star = 1, 2 stars = 64, 3 stars = 128, 4 stars = 196, 5 stars = 255
          int rating = _popmframes[0].Rating;
          if (rating == 1)
          {
            return 1;
          }
          if (rating == 64)
          {
            return 2;
          }
          if (rating == 128)
          {
            return 3;
          }
          if (rating == 196)
          {
            return 4;
          }
          if (rating == 255)
          {
            return 5;
          }
        }
        else
        {
          return _popmframes.Count > 0 ? _popmframes[0].Rating : 0;
        }
        return 0;
      }
      set
      {
        if (_popmframes.Count == 0)
        {
          _popmframes.Add(new PopmFrame("MPTagThat", value, 0));
        }
        else
        {
          _popmframes[0].Rating = value;
        }
        RaisePropertyChanged("Rating");
      }
    }

    public ObservableCollection<PopmFrame> Ratings
    {
      get => _popmframes;
      set => SetProperty(ref _popmframes, value);
    }

    public string ReplayGainTrack
    {
      get => _replaygainTrack;

      set
      {
        if (value != "" && !value.ToLower().Contains("db"))
        {
          value += " dB";
        }
        _replaygainTrack = value;
        RaisePropertyChanged("ReplayGainTrack");
      }
    }

    public string ReplayGainTrackPeak
    {
      get => _replayGainTrackPeak;
      set => SetProperty(ref _replayGainTrackPeak, value);
    }

    public string ReplayGainAlbum
    {
      get { return _replaygainAlbum; }

      set
      {
        if (value != "" && !value.ToLower().Contains("db"))
        {
          value += " dB";
        }
        _replaygainAlbum = value;
        RaisePropertyChanged("ReplayGainAlbum");
      }
    }

    public string ReplayGainAlbumPeak
    {
      get => _replayGainAlbumPeak;
      set => SetProperty(ref _replayGainAlbumPeak, value);
    }


    /// <summary>
    /// SubTitle / More Detailed Description
    /// ID3: TIT3
    /// </summary>
    public string SubTitle
    {
      get => GetFrame("TIT3");
      set
      {
        SetText("TIT3", value);
        RaisePropertyChanged("SubTitle");
      }
    }

    /// <summary>
    /// Text / Lyrics Writer Tag
    /// ID3: TPE4
    /// </summary>
    public string TextWriter
    {
      get => GetFrame("TEXT");
      set
      {
        SetText("TEXT", value);
        RaisePropertyChanged("TextWriter");
      }
    }

    /// <summary>
    /// Title Tag
    /// ID3: TIT2
    /// </summary>
    public string Title
    {
      get => _title;
      set => SetProperty(ref _title, value ?? "");
    }

    /// <summary>
    /// Title SortName Tag
    /// ID3: TSOT
    /// </summary>
    public string TitleSortName
    {
      get => _titleSort;
      set => SetProperty(ref _titleSort, value ?? "");
    }

    /// <summary>
    /// Track Tag
    /// ID3: TRCK
    /// </summary>
    public string Track
    {
      get
      {
        string track = TrackNumber > 0 ? TrackNumber.ToString().PadLeft(NumTrackDigits, '0') : "";
        return TrackCount > 0
                 ? String.Format("{0}/{1}", track, TrackCount.ToString().PadLeft(NumTrackDigits, '0'))
                 : track;
      }

      set
      {
        string[] track = null;
        try
        {
          track = value.Split('/');
          if (track[0] != "")
            TrackNumber = Convert.ToUInt32(track[0]);
          else
            TrackNumber = 0;
        }
        catch (Exception)
        {
          TrackNumber = 0;
        }

        try
        {
          if (track.Length > 1 && track[1] != "")
            TrackCount = Convert.ToUInt32(track[1]);
          else
            TrackCount = 0;
        }
        catch (Exception)
        {
          TrackCount = 0;
        }
        RaisePropertyChanged("Track");
      }
    }

    /// <summary>
    /// The Track Number of the TRCK frame
    /// </summary>
    public UInt32 TrackNumber
    {
      get => _trackNumber;
      set
      {
        SetProperty(ref _trackNumber, value);
        RaisePropertyChanged("Track");
      }
    }

    /// <summary>
    /// The Track Count of the TRCK frame
    /// </summary>
    public UInt32 TrackCount
    {
      get => _trackCount;
      set
      {
        SetProperty(ref _trackCount, value);
        RaisePropertyChanged("Track");
      }
    }

    /// <summary>
    /// Track Length Tag
    /// ID3: TLEN
    /// </summary>
    public string TrackLength
    {
      get => GetFrame("TLEN");
      set
      {
        SetText("TLEN", value);
        RaisePropertyChanged("TrackLength");
      }
    }

    /// <summary>
    /// Year Tag
    /// ID3: TYER
    /// </summary>
    public int Year
    {
      get => _year;
      set => SetProperty(ref _year, value);
    }
    #endregion

    #region Audio File Properties

    /// <summary>
    /// The Duration of the File as string
    /// </summary>
    public string Duration
    {
      get
      {
        DateTime dt = new DateTime(DurationTimespan.Ticks);
        return String.Format("{0:HH:mm:ss.fff}", dt);
      }
    }

    /// <summary>
    /// The Duration of the File as timespan
    /// </summary>
    public TimeSpan DurationTimespan { get => _durationTimeSpan; set => SetProperty(ref _durationTimeSpan, value); }

    /// <summary>
    /// The Filesize in kb
    /// </summary>
    public string FileSize { get => _fileSize; set => SetProperty(ref _fileSize, value); }

    /// <summary>
    /// The Bitrate
    /// </summary>
    public string BitRate { get => _bitRate; set => SetProperty(ref _bitRate, value); }

    /// <summary>
    /// The Sample Rate
    /// </summary>
    public string SampleRate { get => _sampleRate; set => SetProperty(ref _sampleRate, value); }

    /// <summary>
    /// The number of Audio Channels
    /// </summary>
    public string Channels { get => _channels; set => SetProperty(ref _channels, value); }

    /// <summary>
    /// Version of the file
    /// </summary>
    public string Version { get => _version; set => SetProperty(ref _version, value); }

    /// <summary>
    /// File Creation date
    /// </summary>
    public string CreationTime { get => _creationTime; set => SetProperty(ref _creationTime, value); }

    /// <summary>
    /// Last Write Date
    /// </summary>
    public string LastWriteTime { get => _lastWriteTime; set => SetProperty(ref _lastWriteTime, value); }

    #endregion
    #endregion

    #region Public Methods

    public SongData Clone()
    {
      var songClone = new SongData();
      songClone.Init = true;
      songClone.FullFileName = this.FullFileName;
      songClone.FileName = this.FileName;
      songClone.Artist = this.Artist;
      songClone.ArtistSortName = this.ArtistSortName;
      songClone.AlbumArtist = this.AlbumArtist;
      songClone.AlbumArtistSortName = this.AlbumArtistSortName;
      songClone.Album = this.Album;
      songClone.AlbumSortName = this.AlbumSortName;
      songClone.BPM = this.BPM;
      songClone.Comment = this.Comment;
      songClone.CommercialInformation = this.CommercialInformation;
      songClone.Compilation = this.Compilation;
      songClone.Composer = this.Composer;
      songClone.Conductor = this.Conductor;
      songClone.Copyright = this.Copyright;
      songClone.CopyrightInformation = this.CopyrightInformation;
      songClone.Disc = this.Disc;
      songClone.EncodedBy = this.EncodedBy;
      songClone.Interpreter = this.Interpreter;
      songClone.Genre = this.Genre;
      songClone.Grouping = this.Grouping;
      songClone.InvolvedPeople = this.InvolvedPeople;
      songClone.MediaType = this.MediaType;
      songClone.MusicCreditList = this.MusicCreditList;
      songClone.OfficialAudioFileInformation = this.OfficialAudioFileInformation;
      songClone.OfficialArtistInformation = this.OfficialArtistInformation;
      songClone.OfficialAudioSourceInformation = this.OfficialAudioSourceInformation;
      songClone.OfficialInternetRadioInformation = this.OfficialInternetRadioInformation;
      songClone.OfficialPaymentInformation = this.OfficialPaymentInformation;
      songClone.OfficialPublisherInformation = this.OfficialPublisherInformation;
      songClone.OriginalAlbum = this.OriginalAlbum;
      songClone.OriginalFileName = this.OriginalFileName;
      songClone.OriginalLyricsWriter = this.OriginalLyricsWriter;
      songClone.OriginalArtist = this.OriginalArtist;
      songClone.OriginalOwner = this.OriginalOwner;
      songClone.OriginalRelease = this.OriginalRelease;
      songClone.Publisher = this.Publisher;
      songClone.Pictures = this.Pictures;
      songClone.Rating = this.Rating;
      songClone.ReplayGainTrack = this.ReplayGainTrack;
      songClone.ReplayGainTrackPeak = this.ReplayGainTrackPeak;
      songClone.ReplayGainAlbum = this.ReplayGainAlbum;
      songClone.ReplayGainAlbumPeak = this.ReplayGainAlbumPeak;
      songClone.SubTitle = this.SubTitle;
      songClone.TextWriter = this.TextWriter;
      songClone.Title = this.Title;
      songClone.TitleSortName = this.TitleSortName;
      songClone.Track = this.Track;
      songClone.TrackLength = this.TrackLength;
      songClone.Year = this.Year;

      try
      {
        // Clone all Lists individually

        // ObservableCollections have to be handled differently to Lists
        songClone._pictures = new ObservableCollection<Picture>();
        var picList = new List<Picture>();
        for (var i = 0; i < _pictures.Count; i++)
        {
          var pic = new Picture(this._pictures[i]);
          picList.Add(pic);
        }
        songClone._pictures.AddRange(picList);

        songClone._popmframes = new ObservableCollection<PopmFrame>();
        var popmList = new List<PopmFrame>();
        for (var i = 0; i < this._popmframes.Count; i++)
        {
          var popmframe = new PopmFrame(this._popmframes[i]);
          popmList.Add(popmframe);
        }
        songClone._popmframes.AddRange(popmList);

        // Handle Lists
        for (var i = 0; i < this._lyrics.Count; i++)
        {
          var lyric = new Lyric(this._lyrics[i]);
          songClone._lyrics.Add(lyric);
        }

        for (var i = 0; i < this._pictureHashList.Count; i++)
        {
          var hash = this._pictureHashList[i];
          songClone._pictureHashList.Add(hash);
        }

        for (var i = 0; i < this._comments.Count; i++)
        {
          var comment = new Comment(this._comments[i]);
          songClone._comments.Add(comment);
        }
      }
      catch (Exception)
      {
        // ignored
      }
      songClone.Init = false;
      return songClone;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Returns the Value of the Frame with the specified Frame id
    /// </summary>
    /// <param name="frameId"></param>
    /// <returns></returns>
    private string GetFrame(string frameId)
    {
      int index = -1;
      if ((index = Frames.FindIndex((f => f.Id == frameId))) > -1)
      {
        return Frames[index].Value;
      }
      return "";
    }

    /// <summary>
    /// Returns the value of the frame with the specified frame id and value
    /// </summary>
    /// <param name="frameId"></param>
    /// <param name="frameValue"></param>
    /// <returns></returns>
    private string GetFrame(string frameId, string frameValue)
    {
      int index = -1;
      if ((index = Frames.FindIndex((f => f.Id == frameId && f.Value == frameValue))) > -1)
      {
        return Frames[index].Value;
      }
      return "";
    }

    private void SetText(string frameId, string text)
    {
      int index = -1;
      if ((index = Frames.FindIndex((f => f.Id == frameId))) > -1)
      {
        Frames[index].Value = text;
      }
      else
      {
        Frames.Add(new Frame(frameId, "", text));
      }
    }


    private void Pictures_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      RaisePropertyChanged($"Pictures");
    }


    #endregion

    #region overrides

    protected override void OnPropertyChanged(PropertyChangedEventArgs args)
    {
      if (!Init && args.PropertyName != "Changed" && args.PropertyName != "Status" && args.PropertyName != "StatusMsg" && args.PropertyName != "Genre")
      {
        Changed = true;
      }
      base.OnPropertyChanged(args);
    }

    #endregion
  }
}
