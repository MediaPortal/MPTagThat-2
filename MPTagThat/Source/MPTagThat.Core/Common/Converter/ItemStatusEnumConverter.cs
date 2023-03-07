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
using System.Windows.Data;
using System.Windows;
using MPTagThat.Core.Common.Song;
using WPFLocalizeExtension.Engine;

#endregion

namespace MPTagThat.Core.Common.Converter
{
  [ValueConversion(typeof(ItemStatus?), typeof(String))]
  public class ItemStatusEnumConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      switch (value as ItemStatus?)
      {
        case ItemStatus.Ignored:
          return LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "tagChecker_Status_Ignored",
                      LocalizeDictionary.Instance.Culture).ToString();
        case ItemStatus.NoMatch:
          return LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "tagChecker_Status_NoMatch",
                      LocalizeDictionary.Instance.Culture).ToString();
        case ItemStatus.FullMatch:
          return LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "tagChecker_Status_FullMatch",
                      LocalizeDictionary.Instance.Culture).ToString();
        case ItemStatus.FullMatchChanged:
          return LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "tagChecker_Status_FullMatchChanged",
                      LocalizeDictionary.Instance.Culture).ToString();
        case ItemStatus.PartialMatch:
          return LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "tagChecker_Status_PartialMatch",
                      LocalizeDictionary.Instance.Culture).ToString();
        case ItemStatus.Applied:
          return LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "tagChecker_Status_Applied",
                      LocalizeDictionary.Instance.Culture).ToString();
      }
      return null;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return DependencyProperty.UnsetValue;
    }
  }
}
