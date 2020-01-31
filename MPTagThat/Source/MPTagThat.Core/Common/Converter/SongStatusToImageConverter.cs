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

#region MyRegion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Raven.Database.Server.Controllers;

#endregion

namespace MPTagThat.Core.Common.Converter
{
  /// <summary>
  /// Converter to set the Image for a given Song Status
  /// </summary>
   public class SongStatusToImageConverter : IValueConverter
  {
    public BitmapImage Ok = new BitmapImage(new Uri("pack://application:,,,/MPTagThat;component/Resources/Images/Status_Ok.png"));
    public BitmapImage Changed = new BitmapImage(new Uri("pack://application:,,,/MPTagThat;component/Resources/Images/Status_Changed.png"));
    public BitmapImage Warning = new BitmapImage(new Uri("pack://application:,,,/MPTagThat;component/Resources/Images/Status_Warning.png"));
    public BitmapImage Critical = new BitmapImage(new Uri("pack://application:,,,/MPTagThat;component/Resources/Images/Status_Critical.png"));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
      {
        return null;
      }

      var status = value;
      switch ((int)status)
      {
        case -1:
          return null;

        case 0:
          return Ok;

        case 1:
          return Changed;

        case 2:
          return Critical;
      }

      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
