using MPTagThat.Core.Common.Song;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace MPTagThat.SongGrid.ViewModels
{
  public class RowChangedColorConverter : IValueConverter
  {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if ((bool)value)
        return new SolidColorBrush(Colors.Yellow);

      return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class AlternateRowChangedColorConverter : IValueConverter
  {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if ((bool)value)
        return new SolidColorBrush(Colors.Yellow);

      return new SolidColorBrush(Colors.LightBlue);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
