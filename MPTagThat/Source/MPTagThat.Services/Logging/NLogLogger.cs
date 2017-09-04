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
using NLog;
using NLog.Config;
using NLog.Filters;
using NLog.Targets;

#endregion

namespace MPTagThat.Services.Logging
{
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
      var logPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\MPTagThat\Log";
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
                          "[${stacktrace:format=Flat:topFrames=1:separator=\":\":fixedLength=true:padding=-30}]: " +
                          "${message} " +
                          "${exception:format=tostring}";

      config.AddTarget("file", fileTarget);

      level = LogLevel.Debug;

      var rule = new LoggingRule("*", level, fileTarget);

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
    public Logger GetLogger => _logger;

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
        LoggingConfiguration config = LogManager.Configuration;
        for (int i = 0; i < 6; ++i)
        {
          if (LogLevel.FromOrdinal(i) < _level)
          {
            config.LoggingRules[0].DisableLoggingForLevel(LogLevel.FromOrdinal(i));
          }
          else
          {
            config.LoggingRules[0].EnableLoggingForLevel(LogLevel.FromOrdinal(i));
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

    #endregion
  }
}
