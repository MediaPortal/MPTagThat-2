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

using Prism.Mvvm;
using System.Windows.Media.Imaging;

#endregion

namespace MPTagThat.MiscFiles.ViewModels
{
  public class MiscFile : BindableBase
  {
    private string _filename;
    private bool _isTextBoxEnabled;

    public BitmapImage ImageData { get; set; }
    public string FullFileName { get; set; }
    public string Size { get; set; }

    public string FileName
    {
      get => _filename;
      set { SetProperty(ref _filename, value); }
    }

    public bool IsTextBoxEnabled
    {
      get => _isTextBoxEnabled;
      set { SetProperty(ref _isTextBoxEnabled, value); }
    }
  }
}
