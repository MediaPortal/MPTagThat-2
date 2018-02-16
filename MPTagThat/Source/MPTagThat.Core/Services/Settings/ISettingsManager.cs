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

using MPTagThat.Core.Services.Settings.Setting;

namespace MPTagThat.Core.Services.Settings
{
  public interface ISettingsManager
  {
    /// <summary>
    ///   Retrieves an object's public properties from a given Xml file
    /// </summary>
    /// <param name = "settingsObject">Object's instance</param>
    void Load(object settingsObject);

    /// <summary>
    ///   Stores an object's public properties to a given Xml file
    /// </summary>
    /// <param name = "settingsObject">Object's instance</param>
    void Save(object settingsObject);

    /// <summary>
    /// Getter for Options
    /// </summary>
    Options GetOptions { get; set; }

    /// <summary>
    /// Getter / Setter for Startup Settings
    /// </summary>
    StartupSettings StartupSettings { get; set; }
  }
}
