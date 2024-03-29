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

using Prism.Mvvm;
using System.Collections.Generic;

#endregion

namespace MPTagThat.Core.Common
{
  public class KeyMaps
  {
    public List<KeyDef> KeyMap { get; set; }
  }

  public class KeyDef : BindableBase
  {
    public int Id { get; set; }

    private string _key;
    public string Key { get => _key; set => SetProperty(ref _key, value); }

    private string _description;
    public string Description { get => _description; set => SetProperty(ref _description, value); }
  }
}
