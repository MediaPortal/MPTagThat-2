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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MPTagThat.Core;
using MPTagThat.Core.Annotations;
using MPTagThat.Core.Common;
using MPTagThat.Core.Events;
using Prism.Events;
using Prism.Mvvm;
using Syncfusion.Windows.Shared;

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
        var newFile = $"{Path.GetDirectoryName(item.FullFileName)}\\folder.{Path.GetExtension(item.FullFileName)}";
        if (!File.Exists(newFile))
        {
          try
          {
            File.Move(item.FullFileName, newFile);
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
          catch (Exception e)
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
              var f = new MiscFile
              {
                FileName = file.Name,
                FullFileName = file.FullName
              };

              try
              {
                // avoid locking of the file
                var bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.UriSource = new Uri(file.FullName, UriKind.Absolute);
                bmi.EndInit();
                f.ImageData = bmi;
                f.Size = $"({Math.Round(f.ImageData.Width)} x {Math.Round(f.ImageData.Height)})";
              }
              catch (Exception)
              {
                // We might get an Exception on specific files
              }
              MiscFiles.Add(f);
            }
          }
          break;
      }
    }

    #endregion

  }
}
