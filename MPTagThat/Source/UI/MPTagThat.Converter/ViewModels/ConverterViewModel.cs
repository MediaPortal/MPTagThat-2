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
using CommonServiceLocator;
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
using Prism.Mvvm;
using Prism.Regions;
using Syncfusion.Data.Extensions;
using Un4seen.Bass;
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

    #endregion

    #region ctor

    public ConverterViewModel(IRegionManager regionManager)
    {
      _regionManager = regionManager;
      log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger)?.GetLogger;
      log.Trace(">>>");
      _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;
      EventSystem.Subscribe<GenericEvent>(OnMessageReceived);

      // Load the Encoders
      Encoders.Add(new Item("MP3 Encoder", "mp3", ""));
      Encoders.Add(new Item("OGG Encoder", "ogg", ""));
      Encoders.Add(new Item("FLAC Encoder", "flac", ""));
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

            return;
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
      }

      log.Trace("<<<");
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
