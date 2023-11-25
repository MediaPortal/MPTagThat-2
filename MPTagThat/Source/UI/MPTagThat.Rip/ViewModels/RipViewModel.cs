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

using Hqub.MusicBrainz.Entities;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Events;
using MPTagThat.Core.GnuDB;
using MPTagThat.Core.Services.AudioEncoder;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.MediaChangeMonitor;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Core.Utils;
using MPTagThat.Rip.Models;
using Newtonsoft.Json.Linq;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Cd;
using Un4seen.Bass.AddOn.Wma;
using WPFLocalizeExtension.Engine;
using Action = MPTagThat.Core.Common.Action;
using MessageBox = System.Windows.MessageBox;

#endregion

namespace MPTagThat.Rip.ViewModels
{
  public class RipViewModel : BindableBase
  {
    #region Variables

    private object _lock = new object();
    private IRegionManager _regionManager;
    private readonly NLogLogger log;
    private Options _options;

    private Thread _threadRip;
    private bool _ripActive = false;
    private bool _ripCancel;
    private string _encoder = null;

    private int _defaultBitRateIndex;
    private IMediaChangeMonitor _mediaChangeMonitor;
    private CDInfo[] _cds;
    private List<Release> _musicBrainzReleases = new List<Release>();
    private int _driveID = -1;
    private string _mbId;

    #endregion

    #region Properties

    /// <summary>
    /// The Songs in the Grid
    /// </summary>

    private ObservableCollection<RipData> _songs = new ObservableCollection<RipData>();
    public ObservableCollection<RipData> Songs
    {
      get => _songs;
      set => SetProperty(ref _songs, value);
    }

    /// <summary>
    /// The Binding for the AlbumArtist returned by GnuDB
    /// </summary>
    private string _albumArtist;

    public string AlbumArtist
    {
      get => _albumArtist;
      set => SetProperty(ref _albumArtist, value);
    }

    /// <summary>
    /// The Binding for the Album returned by GnuDB
    /// </summary>
    private string _album;

    public string Album
    {
      get => _album;
      set => SetProperty(ref _album, value);
    }

    /// <summary>
    /// The Binding for the Genre returned by GnuDB
    /// </summary>
    private string _genre;

    public string Genre
    {
      get => _genre;
      set => SetProperty(ref _genre, value);
    }

    /// <summary>
    /// The Binding for the Year returned by GnuDB
    /// </summary>
    private string _year;

    public string Year
    {
      get => _year;
      set => SetProperty(ref _year, value);
    }

    /// <summary>
    /// The Binding found by the CD Query
    /// </summary>
    private ObservableCollection<Item> _cdtitles = new ObservableCollection<Item>();

    public ObservableCollection<Item> CDTitles
    {
      get => _cdtitles;
      set
      {
        _cdtitles = value;
        RaisePropertyChanged("CDTitles");
      }
    }

    /// <summary>
    /// The selected CD in the combo
    /// </summary>
    private object _cdSelectedItem;

    public object CDSelectedItem
    {
      get => _cdSelectedItem;
      set
      {
        AlbumArtist = Album = Genre = Year = "";
        Songs.Clear();

        SetProperty(ref _cdSelectedItem, value);
        if (value == null)
        {
          return;
        }

        var item = (Item)_cdSelectedItem;
        var index = Convert.ToInt32(item.Value.Substring(0, item.Value.IndexOf(" -", StringComparison.Ordinal)));
        if (item.Value.ToLower().Contains("gnudb"))
        {
          FetchCDDetails(_cds[index]);
        }
        else
        {
          FetchMusicBrainzDetails(_musicBrainzReleases[index]);
        }
      }
    }


    /// <summary>
    /// The Binding for the Encoders
    /// </summary>
    private ObservableCollection<Item> _encoders = new ObservableCollection<Item>();
    public ObservableCollection<Item> Encoders
    {
      get => _encoders;
      set
      {
        _encoders = value;
        RaisePropertyChanged("Encoders");
      }
    }

    /// <summary>
    /// A Encoder has been selected
    /// </summary>
    private int _encodersSelectedIndex;

    public int EncodersSelectedIndex
    {
      get => _encodersSelectedIndex;
      set
      {
        _options.MainSettings.LastConversionEncoderUsed = Encoders[value].Value;
        SetProperty(ref _encodersSelectedIndex, value);
      }
    }

    /// <summary>
    /// The Root folder for Rip
    /// </summary>
    private string _ripRootFolder;

    public string RipRootFolder
    {
      get => _ripRootFolder;
      set
      {
        _options.MainSettings.RipTargetFolder = value;
        SetProperty(ref _ripRootFolder, value);
      }
    }

    /// <summary>
    /// Eject CD after Ripping
    /// </summary>
    private bool _ejectCD;

    public bool EjectCD
    {
      get => _ejectCD;
      set
      {
        SetProperty(ref _ejectCD, value);
        _options.MainSettings.RipEjectCD = value;
      }
    }

    /// <summary>
    /// Activate Target Folder after Ripping
    /// </summary>
    private bool _activateTargetFolder;

    public bool ActivateTargetFolder
    {
      get => _activateTargetFolder;
      set
      {
        SetProperty(ref _activateTargetFolder, value);
        _options.MainSettings.RipActivateTargetFolder = value;
      }
    }


    /// <summary>
    /// Binding for Wait Cursor
    /// </summary>
    private bool _isBusy;
    public bool IsBusy
    {
      get => _isBusy;
      set => SetProperty(ref _isBusy, value);
    }

    #region Settings Properties

    /// <summary>
    /// The Binding for the Folder/File Format
    /// </summary>
    private string _ripFileFormat;

    public string RipFileFormat
    {
      get => _ripFileFormat;
      set
      {
        SetProperty(ref _ripFileFormat, value);
        _options.MainSettings.RipFileNameFormat = value;
      }
    }

    /// <summary>
    /// The Binding for the Lame Presets
    /// </summary>
    private ObservableCollection<string> _lamePreset = new ObservableCollection<string>();
    public ObservableCollection<string> LamePreset
    {
      get => _lamePreset;
      set
      {
        _lamePreset = value;
        RaisePropertyChanged("LamePreset");
      }
    }

    /// <summary>
    /// The selected Lame Prest
    /// </summary>
    private int _lamePresetSelectedIndex;

    public int LamePresetSelectedIndex
    {
      get => _lamePresetSelectedIndex;
      set
      {
        _options.MainSettings.RipLamePreset = value;
        SetProperty(ref _lamePresetSelectedIndex, value);

        switch (value)
        {
          case 0:
            LamePresetDescription = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "convert_Options_MP3_DescMedium",
              LocalizeDictionary.Instance.Culture).ToString();
            break;

          case 1:
            LamePresetDescription = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "convert_Options_MP3_DescStandard",
              LocalizeDictionary.Instance.Culture).ToString();
            break;

          case 2:
            LamePresetDescription = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "convert_Options_MP3_DescExtreme",
              LocalizeDictionary.Instance.Culture).ToString();
            break;

          case 3:
            LamePresetDescription = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "convert_Options_MP3_DescInsane",
              LocalizeDictionary.Instance.Culture).ToString();
            break;

          case 4:
            LamePresetDescription = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "convert_Options_MP3_DescABR",
              LocalizeDictionary.Instance.Culture).ToString();
            break;
        }

      }
    }

    /// <summary>
    /// Binding to show the Description for the Lame Presets
    /// </summary>
    private string _lamePresetDescription;

    public string LamePresetDescription
    {
      get => _lamePresetDescription;
      set => SetProperty(ref _lamePresetDescription, value);
    }

    /// <summary>
    /// The Binding for ABR
    /// </summary>
    private int _lameABR;

    public int LameABR
    {
      get => _lameABR;
      set
      {
        SetProperty(ref _lameABR, value);
        _options.MainSettings.RipLameABRBitRate = value;
      }
    }

    /// <summary>
    /// The Binding for the LAME Encoder Expert Options
    /// </summary>
    private string _lameExpertOptions;

    public string LameExpertOptions
    {
      get => _lameExpertOptions;
      set
      {
        SetProperty(ref _lameExpertOptions, value);
        _options.MainSettings.RipLameExpert = value;
      }
    }

    /// <summary>
    /// The Binding for the Ogg Quality
    /// </summary>
    private int _oggQuality;

    public int OggQuality
    {
      get => _oggQuality;
      set
      {
        SetProperty(ref _oggQuality, value);
        _options.MainSettings.RipOggQuality = value;
      }
    }

    /// <summary>
    /// The Binding for the OGG Encoder Expert Options
    /// </summary>
    private string _oggExpertOptions;

    public string OggExpertOptions
    {
      get => _oggExpertOptions;
      set
      {
        SetProperty(ref _oggExpertOptions, value);
        _options.MainSettings.RipOggExpert = value;
      }
    }

    /// <summary>
    /// The Binding for the FLAC Quality
    /// </summary>
    private int _flacQuality;

    public int FLACQuality
    {
      get => _flacQuality;
      set
      {
        SetProperty(ref _flacQuality, value);
        _options.MainSettings.RipFlacQuality = value;
      }
    }

    /// <summary>
    /// The Binding for the FLAC Encoder Expert Options
    /// </summary>
    private string _flacExpertOptions;

    public string FLACExpertOptions
    {
      get => _flacExpertOptions;
      set
      {
        SetProperty(ref _flacExpertOptions, value);
        _options.MainSettings.RipFlacExpert = value;
      }
    }

    /// <summary>
    /// The Binding for the OPUS Complexity
    /// </summary>
    private int _opusComplexity;

    public int OPUSComplexity
    {
      get => _opusComplexity;
      set
      {
        SetProperty(ref _opusComplexity, value);
        _options.MainSettings.RipOpusComplexity = value;
      }
    }

    /// <summary>
    /// The Binding for the Opus Encoder Expert Options
    /// </summary>
    private string _opusExpertOptions;

    public string OpusExpertOptions
    {
      get => _opusExpertOptions;
      set
      {
        SetProperty(ref _opusExpertOptions, value);
        _options.MainSettings.RipOpusExpert = value;
      }
    }

    /// <summary>
    /// The Binding for the FAAC Quality
    /// </summary>
    private int _faacQuality;

    public int FAACQuality
    {
      get => _faacQuality;
      set
      {
        SetProperty(ref _faacQuality, value);
        _options.MainSettings.RipFAACQuality = value;
      }
    }

    /// <summary>
    /// The Binding for the FAAC Encoder Expert Options
    /// </summary>
    private string _faacExpertOptions;

    public string FAACExpertOptions
    {
      get => _faacExpertOptions;
      set
      {
        SetProperty(ref _faacExpertOptions, value);
        _options.MainSettings.RipFAACExpert = value;
      }
    }

    /// <summary>
    /// The Binding for the Musepack Presets
    /// </summary>
    private ObservableCollection<Item> _musePackPreset = new ObservableCollection<Item>();
    public ObservableCollection<Item> MusepackPreset
    {
      get => _musePackPreset;
      set
      {
        _musePackPreset = value;
        RaisePropertyChanged("MusepackPreset");
      }
    }

    /// <summary>
    /// The selected Musepack Preset
    /// </summary>
    private int _musePackSelectedIndex;
    public int MusepackSelectedIndex
    {
      get => _musePackSelectedIndex;
      set
      {
        SetProperty(ref _musePackSelectedIndex, value);
        _options.MainSettings.RipEncoderMPCPreset = MusepackPreset[value].Value;
      }
    }

    /// <summary>
    /// The Binding for the Musepack Encoder Expert Options
    /// </summary>
    private string _musepackExpertOptions;

    public string MusepackExpertOptions
    {
      get => _musepackExpertOptions;
      set
      {
        SetProperty(ref _musepackExpertOptions, value);
        _options.MainSettings.RipEncoderMPCExpert = value;
      }
    }

    /// <summary>
    /// The Binding for the WavPack Presets
    /// </summary>
    private ObservableCollection<Item> _wavPackPreset = new ObservableCollection<Item>();
    public ObservableCollection<Item> WavPackPreset
    {
      get => _wavPackPreset;
      set
      {
        _wavPackPreset = value;
        RaisePropertyChanged("WavPackPreset");
      }
    }

    /// <summary>
    /// The Selected WavPack Preset
    /// </summary>
    private int _wavPackSelectedIndex;
    public int WavPackSelectedIndex
    {
      get => _wavPackSelectedIndex;
      set
      {
        SetProperty(ref _wavPackSelectedIndex, value);
        _options.MainSettings.RipEncoderWVPreset = WavPackPreset[value].Value;
      }
    }

    /// <summary>
    /// The Binding for the WavPack Encoder Expert Options
    /// </summary>
    private string _wavPackExpertOptions;

    public string WavPackExpertOptions
    {
      get => _wavPackExpertOptions;
      set
      {
        SetProperty(ref _wavPackExpertOptions, value);
        _options.MainSettings.RipEncoderWVExpert = value;
      }
    }

    /// <summary>
    /// The Binding for the WMA Encoders
    /// </summary>
    private ObservableCollection<Item> _wmaEncoder = new ObservableCollection<Item>();
    public ObservableCollection<Item> WmaEncoder
    {
      get => _wmaEncoder;
      set
      {
        _wmaEncoder = value;
        RaisePropertyChanged("WmaEncoder");
      }
    }

    /// <summary>
    /// The Selected WMA Encoder
    /// </summary>
    private int _wmaEncoderSelectedIndex;
    public int WmaEncoderSelectedIndex
    {
      get => _wmaEncoderSelectedIndex;
      set
      {
        SetProperty(ref _wmaEncoderSelectedIndex, value);
        _options.MainSettings.RipEncoderWMA = WmaEncoder[value].Value;

        switch (value)
        {
          case 0: // WMA Standard
            WmaCBRVBR.Clear();
            WmaCBRVBR.Add(new Item("Constant Bitrate", "Cbr", ""));
            WmaCBRVBR.Add(new Item("Variable Bitrate", "Vbr", ""));
            SetWMACbrVbr();
            break;

          case 1: // WMA Pro
            WmaCBRVBR.Clear();
            WmaCBRVBR.Add(new Item("Constant Bitrate", "Cbr", ""));
            WmaCBRVBR.Add(new Item("Variable Bitrate", "Vbr", ""));
            SetWMACbrVbr();
            break;

          case 2: // WMA LossLess
            WmaCBRVBR.Clear();
            WmaCBRVBR.Add(new Item("Variable Bitrate", "Vbr", ""));
            WmaCBRVBRSelectedIndex = 0;
            break;
        }
      }
    }

    /// <summary>
    /// The binding for the WMA Sample Format
    /// </summary>
    private ObservableCollection<Item> _wmaSampleFormat = new ObservableCollection<Item>();

    public ObservableCollection<Item> WmaSampleFormat
    {
      get => _wmaSampleFormat;
      set
      {
        _wmaSampleFormat = value;
        RaisePropertyChanged("WmaSampleFormat");
      }
    }

    /// <summary>
    /// The selected index in the WMA Sample Format Combo
    /// </summary>
    private int _wmaSampleFormatSelectedIndex;

    public int WmaSampleFormatSelectedIndex
    {
      get => _wmaSampleFormatSelectedIndex;
      set
      {
        SetProperty(ref _wmaSampleFormatSelectedIndex, value);
        _options.MainSettings.RipEncoderWMASample = WmaSampleFormat[value].Value;
        SetWMABitRateCombo();
      }
    }

    /// <summary>
    /// The binding for the VBR CBR
    /// </summary>
    private ObservableCollection<Item> _wmaCBRVBR = new ObservableCollection<Item>();

    public ObservableCollection<Item> WmaCBRVBR
    {
      get => _wmaCBRVBR;
      set
      {
        _wmaCBRVBR = value;
        RaisePropertyChanged("WmaCBRVBR");
      }
    }

    /// <summary>
    /// The selected index in the CBR VBR Combo
    /// </summary>
    private int _wmaCBRVBRSelectedIndex;

    public int WmaCBRVBRSelectedIndex
    {
      get => _wmaCBRVBRSelectedIndex;
      set
      {
        SetProperty(ref _wmaCBRVBRSelectedIndex, value);
        _options.MainSettings.RipEncoderWMACbrVbr = WmaCBRVBR[value].Value;
        SetWMASampleCombo();
      }
    }

    /// <summary>
    /// The binding for the Bitrate
    /// </summary>
    private ObservableCollection<int> _wmaBitRate = new ObservableCollection<int>();

    public ObservableCollection<int> WmaBitRate
    {
      get => _wmaBitRate;
      set
      {
        _wmaBitRate = value;
        RaisePropertyChanged("WmaBitRate");
      }
    }

    /// <summary>
    /// The selected index in the CBR VBR Combo
    /// </summary>
    private int _wmaBitRateSelectedIndex;

    public int WmaBitRateSelectedIndex
    {
      get => _wmaBitRateSelectedIndex;
      set
      {
        SetProperty(ref _wmaBitRateSelectedIndex, value);
        _options.MainSettings.RipEncoderWMABitRate = WmaBitRate[value];
      }
    }


    #endregion

    #endregion

    #region ctor

    public RipViewModel(IRegionManager regionManager)
    {
      _regionManager = regionManager;
      log = ContainerLocator.Current.Resolve<ILogger>()?.GetLogger;
      log.Trace(">>>");
      _options = ContainerLocator.Current.Resolve<ISettingsManager>()?.GetOptions;
      EventSystem.Subscribe<GenericEvent>(OnMessageReceived);

      _mediaChangeMonitor = ContainerLocator.Current.Resolve<IMediaChangeMonitor>();
      _mediaChangeMonitor.MediaInserted += MediaInserted;
      _mediaChangeMonitor.MediaRemoved += MediaRemoved;

      // Load the Encoders
      Encoders.Add(new Item("MP3 Encoder", "mp3", ""));
      Encoders.Add(new Item("OGG Encoder", "ogg", ""));
      Encoders.Add(new Item("FLAC Encoder", "flac", ""));
      Encoders.Add(new Item("OPUS Encoder", "opus", ""));
      Encoders.Add(new Item("AAC Encoder", "m4a", ""));
      Encoders.Add(new Item("WMA Encoder", "wma", ""));
      Encoders.Add(new Item("WAV Encoder", "wav", ""));
      Encoders.Add(new Item("MusePack Encoder", "mpc", ""));
      Encoders.Add(new Item("WavPack Encoder", "wv", ""));
      if (_options.MainSettings.LastConversionEncoderUsed != "")
      {
        var i = 0;
        foreach (var item in Encoders)
        {
          if (item.Value == _options.MainSettings.LastConversionEncoderUsed)
          {
            EncodersSelectedIndex = i;
            break;
          }

          i++;
        }
      }

      RipRootFolder = _options.MainSettings.ConvertRootFolder;
      RipFileFormat = _options.MainSettings.RipFileNameFormat;
      EjectCD = _options.MainSettings.RipEjectCD;
      ActivateTargetFolder = _options.MainSettings.RipActivateTargetFolder;

      LamePreset.Add("Medium");
      LamePreset.Add("Standard");
      LamePreset.Add("Extreme");
      LamePreset.Add("Insane");
      LamePreset.Add("Advanced BitRate (ABR) Mode");

      LamePresetSelectedIndex = _options.MainSettings.RipLamePreset;
      LameABR = _options.MainSettings.RipLameABRBitRate;
      LameExpertOptions = _options.MainSettings.RipLameExpert;

      OggQuality = _options.MainSettings.RipOggQuality;
      OggExpertOptions = _options.MainSettings.RipOggExpert;

      FLACQuality = _options.MainSettings.RipFlacQuality;
      FLACExpertOptions = _options.MainSettings.RipFlacExpert;

      OPUSComplexity = _options.MainSettings.RipOpusComplexity;
      OpusExpertOptions = _options.MainSettings.RipOpusExpert;

      FAACQuality = _options.MainSettings.RipFAACQuality;
      FAACExpertOptions = _options.MainSettings.RipFAACExpert;

      MusepackPreset.Add(new Item("Low/Medium Quality (~  90 kbps)", "thumb", ""));
      MusepackPreset.Add(new Item("Medium Quality     (~ 130 kbps)", "radio", ""));
      MusepackPreset.Add(new Item("High Quality       (~ 180 kbps)", "standard", ""));
      MusepackPreset.Add(new Item("Excellent Quality  (~ 210 kbps)", "xtreme", ""));
      MusepackPreset.Add(new Item("Excellent Quality  (~ 240 kbps)", "insane", ""));
      MusepackPreset.Add(new Item("Highest Quality    (~ 270 kbps)", "braindead", ""));

      var idx = 0;
      foreach (var item in MusepackPreset)
      {
        if (item.Value == _options.MainSettings.RipEncoderMPCPreset)
        {
          MusepackSelectedIndex = idx;
          break;
        }
        idx++;
      }
      MusepackExpertOptions = _options.MainSettings.RipEncoderMPCExpert;

      WavPackPreset.Add(new Item("Fast Mode (fast, but some compromise in compression ratio)", "-f", ""));
      WavPackPreset.Add(new Item("High quality (better compression, but slower)", "-h", ""));

      idx = 0;
      foreach (var item in WavPackPreset)
      {
        if (item.Value == _options.MainSettings.RipEncoderWVPreset)
        {
          WavPackSelectedIndex = idx;
          break;
        }
        idx++;
      }
      WavPackExpertOptions = _options.MainSettings.RipEncoderWVExpert;

      WmaEncoder.Add(new Item("Windows Media Audio Standard", "wma", ""));
      WmaEncoder.Add(new Item("Windows Media Audio Professional", "wmapro", ""));
      WmaEncoder.Add(new Item("Windows Media Audio Lossless", "wmalossless", ""));

      idx = 0;
      foreach (var item in WmaEncoder)
      {
        if (item.Value == _options.MainSettings.RipEncoderWMA)
        {
          WmaEncoderSelectedIndex = idx;
          break;
        }
        idx++;
      }


      // Commands
      MusicFolderOpenCommand = new BaseCommand(MusicFolderOpen);

      BindingOperations.EnableCollectionSynchronization(CDTitles, _lock);
      log.Trace("<<<");
    }

    #endregion

    #region Commands

    /// <summary>
    /// Open a dialog to select the Music Folder 
    /// </summary>
    public ICommand MusicFolderOpenCommand { get; }

    private void MusicFolderOpen(object param)
    {
      using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
      {
        System.Windows.Forms.DialogResult result = dialog.ShowDialog();
        if (result == DialogResult.OK)
        {
          RipRootFolder = dialog.SelectedPath;
          _options.MainSettings.RipTargetFolder = RipRootFolder;
        }
      }
    }

    #endregion

    #region Private Methods

    private void MediaInserted(string eDriveLetter)
    {
      string driveLetter = eDriveLetter.Substring(0, 1);
      Songs.Clear();
      CDTitles.Clear();
      AlbumArtist = Album = Genre = Year = "";
      QueryGnuDB(driveLetter);
      QueryMusicBrainz(driveLetter);

      if (CDTitles.Count > 0)
      {
        CDSelectedItem = CDTitles[0];
      }
    }

    private void MediaRemoved(string eDriveLetter)
    {
      AlbumArtist = Album = Genre = Year = "";
      CDTitles.Clear();
      Songs.Clear();
    }

    /// <summary>
    /// Cancel Rip
    /// </summary>
    private void RipCancel()
    {
      _ripActive = false;
      _ripCancel = true;
      _threadRip.Abort();
      IsBusy = false;
      // Unlock the Drive and open the door, if selected
      BassCd.BASS_CD_Door(_driveID, BASSCDDoor.BASS_CD_DOOR_UNLOCK);
      if (_options.MainSettings.RipEjectCD)
      {
        BassCd.BASS_CD_Door(_driveID, BASSCDDoor.BASS_CD_DOOR_OPEN);
      }
    }

    /// <summary>
    /// Start ripping the CD
    /// </summary>
    private void RipCd()
    {
      if (_ripActive)
      {
        log.Info("Rip already running.");
        return;
      }

      if (Songs.Count == 0)
      {
        log.Info("No files in Song List");
        return;
      }

      if (string.IsNullOrEmpty(RipRootFolder))
      {
        log.Info("Empty Target Folder. Open Dialog.");
        MusicFolderOpen(null);
      }

      try
      {
        if (!Directory.Exists(RipRootFolder))
        {
          log.Info("Creating Rip Target Folder");
          Directory.CreateDirectory(RipRootFolder);
        }
      }
      catch (Exception ex)
      {
        System.Windows.MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "convert_ErrorDirectory", LocalizeDictionary.Instance.Culture).ToString(),
          LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Error_Title", LocalizeDictionary.Instance.Culture).ToString(),
          MessageBoxButton.OK);
        log.Error("Error creating Rip output directory: {0}. {1}", RipRootFolder, ex.Message);
        return;
      }

      if (EncodersSelectedIndex > -1)
      {
        _encoder = Encoders[EncodersSelectedIndex].Value;
      }

      if (_encoder == null)
      {
        MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "convert_ErrorDirectory", LocalizeDictionary.Instance.Culture).ToString(),
          LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "convert_NoEncoder", LocalizeDictionary.Instance.Culture).ToString(),
          MessageBoxButton.OK);
        log.Info("No Encoder selected.");
        return;
      }

      _threadRip = new Thread(RippingThread) { Name = "Riping", Priority = ThreadPriority.Highest };
      _threadRip.Start();
    }

    /// <summary>
    /// Run the conversion
    /// </summary>
    private void RippingThread()
    {
      log.Trace(">>>");

      IsBusy = true;
      _ripCancel = false;

      if (_driveID < 0)
      {
        log.Info("No CD drive selected. Rip not started.");
        log.Trace("<<<");
        return;
      }

      _ripActive = true;
      var targetDir = "";

      // Build the Target Directory
      var artistDir = AlbumArtist == string.Empty ? "Artist" : AlbumArtist;
      var albumDir = Album == string.Empty ? "Album" : Album;

      var outFileFormat = _options.MainSettings.RipFileNameFormat;
      int index = outFileFormat.LastIndexOf('\\');
      if (index > -1)
      {
        targetDir = outFileFormat.Substring(0, index);
        targetDir = targetDir.Replace("%artist%", artistDir);
        targetDir = targetDir.Replace("%albumartist%", artistDir);
        targetDir = targetDir.Replace("%album%", albumDir);
        targetDir = targetDir.Replace("%genre%", Genre);
        targetDir = targetDir.Replace("%year%", Year);
        outFileFormat = outFileFormat.Substring(index + 1);
      }
      else
      {
        targetDir = $@"{artistDir}\{albumDir}";
      }

      targetDir = Util.MakeValidFolderName(targetDir);

      targetDir = $@"{_ripRootFolder}\{targetDir}";

      log.Debug($"Rip: Using Target Folder: {targetDir}");

      try
      {
        if (!Directory.Exists(targetDir))
          Directory.CreateDirectory(targetDir);
      }
      catch (Exception ex)
      {
        log.Error("Error creating Ripping directory: {0}. {1}", targetDir, ex.Message);
        return;
      }

      // Lock the Door
      BassCd.BASS_CD_Door(_driveID, BASSCDDoor.BASS_CD_DOOR_LOCK);

      var currentRow = 0;
      foreach (var song in Songs)
      {
        if (_ripCancel)
        {
          log.Info($"Ripping Audio CD aborted");
          break;
        }

        if (!song.IsChecked)
        {
          currentRow++;
          continue;
        }

        int stream = BassCd.BASS_CD_StreamCreate(_driveID, currentRow, BASSFlag.BASS_STREAM_DECODE);
        if (stream == 0)
        {
          log.Error("Error creating stream for Audio Track {0}. Error: {1}", currentRow.ToString(),
                    Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
          continue;
        }

        log.Info($"Ripping Audio CD Track{currentRow + 1} - {song.Title}");
        var outFile = outFileFormat;
        outFile = outFile.Replace("%albumartist%", artistDir);
        outFile = outFile.Replace("%album%", albumDir);
        outFile = outFile.Replace("%genre%", Genre);
        outFile = outFile.Replace("%year%", Year);
        outFile = outFile.Replace("%artist%", song.Artist);
        outFile = outFile.Replace("%track%", song.Track.PadLeft(_options.MainSettings.NumberTrackDigits, '0'));
        outFile = outFile.Replace("%title%", song.Title);
        outFile = Util.MakeValidFileName(outFile);

        outFile = $@"{targetDir}\{outFile}";

        var audioEncoder = new AudioEncoder();
        outFile = audioEncoder.SetEncoder(_encoder, outFile);

        if (audioEncoder.StartEncoding(stream, currentRow) != BASSError.BASS_OK)
        {
          log.Error("Error starting Encoder for Audio Track {0}. Error: {1}", currentRow.ToString(),
                    Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
        }

        Bass.BASS_StreamFree(stream);
        log.Info($"Finished Ripping Audio CD Track{currentRow + 1}");

        try
        {
          // Now Tag the encoded File
          TagLib.File file = TagLib.File.Create(outFile);
          file.Tag.AlbumArtists = new[] { AlbumArtist };
          file.Tag.Album = Album;
          file.Tag.Genres = new[] { Genre };
          file.Tag.Year = Year == string.Empty ? 0 : Convert.ToUInt32(Year);
          file.Tag.Performers = new[] { song.Artist };
          file.Tag.Track = Convert.ToUInt16(song.Track);
          file.Tag.Title = song.Title;
          file = Util.FormatID3Tag(file);
          file.Save();
        }
        catch (Exception ex)
        {
          log.Error("Error tagging encoded file {0}. Error: {1}", outFile, ex.Message);
        }

        currentRow++;
      }

      _ripActive = false;
      IsBusy = false;

      // Unlock the Drive and open the door, if selected
      BassCd.BASS_CD_Door(_driveID, BASSCDDoor.BASS_CD_DOOR_UNLOCK);
      if (_options.MainSettings.RipEjectCD)
      {
        BassCd.BASS_CD_Door(_driveID, BASSCDDoor.BASS_CD_DOOR_OPEN);
      }

      if (_options.MainSettings.RipActivateTargetFolder)
      {
        _options.MainSettings.LastFolderUsed = targetDir;
        var evt = new GenericEvent
        {
          Action = "activatetargetfolder"
        };
        EventSystem.Publish(evt);
      }

      log.Trace("<<<");
    }

    #region GnuDB 

    /// <summary>
    /// Run a Query to GnuDB
    /// </summary>
    /// <param name="driveLetter"></param>
    private void QueryGnuDB(string driveLetter)
    {
      log.Trace(">>>");
      _driveID = Util.Drive2BassID(Convert.ToChar(driveLetter));
      if (_driveID < 0)
      {
        return;
      }

      log.Info("Starting GnuDB Lookup");
      IsBusy = true;
      try
      {
        GnuDBQuery gnuDbQuery = new GnuDBQuery();

        gnuDbQuery.Connect();
        _cds = gnuDbQuery.GetDiscInfo(Convert.ToChar(driveLetter));
        gnuDbQuery.Disconnect();

        if (_cds != null && _cds.Length > 0)
        {
          log.Debug($"GnuDB: Found {_cds.Length} matching discs.");
          var index = 0;
          foreach (var cd in _cds)
          {
            CDTitles.Add(new Item(cd.Title, $"{index} - GnuDB"));
            index++;
          }
        }
        else
        {
          log.Debug("GnuDB: Disc could not be located in GnuDB.");
        }
      }
      catch (System.Net.WebException webEx)
      {
        if (webEx.Status == WebExceptionStatus.Timeout)
        {
          log.Info("GnuDB: Timeout querying GnuDB. No Data returned for CD");
        }
        else
        {
          log.Error("GnuDB: Exception querying Disc. {0} {1}", webEx.Message, webEx.StackTrace);
        }
      }
      catch (Exception ex)
      {
        log.Error("GnuDB: Exception querying Disc. {0} {1}", ex.Message, ex.StackTrace);
      }
      log.Info("Finished GnuDB Lookup");
      IsBusy = false;

      log.Trace("<<<");
    }

    private void FetchCDDetails(CDInfo cd)
    {
      log.Info($"Fetching GnuDB Details for {cd.Title}");
      IsBusy = true;
      CDInfoDetail MusicCD = new CDInfoDetail();
      try
      {
        GnuDBQuery gnuDbQuery = new GnuDBQuery();
        gnuDbQuery.Connect();
        MusicCD = gnuDbQuery.GetDiscDetails(cd.Category, cd.DiscId);

        if (MusicCD != null)
        {
          AlbumArtist = MusicCD.Artist;
          Album = MusicCD.Title;
          Genre = MusicCD.Genre;
          Year = MusicCD.Year.ToString();

          foreach (CDTrackDetail trackDetail in MusicCD.Tracks)
          {
            if (trackDetail.Artist == null)
            {
              trackDetail.Artist = MusicCD.Artist;
            }

            var song = new RipData
            {
              IsChecked = true,
              Artist = trackDetail.Artist,
              Title = trackDetail.Title,
              Track = trackDetail.Track.ToString(),
              Duration = trackDetail.Duration
            };
            Songs.Add(song);
          }
        }
        else
        {
          int numTracks = BassCd.BASS_CD_GetTracks(_driveID);
          for (int i = 0; i < numTracks; i++)
          {
            CDTrackDetail trackDetail = new CDTrackDetail();
            trackDetail.Track = i + 1;
            trackDetail.Title = $"Track{(i + 1).ToString().PadLeft(2, '0')}";

            var song = new RipData
            {
              IsChecked = true,
              Artist = trackDetail.Artist,
              Title = trackDetail.Title,
              Track = trackDetail.Track.ToString(),
              Duration = trackDetail.Duration
            };
            Songs.Add(song);
          }
        }

        gnuDbQuery.Disconnect();
      }
      catch (System.Net.WebException webEx)
      {
        if (webEx.Status == WebExceptionStatus.Timeout)
        {
          log.Info("GnuDB: Timeout fetching CD Details");
          MusicCD = null;
        }
        else
        {
          log.Error("GnuDB: Exception fetching Disc details. {0} {1}", webEx.Message, webEx.StackTrace);
          MusicCD = null;
        }
      }
      catch (Exception ex)
      {
        log.Error("GnuDB: Exception fetching Disc details. {0} {1}", ex.Message, ex.StackTrace);
        MusicCD = null;
      }
      log.Info("Finished GnuDB Lookup");
      IsBusy = false;
    }

    #endregion

    #region MusicBrainz

    /// <summary>
    /// Runs a CD search against MusicBrainz
    /// </summary>
    /// <param name="driveLetter"></param>
    private void QueryMusicBrainz(string driveLetter)
    {
      log.Trace(">>>");
      _driveID = Util.Drive2BassID(Convert.ToChar(driveLetter));
      if (_driveID < 0)
      {
        return;
      }

      log.Info("Starting MusicBrainz CD Lookup");
      _musicBrainzReleases.Clear();
      IsBusy = true;
      try
      {
        _mbId = BassCd.BASS_CD_GetID(_driveID, BASSCDId.BASS_CDID_MUSICBRAINZ);
        var mbURL = $@"https://musicbrainz.org/ws/2/discid/{_mbId}?fmt=json&inc=artists+recordings+artist-credits";
        var mbResponse = Util.GetWebPage(mbURL);

        var json = JObject.Parse(mbResponse);
        _musicBrainzReleases = GetReleases(ref json);

        var index = 0;
        foreach (var release in _musicBrainzReleases)
        {
          var title = $"{release.Credits[0].Name} / {release.Title} - {release.Country} ({release.Date})";
          CDTitles.Add(new Item(title, $"{index} - MusicBrainz"));
          index++;
        }
      }
      catch (Exception ex)
      {
        log.Error("MusicBrainz: Error parsing Json Result. {0} {1}", ex.Message, ex.StackTrace);
      }
      log.Info("Finished MusicBrainz Lookup");
      IsBusy = false;

      log.Trace("<<<");
    }

    private List<Release> GetReleases(ref JObject json)
    {
      var rel = new List<Release>();
      var releases = (json["releases"] as JArray)?.Select(r => (object)r).ToList();
      if (releases != null)
      {
        foreach (JObject r in releases)
        {
          var release = new Release { Title = (string)r["title"], Country = (string)r["country"], Date = (string)r["date"] };
          var credit = (r["artist-credit"] as JArray)?.Select(c => (object)c).ToList();
          if (credit != null)
          {
            release.Credits = new List<NameCredit>();
            foreach (JObject c in credit)
            {
              var namecredit = new NameCredit { Name = (string)c["name"] };
              release.Credits.Add(namecredit);
            }
          }
          var media = (r["media"] as JArray)?.Select(m => (object)m).ToList();
          if (media != null)
          {
            release.Media = new List<Medium>();
            foreach (JObject m in media)
            {
              var medium = new Medium();
              var tracks = (m["tracks"] as JArray)?.Select(t => (object)t).ToList();
              if (tracks != null)
              {
                medium.Tracks = new List<Track>();
                foreach (JObject t in tracks)
                {
                  var track = new Track();
                  track.Position = (int)t["position"];
                  track.Recording = new Recording();
                  track.Recording.Title = (string)t["title"];
                  track.Recording.Credits = new List<NameCredit>();
                  track.Length = (int)t["length"];
                  foreach (JObject c in t["artist-credit"])
                  {
                    track.Recording.Credits.Add(new NameCredit { Name = (string)c["name"] });
                  }
                  medium.Tracks.Add(track);
                }
                medium.TrackCount = tracks.Count;
              }

              var discs = (m["discs"] as JArray)?.Select(d => (object)d).ToList();
              if (discs != null)
              {
                medium.Discs = new List<Disc>();
                foreach (JObject d in discs)
                {
                  var disc = new Disc { Id = (string)d["id"] };
                  medium.Discs.Add(disc);
                }

              }
              release.Media.Add(medium);
            }
          }
          rel.Add(release);
        }
      }
      return rel;
    }

    private void FetchMusicBrainzDetails(Release release)
    {
      log.Trace(">>>");
      foreach (var credit in release.Credits)
      {
        AlbumArtist += credit.Name + ";";
      }

      AlbumArtist = AlbumArtist.Substring(0, AlbumArtist.Length - 1); // remove the trailing semicolon
      Album = release.Title;
      Year = release.Date;

      foreach (var medium in release.Media)
      {
        bool foundId = false;
        foreach (var disc in medium.Discs)
        {
          if (disc.Id == _mbId)
          {
            foundId = true;
            break;
          }
        }

        if (foundId)
        {
          foreach (var track in medium.Tracks)
          {
            var song = new RipData
            {
              IsChecked = true,
              Title = track.Recording.Title,
              Track = track.Position.ToString(),
            };

            var duration = TimeSpan.FromMilliseconds(Convert.ToDouble(track.Length.Value)).ToString(@"hh\:mm\:ss");
            if (duration.StartsWith("00:"))
            {
              duration = duration.Substring(3);
            }
            song.Duration = duration;

            foreach (var credit in track.Recording.Credits)
            {
              song.Artist += credit.Name + ";";
            }

            song.Artist = song.Artist.Substring(0, song.Artist.Length - 1);
            Songs.Add(song);
          }

          break;
        }
      }



      log.Trace("<<<");
    }


    #endregion

    #region Encoder Settings

    /// <summary>
    ///   Sets the Mode according to the Settings / Selection
    /// </summary>
    private void SetWMACbrVbr()
    {
      var idx = 0;
      foreach (Item item in WmaCBRVBR)
      {
        if ((item.Value as string).StartsWith(_options.MainSettings.RipEncoderWMACbrVbr))
        {
          WmaCBRVBRSelectedIndex = idx;
          break;
        }

        idx++;
      }
    }

    /// <summary>
    ///   Fills the Sample Format Combo box
    /// </summary>
    private void SetWMASampleCombo()
    {
      Item[] modeTab = null;
      var defaultValue = 0;
      var vbrcbr = WmaCBRVBR[WmaCBRVBRSelectedIndex].Value;
      string encoder = WmaEncoder[WmaEncoderSelectedIndex].Value;

      if (encoder == "wma")
      {
        if (vbrcbr == "Cbr")
        {
          modeTab = _options.WmaStandardSampleCBR;
          defaultValue = 10;
          _defaultBitRateIndex = 4;
        }
        else
        {
          modeTab = _options.WmaStandardSampleVBR;
          defaultValue = 0;
          _defaultBitRateIndex = 4;
        }
      }
      else if (encoder == "wmapro")
      {
        if (vbrcbr == "Cbr")
        {
          modeTab = _options.WmaProSampleCBR;
          defaultValue = 1;
          _defaultBitRateIndex = 4;
        }
        else
        {
          modeTab = _options.WmaProSampleVBR;
          defaultValue = 0;
          _defaultBitRateIndex = 4;
        }
      }
      else
      {
        // Lossless
        modeTab = _options.WmaLosslessSampleVBR;
        defaultValue = 0;
        _defaultBitRateIndex = 0;
      }
      WmaSampleFormat.Clear();
      WmaSampleFormat.AddRange(modeTab);

      bool found = false;
      var idx = 0;
      foreach (Item item in WmaSampleFormat)
      {
        if ((string)item.Value == _options.MainSettings.RipEncoderWMASample)
        {
          WmaSampleFormatSelectedIndex = idx;
          found = true;
          break;
        }
        idx++;
      }

      if (!found)
        WmaSampleFormatSelectedIndex = defaultValue;
    }

    /// <summary>
    ///   Fills the Bitrate combo, according to the selection in the Sample and CbrVbr Combo
    /// </summary>
    private void SetWMABitRateCombo()
    {
      var vbrcbr = WmaCBRVBR[WmaCBRVBRSelectedIndex].Value;
      var sampleFormat = WmaSampleFormat[WmaSampleFormatSelectedIndex].Value.Split(',');

      BASSWMAEncode encodeFlags = BASSWMAEncode.BASS_WMA_ENCODE_DEFAULT;

      string encoder = WmaEncoder[WmaEncoderSelectedIndex].Value;
      if (encoder == "wmapro" || encoder == "wmalossless")
        encodeFlags = encodeFlags | BASSWMAEncode.BASS_WMA_ENCODE_PRO;
      else
        encodeFlags = encodeFlags | BASSWMAEncode.BASS_WMA_ENCODE_STANDARD;

      if (vbrcbr == "Cbr")
        encodeFlags = encodeFlags | BASSWMAEncode.BASS_WMA_ENCODE_RATES_CBR;
      else
        encodeFlags = encodeFlags | BASSWMAEncode.BASS_WMA_ENCODE_RATES_VBR;

      if (sampleFormat[0] == "24")
        encodeFlags = encodeFlags | BASSWMAEncode.BASS_WMA_ENCODE_24BIT;

      WmaBitRate.Clear();
      if (encoder == "wmalossless")
      {
        WmaBitRate.Add(100);
        WmaBitRateSelectedIndex = 0;
      }
      else
      {
        int[] bitRates = BassWma.BASS_WMA_EncodeGetRates(Convert.ToInt32(sampleFormat[2]),
                                                         Convert.ToInt32(sampleFormat[1]), encodeFlags);
        foreach (var bitrate in bitRates)
        {
          WmaBitRate.Add(bitrate);
        }

        bool found = false;
        var idx = 0;
        foreach (var bitrate in WmaBitRate)
        {
          if (bitrate == _options.MainSettings.RipEncoderWMABitRate)
          {
            WmaBitRateSelectedIndex = idx;
            found = true;
            break;
          }
          idx++;
        }

        if (!found)
        {
          if (WmaBitRate.Count - 1 < _defaultBitRateIndex)
          {
            WmaBitRateSelectedIndex = 0;
          }
          else
          {
            WmaBitRateSelectedIndex = _defaultBitRateIndex;
          }
        }
      }
    }

    #endregion

    #endregion

    #region Event Handling

    /// <summary>
    /// General Message Handler
    /// </summary>
    /// <param name="msg"></param>
    private void OnMessageReceived(GenericEvent msg)
    {
      switch (msg.Action.ToLower())
      {
        case "conversionprogress":
          var rowinddex = (int)msg.MessageData["rowindex"];
          var percent = (double)msg.MessageData["percent"];
          Songs[rowinddex].PercentComplete = percent;
          break;

        case "command":

          var command = (Action.ActionType)msg.MessageData["command"];
          log.Trace($"Command {command}");
          if (command == Action.ActionType.RIP)
          {
            RipCd();
            return;
          }

          if (command == Action.ActionType.RIPCANCEL)
          {
            RipCancel();
          }

          if (command == Action.ActionType.GNUDBQUERY)
          {
            var cdinfos = BassCd.BASS_CD_GetInfos(true);
            for (var i = 0; i < cdinfos.Length; i++)
            {
              if (BassCd.BASS_CD_IsReady(i))
              {
                MediaInserted(cdinfos[i].DriveLetter.ToString());
              }
            }

          }
          break;
      }
    }

    #endregion

  }
}
