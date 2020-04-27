#region Copyright (C) 2005-2020 Team MediaPortal

// Copyright (C) 2005-2020 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace DeployTool
{
  /// <summary>
  /// Sets an option, with arguments.
  /// </summary>
  public interface ICommandLineOptions
  {
    /// <summary>
    /// Sets an option, with arguments.
    /// </summary>
    /// <param name="option">The option.</param>
    /// <param name="argument">The argument (can be null).</param>
    void SetOption(string option, string argument);

    /// <summary>
    /// Displays the options to console
    /// </summary>
    void DisplayOptions();
  }
}
