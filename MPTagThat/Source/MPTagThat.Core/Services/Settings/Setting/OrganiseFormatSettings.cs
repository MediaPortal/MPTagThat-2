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

#region

using Prism.Ioc;
using System.Collections.Generic;

#endregion

namespace MPTagThat.Core.Services.Settings.Setting
{
  public class OrganiseFormatSettings : ParameterFormat
  {
    #region Variable

    private int _lastUsedFolderIndex = -1;
    private List<string> _lastUsedFolders = new List<string>();

    #endregion

    #region Properties

    [Setting(SettingScope.User, "")]
    public List<string> LastUsedFolders
    {
      get { return _lastUsedFolders; }
      set { _lastUsedFolders = value; }
    }

    [Setting(SettingScope.User, "-1")]
    public int LastUsedFolderIndex
    {
      get { return _lastUsedFolderIndex; }
      set { _lastUsedFolderIndex = value; }
    }

    [Setting(SettingScope.User, "false")]
    public bool CopyFiles { get; set; }

    [Setting(SettingScope.User, "true")]
    public bool OverWriteFiles { get; set; }

    [Setting(SettingScope.User, "true")]
    public bool CopyNonMusicFiles { get; set; }

    [Setting(SettingScope.User, "")]
    public string LastUsedScript { get; set; }

    #endregion

    #region Public Methods

    public void Save()
    {
      ContainerLocator.Current.Resolve<ISettingsManager>().Save(this);
    }

    #endregion
  }
}
