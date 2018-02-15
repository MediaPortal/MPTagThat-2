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
using System.Windows.Forms;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using Microsoft.Practices.ServiceLocation;
#endregion

namespace MPTagThat.Services.Settings
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

    public string ConfigDir
    {
      get { return _configDir; }
    }

    public string[] Themes
    {
      get { return availableThemes; }
    }

    public MPTagThatSettings MainSettings
    {
      get { return _MPTagThatSettings; }
    }

    public CaseConversionSettings ConversionSettings
    {
      get { return _caseConversionSettings; }
    }

    public FileNameToTagFormatSettings FileNameToTagSettings
    {
      get { return _fileNameToTagSettings; }
    }

    public List<string> FileNameToTagSettingsTemp
    {
      get { return _fileNameToTagSettingsTemp; }
    }

    public TagToFileNameFormatSettings TagToFileNameSettings
    {
      get { return _tagToFileNameSettings; }
    }

    public List<string> TagToFileNameSettingsTemp
    {
      get { return _tagToFileNameSettingsTemp; }
    }

    public OrganiseFormatSettings OrganiseSettings
    {
      get { return _organiseSettings; }
    }

    public List<string> OrganiseSettingsTemp
    {
      get { return _organiseSettingsTemp; }
    }

    public TreeViewFilterSettings TreeViewSettings
    {
      get { return _treeViewFilterSettings; }
    }

    public List<SongData> CopyPasteBuffer
    {
      get { return _copyPasteBuffer; }
    }

    public ArrayList FindBuffer { get; set; }

    public ArrayList ReplaceBuffer { get; set; }

    public int ReadOnlyFileHandling { get; set; }

    //public static SongList Songlist { get; set; }

    public StartupSettings StartupSettings { get; set; }

    #endregion

    #region ctor

    public Options()
    { }

    #endregion

    #region Init

    public void InitOptions()
    {
      var settings = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager);
      if (settings != null)
      {
        StartupSettings = settings.StartupSettings;
        if (StartupSettings.Portable)
          _configDir = $@"{Application.StartupPath}\Config";
        else
          _configDir = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\MPTagThat\Config";

        _MPTagThatSettings = new MPTagThatSettings();
        settings.Load(_MPTagThatSettings);

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
          _fileNameToTagSettings.FormatValues.Add(@"<K> - <T>");
          _fileNameToTagSettings.FormatValues.Add(@"<A> - <T>");
          _fileNameToTagSettings.FormatValues.Add(@"<K> - <A> - <T>");
          _fileNameToTagSettings.FormatValues.Add(@"<A> - <K> - <T>");
          _fileNameToTagSettings.FormatValues.Add(@"<A>\<B>\<K> - <T>");
          _fileNameToTagSettings.FormatValues.Add(@"<A>\<B>\<A> - <K> - <T>");
          _fileNameToTagSettings.FormatValues.Add(@"<A>\<B>\<K> - <A> - <T>");
        }

        _fileNameToTagSettingsTemp = new List<string>(_fileNameToTagSettings.FormatValues);

        _tagToFileNameSettings = new TagToFileNameFormatSettings();
        settings.Load(_tagToFileNameSettings);

        // Set Default Values, when starting the first Time
        if (_tagToFileNameSettings.FormatValues.Count == 0)
        {
          // Add Default Values
          _tagToFileNameSettings.FormatValues.Add(@"<K> - <T>");
          _tagToFileNameSettings.FormatValues.Add(@"<A> - <T>");
          _tagToFileNameSettings.FormatValues.Add(@"<K> - <A> - <T>");
          _tagToFileNameSettings.FormatValues.Add(@"<A> - <K> - <T>");
        }

        _tagToFileNameSettingsTemp = new List<string>(_tagToFileNameSettings.FormatValues);

        _organiseSettings = new OrganiseFormatSettings();
        settings.Load(_organiseSettings);

        // Set Default Values, when starting the first Time
        if (_organiseSettings.FormatValues.Count == 0)
        {
          // Add Default values
          _organiseSettings.FormatValues.Add(@"<A>\<B>\<K> - <T>");
          _organiseSettings.FormatValues.Add(@"<A:1>\<A>\<B>\<K> - <T>");
          _organiseSettings.FormatValues.Add(@"<O>\<B>\<K> - <A> - <T>");
          _organiseSettings.FormatValues.Add(@"<O:1>\<A>\<B>\<K> - <T>");
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

      //Songlist = new SongList();
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
