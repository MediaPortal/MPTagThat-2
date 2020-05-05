#region Copyright (C) 2017 Team MediaPortal
// Copyright (C) 2017 Team MediaPortal
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

using WPFLocalizeExtension.Extensions;

namespace MPTagThat.Core.Common
{
  public class GridViewColumn
  {
    private string _type = "text";
    
    public GridViewColumn(string name, string type, int width, bool display, bool readOnly, bool allowFilter)
    {
      Name = name;
      _type = type;
      Width = width;
      Display = display;
      Readonly = readOnly;
      AllowFilter = allowFilter;
    }

    public GridViewColumn() {}

    public string Name { get; set; }

    public bool Display { get; set; } = true;

    public bool Readonly { get; set; }

    public bool AllowFilter { get; set; }

    public int Width { get; set; } = 100;

    public string Type
    {
      get { return _type.ToLower(); }
      set { _type = value; }
    }
  }
}
