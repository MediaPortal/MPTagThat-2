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

using System.Collections.ObjectModel;
using System.IO;

#endregion

namespace MPTagThat.Treeview.Model
{
  // RootItems
  // - Special items are "RootItems" such as DriveRootItem with as children DriveItems
  //   other RootItems might be DriveNoChildRootItem, FavoritesRootItem, SpecialFolderRootItem, 
  //   (to do) LibraryRootItem, NetworkRootItem, HistoryRootItem.
  // - We use RootItem(s) as a RootNode for trees, their Children (for example DriveItems) are copied to RootChildren VM
  // - Binding in View: TreeView.ItemsSource="{Binding Path=NavTreeVm.RootChildren}"

  // DriveRootItem has DriveItems as children 
  public class DriveRootItem : NavTreeItem
  {
    public DriveRootItem()
    {
      //Constructor sets some properties
      Name = "DriveRoot";
      IsExpanded = true;
      IsSelected = false;
      Info = null;
      FullPathName = "$xxDriveRoot$";
    }

    public override ObservableCollection<INavTreeItem> GetMyChildren()
    {
      ObservableCollection<INavTreeItem> childrenList = new ObservableCollection<INavTreeItem>() { };
      INavTreeItem item1;
      string fn = "";

      DriveInfo[] allDrives = DriveInfo.GetDrives();
      foreach (DriveInfo drive in allDrives)
        if (drive.IsReady)
        {
          item1 = new DriveItem();

          // Some processing for the FriendlyName
          fn = drive.Name.Replace(@"\", "");
          item1.FullPathName = fn;
          if (drive.VolumeLabel == string.Empty)
          {
            fn = drive.DriveType.ToString() + " (" + fn + ")";
          }
          else if (drive.DriveType == DriveType.CDRom)
          {
            fn = drive.DriveType.ToString() + " " + drive.VolumeLabel + " (" + fn + ")";
          }
          else
          {
            fn = drive.VolumeLabel + " (" + fn + ")";
          }

          item1.Name = fn;
          item1.Info = drive;
          childrenList.Add(item1);
        }

      return childrenList;
    }
  }
}
