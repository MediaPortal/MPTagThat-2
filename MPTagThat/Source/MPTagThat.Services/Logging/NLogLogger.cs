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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Windows;
using NLog;
using NLog.Config;
using NLog.Filters;
using NLog.Targets;
using Prism.Logging;

#endregion

namespace MPTagThat.Services.Logging
{
  /// <summary>
  /// Logging class to be used within the product
  /// </summary>
  public class NLogLogger : ILoggerFacade
  {
    #region Variables

    private readonly string fileName; //holds the file to write to.
    private LogLevel level; //holds the treshold for the log level.
    private Logger _logger = null;
    private const int MAXARCHIVES = 10;

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

      this.fileName = $@"{logPath}\{fileName}";

      var ext = Path.GetExtension(this.fileName);
      var fileNamePattern = Path.ChangeExtension(this.fileName, ".{#}" + ext);
      this.level = level;

      // Now configure the NLOG File Target looger
      var config = new LoggingConfiguration();
      var fileTarget = new FileTarget();
      fileTarget.FileName = this.fileName;
      fileTarget.MaxArchiveFiles = MAXARCHIVES;
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

    #region ILoggerFacade implementation

    public void Log(string message, Category category, Priority priority)
    {
      switch (category)
      {
        case Category.Debug:
          _logger.Debug(message);
          break;
        case Category.Exception:
          _logger.Error(message);
          break;
        case Category.Info:
          _logger.Info(message);
          break;
        case Category.Warn:
          _logger.Error(message);
          break;
      }
    }

    #endregion
  }
}
