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
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;

#endregion

namespace MPTagThat.Treeview.Model
{
  // DriveItem has Folders and Files as children
  public class DriveItem : NavTreeItem
  {
    public override ObservableCollection<INavTreeItem> GetMyChildren()
    {
      ObservableCollection<INavTreeItem> childrenList = new ObservableCollection<INavTreeItem>() { };
      INavTreeItem item1;

      DriveInfo drive = new DriveInfo(this.FullPathName);
      if (!drive.IsReady) return childrenList;

      DirectoryInfo di = new DirectoryInfo(((DriveInfo)drive).RootDirectory.Name);
      if (!di.Exists) return childrenList;

      try
      {
        var folders = di.GetDirectories().OrderBy(x => x.Name);
        foreach (DirectoryInfo folder in folders)
        {
          // ignore Recycle Bin and other special folders
          if (folder.Name.ToLower().Equals("$recycle.bin") || folder.Name.Equals("Config.Msi") || folder.Name.Equals("System Volume Information"))
          {
            continue;
          }
          item1 = new FolderItem();
          item1.FullPathName = FullPathName + "\\" + folder.Name;
          item1.Name = folder.Name;
          item1.Info = folder;
          childrenList.Add(item1);
        }
      }
      catch (UnauthorizedAccessException)
      {
        // On Purpose empty: catches all access errors
      }

      return childrenList;
    }
  }
}
