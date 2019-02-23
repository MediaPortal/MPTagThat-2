using System;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MPTagThat.Treeview.Views
{
  public class ImageConverter : IValueConverter
  {

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value is DriveInfo driveInfo)
      {
        return Core.Utils.ShellIcon.GetSmallIcon(driveInfo.Name, true);
      }

      if (value is DirectoryInfo dirInfo)
      {
        try
        {
          var folderIcons = Directory.GetFiles(dirInfo.FullName, "*.*", SearchOption.TopDirectoryOnly)
            .Where(s => s.ToLower().EndsWith("folder.jpg") ||  s.ToLower().EndsWith("folder.png") ||  s.ToLower().EndsWith("albumartsmall.jpg")).ToList();
          if (folderIcons.Count > 0)
          {
            return new BitmapImage(new Uri(folderIcons.First(), UriKind.Absolute));
          }
        }
        catch
        {
          // UnPurpose empty
        }
        return Core.Utils.ShellIcon.GetSmallIcon(dirInfo.FullName, true);
      }

      return new BitmapImage();
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
