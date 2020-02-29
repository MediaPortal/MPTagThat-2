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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;

#endregion

namespace MPTagThat.Core.Utils
{
  public class Mp3Val
  {
    #region Variables

    private static List<string> StdOutList;
    private static readonly NLogLogger log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger)?.GetLogger;
    private static Options _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;
    #endregion

    #region File Validation

    public static Util.MP3Error ValidateMp3File(string fileName, out string strError)
    {
      ValidateOrFixFile(fileName, false);

      strError = "";
      // we might have an error in mp3val. the Log should contain the error
      if (StdOutList.Count == 0)
      {
        return Util.MP3Error.NoError;
      }

      var error = Util.MP3Error.NoError;

      // No errors found
      if (StdOutList[0].Contains("Done!"))
      {
        return Util.MP3Error.NoError;
      }
      else if (StdOutList[0].Contains(@"No supported tags in the file"))
      {
        return Util.MP3Error.NoError; // Fixed by MPTagThat :-)
      }
      else if (StdOutList[0].Contains(@"Garbage at the beginning of the file"))
      {
        error = Util.MP3Error.Fixable; // Fixable error
        strError = StdOutList[0];
      }
      else if (StdOutList[0].Contains(@"Garbage at the end of the file"))
      {
        error = Util.MP3Error.Fixable; // Fixable error
        strError = StdOutList[0];
      }
      else if (StdOutList[0].Contains(@"MPEG stream error, resynchronized successfully"))
      {
        error = Util.MP3Error.Fixable; // Fixable error
        strError = StdOutList[0];
      }
      else if (StdOutList[0].Contains(@"This is a RIFF file, not MPEG stream"))
      {
        error = Util.MP3Error.Fixable; // Fixable error
        strError = StdOutList[0];
      }
      else if (StdOutList[0].Contains(@"It seems that file is truncated or there is garbage at the end of the file"))
      {
        error = Util.MP3Error.Fixable; // Fixable error
        strError = StdOutList[0];
      }
      else if (StdOutList[0].Contains(@"Wrong number of MPEG frames specified in Xing header"))
      {
        error = Util.MP3Error.Fixable; // Fixable error
        strError = StdOutList[0];
      }
      else if (StdOutList[0].Contains(@"Wrong number of MPEG data bytes specified in Xing header"))
      {
        error = Util.MP3Error.Fixable; // Fixable error
        strError = StdOutList[0];
      }
      else if (StdOutList[0].Contains(@"Wrong number of MPEG frames specified in VBRI header"))
      {
        error = Util.MP3Error.Fixable; // Fixable error
        strError = StdOutList[0];
      }
      else if (StdOutList[0].Contains(@"Wrong number of MPEG data bytes specified in VBRI header"))
      {
        error = Util.MP3Error.Fixable; // Fixable error
        strError = StdOutList[0];
      }
      else if (StdOutList[0].Contains(@"Wrong CRC in"))
      {
        error = Util.MP3Error.Fixable; // Fixable error
        strError = StdOutList[0];
      }
      else if (StdOutList[0].Contains(@"Several APEv2 tags in one file"))
      {
        return Util.MP3Error.NoError; // Handled by MPTagThat
      }
      else if (StdOutList[0].Contains(@"Too few MPEG frames"))
      {
        error = Util.MP3Error.NonFixable; // Non Fixable error
        strError = StdOutList[0];
      }
      else if (StdOutList[0].Contains(@"VBR detected, but no VBR header is present. Seeking may not work properly"))
      {
        error = Util.MP3Error.NonFixable; // Non Fixable error
        strError = StdOutList[0];
      }
      else if (StdOutList[0].Contains(@"Different MPEG versions or layers in one file"))
      {
        error = Util.MP3Error.NonFixable; // Non Fixable error
        strError = StdOutList[0];
      }
      else if (StdOutList[0].Contains(@"Non-layer-III frame encountered"))
      {
        error = Util.MP3Error.NonFixable; // Non Fixable error
        strError = StdOutList[0];
      }

      if (error == Util.MP3Error.Fixable)
      {
        log.Warn($"MP3 Validate Fixable error: {StdOutList[0]}");
      }
      else if (error == Util.MP3Error.NonFixable)
      {
        log.Warn($"MP3 Validate Non-Fixable error: {StdOutList[0]}");
      }

      // This happens, if we fixed an error
      if (StdOutList.Count > 2)
      {
        if (StdOutList[StdOutList.Count - 2].Contains(@"FIXED:"))
        {
          error = Util.MP3Error.Fixed;
        }
      }

      return error;
    }

    public static Util.MP3Error FixMp3File(string fileName, out string strError)
    {
      ValidateOrFixFile(fileName, true);
      strError = "";
      // we might have an error in mp3val. the Log should contain the error
      if (StdOutList.Count == 0)
      {
        return Util.MP3Error.NoError;
      }

      var error = Util.MP3Error.NoError;

      // No errors found
      if (StdOutList[0].Contains("Done!"))
      {
        return Util.MP3Error.NoError;
      }

      // This happens, if we fixed an error
      if (StdOutList.Count > 2)
      {
        if (StdOutList[StdOutList.Count - 2].Contains(@"FIXED:"))
        {
          error = Util.MP3Error.Fixed;
        }
      }

      strError = StdOutList[0];
      return error;
    }

    #endregion

    #region Mp3Val Handling

    private static List<string> ValidateOrFixFile(string mp3file, bool fix)
    {
      if (StdOutList == null)
      {
        StdOutList = new List<string>();
      }

      var parm = "-si ";
      if (_options.MainSettings.MP3AutoFix || fix)
      {
        parm += "-f -nb -t ";
      }

      parm = $"{parm} \"{mp3file}\"";

      return ExecuteProcReturnStdOut(parm, 3000);
    }

    /// <summary>
    ///   Executes commandline processes and parses their output
    /// </summary>
    /// <param name = "aArguments">The arguments to supply for the given process</param>
    /// <param name = "aExpectedTimeoutMs">How long the function will wait until the tool's execution will be aborted</param>
    /// <returns>A list containing the redirected StdOut line by line</returns>
    public static List<string> ExecuteProcReturnStdOut(string aArguments, int aExpectedTimeoutMs)
    {
      StdOutList.Clear();

      var MP3ValProc = new Process();
      var ProcOptions = new ProcessStartInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Bin\mp3val.exe"))
      {
        Arguments = aArguments,
        UseShellExecute = false,
        RedirectStandardError = true,
        RedirectStandardOutput = true,
        StandardOutputEncoding = Encoding.GetEncoding("ISO-8859-1"),
        StandardErrorEncoding = Encoding.GetEncoding("ISO-8859-1"),
        CreateNoWindow = true,
        ErrorDialog = false
      };

      MP3ValProc.OutputDataReceived += StdOutDataReceived;
      MP3ValProc.ErrorDataReceived += StdErrDataReceived;
      MP3ValProc.EnableRaisingEvents = true; // We want to know when and why the process died        
      MP3ValProc.StartInfo = ProcOptions;
      if (File.Exists(ProcOptions.FileName))
      {
        try
        {
          MP3ValProc.Start();
          MP3ValProc.BeginErrorReadLine();
          MP3ValProc.BeginOutputReadLine();

          // wait this many seconds until mp3val has to be finished
          MP3ValProc.WaitForExit(aExpectedTimeoutMs);
          if (MP3ValProc.HasExited && MP3ValProc.ExitCode != 0)
          {
            log.Warn($"MP3Val: Did not exit properly with arguments: {aArguments}, exitcode: {MP3ValProc.ExitCode}");
          }
        }
        catch (Exception ex)
        {
          log.Error("MP3Val: Error executing mp3val: {0}", ex.Message);
        }
      }
      else
      {
        log.Warn($"MP3VAL: Could not start {ProcOptions.FileName} because it doesn't exist!");
      }

      return StdOutList;
    }

    #endregion

    #region output handler

    private static void StdErrDataReceived(object sendingProc, DataReceivedEventArgs errLine)
    {
      if (!string.IsNullOrEmpty(errLine.Data))
      {
        log.Error("MP3Val: Error executing mp3val: {0}", errLine.Data);
      }
    }

    private static void StdOutDataReceived(object sendingProc, DataReceivedEventArgs e)
    {
      if (!string.IsNullOrEmpty(e.Data))
      {
        if (e.Data.Contains(@"Analyzing file"))
        {
          return;
        }

        StdOutList.Add(e.Data);
      }
    }

    #endregion
  }
}
