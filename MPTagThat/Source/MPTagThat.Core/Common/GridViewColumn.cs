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
  /// <summary>
  /// This class represents the layout of a GridView Column 
  /// </summary>
  public class GridViewColumn
  {
    #region Variables
    
    private string _type = "text";
    
    #endregion

    #region ctor

    public GridViewColumn(string name, string type, int width, bool display, bool readOnly, bool allowFilter, int displayIndex)
    {
      Name = name;
      _type = type;
      Width = width;
      Display = display;
      Readonly = readOnly;
      AllowFilter = allowFilter;
      DisplayIndex = displayIndex;
    }

    public GridViewColumn() {}

    #endregion

    #region Properties

    /// <summary>
    /// The Name of the column used in Bindings
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Should it be displayed?
    /// </summary>
    public bool Display { get; set; } = true;

    /// <summary>
    /// Is it Readonly
    /// </summary>
    public bool Readonly { get; set; }

    /// <summary>
    /// Is filtering allowed
    /// </summary>
    public bool AllowFilter { get; set; }

    /// <summary>
    /// The width of the column
    /// </summary>
    public int Width { get; set; } = 100;

    /// <summary>
    /// The Type of the column
    /// </summary>
    public string Type
    {
      get => _type.ToLower();
      set => _type = value;
    }

    /// <summary>
    /// Where in the grid do we want the column be displayed
    /// </summary>
    public int DisplayIndex { get; set; }

    #endregion
  }
}
