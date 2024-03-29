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

#region

using System.Globalization;
using System.IO;
using System.Reflection;

#endregion

namespace MPTagThat.Dialogs.ViewModels
{
  public class AboutViewModel : DialogViewModelBase
  {
    #region Propertties

    public string Version { get; set; }
    public string BuildDate { get; set; }

    #endregion

    #region ctor

    public AboutViewModel()
    {
      CultureInfo.CurrentCulture = new CultureInfo("de-AT", false);
      var assembly = Assembly.GetExecutingAssembly();
      Version = assembly.GetName().Version.ToString();
      var lastWrite = File.GetLastWriteTime(assembly.Location);
      BuildDate = $"{lastWrite.ToShortDateString()} {lastWrite.ToShortTimeString()}";
    }

    #endregion



  }
}
