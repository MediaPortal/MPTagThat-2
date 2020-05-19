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
using System.IO;
using CommonServiceLocator;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using Un4seen.Bass;
using Un4seen.Bass.Misc;

#endregion

namespace MPTagThat.Core.Services.AudioEncoder
{
  public class AudioEncoder : IAudioEncoder
  {
    #region Variables

    private readonly string _pathToEncoders = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin\\Encoder\\");
    private string _encoder;
    private string _outFile;
    private readonly NLogLogger log;
    private readonly Options _options;
    private bool _isAborted;

    #endregion

    #region ctor

    public AudioEncoder()
    {
      log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger)?.GetLogger;
      _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;
    }

    #endregion

        #region IAudioEncoder Members

    /// <summary>
    ///   Sets the Encoder and the Outfile Name
    /// </summary>
    /// <param name = "encoder"></param>
    /// <param name = "outFile"></param>
    /// <returns>Formatted Outfile with Extension</returns>
    public string SetEncoder(string encoder, string outFile)
    {
      _encoder = encoder;
      _outFile = SetOutFileExtension(outFile);
      return _outFile;
    }

    /// <summary>
    ///   Starts encoding using the given Parameters
    /// </summary>
    /// <param name = "stream"></param>
    /// <param name = "rowIndex"></param>
    public BASSError StartEncoding(int stream, int rowIndex)
    {
      var encoder = SetEncoderSettings(stream);
      encoder.EncoderDirectory = _pathToEncoders;
      encoder.OutputFile = _outFile;
      encoder.InputFile = null; // Use stdin

      var encoderHandle = encoder.Start(null, IntPtr.Zero, false);
      if (!encoderHandle)
      {
        return Bass.BASS_ErrorGetCode();
      }

      long pos;
      var chanLength = Bass.BASS_ChannelGetLength(stream);
      _isAborted = false;

      GenericEvent evt = new GenericEvent
      {
        Action = "conversionprogress"
      };
      evt.MessageData.Add("rowindex", rowIndex);
      evt.MessageData.Add("percent", 0);
      
      var encBuffer = new byte[60000]; // our encoding buffer
      while (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING && !_isAborted)
      {
        // getting sample data will automatically feed the encoder
        Bass.BASS_ChannelGetData(stream, encBuffer, encBuffer.Length);
        pos = Bass.BASS_ChannelGetPosition(stream);
        double percentComplete = pos / (double)chanLength * 100.0;

        // Send the message
        evt.MessageData["percent"] = percentComplete;
        EventSystem.Publish(evt);
      }

      encoder.Stop();
      return BASSError.BASS_OK;
    }

    public void AbortEncoding()
    {
      _isAborted = true;
    }

    #endregion

    #region Private Methods

    private string SetOutFileExtension(string outFile)
    {
      string outFileName = outFile;
      switch (_encoder)
      {
        case "mp3":
          outFileName += ".mp3";
          break;

        case "ogg":
          outFileName += ".ogg";
          break;

        case "flac":
          outFileName += ".flac";
          break;

        case "m4a":
          outFileName += ".m4a";
          break;

        case "wav":
          outFileName += ".wav";
          break;

        case "wma":
          outFileName += ".wma";
          break;

        case "mpc":
          outFileName += ".mpc";
          break;

        case "wv":
          outFileName += ".wv";
          break;
      }
      return outFileName;
    }

    private BaseEncoder SetEncoderSettings(int stream)
    {
      BaseEncoder encoder = null;
      switch (_encoder)
      {
        case "mp3":
          EncoderLAME encLame = new EncoderLAME(stream);
          if (_options.MainSettings.RipLameExpert.Length > 0)
          {
            encLame.LAME_CustomOptions = _options.MainSettings.RipLameExpert;
            encLame.LAME_UseCustomOptionsOnly = true;
          }
          else
          {
            if (_options.MainSettings.RipLamePreset == (int)Options.LamePreset.ABR)
              encLame.LAME_PresetName = _options.MainSettings.RipLameABRBitRate.ToString();
            else
              encLame.LAME_PresetName =
                Enum.GetName(typeof (Options.LamePreset), _options.MainSettings.RipLamePreset)?.ToLower();
          }
          encoder = encLame;
          break;

        case "ogg":
          EncoderOGG encOgg = new EncoderOGG(stream);
          if (_options.MainSettings.RipOggExpert.Length > 0)
          {
            encOgg.OGG_CustomOptions = _options.MainSettings.RipOggExpert;
            encOgg.OGG_UseCustomOptionsOnly = true;
          }
          else
          {
            encOgg.OGG_Quality = Convert.ToInt32(_options.MainSettings.RipOggQuality);
          }
          encoder = encOgg;
          break;

        case "flac":
          EncoderFLAC encFlac = new EncoderFLAC(stream);
          if (_options.MainSettings.RipFlacExpert.Length > 0)
          {
            encFlac.FLAC_CustomOptions = _options.MainSettings.RipFlacExpert;
            encFlac.FLAC_UseCustomOptionsOnly = true;
          }
          else
          {
            encFlac.FLAC_CompressionLevel = _options.MainSettings.RipFlacQuality;
          }
          // put a 1k padding block for Tagging in front
          encFlac.FLAC_Padding = 1024;
          encoder = encFlac;
          break;

        case "m4a":
          EncoderFAAC encAAC = new EncoderFAAC(stream);

          var bitrate =
            Convert.ToInt32(_options.MainSettings.RipEncoderAACBitRate.Substring(0,
                                                                                _options.MainSettings.
                                                                                  RipEncoderAACBitRate.IndexOf(' ')));
          encAAC.FAAC_Bitrate = bitrate;
          encAAC.FAAC_Quality = 100;
          encAAC.FAAC_UseQualityMode = true;
          encAAC.FAAC_WrapMP4 = true;

          encoder = encAAC;
          break;

        case "wav":
          var encWav = new EncoderWAV(stream);
          encoder = encWav;
          break;

        case "wma":
          var encWma = new EncoderWMA(stream);
          var sampleFormat = _options.MainSettings.RipEncoderWMASample.Split(',');
          var encoderFormat = _options.MainSettings.RipEncoderWMA;
          if (encoderFormat == "wmapro" || encoderFormat == "wmalossless")
            encWma.WMA_UsePro = true;
          else
            encWma.WMA_ForceStandard = true;

          if (_options.MainSettings.RipEncoderWMACbrVbr == "Vbr")
          {
            encWma.WMA_UseVBR = true;
            encWma.WMA_VBRQuality = Convert.ToInt32(_options.MainSettings.RipEncoderWMABitRate);
          }
          else
            encWma.WMA_Bitrate = Convert.ToInt32(_options.MainSettings.RipEncoderWMABitRate) / 1000;


          if (sampleFormat[0] == "24")
            encWma.WMA_Use24Bit = true;

          encoder = encWma;
          break;

        case "mpc":
          var encMpc = new EncoderMPC(stream);
          if (_options.MainSettings.RipEncoderMPCExpert.Length > 0)
          {
            encMpc.MPC_CustomOptions = _options.MainSettings.RipEncoderMPCExpert;
            encMpc.MPC_UseCustomOptionsOnly = true;
          }
          else
          {
            encMpc.MPC_Preset =
              (EncoderMPC.MPCPreset)Enum.Parse(typeof (EncoderMPC.MPCPreset), _options.MainSettings.RipEncoderMPCPreset);
          }
          encoder = encMpc;
          break;

        case "wv":
          var encWv = new EncoderWavPack(stream);
          if (_options.MainSettings.RipEncoderWVExpert.Length > 0)
          {
            encWv.WV_CustomOptions = _options.MainSettings.RipEncoderWVExpert;
            encWv.WV_UseCustomOptionsOnly = true;
          }
          else
          {
            if (_options.MainSettings.RipEncoderWVPreset == "-f")
              encWv.WV_FastMode = true;
            else
              encWv.WV_HighQuality = true;
          }
          encoder = encWv;
          break;
      }

      return encoder;
    }

    #endregion
  }
}
