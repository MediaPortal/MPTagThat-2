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

namespace MPTagThat.Core.Services.Logging
{
  public interface ILogger
  {
    /// <summary>
    /// Returns the Instance of the Logger
    /// </summary>
    NLogLogger GetLogger { get; }

    /// <summary>
    ///   Gets or sets the log level.
    /// </summary>
    /// <value>A <see cref = "NLog.LogLevel" /> value that indicates the minimum level messages must have to be 
    ///   written to the file.</value>
    LogLevel Level { get; set; }

    /// <summary>
    /// Write a Trace Log Message
    /// </summary>
    /// <param name="msg"></param>
    void Trace(string msg);

    /// <summary>
    /// Write a Trace Log Message
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    void Trace(string msg, object[] args);

    /// <summary>
    /// Write a Debug Log Message
    /// </summary>
    /// <param name="msg"></param>
    void Debug(string msg);

    /// <summary>
    /// Write a Debug Log Message
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    void Debug(string msg, object[] args);

    /// <summary>
    /// Write a Info Log Message
    /// </summary>
    /// <param name="msg"></param>
    void Info(string msg);

    /// <summary>
    /// Write a Info Log Message
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    void Info(string msg, object[] args);

    /// <summary>
    /// Write a Warn Log Message
    /// </summary>
    /// <param name="msg"></param>
    void Warn(string msg);

    /// <summary>
    /// Write a Warn Log Message
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    void Warn(string msg, object[] args);

    /// <summary>
    /// Write a Error Log Message
    /// </summary>
    /// <param name="msg"></param>
    void Error(string msg);

    /// <summary>
    /// Write a Error Log Message
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    void Error(string msg, object[] args);
    void Error(string v, string message);
    void Error(string v, string folderName, string message);
  }
}
