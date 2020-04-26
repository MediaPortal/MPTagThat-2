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
using System.IO;
using CommonServiceLocator;
using MPTagThat.Core.Services.Settings;
using NLog;
using NLog.Config;
using NLog.Filters;
using NLog.Targets;

#endregion

namespace MPTagThat.Core.Services.Logging
{
  public enum LogLevel
  {
    Trace,
    Debug,
    Info,
    Warn,
    Error,
  }

  /// <summary>
  /// Logging class to be used within the product
  /// </summary>
  public class NLogLogger : ILogger
  {
    #region Variables

    private LogLevel _level; //holds the treshold for the log level.
    private readonly Logger _logger;
    private const int Maxarchives = 10;

    #endregion

    #region ctor

    public NLogLogger(string fileName, LogLevel level, int portable)
    {
      var logPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\MPTagThat2\Log";
      if (portable == 1)
      {
        logPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\Log";
      }

      if (!Directory.Exists(logPath))
        Directory.CreateDirectory(logPath);

      var fullFileName = $@"{logPath}\{fileName}";
      _level = level;

      // Now configure the NLOG File Target looger
      var config = new LoggingConfiguration();
      var fileTarget = new FileTarget();
      fileTarget.FileName = fullFileName;
      fileTarget.MaxArchiveFiles = Maxarchives;
      fileTarget.ArchiveFileName = $@"{logPath}\Archive\{Path.GetFileNameWithoutExtension(fileName)}.log";
      fileTarget.ArchiveNumbering = ArchiveNumberingMode.Rolling;
      fileTarget.ArchiveOldFileOnStartup = true;
      fileTarget.KeepFileOpen = true;
      fileTarget.OpenFileCacheTimeout = 30;

      fileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.ffffff} " +
                          "[${level:fixedLength=true:padding=5}]" +
                          "[${threadid:padding=3}]" +
                          "[${stacktrace:format=Flat:topFrames=3:skipFrames=2:separator=\":\":fixedLength=true:padding=-40}]: " +
                          "${message} " +
                          "${exception:format=tostring}";

      config.AddTarget("file", fileTarget);

      var rule = new LoggingRule("*", ConvertLogLevel(level), fileTarget);

      // Create a filter to disable Raven Database Debugging
      var filter = new ConditionBasedFilter { Action = FilterResult.Ignore, Condition = "starts-with('${logger}','Raven')" };
      rule.Filters.Add(filter);
      filter = new ConditionBasedFilter { Action = FilterResult.Ignore, Condition = "contains('${logger}', 'Rachis')" };
      rule.Filters.Add(filter);
      config.LoggingRules.Add(rule);

      LogManager.Configuration = config;

      _logger = LogManager.GetLogger("MPTagThat");
    }

    #endregion

    #region ILogger Implementation

    /// <summary>
    /// Returns the defined Logger
    /// </summary>
    public NLogLogger GetLogger => this;

    /// <summary>
    ///   Gets or sets the log level.
    /// </summary>
    /// <value>A <see cref = "LogLevel" /> value that indicates the minimum level messages must have to be 
    ///   written to the file.</value>
    public LogLevel Level
    {
      get { return _level; }
      set
      {
        _level = value;
        NLog.LogLevel internalLevel = ConvertLogLevel(_level);
        
        LoggingConfiguration config = LogManager.Configuration;
        for (int i = 0; i < 6; ++i)
        {
          if (NLog.LogLevel.FromOrdinal(i) < internalLevel)
          {
            config.LoggingRules[0].DisableLoggingForLevel(NLog.LogLevel.FromOrdinal(i));
          }
          else
          {
            config.LoggingRules[0].EnableLoggingForLevel(NLog.LogLevel.FromOrdinal(i));
          }
        }

        //TODO: Add RavenDebug, once the options are set
        /*
        if (Options.StartupSettings.RavenDebug && config.LoggingRules[0].Filters.Count > 0)
        {
          config.LoggingRules[0].Filters.RemoveAt(0);
        }
        */

        LogManager.Configuration = config;
      }
    }

    /// <summary>
    /// Write a Trace Log Message
    /// </summary>
    /// <param name="msg"></param>
    public void Trace(string msg)
    {
      WriteLog(NLog.LogLevel.Trace, msg);
    }

    /// <summary>
    /// Write a Trace Log Message
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    public void Trace(string msg, object[] args)
    {
      WriteLog(NLog.LogLevel.Trace, msg, args);
    }

    /// <summary>
    /// Write a Debug Log Message
    /// </summary>
    /// <param name="msg"></param>
    public void Debug(string msg)
    {
      WriteLog(NLog.LogLevel.Debug, msg);
    }
    /// <summary>
    /// Write a Debug Log Message
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    public void Debug(string msg, object[] args)
    {
      WriteLog(NLog.LogLevel.Debug, msg, args);
    }
    /// <summary>
    /// Write a Info Log Message
    /// </summary>
    /// <param name="msg"></param>
    public void Info(string msg)
    {
      WriteLog(NLog.LogLevel.Info, msg);
    }
    /// <summary>
    /// Write a Info Log Message
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    public void Info(string msg, object[] args)
    {
      WriteLog(NLog.LogLevel.Info, msg, args);
    }

    /// <summary>
    /// Write a Warn Log Message
    /// </summary>
    /// <param name="msg"></param>
    public void Warn(string msg)
    {
      WriteLog(NLog.LogLevel.Warn, msg);
    }

    /// <summary>
    /// Write a Warn Log Message
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    public void Warn(string msg, object[] args)
    {
      WriteLog(NLog.LogLevel.Warn, msg, args);
    }

    /// <summary>
    /// Write a Error Log Message
    /// </summary>
    /// <param name="msg"></param>
    public void Error(string msg)
    {
      WriteLog(NLog.LogLevel.Error, msg);
    }

    /// <summary>
    /// Write a Error Log Message
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    public void Error(string msg, object[] args)
    {
      WriteLog(NLog.LogLevel.Error, msg, args);
    }

    public void Error(string v, string message)
    {
      WriteLog(NLog.LogLevel.Error, v, new object[]{ message});
    }

    public void Error(string v, string folderName, string message)
    {
      WriteLog(NLog.LogLevel.Error, v, new object[] { folderName, message });
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Write the actual message to Nlog Logger
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    private void WriteLog(NLog.LogLevel level, string message)
    {
      _logger.Log(level, message);
    }

    /// <summary>
    /// Write the actual message to Nlog Logger
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    private void WriteLog(NLog.LogLevel level, string message, object[] args)
    {
      _logger.Log(level, message, args);
    }

    /// <summary>
    /// Convert the Log Level from the internal MPTagThat Loglevel to NLog
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    private NLog.LogLevel ConvertLogLevel(LogLevel level)
    {
      NLog.LogLevel nlogLogLevel = NLog.LogLevel.Debug;

      switch (level)
      {
        case LogLevel.Debug:
          nlogLogLevel = NLog.LogLevel.Debug;
          break;

        case LogLevel.Error:
          nlogLogLevel = NLog.LogLevel.Error;
          break;

        case LogLevel.Info:
          nlogLogLevel = NLog.LogLevel.Info;
          break;

        case LogLevel.Warn:
          nlogLogLevel = NLog.LogLevel.Warn;
          break;

        case LogLevel.Trace:
          nlogLogLevel = NLog.LogLevel.Trace;
          break;
      }

      return nlogLogLevel;
    }

    #endregion
  }
}
