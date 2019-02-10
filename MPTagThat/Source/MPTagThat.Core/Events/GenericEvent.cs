﻿#region Copyright (C) 2018 Team MediaPortal
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

using System.Collections.Generic;

namespace MPTagThat.Core.Events
{
  /// <summary>
  /// This is a Generic Event object, which is used to send generic messages
  /// It is being sent via EventAggregator
  /// </summary>
  public class GenericEvent
  {
    /// <summary>
    /// The Action of the Event
    /// </summary>
    public string Action  { get; set; } = "";

    /// <summary>
    /// Individual Parameter needed by the message
    /// </summary>
    public IDictionary<string, object> MessageData { get; set; } = new Dictionary<string, object>();
  }
}
