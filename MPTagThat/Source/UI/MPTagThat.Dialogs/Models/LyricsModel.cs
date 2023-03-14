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

using Prism.Mvvm;

namespace MPTagThat.Dialogs.Models
{
  public class LyricsModel : BindableBase
  {
    #region Properties

    public int Row { get; set; }

    private bool _isSelected = false;

    public bool IsSelected
    {
      get => _isSelected;
      set => SetProperty(ref _isSelected, value);
    }

    private string _artistAndTitle;

    public string ArtistAndTitle
    {
      get => _artistAndTitle;
      set => SetProperty(ref _artistAndTitle, value);
    }

    private string _site;
    public string Site
    {
      get => _site;
      set => SetProperty(ref _site, value);
    }

    private string _lyric;

    public string Lyric
    {
      get => _lyric;
      set => SetProperty(ref _lyric, value);
    }
    #endregion
  }
}
