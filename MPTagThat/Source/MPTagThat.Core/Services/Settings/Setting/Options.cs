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
using System.ComponentModel;
using System.Windows.Forms;
using CommonServiceLocator;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;

#endregion

namespace MPTagThat.Core.Services.Settings.Setting
{
  public class Options
  {
    #region private Variables

    private MPTagThatSettings _MPTagThatSettings;
    private CaseConversionSettings _caseConversionSettings;
    private FileNameToTagFormatSettings _fileNameToTagSettings;
    private List<string> _fileNameToTagSettingsTemp;

    private TagToFileNameFormatSettings _tagToFileNameSettings;
    private List<string> _tagToFileNameSettingsTemp;

    private TreeViewFilterSettings _treeViewFilterSettings;

    private OrganiseFormatSettings _organiseSettings;
    private List<string> _organiseSettingsTemp;

    private List<SongData> _copyPasteBuffer;

    private string _configDir;
    private readonly string[] availableThemes = new[] { "ControlDefault", "Office2007Silver", "Office2007Black", "Office2010Blue", "Office2010Silver", "Office2010Black" };

    #endregion

    #region public Variables

    public string HelpLocation = "http://www.team-mediaportal.com/manual/MediaPortalTools/MPTagThat";
    public string ForumLocation = "http://forum.team-mediaportal.com/forums/mptagthat.261";

    public Item[] MediaTypes = new[]
                    {
                      new Item("","",""), // empty item on purpose 
                      new Item("DIG (Other digital media)","DIG","Other digital media"),
                      new Item("DIG/A (Analogue transfer from media)","DIG/A","Analogue transfer from media"),
                      new Item("ANA (Other analogue media)","ANA","Other analogue media"),
                      new Item("ANA/WAC (Wax cylinder)","ANA/WAC","Wax cylinder"),
                      new Item("ANA/8CA (8-track tape cassette)","ANA/8CA","8-track tape cassette"),
                      new Item("CD (CD)","CD","CD"),
                      new Item("CD/A (Analogue transfer from media)","CD/A","Analogue transfer from media"),
                      new Item("CD/DD (DDD)","CD/DD","DDD"),
                      new Item("CD/AD (ADD)","CD/AD","ADD"),
                      new Item("CD/AA (AAD)","CD/AA","AAD"),
                      new Item("LD (Laserdisc)","LD","Laserdisc"),
                      new Item("TT (Turntable records)","TT","Turntable records"),
                      new Item("TT/33 (Turntable 33.33 rpm)","TT/33","Turntable 33.33 rpm"),
                      new Item("TT/45 (Turntable 45 rpm)","TT/45","Turntable 45 rpm"),
                      new Item("TT/71 (Turntable 71.29 rpm)","TT/71","Turntable 71.29 rpm"),
                      new Item("TT/76 (Turntable 76.59 rpm)","TT/76","Turntable 76.59 rpm"),
                      new Item("TT/78 (Turntable 78.26 rpm)","TT/78","Turntable 78.26 rpm"),
                      new Item("TT/80 (Turntable 80 rpm)","TT/80","Turntable 80 rpm"),
                      new Item("MD (MiniDisc)","MD","MiniDisc"),
                      new Item("MD/A (MiniDisc - Analogue transfer from media)","MD/A","MiniDisc - Analogue transfer from media"),
                      new Item("DAT (DAT)","DAT","DAT"),
                      new Item("DAT/A (DAT - Analogue transfer from media)","DAT/A","DAT - Analogue transfer from media"),
                      new Item("DAT/1 (DAT - standard, 48 kHz/16 bits, linear)","DAT/1","DAT - standard, 48 kHz/16 bits, linear"),
                      new Item("DAT/2 (DAT - mode 2, 32 kHz/16 bits, linear)","DAT/2","DAT - mode 2, 32 kHz/16 bits, linear"),
                      new Item("DAT/3 (DAT - mode 3, 32 kHz/12 bits, non-linear, low speed)","DAT/3","DAT - mode 3, 32 kHz/12 bits, non-linear, low speed"),
                      new Item("DAT/4 (DAT - mode 4, 32 kHz/12 bits, 4 channels)","DAT/4","DAT - mode 4, 32 kHz/12 bits, 4 channels"),
                      new Item("DAT/5 (DAT - mode 5, 44.1 kHz/16 bits, linear)","DAT/5","DAT - mode 5, 44.1 kHz/16 bits, linear"),
                      new Item("DAT/6 (DAT - mode 6, 44.1 kHz/16 bits, 'wide track' play)","DAT/6","DAT - mode 6, 44.1 kHz/16 bits, 'wide track' play"),
                      new Item("DCC (DCC)","DCC","DCC"),
                      new Item("DCC/A (DCC - Analogue transfer from media)","DCC/A","DCC - Analogue transfer from media"),
                      new Item("DVD (DVD)","DVD","DVD"),
                      new Item("DVD/A (DVD - Analogue transfer from media)","DVD/A","DVD - Analogue transfer from media"),
                      new Item("TV (Television)","TV","Television"),
                      new Item("TV/PAL (PAL)","TV/PAL","PAL"),
                      new Item("TV/NTSC (NTSC)","TV/NTSC","NTSC"),
                      new Item("TV/SECAM (SECAM)","TV/SECAM","SECAM"),
                      new Item("VID (Video)","VID","Video"),
                      new Item("VID/PAL (Video PAL)","VID/PAL","Video PAL"),
                      new Item("VID/NTSC (Video NTSC)","VID/NTSC","Video NTSC"),
                      new Item("VID/SECAM (Video SECAM)","VID/SECAM","Video SECAM"),
                      new Item("VID/VHS (Video VHS)","VID/VHS","Video VHS"),
                      new Item("VID/SVHS (Video S-VHS)","VID/SVHS","Video S-VHS"),
                      new Item("VID/BETA (Video BETAMAX)","VID/BETA","Video BETAMAX"),
                      new Item("RAD (Radio)","RAD","Radio"),
                      new Item("RAD/FM (Radio FM)","RAD/FM","Radio FM"),
                      new Item("RAD/AM (Radio AM)","RAD/AM","Radio AM"),
                      new Item("RAD/LW (Radio LW)","RAD/LW","Radio LW"),
                      new Item("RAD/MW (Radio MW)","RAD/MW","Radio MW"),
                      new Item("TEL (Telephone)","TEL","Telephone"),
                      new Item("TEL/I (ISDN)","TEL/I","ISDN"),
                      new Item("MC (MC (normal cassette))","MC","MC (normal cassette)"),
                      new Item("MC/4 (MC 4.75 cm/s (normal speed for a two sided cassette))","MC/4","MC 4.75 cm/s (normal speed for a two sided cassette)"),
                      new Item("MC/9 (MC 9.5 cm/s)","MC/9","MC 9.5 cm/s"),
                      new Item("MC/I (MC Type I cassette (ferric/normal))","MC/I","MC Type I cassette (ferric/normal)"),
                      new Item("MC/II (MC Type II cassette (chrome))","MC/II","MC Type II cassette (chrome)"),
                      new Item("MC/III (MC Type III cassette (ferric chrome))","MC/III","MC Type III cassette (ferric chrome)"),
                      new Item("MC/IV (MC Type IV cassette (metal))","MC/IV","MC Type IV cassette (metal)"),
                      new Item("REE (Reel)","REE","Reel"),
                      new Item("REE/9 (Reel 9.5 cm/s)","REE/9","Reel 9.5 cm/s"),
                      new Item("REE/19 (Reel 19 cm/s)","REE/19","Reel 19 cm/s"),
                      new Item("REE/38 (Reel 38 cm/s)","REE/38","Reel 38 cm/s"),
                      new Item("REE/76 (Reel 76 cm/s)","REE/76","Reel 76 cm/s"),
                      new Item("REE/I (Reel Type I cassette (ferric/normal))","REE/I","Reel Type I cassette (ferric/normal)"),
                      new Item("REE/II (Reel Type II cassette (chrome))","REE/II","Reel Type II cassette (chrome)"),
                      new Item("REE/III (Reel Type III cassette (ferric chrome))","REE/III","Reel Type III cassette (ferric chrome)"),
                      new Item("REE/IV (Reel Type IV cassette (metal))","REE/IV","Reel Type IV cassette (metal)")
                    };


    public Item[] WmaStandardSampleVBR = new[]
                                                  {
                                                    new Item("16 bits, stereo, 44100 Hz", "16,2,44100", ""),
                                                    new Item("16 bits, stereo, 48000 Hz", "16,2,48000", ""),
                                                  };

    public Item[] WmaStandardSampleCBR = new[]
                                                  {
                                                    new Item("16 bits, mono, 8000 Hz", "16,1,8000", ""),
                                                    new Item("16 bits, stereo, 8000 Hz", "16,2,8000", ""),
                                                    new Item("16 bits, mono, 11025 Hz", "16,1,11025", ""),
                                                    new Item("16 bits, mono, 16000 Hz", "16,1,16000", ""),
                                                    new Item("16 bits, stereo, 16000 Hz", "16,2,16000", ""),
                                                    new Item("16 bits, mono, 22050 Hz", "16,1,22050", ""),
                                                    new Item("16 bits, stereo, 22050 Hz", "16,2,22050", ""),
                                                    new Item("16 bits, mono, 32000 Hz", "16,1,32000", ""),
                                                    new Item("16 bits, stereo, 32000 Hz", "16,2,32000", ""),
                                                    new Item("16 bits, mono, 44100 Hz", "16,1,44100", ""),
                                                    new Item("16 bits, stereo, 44100 Hz", "16,2,44100", ""),
                                                    new Item("16 bits, stereo, 48000 Hz", "16,2,48000", ""),
                                                  };

    public Item[] WmaLosslessSampleVBR = new[]
                                                  {
                                                    new Item("16 bits, stereo, 44100 Hz", "16,2,44100", ""),
                                                    new Item("24 bits, stereo, 44100 Hz", "24,2,44100", ""),
                                                    new Item("24 bits, stereo, 48000 Hz", "24,2,48000", ""),
                                                    new Item("24 bits, 6 Channels, 48000 Hz", "24,6,48000", ""),
                                                    new Item("24 bits, stereo, 88200 Hz", "24,2,88200", ""),
                                                    new Item("24 bits, 6 Channels, 88200 Hz", "24,6,88200", ""),
                                                    new Item("24 bits, stereo, 96000 Hz", "24,2,96000", ""),
                                                    new Item("24 bits, 6 Channels, 96000 Hz", "24,6,96000", ""),
                                                  };


    public Item[] WmaProSampleCBR = new[]
                                             {
                                               new Item("16 bits, stereo, 32000 Hz", "16,2,32000", ""),
                                               new Item("24 bits, stereo, 44100 Hz", "24,2,44100", ""),
                                               new Item("16 bits, stereo, 44100 Hz", "16,2,44100", ""),
                                               new Item("24 bits, 6 Channels, 44100 Hz", "24,6,44100", ""),
                                               new Item("16 bits, 6 Channels, 44100 Hz", "16,6,44100", ""),
                                               new Item("24 bits, stereo, 48000 Hz", "24,2,48000", ""),
                                               new Item("16 bits, stereo, 48000 Hz", "16,2,48000", ""),
                                               new Item("24 bits, 6 Channels, 48000 Hz", "24,6,48000", ""),
                                               new Item("16 bits, 6 Channels, 48000 Hz", "16,6,48000", ""),
                                               new Item("24 bits, 8 Channels, 48000 Hz", "24,8,48000", ""),
                                               new Item("16 bits, 8 Channels, 48000 Hz", "16,8,48000", ""),
                                               new Item("24 bits, stereo, 88200 Hz", "24,2,88200", ""),
                                               new Item("24 bits, stereo, 96000 Hz", "24,2,96000", ""),
                                               new Item("24 bits, 6 Channels, 96000 Hz", "24,6,96000", ""),
                                               new Item("24 bits, 8 Channels, 96000 Hz", "24,8,96000", ""),
                                             };

    public Item[] WmaProSampleVBR = new[]
                                             {
                                               new Item("24 bits, stereo, 44100 Hz", "24,2,44100", ""),
                                               new Item("16 bits, 6 Channels, 44100 Hz", "16,6,44100", ""),
                                               new Item("24 bits, stereo, 48000 Hz", "24,2,48000", ""),
                                               new Item("24 bits, 6 Channels, 48000 Hz", "24,6,48000", ""),
                                               new Item("24 bits, stereo, 88200 Hz", "24,2,88200", ""),
                                               new Item("24 bits, 6 Channels, 88200 Hz", "24,6,88200", ""),
                                               new Item("24 bits, stereo, 96000 Hz", "24,2,96000", ""),
                                               new Item("24 bits, 6 Channels, 96000 Hz", "24,6,96000", ""),
                                             };

    public string[] BitRatesLCAAC = new[]
                                             {
                                               "320 kbps", "288 kbps", "256 kbps", "224 kbps",
                                               "192 kbps", "160 kbps", "128 kbps", "112 kbps",
                                               "96 kbps", "80 kbps", "64 kbps", "56 kbps",
                                               "48 kbps", "40 kbps", "32 kbps", "28 kbps",
                                               "24 kbps", "20 kbps", "16 kbps", "12 kbps",
                                               "10 kbps", "8 kbps"
                                             };

    #endregion

    #region Enums

    #region LamePreset enum

    public enum LamePreset
    {
      Medium = 0,
      Standard = 1,
      Extreme = 2,
      Insane = 3,
      ABR = 4
    }

    #endregion

    #region ParameterFormat enum

    public enum ParameterFormat
    {
      FileNameToTag = 1,
      TagToFileName = 2,
      RipFileName = 3,
      Organise = 4
    }

    #endregion

    #endregion

    #region Properties

    public string ConfigDir => _configDir;

    public string[] Themes => availableThemes;

    public MPTagThatSettings MainSettings => _MPTagThatSettings;

    public CaseConversionSettings ConversionSettings => _caseConversionSettings;

    public FileNameToTagFormatSettings FileNameToTagSettings => _fileNameToTagSettings;

    public List<string> FileNameToTagSettingsTemp => _fileNameToTagSettingsTemp;

    public TagToFileNameFormatSettings TagToFileNameSettings => _tagToFileNameSettings;

    public List<string> TagToFileNameSettingsTemp => _tagToFileNameSettingsTemp;

    public OrganiseFormatSettings OrganiseSettings => _organiseSettings;

    public List<string> OrganiseSettingsTemp => _organiseSettingsTemp;

    public TreeViewFilterSettings TreeViewSettings => _treeViewFilterSettings;

    public List<SongData> CopyPasteBuffer => _copyPasteBuffer;

    public List<string> FindBuffer { get; set; }

    public List<string> ReplaceBuffer { get; set; }

    public int ReadOnlyFileHandling { get; set; }

    public SongList Songlist { get; set; }

    public StartupSettings StartupSettings { get; set; }

    public bool NumberOnclick { get; set; } = false;

    public int AutoNumber { get; set; }

    public KeyMaps KeyMap { get; set; }
    #endregion

    #region ctor

    public Options()
    { }

    #endregion

    #region Init

    public void InitOptions()
    {
      if (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) is ISettingsManager settings)
      {
        StartupSettings = settings.StartupSettings;
        if (StartupSettings.Portable)
          _configDir = $@"{Application.StartupPath}\Config";
        else
          _configDir = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\MPTagThat2\Config";

        _MPTagThatSettings = new MPTagThatSettings();
        settings.Load(_MPTagThatSettings);

        // Set default values for Lyrics Search Sites, when starting the first time
        if (_MPTagThatSettings.SelectedLyricSites.Count == 0)
        {
          _MPTagThatSettings.SelectedLyricSites = _MPTagThatSettings.LyricSites;
        }

        // Set default values for Album Search Sites, when starting the first time
        if (_MPTagThatSettings.SelectedAlbumInfoSites.Count == 0)
        {
          _MPTagThatSettings.SelectedAlbumInfoSites = _MPTagThatSettings.AlbumInfoSites;
        }

        _caseConversionSettings = new CaseConversionSettings();
        settings.Load(_caseConversionSettings);
        // Set Default Values, when starting the first Time
        if (_caseConversionSettings.CaseConvExceptions.Count == 0)
        {
          _caseConversionSettings.CaseConvExceptions.Add("I");
          _caseConversionSettings.CaseConvExceptions.Add("II");
          _caseConversionSettings.CaseConvExceptions.Add("III");
          _caseConversionSettings.CaseConvExceptions.Add("IV");
          _caseConversionSettings.CaseConvExceptions.Add("V");
          _caseConversionSettings.CaseConvExceptions.Add("VI");
          _caseConversionSettings.CaseConvExceptions.Add("VII");
          _caseConversionSettings.CaseConvExceptions.Add("VIII");
          _caseConversionSettings.CaseConvExceptions.Add("IX");
          _caseConversionSettings.CaseConvExceptions.Add("X");
          _caseConversionSettings.CaseConvExceptions.Add("XI");
          _caseConversionSettings.CaseConvExceptions.Add("XII");
          _caseConversionSettings.CaseConvExceptions.Add("feat.");
          _caseConversionSettings.CaseConvExceptions.Add("vs.");
          _caseConversionSettings.CaseConvExceptions.Add("DJ");
          _caseConversionSettings.CaseConvExceptions.Add("I'm");
          _caseConversionSettings.CaseConvExceptions.Add("I'll");
          _caseConversionSettings.CaseConvExceptions.Add("I'd");
          _caseConversionSettings.CaseConvExceptions.Add("UB40");
          _caseConversionSettings.CaseConvExceptions.Add("U2");
          _caseConversionSettings.CaseConvExceptions.Add("NRG");
          _caseConversionSettings.CaseConvExceptions.Add("ZZ");
          _caseConversionSettings.CaseConvExceptions.Add("OMD");
          _caseConversionSettings.CaseConvExceptions.Add("A1");
          _caseConversionSettings.CaseConvExceptions.Add("U96");
          _caseConversionSettings.CaseConvExceptions.Add("2XLC");
          _caseConversionSettings.CaseConvExceptions.Add("ATB");
          _caseConversionSettings.CaseConvExceptions.Add("EMF");
          _caseConversionSettings.CaseConvExceptions.Add("CD");
          _caseConversionSettings.CaseConvExceptions.Add("CD1");
          _caseConversionSettings.CaseConvExceptions.Add("CD2");
          _caseConversionSettings.CaseConvExceptions.Add("MC");
          _caseConversionSettings.CaseConvExceptions.Add("USA");
          _caseConversionSettings.CaseConvExceptions.Add("UK");
          _caseConversionSettings.CaseConvExceptions.Add("TLC");
          _caseConversionSettings.CaseConvExceptions.Add("UFO");
          _caseConversionSettings.CaseConvExceptions.Add("AC");
          _caseConversionSettings.CaseConvExceptions.Add("DC");
          _caseConversionSettings.CaseConvExceptions.Add("DMX");
          _caseConversionSettings.CaseConvExceptions.Add("ABBA");
        }


        _fileNameToTagSettings = new FileNameToTagFormatSettings();
        settings.Load(_fileNameToTagSettings);

        // Set Default Values, when starting the first Time
        if (_fileNameToTagSettings.FormatValues.Count == 0)
        {
          // Add Default Values
          _fileNameToTagSettings.FormatValues.Add(@"%track% - %title%");
          _fileNameToTagSettings.FormatValues.Add(@"%artist% - %title%");
          _fileNameToTagSettings.FormatValues.Add(@"%track% - %artist% - %title%");
          _fileNameToTagSettings.FormatValues.Add(@"%artist% - %track% - %title%");
          _fileNameToTagSettings.FormatValues.Add(@"%artist%\%album%\%track% - %title%");
          _fileNameToTagSettings.FormatValues.Add(@"%artist%\%album%\%artist% - %track% - %title%");
          _fileNameToTagSettings.FormatValues.Add(@"%artist%\%album%\%track% - %artist% - %title%");
        }

        _fileNameToTagSettingsTemp = new List<string>(_fileNameToTagSettings.FormatValues);

        _tagToFileNameSettings = new TagToFileNameFormatSettings();
        settings.Load(_tagToFileNameSettings);

        // Set Default Values, when starting the first Time
        if (_tagToFileNameSettings.FormatValues.Count == 0)
        {
          // Add Default Values
          _tagToFileNameSettings.FormatValues.Add(@"%track% - %title%");
          _tagToFileNameSettings.FormatValues.Add(@"%artist% - %title%");
          _tagToFileNameSettings.FormatValues.Add(@"%track% - %artist% - %title%");
          _tagToFileNameSettings.FormatValues.Add(@"%artist% - %track% - %title%");
        }

        _tagToFileNameSettingsTemp = new List<string>(_tagToFileNameSettings.FormatValues);

        _organiseSettings = new OrganiseFormatSettings();
        settings.Load(_organiseSettings);

        // Set Default Values, when starting the first Time
        if (_organiseSettings.FormatValues.Count == 0)
        {
          // Add Default values
          _organiseSettings.FormatValues.Add(@"%artist%\%album%\%track% - %title%");
          _organiseSettings.FormatValues.Add(@"%artist:1%\%artist%\%album%\%track% - %title%");
          _organiseSettings.FormatValues.Add(@"%albumartist%\%album%\%track% - %artist% - %title%");
          _organiseSettings.FormatValues.Add(@"%albumartist:1%\%artist%\%album%\%track% - %title%");
        }

        _organiseSettingsTemp = new List<string>(_organiseSettings.FormatValues);

        _treeViewFilterSettings = new TreeViewFilterSettings();
        settings.Load(_treeViewFilterSettings);
      }

      // Set default values
      if (_treeViewFilterSettings.Filter.Count == 0)
      {
        TreeViewFilter filter = new TreeViewFilter();
        filter.Name = "";
        filter.FileMask = "";
        filter.FileFilter = "*.*";
        _treeViewFilterSettings.Filter.Add(filter);
      }

      _copyPasteBuffer = new List<SongData>();

      ReadOnlyFileHandling = 2; // Don't change attribute as a default.

      Songlist = new SongList();
    }

    #endregion

    #region Public Methods

    public void SaveAllSettings()
    {
      var settings = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager);
      if (settings != null)
      {
        settings.Save(_MPTagThatSettings);
        settings.Save(_fileNameToTagSettings);
        settings.Save(_tagToFileNameSettings);
        settings.Save(_caseConversionSettings);
        settings.Save(_organiseSettings);
        settings.Save(_treeViewFilterSettings);
      }
    }

    #endregion
  }
}
