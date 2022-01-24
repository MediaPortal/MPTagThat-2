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

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

#endregion

namespace MPTagThat.Core.Common.Converter
{
  /// <summary>
  /// This Converter is used to adjust the value of the Foreground in the Song grid to be adapted to the background
  /// I.e. for darker Backgrounds it will use White and for Lighter Backgrounds it will use Black
  /// 
  /// The color value consists of three channels representing the red, green, and blue components.
  /// Each of the three channels can contain a numeric value from 0 to 255, so the color’s total value can range from 0 to 3 * 255 = 765.
  /// The threshold between defining a color as “light” or “dark” is between these values, at 765 / 2 = 382.5.
  /// If the actual color value is below this threshold, the background color is “dark” and we return white as foreground color,
  /// since this is the lightest possible color. Otherwise the background color is “light” and therefore we use black as foreground color.
  /// </summary>
  public class ColorContrastConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is SolidColorBrush)
      {
        var color = (value as SolidColorBrush).Color;
        const int threshold = 3*255/2;
        var sum = color.R + color.G + color.B;
        return new SolidColorBrush(sum > threshold ? Colors.Black : Colors.White);
      }
      return DependencyProperty.UnsetValue;
    }
 
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return DependencyProperty.UnsetValue;
    }
  }
}
