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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Utils;
using Prism.Events;
using Prism.Mvvm;
using System.Windows.Media;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using Prism.Ioc;

#endregion

namespace MPTagThat.MiscFiles.ViewModels
{
  public class MiscFilesViewModel : BindableBase
  {
    #region Variables
    
    private object _lock = new object();
    private MiscFile _currentItem;
    private Options _options = ContainerLocator.Current.Resolve<ISettingsManager>()?.GetOptions;

    #endregion

    #region Properties

    /// <summary>
    /// Binding to hold the NBon-Music File
    /// </summary>
    private ObservableCollection<MiscFile> _miscFiles;
    public ObservableCollection<MiscFile> MiscFiles
    {
      get => _miscFiles;
      set => SetProperty(ref _miscFiles, value);
    }

    /// <summary>
    /// Binding to enable/disable the Context Menu
    /// </summary>
    private bool _isContextMenuRenameEnabled;
    public bool ContextMenuRenameEnabled
    {
      get => _isContextMenuRenameEnabled;
      set => SetProperty(ref _isContextMenuRenameEnabled, value);
    }

    /// <summary>
    /// Sets the Backcolor for the objects
    /// </summary>
    private Brush _backColor;
    public Brush BackColor
    {
      get => _backColor;
      set => SetProperty(ref _backColor, value);
    }

    #endregion

    #region ctor

    public MiscFilesViewModel()
    {
      _miscFiles = new ObservableCollection<MiscFile>();
      RenameFileCommand = new BaseCommand(RenameFile);
      DeleteFileCommand = new BaseCommand(DeleteFile);
      SelectionChangedCommand = new BaseCommand(SelectionChanged);
      EnterKeyPressedCommand = new BaseCommand(EnterKeypressed);
      EventSystem.Subscribe<GenericEvent>(OnMessageReceived,ThreadOption.PublisherThread);
      BindingOperations.EnableCollectionSynchronization(MiscFiles, _lock);
    }

    #endregion

    #region Commands

    public ICommand SelectionChangedCommand { get; }

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
        if (items.Count == 0)
        {
          return;
        }
        if (items.Count > 1)
        {
          // More than one file selected.
          ContextMenuRenameEnabled = false;
          _currentItem = null;
          return;
        }

        // Check, if there is already a file named folder.*
        if (_miscFiles.Any(item => item.FileName.StartsWith("folder.")))
        {
          ContextMenuRenameEnabled = false;
        }

        items[0].IsTextBoxEnabled = true;
        _currentItem = items[0];
      }
    }

    public ICommand EnterKeyPressedCommand { get; }

    /// <summary>
    /// Some manual change has been done to rename a file
    /// </summary>
    /// <param name="param"></param>
    private void EnterKeypressed(object param)
    {
      if (_currentItem == null || param == null)
      {
        return;
      }

      var item = _currentItem;
      var newFile = $"{Path.GetDirectoryName(_currentItem.FullFileName)}\\{(string)param}";
      if (!File.Exists(newFile))
      {
        try
        {
          File.Move(_currentItem.FullFileName, newFile);
          item.FullFileName = newFile;
          item.FileName = Path.GetFileName(newFile);
          MiscFiles.Add(item);
        }
        catch (Exception)
        {
          // ignored
        }
      }
    }


    public ICommand RenameFileCommand { get; }

    /// <summary>
    /// Rename File to Folder.jpg
    /// </summary>
    /// <param name="param"></param>
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
            item.FullFileName = newFile;
            item.FileName = Path.GetFileName(newFile);
          }
          catch (Exception)
          {
            // ignored
          }
        }
      }
    }

    public ICommand DeleteFileCommand { get; }

    /// <summary>
    /// Delete the selected File
    /// </summary>
    /// <param name="param"></param>
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

    #region Private Methods

    /// <summary>
    ///  Get the non Music files from Current folder
    /// </summary>
    private List<string> GetNonMusicFiles()
    {
      var files = new List<string>();
      foreach (var file in Directory.GetFiles(_options.MainSettings.LastFolderUsed))
      {
        if (!Util.IsAudio(file))
        {
          files.Add(file);
        }
      }

      return files;
    }

    private void FillFilesCollection(ref List<string> files)
    {
      MiscFiles.Clear();
      foreach (var file in files)
      {
        if (file == null)
        {
          return;
        }

        var imgFailure = false;
        var nonPicFile = true;
        if (Util.IsPicture(Path.GetFileName(file)))
        {
          nonPicFile = false;
          var f = new MiscFile
          {
            FileName = Path.GetFileName(file),
            FullFileName = file
          };

          try
          {
            var bmi = GetImageFromFile(file, out var size);
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
        if (nonPicFile || imgFailure)
        {
          // For a non Picture file or if we had troubles creating the thumb, see if we have a file specific icon
          var defaultName = "";
          var extension = Path.GetExtension(file).Trim('.');
          var f = new MiscFile
          {
            FileName = Path.GetFileName(file),
            FullFileName = file
          };
          if (extension.Length > 0)
          {
            try
            {
              defaultName = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\MPTagThat2\Fileicons\{extension}.png";
            }
            catch (Exception) {}
          }
          if (File.Exists(defaultName))
          {
            var bmi = GetImageFromFile(defaultName, out var size);
            if (bmi != null)
            {
              f.ImageData = bmi;
              f.Size = size;
              MiscFiles.Add(f);
            }
          }
          else
          {
            var bmi = GetImageFromFile( $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\MPTagThat2\Fileicons\unknown.png", out var size);
            if (bmi != null)
            {
              f.ImageData = bmi;
              f.Size = size;
              MiscFiles.Add(f);
            }
          }
        }
      }
    }

    #endregion

    #region Events

    /// <summary>
    /// Handle messages
    /// </summary>
    /// <param name="msg"></param>
    private void OnMessageReceived(GenericEvent msg)
    {
      switch (msg.Action.ToLower())
      {
        case "miscfileschanged":
          List<string> files = new List<string>();
          if (msg.MessageData.ContainsKey("files"))
          {
            // Called from Folder Scan
            files = (List<string>)msg.MessageData["files"];
          }
          else
          {
            // Called outside Folder Scan, e.g. Cover Search placed Folder.jpg
            files = GetNonMusicFiles();
          }
          FillFilesCollection(ref files);
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
        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
          img = new FreeImageBitmap(fs);
        }
      }
      catch (Exception ex)
      {
        ContainerLocator.Current.Resolve<ILogger>().GetLogger.Error("File has invalid Picture: {0} {1}", fileName, ex.Message);
      }

      if (img != null)
      {
        size = $"{img.Width} x {img.Height}";

        // convert Image Size to 64 x 64 for display in the Imagelist
        img.Rescale(64, 64, FREE_IMAGE_FILTER.FILTER_BOX);

        var bitmapImage = new BitmapImage();
        using (var memory = new MemoryStream())
        {
          img.Save(memory, FREE_IMAGE_FORMAT.FIF_PNG);
          memory.Position = 0;

          
          bitmapImage.BeginInit();
          bitmapImage.StreamSource = memory;
          bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
          bitmapImage.EndInit();
          bitmapImage.Freeze();
          img.Dispose();
        }
        return bitmapImage;
      }
      return null;
    }


    #endregion
  }
}
