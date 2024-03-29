﻿#region Copyright (C) 2022 Team MediaPortal
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

using MPTagThat.Core.Services.Settings.Setting;

namespace MPTagThat.Core.Services.Settings
{
  /// <summary>
  ///   Main Config Service
  /// </summary>
  public class SettingsManager : ISettingsManager
  {
    public SettingsManager()
    {
      GetOptions = new Options();
      StartupSettings = new StartupSettings();
    }

    #region ISettingsManager Members

    /// <summary>
    ///   Retrieves an object's public properties from an Xml file
    /// </summary>
    /// <param name = "settingsObject">Object's instance</param>
    public void Load(object settingsObject)
    {
      ObjectParser.Deserialize(settingsObject);
    }

    /// <summary>
    ///   Stores an object's public properties to an Xml file
    /// </summary>
    /// <param name = "settingsObject">Object's instance</param>
    public void Save(object settingsObject)
    {
      ObjectParser.Serialize(settingsObject);
    }

    /// <summary>
    /// Return the Options object, which holds global config values
    /// </summary>
    public Options GetOptions { get; set; }

    /// <summary>
    /// Getter for Startup Settings
    /// </summary>
    public StartupSettings StartupSettings { get; set; }

    #endregion
  }
}
