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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTagThat.Core.Events
{
  /// <summary>
  /// This is the Event object, which is used to update the Progressbar in the Main shell
  /// It is being sent via EventAggregator
  /// </summary>
  public class ProgressBarEvent
  {
    public int MinValue { get; set; } = 0;
    public int MaxValue { get; set; } = 0;
    public int CurrentProgress { get; set; } = 0;
    public string CurrentFile { get; set; } = "";
  }
}
