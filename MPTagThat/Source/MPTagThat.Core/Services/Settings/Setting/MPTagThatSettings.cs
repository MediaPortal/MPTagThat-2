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
using System.Drawing;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;

#endregion

namespace MPTagThat.Core.Services.Settings.Setting
{
  public class MPTagThatSettings
  {
    #region Variables

    private string _lastRipEncoderUsed;
    private int _numTrackDigits = 2;

    #endregion

    #region Properties

    #region Layout
    
    [Setting(SettingScope.User, "")]
    public string LastFolderUsed { get; set; } = "";

    [Setting(SettingScope.User, "false")]
    public bool ScanSubFolders { get; set; }

    [Setting(SettingScope.User, "")]
    public Point FormLocation { get; set; }

    [Setting(SettingScope.User, "")]
    public Size FormSize { get; set; } = new Size(1200, 1024);

    [Setting(SettingScope.User, "false")]
    public bool FormIsMaximized { get; set; }

    #endregion

    [Setting(SettingScope.User, "")]
    public string ActiveScript { get; set; } = "";

    [Setting(SettingScope.User, "mp3")]
    public string LastConversionEncoderUsed { get; set; }

    [Setting(SettingScope.User, "0")]
    public int PlayerSpectrumIndex { get; set; }

    [Setting(SettingScope.User, "")]
    public string SingleEditLastUsedScript { get; set; }

    [Setting(SettingScope.User, "")]
    public List<string> RecentFolders { get; set; } = new List<string>();

    [Setting(SettingScope.User, "false")]
    public bool ChangeReadOnlyAttribute { get; set; }

    [Setting(SettingScope.User, "")]
    public List<string> MusicShares { get; set; } = new List<string>();

    [Setting(SettingScope.User, "")]
    public List<string> MusicDatabaseQueries { get; set; } = new List<string>();

    #region Tags

    [Setting(SettingScope.User, "Latin1")]
    public string CharacterEncoding { get; set; }

    [Setting(SettingScope.User, "2")]
    public int NumberTrackDigits
    {
      get { return _numTrackDigits; }
      set { _numTrackDigits = value; }
    }

    [Setting(SettingScope.User, "3")]
    public int ID3V2Version { get; set; }

    [Setting(SettingScope.User, "3")]
    public int ID3Version { get; set; }

    [Setting(SettingScope.User, "false")]
    public bool RemoveID3V1 { get; set; }

    [Setting(SettingScope.User, "false")]
    public bool RemoveID3V2 { get; set; }

    [Setting(SettingScope.User, "false")]
    public bool CopyArtist { get; set; }

    [Setting(SettingScope.User, "true")]
    public bool UseCaseConversion { get; set; }

    [Setting(SettingScope.User, "true")]
    public bool CreateFolderThumb { get; set; }

    [Setting(SettingScope.User, "true")]
    public bool EmbedFolderThumb { get; set; }

    [Setting(SettingScope.User, "true")]
    public bool OverwriteExistingCovers { get; set; }

    [Setting(SettingScope.User, "false")]
    public bool AutoFillNumberOfTracks { get; set; }

    [Setting(SettingScope.User, "false")]
    public bool MP3Validate { get; set; }

    [Setting(SettingScope.User, "false")]
    public bool MP3AutoFix { get; set; }

    [Setting(SettingScope.User, "")]
    public List<string> AlbumInfoSites { get; set; } = new List<string>() { "MusicBrainz", "Discogs", "LastFM" };

    [Setting(SettingScope.User, "")]
    public List<string> SelectedAlbumInfoSites { get; set; } = new List<string>();

    [Setting(SettingScope.User, "false")]
    public bool OnlySaveFolderThumb { get; set; }

    [Setting(SettingScope.User, "false")]
    public bool ClearUserFrames { get; set; }

    [Setting(SettingScope.User, "false")]
    public bool ChangeCoverSize { get; set; }

    [Setting(SettingScope.User, "500")]
    public int MaxCoverWidth { get; set; }

    [Setting(SettingScope.User, "")]
    public List<string> CustomGenres { get; set; } = new List<string>();

    [Setting(SettingScope.User, "")]
    public List<string> PreferredMusicBrainzCountries { get; set; } = new List<string>() { "DE", "XE", "US" };

    #endregion

    #region Lyrics

    [Setting(SettingScope.User, "")]
    public List<string> LyricSites { get; set; } = new List<string>() { "Lyrics007", "Lyricsmode", "LyricsNet" ,"LyricsOnDemand", "LyricWiki" };

    [Setting(SettingScope.User, "")]
    public List<string> SelectedLyricSites { get; set; } = new List<string>();

    [Setting(SettingScope.User, "false")]
    public bool SwitchArtist { get; set; }

    #endregion

    #region General

    [Setting(SettingScope.User, "en")]
    public string Language { get; set; }

    [Setting(SettingScope.User, "Office2016Colorful")]
    public string Theme { get; set; }

    [Setting(SettingScope.User, "Yellow")]
    public string ChangedRowColor { get; set; }

    [Setting(SettingScope.User, "White")]
    public string RowColor { get; set; }

    [Setting(SettingScope.User, "LightBlue")]
    public string AlternateRowColor { get; set; }

    [Setting(SettingScope.User, "Debug")]
    public string DebugLevel { get; set; }

    #endregion

    #region Ripping

    [Setting(SettingScope.User, "")]
    public string RipTargetFolder { get; set; }

    [Setting(SettingScope.User, "")]
    public string ConvertRootFolder { get; set; }

    [Setting(SettingScope.User, "mp3")]
    public string RipEncoder { get; set; }

    [Setting(SettingScope.User, @"%artist%\%album%\%track% - %title%")]
    public string ConvertFileNameFormat { get; set; }

    [Setting(SettingScope.User, @"%artist%\%album%\%track% - %title%")]
    public string RipFileNameFormat { get; set; }

    [Setting(SettingScope.User, "true")]
    public bool RipEjectCD { get; set; }

    [Setting(SettingScope.User, "false")]
    public bool RipActivateTargetFolder { get; set; }

    #region MP3

    [Setting(SettingScope.User, "0")]
    public int RipLamePreset { get; set; }

    [Setting(SettingScope.User, "0")]
    public int RipLameABRBitRate { get; set; }

    [Setting(SettingScope.User, "")]
    public string RipLameExpert { get; set; }

    #endregion

    #region Ogg

    [Setting(SettingScope.User, "3")]
    public int RipOggQuality { get; set; }

    [Setting(SettingScope.User, "")]
    public string RipOggExpert { get; set; }

    #endregion

    #region OPUS

    [Setting(SettingScope.User, "10")]
    public int RipOpusComplexity { get; set; }

    [Setting(SettingScope.User, "")]
    public string RipOpusExpert { get; set; }

    #endregion

    #region FLAC

    [Setting(SettingScope.User, "4")]
    public int RipFlacQuality { get; set; }

    [Setting(SettingScope.User, "")]
    public string RipFlacExpert { get; set; }

    #endregion

    #region FAAC

    [Setting(SettingScope.User, "100")]
    public int RipFAACQuality { get; set; }
    
    [Setting(SettingScope.User, "")]
    public string RipFAACExpert { get; set; }

    #endregion

    #region WMA

    [Setting(SettingScope.User, "wma")]
    public string RipEncoderWMA { get; set; }

    [Setting(SettingScope.User, "16,2,44100")]
    public string RipEncoderWMASample { get; set; }

    [Setting(SettingScope.User, "50")]
    public int RipEncoderWMABitRate { get; set; }

    [Setting(SettingScope.User, "Vbr")]
    public string RipEncoderWMACbrVbr { get; set; }

    #endregion

    #region MPC

    [Setting(SettingScope.User, "standard")]
    public string RipEncoderMPCPreset { get; set; }

    [Setting(SettingScope.User, "")]
    public string RipEncoderMPCExpert { get; set; }

    #endregion

    #region WV

    [Setting(SettingScope.User, "-h")]
    public string RipEncoderWVPreset { get; set; }

    [Setting(SettingScope.User, "")]
    public string RipEncoderWVExpert { get; set; }

    #endregion

    #endregion

    #endregion
  }
}
