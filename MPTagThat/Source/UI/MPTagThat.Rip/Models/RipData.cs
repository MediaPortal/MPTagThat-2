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

using System.Windows.Media.Animation;
using MPTagThat.Core.Common.Song;
using Prism.Mvvm;

#endregion

namespace MPTagThat.Rip.Models
{
  public class RipData : BindableBase
  {
    public bool Changed => false;

    private bool _isChecked;

    public bool IsChecked
    {
      get => _isChecked;
      set => SetProperty(ref _isChecked, value);
    }

    private double _percentComplete = 0;

    public double PercentComplete
    {
      get => _percentComplete;
      set => SetProperty(ref _percentComplete, value);
    }

    private string _track;

    public string Track
    {
      get => _track;
      set => SetProperty(ref _track, value);
    }

    private string _artist;

    public string Artist
    {
      get => _artist;
      set => SetProperty(ref _artist, value);
    }

    private string _title;

    public string Title
    {
      get => _title;
      set => SetProperty(ref _title, value);
    }

    private string _duration;

    public string Duration
    {
      get => _duration;
      set => SetProperty(ref _duration, value);
    }

  }
}
