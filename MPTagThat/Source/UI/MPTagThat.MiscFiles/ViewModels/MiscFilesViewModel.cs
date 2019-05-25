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
using System.Collections.ObjectModel;
using FreeImageAPI;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommonServiceLocator;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Utils;
using Prism.Events;
using Prism.Mvvm;

#endregion

namespace MPTagThat.MiscFiles.ViewModels
{
  public class MiscFilesViewModel : BindableBase
  {
    #region Variables

    private ObservableCollection<MiscFile> _miscFiles;
    private bool _isContextMenuRenameEnabled;

    #endregion

    #region Properties

    public ObservableCollection<MiscFile> MiscFiles
    {
      get => _miscFiles;
      set => SetProperty(ref _miscFiles, value);
    }

    public bool ContextMenuRenameEnabled
    {
      get => _isContextMenuRenameEnabled;
      set => SetProperty(ref _isContextMenuRenameEnabled, value);
    }

    #endregion

    #region ctor

    public MiscFilesViewModel()
    {
      _miscFiles = new ObservableCollection<MiscFile>();
      _renameFileCommand = new BaseCommand(RenameFile);
      _deleteFileCommand = new BaseCommand(DeleteFile);
      _selectionChangedCommand = new BaseCommand(SelectionChanged);
      EventSystem.Subscribe<GenericEvent>(OnMessageReceived,ThreadOption.UIThread);
    }

    #endregion

    #region Commands

    private ICommand _selectionChangedCommand;
    public ICommand SelectionChangedCommand => _selectionChangedCommand;

    /// <summary>
    /// Check if the Rename to Folder Menu should be enabled.
    /// </summary>
    /// <param name="param"></param>
    private void SelectionChanged(object param)
    {
      ContextMenuRenameEnabled = true;
      if (param != null)
      {
        var items = (param as ObservableCollection<object>).Cast<MiscFile>().ToList();
        if (items.Count > 1)
        {
          // More than one file selected.
          ContextMenuRenameEnabled = false;
          return;
        }

        // Check, if there is already a file named folder.*
        if (_miscFiles.Any(item => item.FileName.StartsWith("folder.")))
        {
          ContextMenuRenameEnabled = false;
        }
      }
    }


    private ICommand _renameFileCommand;
    public ICommand RenameFileCommand => _renameFileCommand;

    private void RenameFile(object param)
    {
      if (param != null)
      {
        var item = (MiscFile) param;
        var newFile = $"{Path.GetDirectoryName(item.FullFileName)}\\folder{Path.GetExtension(item.FullFileName)}";
        if (!File.Exists(newFile))
        {
          try
          {
            File.Move(item.FullFileName, newFile);
            MiscFiles.Remove(item);
            item.FullFileName = newFile;
            item.FileName = Path.GetFileName(newFile);
            MiscFiles.Add(item);
          }
          catch (Exception e)
          {
            // ignored
          }
        }
      }
    }

    private ICommand _deleteFileCommand;
    public ICommand DeleteFileCommand => _deleteFileCommand;

    private void DeleteFile(object param)
    {
      if (param != null)
      {
        var items = (param as ObservableCollection<object>).Cast<MiscFile>().ToList();
        foreach (var item in items)
        {
          try
          {
            File.Delete(item.FullFileName);
            MiscFiles.Remove(item);
          }
          catch (Exception)
          {
            // ignored
          }
        }
      }
    }

    #endregion

    #region Events

    private void OnMessageReceived(GenericEvent msg)
    {
      switch (msg.Action.ToLower())
      {
        case "miscfileschanged":
          if (msg.MessageData.ContainsKey("files"))
          {
            var files = (List<FileInfo>)msg.MessageData["files"];
            MiscFiles.Clear();
            foreach (var file in files)
            {
              if (file == null)
              {
                return;
              }

              var imgFailure = false;
              var nonPicFile = true;
              if (Util.IsPicture(file.Name))
              {
                nonPicFile = false;
                var f = new MiscFile
                {
                  FileName = file.Name,
                  FullFileName = file.FullName
                };

                try
                {
                  var bmi = GetImageFromFile(file.FullName, out var size);
                  if (bmi != null)
                  {
                    f.ImageData = bmi;
                    f.Size = size;
                    MiscFiles.Add(f);
                  }
                  else
                  {
                    imgFailure = true;
                  }
                }
                catch (Exception)
                {
                  imgFailure = true;
                }

                
              }
            }
          }
          break;
      }
    }

    #endregion

    #region Private Methods

    /// <summary>
    ///   Return an image from a given filename
    /// </summary>
    /// <param name = "fileName"></param>
    /// <param name = "size"></param>
    /// <returns></returns>
    private BitmapImage GetImageFromFile(string fileName, out string size)
    {
      FreeImageBitmap img = null;
      size = "";
      try
      {
        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
          img = new FreeImageBitmap(fs);
          fs.Close();
        }

        size = $"{img.Width} x {img.Height}";

        // convert Image Size to 64 x 64 for display in the Imagelist
        img.Rescale(64, 64, FREE_IMAGE_FILTER.FILTER_BOX);
      }
      catch (Exception ex)
      {
        (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger).GetLogger.Error("File has invalid Picture: {0} {1}", fileName, ex.Message);
      }

      if (img != null)
      {
        using (var memory = new MemoryStream())
        {
          img.Save(memory, FREE_IMAGE_FORMAT.FIF_PNG);
          memory.Position = 0;

          var bitmapImage = new BitmapImage();
          bitmapImage.BeginInit();
          bitmapImage.StreamSource = memory;
          bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
          //bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
          bitmapImage.EndInit();

          return bitmapImage;
        }
      }

      return null;
    }


    #endregion
  }
}
