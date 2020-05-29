using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using CommonServiceLocator;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Utils;
using TagLib;
using TagLib.Id3v2;
using TagLib.Ogg;
using WPFLocalizeExtension.Engine;
using Frame = MPTagThat.Core.Common.Song.Frame;
using Picture = MPTagThat.Core.Common.Song.Picture;

namespace MPTagThat.Core.Common.Song
{
  public class Song
  {
    #region Variables

    private static ILogger log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger).GetLogger;

    #endregion

    #region Pubic Methods

    /// <summary>
    /// Read the Tags from the File 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static SongData Create(string fileName)
    {
      SongData song = new SongData();
      TagLib.File file = null;
      bool error = false;

      try
      {
        TagLib.ByteVector.UseBrokenLatin1Behavior = true;
        file = TagLib.File.Create(fileName);
      }
      catch (CorruptFileException)
      {
        log.Warn($"File Read: Ignoring song {fileName} - Corrupt File!");
        error = true;
      }
      catch (UnsupportedFormatException)
      {
        log.Warn($"File Read: Ignoring song {fileName} - Unsupported format!");
        error = true;
      }
      catch (FileNotFoundException)
      {
        log.Warn($"File Read: Ignoring song {fileName} - Physical file no longer existing!");
        error = true;
      }
      catch (Exception ex)
      {
        log.Error($"File Read: Error processing file: {fileName} {ex.Message}");
        error = true;
      }

      if (error)
      {
        return null;
      }

      TagLib.Id3v2.Tag id3v2tag = null;
      try
      {
        if (file.MimeType.Substring(file.MimeType.IndexOf("/") + 1) == "mp3")
        {
          id3v2tag = file.GetTag(TagTypes.Id3v2, false) as TagLib.Id3v2.Tag;
        }
      }
      catch (Exception ex)
      {
        log.Error($"File Read: Error retrieving id3tag: {fileName} {ex.Message}");
        return null;
      }

      #region Set Common Values

      FileInfo fi = new FileInfo(fileName);
      try
      {
        song.Id = null; // Raven should generate the ID
        song.FullFileName = fileName;
        song.FileName = Path.GetFileName(fileName);
        song.Readonly = fi.IsReadOnly;
        song.TagType = file.MimeType.Substring(file.MimeType.IndexOf("/") + 1);
      }
      catch (Exception ex)
      {
        log.Error($"File Read: Error setting Common tags: {fileName} {ex.Message}");
        return null;
      }
      #endregion

      #region Set Tags

      try
      {
        // Artist
        song.Artist = String.Join(";", file.Tag.Performers);
        if (song.Artist.Contains("AC;DC"))
        {
          song.Artist = song.Artist.Replace("AC;DC", "AC/DC");
        }

        song.ArtistSortName = String.Join(";", file.Tag.PerformersSort);
        if (song.ArtistSortName.Contains("AC;DC"))
        {
          song.ArtistSortName = song.ArtistSortName.Replace("AC;DC", "AC/DC");
        }

        song.AlbumArtist = String.Join(";", file.Tag.AlbumArtists);
        if (song.AlbumArtist.Contains("AC;DC"))
        {
          song.AlbumArtist = song.AlbumArtist.Replace("AC;DC", "AC/DC");
        }

        song.AlbumArtistSortName = String.Join(";", file.Tag.AlbumArtistsSort);
        if (song.AlbumArtistSortName.Contains("AC;DC"))
        {
          song.AlbumArtistSortName = song.AlbumArtistSortName.Replace("AC;DC", "AC/DC");
        }

        song.Album = file.Tag.Album ?? "";
        song.AlbumSortName = file.Tag.AlbumSort ?? "";

        song.BPM = (int)file.Tag.BeatsPerMinute;
        song.Compilation = id3v2tag?.IsCompilation ?? false;
        song.Composer = string.Join(";", file.Tag.Composers);
        song.Conductor = file.Tag.Conductor ?? "";
        song.Copyright = file.Tag.Copyright ?? "";

        song.DiscNumber = file.Tag.Disc;
        song.DiscCount = file.Tag.DiscCount;

        song.Genre = string.Join(";", file.Tag.Genres);
        song.Grouping = file.Tag.Grouping ?? "";
        song.Title = file.Tag.Title ?? "";
        song.TitleSortName = file.Tag.TitleSort ?? "";

        song.MusicBrainzArtistId = file.Tag.MusicBrainzArtistId ?? "";
        song.MusicBrainzDiscId = file.Tag.MusicBrainzDiscId ?? "";
        song.MusicBrainzReleaseArtistId = file.Tag.MusicBrainzReleaseArtistId ?? "";
        song.MusicBrainzReleaseCountry = file.Tag.MusicBrainzReleaseCountry ?? "";
        song.MusicBrainzReleaseGroupId = file.Tag.MusicBrainzReleaseGroupId ?? "";
        song.MusicBrainzReleaseId = file.Tag.MusicBrainzReleaseId ?? "";
        song.MusicBrainzReleaseStatus = file.Tag.MusicBrainzReleaseStatus ?? "";
        song.MusicBrainzTrackId = file.Tag.MusicBrainzTrackId ?? "";
        song.MusicBrainzReleaseType = file.Tag.MusicBrainzReleaseType ?? "";

        song.ReplayGainTrack = double.IsNaN(file.Tag.ReplayGainTrackGain) ? "" : file.Tag.ReplayGainTrackGain.ToString(CultureInfo.InvariantCulture);
        song.ReplayGainTrackPeak = double.IsNaN(file.Tag.ReplayGainTrackPeak) ? "" : file.Tag.ReplayGainTrackPeak.ToString(CultureInfo.InvariantCulture);
        song.ReplayGainAlbum = double.IsNaN(file.Tag.ReplayGainAlbumGain) ? "" : file.Tag.ReplayGainAlbumGain.ToString(CultureInfo.InvariantCulture);
        song.ReplayGainAlbumPeak = double.IsNaN(file.Tag.ReplayGainAlbumPeak) ? "" : file.Tag.ReplayGainAlbumPeak.ToString(CultureInfo.InvariantCulture);

        song.TrackNumber = file.Tag.Track;
        song.TrackCount = file.Tag.TrackCount;
        song.Year = (int)file.Tag.Year;

        // Pictures
        foreach (IPicture picture in file.Tag.Pictures)
        {
          Picture pic = new Picture
          {
            Type = picture.Type,
            MimeType = picture.MimeType,
            Description = picture.Description,
            Data = picture.Data.Data
          };

          song.Pictures.Add(pic);
        }

        // Comments
        if (song.IsMp3 && id3v2tag != null)
        {
          foreach (CommentsFrame commentsframe in id3v2tag.GetFrames<CommentsFrame>())
          {
            song.ID3Comments.Add(new Comment(commentsframe.Description, commentsframe.Language, commentsframe.Text));
          }
        }
        else
        {
          song.Comment = file.Tag.Comment;
        }

        // Lyrics
        song.Lyrics = file.Tag.Lyrics;
        if (song.IsMp3 && id3v2tag != null)
        {
          foreach (UnsynchronisedLyricsFrame lyricsframe in id3v2tag.GetFrames<UnsynchronisedLyricsFrame>())
          {
            // Only add non-empty Frames
            if (lyricsframe.Text != "")
            {
              song.LyricsFrames.Add(new Lyric(lyricsframe.Description, lyricsframe.Language, lyricsframe.Text));
            }
          }
        }

        // Rating
        if (song.IsMp3)
        {
          TagLib.Id3v2.PopularimeterFrame popmFrame = null;
          // First read in all POPM Frames found
          if (id3v2tag != null)
          {
            foreach (PopularimeterFrame popmframe in id3v2tag.GetFrames<PopularimeterFrame>())
            {
              // Only add valid POPM Frames
              if (popmframe.User != "" && popmframe.Rating > 0)
              {
                song.Ratings.Add(new PopmFrame(popmframe.User, (int)popmframe.Rating, (int)popmframe.PlayCount));
              }
            }

            popmFrame = TagLib.Id3v2.PopularimeterFrame.Get(id3v2tag, "MPTagThat", false);
            if (popmFrame != null)
            {
              song.Rating = popmFrame.Rating;
            }
          }

          if (popmFrame == null)
          {
            // Now check for Ape Rating
            TagLib.Ape.Tag apetag = file.GetTag(TagTypes.Ape, true) as TagLib.Ape.Tag;
            TagLib.Ape.Item apeItem = apetag.GetItem("RATING");
            if (apeItem != null)
            {
              string rating = apeItem.ToString();
              try
              {
                song.Rating = Convert.ToInt32(rating);
              }
              catch (Exception)
              { }
            }
          }
        }
        else if (song.TagType == "ape")
        {
          TagLib.Ape.Tag apetag = file.GetTag(TagTypes.Ape, true) as TagLib.Ape.Tag;
          TagLib.Ape.Item apeItem = apetag.GetItem("RATING");
          if (apeItem != null)
          {
            string rating = apeItem.ToString();
            try
            {
              song.Rating = Convert.ToInt32(rating);
            }
            catch (Exception)
            { }
          }
        }
        else if (song.TagType == "ogg" || song.TagType == "flac")
        {
          XiphComment xiph = file.GetTag(TagLib.TagTypes.Xiph, false) as XiphComment;
          string[] rating = xiph.GetField("RATING");
          if (rating.Length > 0)
          {
            try
            {
              song.Rating = Convert.ToInt32(rating[0]);
            }
            catch (Exception)
            { }
          }
        }
      }
      catch (Exception ex)
      {
        log.Error($"Exception setting Tags for file: {fileName}. {ex.Message}");
      }

      #endregion

      #region Set Properties

      try
      {
        song.DurationTimespan = file.Properties.Duration;

        var fileLength = (int)(fi.Length / 1024);
        song.FileSize = fileLength.ToString();

        song.BitRate = file.Properties.AudioBitrate.ToString();
        song.SampleRate = file.Properties.AudioSampleRate.ToString();
        song.Channels = file.Properties.AudioChannels.ToString();
        song.Version = file.Properties.Description;
        song.CreationTime = $"{fi.CreationTime:yyyy-MM-dd HH:mm:ss}";
        song.LastWriteTime = $"{fi.LastWriteTime:yyyy-MM-dd HH:mm:ss}";
      }
      catch (Exception ex)
      {
        log.Error($"Exception setting Properties for file: {fileName}. {ex.Message}");
      }

      #endregion

      // Now copy all Text frames of an ID3 V2
      try
      {
        if (song.IsMp3 && id3v2tag != null)
        {
          foreach (TagLib.Id3v2.Frame frame in id3v2tag.GetFrames())
          {
            string id = frame.FrameId.ToString();
            if (!Util.StandardFrames.Contains(id) && Util.ExtendedFrames.Contains(id))
            {
              song.Frames.Add(new Frame(id, "", frame.ToString()));
            }
            else if (!Util.StandardFrames.Contains(id) && !Util.ExtendedFrames.Contains(id))
            {
              if ((Type)frame.GetType() == typeof(UserTextInformationFrame))
              {
                // Don't add special user frames, like replay gain or musicbrainz, as they are handled in taglib tags
                if (!Util.IsSpecialUserFrame((frame as UserTextInformationFrame)?.Description))
                {
                  song.UserFrames.Add(new Frame(id, (frame as UserTextInformationFrame)?.Description ?? "",
                                               (frame as UserTextInformationFrame)?.Text.Length == 0
                                                 ? ""
                                                 : (frame as UserTextInformationFrame).Text[0]));
                }
              }
              else if ((Type)frame.GetType() == typeof(PrivateFrame))
              {
                song.UserFrames.Add(new Frame(id, (frame as PrivateFrame).Owner ?? "",
                                               (frame as PrivateFrame).PrivateData == null
                                                 ? ""
                                                 : (frame as PrivateFrame).PrivateData.ToString()));
              }
              else if ((Type)frame.GetType() == typeof(UniqueFileIdentifierFrame))
              {
                song.UserFrames.Add(new Frame(id, (frame as UniqueFileIdentifierFrame).Owner ?? "",
                                               (frame as UniqueFileIdentifierFrame).Identifier == null
                                                 ? ""
                                                 : (frame as UniqueFileIdentifierFrame).Identifier.ToString()));
              }
              else if ((Type)frame.GetType() == typeof(UnknownFrame))
              {
                song.UserFrames.Add(new Frame(id, "",
                                               (frame as UnknownFrame).Data == null
                                                 ? ""
                                                 : (frame as UnknownFrame).Data.ToString()));
              }
              else
              {
                song.UserFrames.Add(new Frame(id, "", frame.ToString()));
              }
            }
          }

          song.ID3Version = id3v2tag.Version;
        }
      }
      catch (Exception ex)
      {
        log.Error("Exception getting User Defined frames for file: {0}. {1}", fileName, ex.Message);
      }

      return song;
    }

    /// <summary>
    /// Clear all the tags
    /// </summary>
    /// <param name="song"></param>
    /// <returns></returns>
    public static SongData ClearTag(SongData song)
    {
      song.Artist = "";
      song.ArtistSortName = "";
      song.AlbumArtist = "";
      song.AlbumArtistSortName = "";
      song.Album = "";
      song.AlbumSortName = "";
      song.BPM = 0;
      song.ID3Comments.Clear();
      song.Frames.Clear();
      song.Compilation = false;
      song.Composer = "";
      song.Conductor = "";
      song.Copyright = "";
      song.DiscNumber = 0;
      song.DiscCount = 0;
      song.Genre = "";
      song.Grouping = "";
      song.LyricsFrames.Clear();
      song.Pictures.Clear();
      song.Title = "";
      song.TrackNumber = 0;
      song.TrackCount = 0;
      song.Year = 0;
      song.Ratings.Clear();
      song.CommercialInformation = "";
      song.CopyrightInformation = "";
      song.EncodedBy = "";
      song.Interpreter = "";
      song.InvolvedPeople = "";
      song.MediaType = "";
      song.MusicCreditList = "";
      song.OfficialAudioFileInformation = "";
      song.OfficialArtistInformation = "";
      song.OfficialAudioSourceInformation = "";
      song.OfficialInternetRadioInformation = "";
      song.OfficialPaymentInformation = "";
      song.OfficialPublisherInformation = "";
      song.OriginalAlbum = "";
      song.OriginalFileName = "";
      song.OriginalLyricsWriter = "";
      song.OriginalArtist = "";
      song.OriginalOwner = "";
      song.OriginalRelease = "";
      song.Publisher = "";
      song.ReplayGainTrack = "";
      song.ReplayGainTrackPeak = "";
      song.ReplayGainAlbum = "";
      song.ReplayGainAlbumPeak = "";
      song.SubTitle = "";
      song.TextWriter = "";
      song.TitleSortName = "";
      song.TrackLength = "";
      song.UserFrames.Clear();
      
      return song;
    }

    /// <summary>
    /// Save the Modified file
    /// </summary>
    /// <param name="song"></param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public static bool SaveFile(SongData song, ref string errorMessage)
    {
      errorMessage = "";
      if (!song.Changed)
      {
        return true;
      }

      var options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;

      /*
      if (song.Readonly && !options.MainSettings.ChangeReadOnlyAttributte &&
            (options.ReadOnlyFileHandling == 0 || options.ReadOnlyFileHandling == 2))
      {
        Form dlg = new ReadOnlyDialog(song.FullFileName);
        DialogResult dlgResult = dlg.ShowDialog();

        switch (dlgResult)
        {
          case DialogResult.Yes:
            options.ReadOnlyFileHandling = 0; // Yes 
            break;

          case DialogResult.OK:
            options.ReadOnlyFileHandling = 1; // Yes to All 
            break;

          case DialogResult.No:
            options.ReadOnlyFileHandling = 2; // No 
            break;

          case DialogResult.Cancel:
            options.ReadOnlyFileHandling = 3; // No to All 
            break;
        }
      }
      */

      if (song.Readonly)
      {
        if (!options.MainSettings.ChangeReadOnlyAttribute && options.ReadOnlyFileHandling > 1)
        {
          errorMessage = "File is readonly";
          return false;
        }

        try
        {

          System.IO.File.SetAttributes(song.FullFileName,
                                       System.IO.File.GetAttributes(song.FullFileName) & ~FileAttributes.ReadOnly);

          song.Readonly = false;
        }
        catch (Exception ex)
        {
          log.Error($"File Save: Can't reset Readonly attribute: {song.FullFileName} {ex.Message}");
          errorMessage = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_ErrorResetAttr",
            LocalizeDictionary.Instance.Culture).ToString();
          return false;
        }
      }

      TagLib.File file = null;
      bool error = false;
      try
      {
        TagLib.ByteVector.UseBrokenLatin1Behavior = true;
        file = TagLib.File.Create(song.FullFileName);
      }
      catch (CorruptFileException)
      {
        log.Warn($"File Read: Ignoring song {song.FullFileName} - Corrupt File!");
        errorMessage = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_CorruptFile",
          LocalizeDictionary.Instance.Culture).ToString();
        error = true;
      }
      catch (UnsupportedFormatException)
      {
        log.Warn($"File Read: Ignoring song {song.FullFileName} - Unsupported format!");
        errorMessage = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_UnsupportedFormat",
          LocalizeDictionary.Instance.Culture).ToString();
        error = true;
      }
      catch (FileNotFoundException)
      {
        log.Warn($"File Read: Ignoring song {song.FullFileName} - Physical file no longer existing!");
        errorMessage = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_NonExistingFile",
          LocalizeDictionary.Instance.Culture).ToString();
        error = true;
      }
      catch (Exception ex)
      {
        log.Error($"File Read: Error processing file: {song.FullFileName} {ex.Message}");
        errorMessage = string.Format(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_ErrorReadingFile",
          LocalizeDictionary.Instance.Culture).ToString(), song.FullFileName);
        error = true;
      }

      if (file == null || error)
      {
        log.Error("File Read: Error processing file.: {0}", song.FullFileName);
        return false;
      }

      try
      {
        // Get the ID3 Frame for ID3 specifc frame handling
        TagLib.Id3v1.Tag id3v1tag = null;
        TagLib.Id3v2.Tag id3v2tag = null;
        if (song.IsMp3)
        {
          id3v1tag = file.GetTag(TagTypes.Id3v1, true) as TagLib.Id3v1.Tag;
          id3v2tag = file.GetTag(TagTypes.Id3v2, true) as TagLib.Id3v2.Tag;
        }

        // Remove Tags, if they have been removed in TagEdit Panel
        foreach (TagLib.TagTypes tagType in song.TagsRemoved)
        {
          file.RemoveTags(tagType);
        }


        if (file.Tag != null)
        {
          #region Main Tags

          string[] splitValues = song.Artist.Split(new[] { ';', '|' });
          file.Tag.Performers = splitValues;

          splitValues = song.AlbumArtist.Split(new[] { ';', '|' });
          file.Tag.AlbumArtists = splitValues;

          file.Tag.Album = song.Album.Trim();
          file.Tag.BeatsPerMinute = (uint)song.BPM;


          if (song.Comment != "")
          {
            file.Tag.Comment = "";
            if (song.IsMp3)
            {
              id3v1tag.Comment = song.Comment;
              foreach (Comment comment in song.ID3Comments)
              {
                CommentsFrame commentsframe = CommentsFrame.Get(id3v2tag, comment.Description, comment.Language, true);
                commentsframe.Text = comment.Text;
                commentsframe.Description = comment.Description;
                commentsframe.Language = comment.Language;
              }
            }
            else
            {
              file.Tag.Comment = song.Comment;
            }
          }
          else
          {
            if (song.IsMp3 && id3v2tag != null)
            {
              id3v2tag.RemoveFrames("COMM");
            }
            else
            {
              file.Tag.Comment = "";
            }
          }

          if (song.IsMp3)
          {
            id3v2tag.IsCompilation = song.Compilation;
          }

          file.Tag.Disc = song.DiscNumber;
          file.Tag.DiscCount = song.DiscCount;

          splitValues = song.Genre.Split(new[] { ';', '|' });
          file.Tag.Genres = splitValues;

          file.Tag.Title = song.Title;

          file.Tag.Track = song.TrackNumber;
          file.Tag.TrackCount = song.TrackCount;

          file.Tag.Year = (uint)song.Year;

          double gain;
          var replayGainTrack =  string.IsNullOrEmpty(song.ReplayGainTrack) ? "" : song.ReplayGainTrack.Substring(0, song.ReplayGainTrack.IndexOf(" ", StringComparison.Ordinal));
          if (double.TryParse(replayGainTrack, NumberStyles.Any, CultureInfo.InvariantCulture, out gain))
          {
            file.Tag.ReplayGainTrackGain = gain;
          }
          if (Double.TryParse(song.ReplayGainTrackPeak, NumberStyles.Any, CultureInfo.InvariantCulture, out gain))
          {
            file.Tag.ReplayGainTrackPeak = gain;
          }
          var replayGainAlbum =  string.IsNullOrEmpty(song.ReplayGainAlbum) ? "" : song.ReplayGainAlbum.Substring(0, song.ReplayGainAlbum.IndexOf(" ", StringComparison.Ordinal));
          if (Double.TryParse(replayGainAlbum, NumberStyles.Any, CultureInfo.InvariantCulture, out gain))
          {
            file.Tag.ReplayGainAlbumGain = gain;
          }
          if (Double.TryParse(song.ReplayGainAlbumPeak, NumberStyles.Any, CultureInfo.InvariantCulture, out gain))
          {
            file.Tag.ReplayGainAlbumPeak = gain;
          }

          #endregion

          #region MusicBrainz

          file.Tag.MusicBrainzArtistId = song.MusicBrainzArtistId;
          file.Tag.MusicBrainzDiscId = song.MusicBrainzDiscId;
          file.Tag.MusicBrainzReleaseArtistId = song.MusicBrainzReleaseArtistId;
          file.Tag.MusicBrainzReleaseCountry = song.MusicBrainzReleaseCountry;
          file.Tag.MusicBrainzReleaseGroupId = song.MusicBrainzReleaseGroupId;
          file.Tag.MusicBrainzReleaseId = song.MusicBrainzReleaseId;
          file.Tag.MusicBrainzReleaseStatus = song.MusicBrainzReleaseStatus;
          file.Tag.MusicBrainzTrackId = song.MusicBrainzTrackId;
          file.Tag.MusicBrainzReleaseType = song.MusicBrainzReleaseType;

          #endregion

          #region Detailed Information

          splitValues = song.Composer.Split(new[] { ';', '|' });
          file.Tag.Composers = splitValues;
          file.Tag.Conductor = song.Conductor;
          file.Tag.Copyright = song.Copyright;
          file.Tag.Grouping = song.Grouping;

          splitValues = song.ArtistSortName.Split(new[] { ';', '|' });
          file.Tag.PerformersSort = splitValues;
          splitValues = song.AlbumArtistSortName.Split(new[] { ';', '|' });
          file.Tag.AlbumArtistsSort = splitValues;
          file.Tag.AlbumSort = song.AlbumSortName;
          file.Tag.TitleSort = song.TitleSortName;

          #endregion

          #region Picture

          List<TagLib.Picture> pics = new List<TagLib.Picture>();
          foreach (Picture pic in song.Pictures)
          {
            TagLib.Picture tagPic = new TagLib.Picture();

            try
            {
              byte[] byteArray = pic.Data;
              ByteVector data = new ByteVector(byteArray);
              tagPic.Data = data;
              tagPic.Description = pic.Description;
              tagPic.MimeType = "image/jpg";
              tagPic.Type = pic.Type;
              pics.Add(tagPic);
            }
            catch (Exception ex)
            {
              log.Error("Error saving Picture: {0}", ex.Message);
            }

            file.Tag.Pictures = pics.ToArray();
          }

          // Clear the picture
          if (song.Pictures.Count == 0)
          {
            file.Tag.Pictures = pics.ToArray();
          }

          #endregion

          #region Lyrics

          if (song.Lyrics != null && song.Lyrics != "")
          {
            file.Tag.Lyrics = song.Lyrics;
            if (song.IsMp3)
            {
              id3v2tag.RemoveFrames("USLT");
              foreach (Lyric lyric in song.LyricsFrames)
              {
                UnsynchronisedLyricsFrame lyricframe = UnsynchronisedLyricsFrame.Get(id3v2tag, lyric.Description,
                  lyric.Language, true);
                lyricframe.Text = lyric.Text;
                lyricframe.Description = lyric.Description;
                lyricframe.Language = lyric.Language;
              }
            }
            else
              file.Tag.Lyrics = song.Lyrics;
          }
          else
          {
            file.Tag.Lyrics = "";
          }

          #endregion

          #region Ratings

          if (song.IsMp3)
          {
            id3v2tag.RemoveFrames("POPM");
            if (song.Ratings.Count > 0)
            {
              foreach (PopmFrame rating in song.Ratings)
              {
                PopularimeterFrame popmFrame = PopularimeterFrame.Get(id3v2tag, rating.User, true);
                popmFrame.Rating = Convert.ToByte(rating.Rating);
                popmFrame.PlayCount = Convert.ToUInt32(rating.PlayCount);
              }
            }
          }
          else if (song.TagType == "ogg" || song.TagType == "flac")
          {
            if (song.Ratings.Count > 0)
            {
              XiphComment xiph = file.GetTag(TagLib.TagTypes.Xiph, true) as XiphComment;
              xiph.SetField("RATING", song.Rating.ToString());
            }
          }

          #endregion

          #region Non- Standard Taglib and User Defined Frames

          if (options.MainSettings.ClearUserFrames)
          {
            foreach (Frame frame in song.UserFrames)
            {
              ByteVector frameId = new ByteVector(frame.Id);

              if (frame.Id == "TXXX")
              {
                id3v2tag.SetUserTextAsString(frame.Description, "", true);
              }
              else
              {
                id3v2tag.SetTextFrame(frameId, "");
              }
            }
          }

          List<Frame> allFrames = new List<Frame>();
          allFrames.AddRange(song.Frames);

          // The only way to avoid duplicates of User Frames is to delete them by assigning blank values to them
          if (song.SavedUserFrames != null && !options.MainSettings.ClearUserFrames)
          {
            // Clean the previously saved Userframes, to avoid duplicates
            foreach (Frame frame in song.SavedUserFrames)
            {
              ByteVector frameId = new ByteVector(frame.Id);

              if (frame.Id == "TXXX")
              {
                id3v2tag.SetUserTextAsString(frame.Description, "", true);
              }
              else
              {
                id3v2tag.SetTextFrame(frameId, "");
              }
            }

            allFrames.AddRange(song.UserFrames);
          }

          foreach (Frame frame in allFrames)
          {
            ByteVector frameId = new ByteVector(frame.Id);

            // The only way to avoid duplicates of User Frames is to delete them by assigning blank values to them
            if (frame.Id == "TXXX")
            {
              if (frame.Description != "")
              {
                id3v2tag.SetUserTextAsString(frame.Description, "", true);
                id3v2tag.SetUserTextAsString(frame.Description, frame.Value, true);
              }
            }
            else
            {
              id3v2tag.SetTextFrame(frameId, "");
              id3v2tag.SetTextFrame(frameId, frame.Value);
            }
          }

          #endregion


          // Now, depending on which frames the user wants to save, we will remove the other Frames
          file = Util.FormatID3Tag(file);

          // Set the encoding for ID3 Tags
          if (song.IsMp3)
          {
            TagLib.Id3v2.Tag.ForceDefaultEncoding = true;
            switch (options.MainSettings.CharacterEncoding)
            {
              case "Latin1":
                TagLib.Id3v2.Tag.DefaultEncoding = StringType.Latin1;
                break;

              case "UTF16":
                TagLib.Id3v2.Tag.DefaultEncoding = StringType.UTF16;
                break;

              case "UTF16-BE":
                TagLib.Id3v2.Tag.DefaultEncoding = StringType.UTF16BE;
                break;

              case "UTF8":
                TagLib.Id3v2.Tag.DefaultEncoding = StringType.UTF8;
                break;

              case "UTF16-LE":
                TagLib.Id3v2.Tag.DefaultEncoding = StringType.UTF16LE;
                break;
            }
          }
        }
        // Save the file
        file.Save();
      }
      catch (Exception ex)
      {
        log.Error("File Save: Error processing file: {0} {1}", song.FullFileName, ex.Message);
        errorMessage = string.Format(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_ErrorSave",
          LocalizeDictionary.Instance.Culture).ToString(), song.FullFileName);
        error = true;
      }

      if (error)
      {
        return false;
      }

      return true;
    }

    #endregion
  }
}
