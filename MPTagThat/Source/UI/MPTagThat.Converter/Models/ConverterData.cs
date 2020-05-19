#region Copyright (C) 2020 Team MediaPortal
// Copyright (C) 2020 Team MediaPortal
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

using System;
using MPTagThat.Core.Common.Song;
using Prism.Mvvm;

#endregion

namespace MPTagThat.Converter.Models
{
  public class ConverterData : BindableBase
  {
    private double _percentComplete = 0;

    public double PercentComplete
    {
      get => _percentComplete;
      set => SetProperty(ref _percentComplete, value);
    }

    public string FileName => Song.FullFileName;

    private string _newFileName = "";

    public string NewFileName
    {
      get => _newFileName; 
      set => SetProperty(ref _newFileName, value);
    }

    public SongData Song { get; set; }

    public string Status { get; set; } = "";
  }
}
