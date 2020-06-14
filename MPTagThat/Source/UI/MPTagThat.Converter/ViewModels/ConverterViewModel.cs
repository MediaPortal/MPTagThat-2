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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MPTagThat.Converter.Models;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.AudioEncoder;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Core.Utils;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Syncfusion.Data.Extensions;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Wma;
using WPFLocalizeExtension.Engine;
using Action = MPTagThat.Core.Common.Action;

#endregion

namespace MPTagThat.Converter.ViewModels
{
  public class ConverterViewModel : BindableBase
  {
    #region Variables

    private object _lock = new object();
    private IRegionManager _regionManager;
    private readonly NLogLogger log;
    private Options _options;

    private Thread _threadConvert;
    private bool _conversionActive = false;
    private CancellationTokenSource  _cts;
    private string _encoder = null;

    private int _defaultBitRateIndex;

    #endregion

    #region ctor

    public ConverterViewModel(IRegionManager regionManager)
    {
      _regionManager = regionManager;
      log = ContainerLocator.Current.Resolve<ILogger>()?.GetLogger;
      log.Trace(">>>");
      _options = ContainerLocator.Current.Resolve<ISettingsManager>()?.GetOptions;
      EventSystem.Subscribe<GenericEvent>(OnMessageReceived);

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

      ConvertRootFolder = _options.MainSettings.ConvertRootFolder;
      ConvertFileFormat = _options.MainSettings.RipFileNameFormat;
      
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

      MusepackPreset.Add(new Item("Low/Medium Quality (~  90 kbps)","thumb",""));
      MusepackPreset.Add(new Item("Medium Quality     (~ 130 kbps)","radio",""));
      MusepackPreset.Add(new Item("High Quality       (~ 180 kbps)","standard",""));
      MusepackPreset.Add(new Item("Excellent Quality  (~ 210 kbps)","xtreme",""));
      MusepackPreset.Add(new Item("Excellent Quality  (~ 240 kbps)","insane",""));
      MusepackPreset.Add(new Item("Highest Quality    (~ 270 kbps)","braindead",""));

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

      WavPackPreset.Add(new Item("Fast Mode (fast, but some compromise in compression ratio)","-f",""));
      WavPackPreset.Add(new Item("High quality (better compression, but slower)","-h",""));

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

      WmaEncoder.Add(new Item("Windows Media Audio Standard","wma",""));
      WmaEncoder.Add(new Item("Windows Media Audio Professional","wmapro",""));
      WmaEncoder.Add(new Item("Windows Media Audio Lossless","wmalossless",""));

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
      ContextMenuClearListCommand = new BaseCommand(ContextMenuClearList);
      ContextMenuSelectAllCommand = new BaseCommand(ContextMenuSelectAll);

      log.Trace("<<<");
    }

    #endregion

    #region Properties

    /// <summary>
    /// The Songs in the Grid
    /// </summary>

    private ObservableCollection<ConverterData> _songs = new ObservableCollection<ConverterData>();
    public ObservableCollection<ConverterData> Songs
    {
      get => _songs;
      set => SetProperty(ref _songs, value);
    }

    /// <summary>
    /// The selected Items in the Grid
    /// </summary>
    private ObservableCollection<object> _selectedItems = new ObservableCollection<object>();
    public ObservableCollection<object> SelectedItems
    {
      get => _selectedItems;
      set => SetProperty(ref _selectedItems, value);
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
    /// The Root folder for Convert
    /// </summary>
    private string _convertRootFolder;

    public string ConvertRootFolder
    {
      get => _convertRootFolder;
      set
      {
        _options.MainSettings.ConvertRootFolder = value;
        SetProperty(ref _convertRootFolder, value);
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
    private string _convertFileFormat;

    public string ConvertFileFormat
    {
      get => _convertFileFormat;
      set
      {
        SetProperty(ref _convertFileFormat, value);
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

    #region Commands

    /// <summary>
    /// Remove the selected Items from the list
    /// </summary>
    public ICommand ContextMenuClearListCommand { get; }

    private void ContextMenuClearList(object param)
    {
      var songs = (param as ObservableCollection<object>).Cast<ConverterData>().ToList();
      foreach (var song in songs)
      {
        Songs.Remove(song);
      }
    }

    /// <summary>
    /// Select all songs in the list
    /// </summary>
    public ICommand ContextMenuSelectAllCommand { get; }

    private void ContextMenuSelectAll(object param)
    {
      Songs.ForEach(song => SelectedItems.Add(song));
    }

    #endregion

    #region Private Methods

    /// <summary>
    ///   Cancel the Conversion Process
    /// </summary>
    public void ConvertFilesCancel()
    {
      if (_cts != null)
      {
        _cts.Cancel();
      }
      _conversionActive = false;
    }

    /// <summary>
    ///   Converts the selected files in the Grid
    /// </summary>
    public void ConvertFiles()
    {
      if (_conversionActive)
      {
        log.Info("Conversion already running.");
        return;
      }

      if (Songs.Count == 0)
      {
        log.Info("No files in Conversion List");
        return;
      }

      if (string.IsNullOrEmpty(ConvertRootFolder))
      {
        log.Info("Empty Conversion Folder. Defaulting to Root of First Song.");
        var path = Songs[0].Song.FullFileName;
        var root = Path.GetPathRoot(path);
        while (true)
        {
          var temp = Path.GetDirectoryName(path);
          if (temp != null && temp.Equals(root))
            break;
          path = temp;
        }
        ConvertRootFolder = path;
      }

      try
      {
        if (!Directory.Exists(ConvertRootFolder))
        {
          log.Info("Creating Conversion Target Folder");
          Directory.CreateDirectory(ConvertRootFolder);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "convert_ErrorDirectory", LocalizeDictionary.Instance.Culture).ToString(),
          LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Error_Title", LocalizeDictionary.Instance.Culture).ToString(),
          MessageBoxButton.OK);
        log.Error("Error creating Conversion output directory: {0}. {1}", ConvertRootFolder, ex.Message);
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

      _threadConvert = new Thread(ConversionThread);
      _threadConvert.Name = "Conversion";
      _threadConvert.Priority = ThreadPriority.Highest;
      _threadConvert = new Thread(ConversionThread);
      _threadConvert.Start();
    }

    /// <summary>
    /// Run the conversion
    /// </summary>
    private void ConversionThread()
    {
      log.Trace(">>>");
      
      IsBusy = true;
      _cts = new CancellationTokenSource();

      var po = new ParallelOptions
      {
        CancellationToken = _cts.Token, MaxDegreeOfParallelism = System.Environment.ProcessorCount
      };

      _conversionActive = true;
      try
      {
        Parallel.For(0, _songs.Count, po, index =>
        {
          po.CancellationToken.ThrowIfCancellationRequested();
          var song = _songs[index];
          var inputFile = song.Song.FullFileName;
          var outFile = Util.ReplaceParametersWithTrackValues(_options.MainSettings.RipFileNameFormat, song.Song);
          outFile = Path.Combine(ConvertRootFolder, outFile);
          string directoryName = Path.GetDirectoryName(outFile);

          // Now check the validity of the directory
          if (directoryName != null && !Directory.Exists(directoryName))
          {
            try
            {
              Directory.CreateDirectory(directoryName);
            }
            catch (Exception e1)
            {
              log.Error("Error creating folder: {0} {1]", directoryName, e1.Message);
              song.Status = e1.Message;
              return;
            }
          }

          var audioEncoder = new AudioEncoder();
          outFile = audioEncoder.SetEncoder(_encoder, outFile);
          song.NewFileName = outFile;

          if (inputFile == outFile)
          {
            song.Status = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings",
              "convert_SameFile", LocalizeDictionary.Instance.Culture).ToString();
            log.Error("No conversion for {0}. Output would overwrite input", inputFile);
            return;
          }

          int stream = Bass.BASS_StreamCreateFile(inputFile, 0, 0, BASSFlag.BASS_STREAM_DECODE);
          if (stream == 0)
          {
            song.Status = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings",
                            "convert_OpenFileError", LocalizeDictionary.Instance.Culture).ToString()
                          + ": " + Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode());

            log.Error("Error creating stream for file {0}. Error: {1}", inputFile,
              Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
            return;
          }

          log.Info($"Convert file {inputFile} -> {outFile}");

          if (audioEncoder.StartEncoding(stream, index) != BASSError.BASS_OK)
          {
            song.Status = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings",
                            "convert_ErrorEncoding", LocalizeDictionary.Instance.Culture).ToString()
                          + ": " + Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode());

            log.Error("Error starting Encoder for File {0}. Error: {1}", inputFile,
              Enum.GetName(typeof(BASSError), Bass.BASS_ErrorGetCode()));
            Bass.BASS_StreamFree(stream);
            return;
          }

          song.PercentComplete = 100;

          Bass.BASS_StreamFree(stream);

          try
          {
            // Now Tag the encoded File
            TagLib.File tagInFile = TagLib.File.Create(inputFile);
            TagLib.File tagOutFile = TagLib.File.Create(outFile);
            tagOutFile.Tag.AlbumArtists = tagInFile.Tag.AlbumArtists;
            tagOutFile.Tag.Album = tagInFile.Tag.Album;
            tagOutFile.Tag.Genres = tagInFile.Tag.Genres;
            tagOutFile.Tag.Year = tagInFile.Tag.Year;
            tagOutFile.Tag.Performers = tagInFile.Tag.Performers;
            tagOutFile.Tag.Track = tagInFile.Tag.Track;
            tagOutFile.Tag.TrackCount = tagInFile.Tag.TrackCount;
            tagOutFile.Tag.Title = tagInFile.Tag.Title;
            tagOutFile.Tag.Comment = tagInFile.Tag.Comment;
            tagOutFile.Tag.Composers = tagInFile.Tag.Composers;
            tagOutFile.Tag.Conductor = tagInFile.Tag.Conductor;
            tagOutFile.Tag.Copyright = tagInFile.Tag.Copyright;
            tagOutFile.Tag.Disc = tagInFile.Tag.Disc;
            tagOutFile.Tag.DiscCount = tagInFile.Tag.DiscCount;
            tagOutFile.Tag.Lyrics = tagInFile.Tag.Lyrics;
            tagOutFile.Tag.Pictures = tagInFile.Tag.Pictures;
            tagOutFile = Util.FormatID3Tag(tagOutFile);
            tagOutFile.Save();

            log.Info($"Finished converting file {inputFile} -> {outFile}");
          }
          catch (Exception ex)
          {
            log.Error("Error tagging encoded file {0}. Error: {1}", outFile, ex.Message);
          }
        });
      }
      catch (OperationCanceledException e)
      {
        log.Info("Parallel Tasks aborted");
      }
      finally
      {
        _cts.Dispose();
        _conversionActive = false;
        IsBusy = false;
      }

      log.Trace("<<<");
    }

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

          if (command == Action.ActionType.CONVERT)
          {
            ConvertFiles();
            return;
          }

          if (command == Action.ActionType.CONVERTCANCEL)
          {
            ConvertFilesCancel();
          }
          break;

        case "addtoconversionlist":
          var songs = (List<SongData>)msg.MessageData["songs"];
          foreach (var song in songs)
          {
            Songs.Add(new ConverterData() { Song = song });
          }
          break;
      }
    }

    #endregion
  }
}
