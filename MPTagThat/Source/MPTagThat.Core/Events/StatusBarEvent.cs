#region Copyright (C) 2018 Team MediaPortal
// Copyright (C) 2018 Team MediaPortal
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

namespace MPTagThat.Core.Events
{
  /// <summary>
  /// This is the Event object, which is used to update the Statusbar in the Main shell
  /// It is being sent via EventAggregator
  /// </summary>
  public class StatusBarEvent
  {
    public int NumberOfFiles { get; set; } = 0;
    public int NumberOfSelectedFiles { get; set; } = 0;
    public string CurrentFile { get; set; } = "";
    public string CurrentFolder { get; set; } = "";
    public int CurrentProgress { get; set; } = 0;
  }
}
