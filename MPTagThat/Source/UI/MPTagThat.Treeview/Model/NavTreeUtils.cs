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
using System.Collections.Generic;
using System.Linq;

#endregion

namespace MPTagThat.Treeview.Model
{
  public static class NavTreeUtils
  {
    #region Variables

    private const string LastPartRootItemName = "RootItem";    // Convention: Root items end with:
    private static string strSeparator = "[+]";

    #endregion


    // Using convention and reflection to get RootItem iRootNr
    //If iRootNr>= ListNavTreeRootItemsByConvention().Count we use driveItem by default
    public static NavTreeItem ReturnRootItem(int iRootNr)
    {
      // Set default System.Type
      Type selectedType = typeof(DriveRootItem);
      string selectedName = "Drive";

      // Can you find other type given the conventions ..RootItem name and iRootNr
      var entityTypes =
        from t in System.Reflection.Assembly.GetAssembly(typeof(NavTreeItem)).GetTypes() where t.IsSubclassOf(typeof(NavTreeItem)) select t;

      int i = 0;
      foreach (var tt in entityTypes)
      {
        if (tt.Name.EndsWith(LastPartRootItemName))
        {
          if (i == iRootNr)
          {
            selectedType = Type.GetType(tt.FullName);
            selectedName = tt.Name.Replace(LastPartRootItemName, "");
            break;
          }
          i++;
        }
      }

      // Use selectedType to create root ..         
      NavTreeItem rootItem = (NavTreeItem)Activator.CreateInstance(selectedType);
      rootItem.Name = selectedName;
      return rootItem;
    }

    // Supporting procedure
    private static void GetNodeFromNameLocal(ref INavTreeItem item, string[] pathArray, int iLevel)
    {
      string name;

      // Check children
      name = pathArray[iLevel];
      INavTreeItem child;
      INavTreeItem selected = null;

      for (int i = 0; (i <= item.Children.Count() - 1) && (selected == null); i++)
      {
        child = item.Children[i];
        if (name == child.FullPathName) selected = child;
      }

      item = selected;

      // If we have a hit, step deeper
      iLevel++;
      if ((iLevel <= pathArray.Length - 1) && (item != null)) GetNodeFromNameLocal(ref item, pathArray, iLevel);
    }

    private static void GetNodeFromName(INavTreeItem rootNode, string fullPathNames, ref INavTreeItem selectedNode)
    {
      // Just setup a call to GetNodeFromNameLocal to do the work

      // note: to copy or not to copy (pointer, content), all seems ok
      selectedNode = null;

      if ((fullPathNames == null) || (fullPathNames == ""))
      {
        return;
      }

      // make a pathArray. 
      // Note now it is not anymore [(drive) (folder)] but [(drive) [(drive) (folder)]]   
      string[] separator = new string[] { strSeparator };
      string[] pathArray = fullPathNames.Split(separator, StringSplitOptions.RemoveEmptyEntries);
      if (pathArray.Length == 0) { return; };

      // Get the node holding the Items
      selectedNode = rootNode;

      int iLevel = 0;
      GetNodeFromNameLocal(ref selectedNode, pathArray, iLevel);
    }

    // Procedure used in Treeviewmodel to expand to current folder
    public static void ExpandCurrentFolder(List<string> CurrentFolder, INavTreeItem treeRootItem)
    {
      // try to open all old snapshot nodes
      INavTreeItem Selected = null;
      for (int i = 0; i < CurrentFolder.Count; i++)
      {
        GetNodeFromName(treeRootItem, CurrentFolder[i], ref Selected);
        if (Selected != null)
        {
          Selected.IsExpanded = true;
          if (i == CurrentFolder.Count - 1)
          {
            Selected.IsSelected = true;
          }
        }
      }
    }
  }
}
