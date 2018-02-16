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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Practices.ServiceLocation;
using MPTagThat.Core.Common;
using MPTagThat.Core.Services.Logging;
using Syncfusion.UI.Xaml.Grid;

#endregion

namespace MPTagThat.Core.Utils
{
  public sealed class Util
  {
    #region Enum

    public enum MP3Error : int
    {
      NoError = 0,
      Fixable = 1,
      NonFixable = 2,
      Fixed = 3
    }

    #endregion

    #region Variables

    private static Util instance = new Util();
    private static ILogger log;
    private static readonly object padlock = new object();

    #endregion

    #region ctor

    private Util()
    {
      log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger).GetLogger;
    }

    #endregion

    #region UI Related Methods

    /// <summary>
    ///   Formats a Grid Column based on the Settings
    /// </summary>
    /// <param name = "setting"></param>
    public static GridColumn FormatGridColumn(GridViewColumn setting)
    {
      GridColumn column;
      switch (setting.Type.ToLower())
      {
        case "image":
          column = new GridImageColumn();
          //((GridImageColumn)column) = new Bitmap(1, 1);   // Default empty Image
          break;

        case "process":
          column = new GridPercentColumn();
          break;

        case "check":
          column = new GridCheckBoxColumn();
          break;

        case "rating":
          //column = new DataGridViewRatingColumn();
          column = new GridTextColumn();
          break;

        default:
          column = new GridTextColumn();
          break;
      }

      column.HeaderText = setting.Title;
      column.IsReadOnly = setting.Readonly;
      column.IsHidden = !setting.Display;
      column.Width = setting.Width;
      //column.IsFrozen = setting.Frozen;
      // For columns bound to a data Source set the property
      //if (setting.Bound)
      //{
        column.MappingName = setting.Name;
      //}

      switch (setting.Type.ToLower())
      {
        case "text":
        case "process":
        //  column.ValueType = typeof(string);
          break;
        case "number":
        case "check":
        case "rating":
        //  column.ValueType = typeof(int);
          break;
      }

      return column;
    }

    #endregion

    #region Folder related methods

    /// <summary>
    /// Deletes folder, setting the attributes before to allow deletion
    /// </summary>
    /// <param name="folderName"></param>
    public static void DeleteFolder(string folderName)
    {
      try
      {
        if (Directory.Exists(folderName))
        {
          var dirInfo = new DirectoryInfo(folderName);
          dirInfo.Attributes = dirInfo.Attributes & ~FileAttributes.ReadOnly;
          Directory.Delete(folderName, true);
        }
      }
      catch (Exception ex)
      {
        log.Error("Exception deleting folder: {0} {1}", folderName, ex.Message);
      }
    }

    #endregion
  }
}
