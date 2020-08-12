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

#region 

using System;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Shell32;

#endregion

namespace MPTagThat.Treeview.Views
{
  public class ImageConverter : IValueConverter
  {
    private BitmapImage _artistIcon = new BitmapImage(new Uri("pack://application:,,,/MPTagThat.TagEdit;component/Resources/Images/Musician_Add_32x.png"));
    private BitmapImage _albumIcon = new BitmapImage(new Uri("pack://application:,,,/MPTagThat;component/Resources/Images/Album_32x.png"));
    private BitmapImage _genreIcon = new BitmapImage(new Uri("pack://application:,,,/MPTagThat;component/Resources/Images/Genre_32x.png"));

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value is FolderItem folderItem)
      {
        if (folderItem.Type.ToLower() == "file folder")
        {
          try
          {
            var folderIcons = Directory.GetFiles(folderItem.Path, "*.*", SearchOption.TopDirectoryOnly)
              .Where(s => s.ToLower().EndsWith("folder.jpg") ||  s.ToLower().EndsWith("folder.png") ||  s.ToLower().EndsWith("albumartsmall.jpg")).ToList();
            if (folderIcons.Count > 0)
            {
              var bitmap = new BitmapImage();
              bitmap.BeginInit();
              bitmap.UriSource = new Uri(folderIcons.First());
              bitmap.CacheOption = BitmapCacheOption.OnLoad;
              bitmap.EndInit();
              return bitmap;
            }
          }
          catch
          {
            // On Purpose empty
          }
        }
        return Core.Utils.ShellIcon.GetSmallIcon(folderItem.Path, true);
      }

      if (value is string type)
      {
        switch (type)
        {
          case "Artist":
          case "AlbumArtist":
            return _artistIcon;

          case "Album":
            return _albumIcon;

          case "Genre":
            return _genreIcon;
        }
      }
      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
