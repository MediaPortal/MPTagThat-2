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
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Settings;
using Prism.Events;
using Prism.Ioc;

#endregion

namespace MPTagThat.Core.Common.Converter
{
  /// <summary>
  /// Converter to set the color for alternate rows
  /// </summary>
  public class AlternateRowChangedColorConverter : IValueConverter
  {
    private Color _changedRowColor = (Color) ColorConverter.ConvertFromString(ContainerLocator.Current.Resolve<ISettingsManager>()?.GetOptions.MainSettings.ChangedRowColor);
    private Color _alternateRowColor = (Color) ColorConverter.ConvertFromString(ContainerLocator.Current.Resolve<ISettingsManager>()?.GetOptions.MainSettings.AlternateRowColor);

    public AlternateRowChangedColorConverter()
    {
      EventSystem.Subscribe<GenericEvent>(OnMessageReceived, ThreadOption.UIThread);
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if ((bool)value)
        return new SolidColorBrush(_changedRowColor);

      return new SolidColorBrush(_alternateRowColor);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return DependencyProperty.UnsetValue;
    }

    #region Event Handling

    private void OnMessageReceived(GenericEvent msg)
    {
      switch (msg.Action.ToLower())
      {
        case "themecolorchanged":
          _alternateRowColor = (Color) ColorConverter.ConvertFromString(ContainerLocator.Current.Resolve<ISettingsManager>()?.GetOptions.MainSettings.AlternateRowColor);
          _changedRowColor = (Color) ColorConverter.ConvertFromString(ContainerLocator.Current.Resolve<ISettingsManager>()?.GetOptions.MainSettings.ChangedRowColor);
          break;
      }
    }

    #endregion
  }
}
