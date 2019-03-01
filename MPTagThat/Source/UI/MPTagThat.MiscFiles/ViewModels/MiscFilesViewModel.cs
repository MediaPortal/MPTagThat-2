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
using System.IO;
using System.Windows.Media.Imaging;
using MPTagThat.Core;
using MPTagThat.Core.Events;
using Prism.Events;
using Syncfusion.Windows.Shared;

#endregion

namespace MPTagThat.MiscFiles.ViewModels
{
  public class MiscFilesViewModel : NotificationObject
  {
    private ObservableCollection<MiscFile> _miscFiles;

    public ObservableCollection<MiscFile> MiscFiles
    {
      get => _miscFiles;
      set
      {
        _miscFiles = value;
        this.RaisePropertyChanged(() => this.MiscFiles);
      }
    }

    public MiscFilesViewModel()
    {
      _miscFiles = new ObservableCollection<MiscFile>();
      EventSystem.Subscribe<GenericEvent>(OnMessageReceived,ThreadOption.UIThread);
    }

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
              };

              try
              {
                f.ImageData = new BitmapImage(new Uri(file.FullName));
                f.Size = $"({f.ImageData.Width} x {f.ImageData.Height})";
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
  }
}
