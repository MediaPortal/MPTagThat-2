#region Copyright (C) 2022 Team MediaPortal
// Copyright (C) 2022 Team MediaPortal
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

using MPTagThat.Core;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.MusicDatabase;
using MPTagThat.Core.Utils;
using Prism.Ioc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TagLib;
using File = System.IO.File;

#endregion

namespace MPTagThat.SongGrid.Commands
{
  /// <summary>
  /// This command implements the Saving of changes to a song 
  /// </summary>
  [SupportedCommandType("Save")]
  [SupportedCommandType("SaveAll")]
  public class CmdSave : Command
  {
    #region Variables

    public object[] Parameters { get; private set; }
    private bool bErrors = false;

    #endregion

    #region ctor
    public CmdSave(object[] parameters)
    {
      Parameters = parameters;
      NeedsPostprocessing = true;

      if ((string)parameters[0] == "SaveAll")
      {
        NeedsCallback = true;
      }
    }

    #endregion

    #region Command Implementation

    public override Task<(bool Changed, SongData song)> Execute(SongData song)
    {
      if (!SaveTrack(ref song))
      {
        bErrors = true;
      }

      // returning false here, since we are setting the Track Status in Save Track
      // and don't want to have it changed again in calling routine
      return Task.FromResult((false, song));
    }

    public override bool PostProcess(SongData song)
    {
      options.ReadOnlyFileHandling = 2; //No
      return bErrors;
    }

    #endregion

    #region Private Methods

    /// <summary>
    ///   Does the actual save of the Song
    /// </summary>
    /// <param name = "song"></param>
    /// <returns></returns>
    private bool SaveTrack(ref SongData song)
    {
      log.Trace(">>>");
      try
      {
        if (song.Changed)
        {
          Util.StatusCurrentFile($"Saving file {song.FullFileName}");
          log.Debug($"Save: Saving song: {song.FullFileName}");

          // The track to be saved, may be currently playing. If this is the case stop playback to free the file
          //if (track.FullFileName == TracksGrid.MainForm.Player.CurrentSongPlaying)
          //{
          //  Log.Debug("Save: Song is played in Player. Stop playback to free the file");
          //  TracksGrid.MainForm.Player.Stop();
          //}

          if (options.MainSettings.CopyArtist && song.AlbumArtist == "")
          {
            song.AlbumArtist = song.Artist;
          }

          if (options.MainSettings.UseCaseConversion)
          {
            CaseConversion.CaseConvert(ref song);
          }

          var originalFileName = song.FullFileName; // Need the original filename for database update, in case a rename happens

          // Save the file 
          var errorMessage = "";
          if (Song.SaveFile(song, ref errorMessage))
          {
            if (RenameFile(song, out var newFileName))
            {
              // Read the Song with the new Information
              song = Song.Create(newFileName);
            }

            // Check, if we need to create a folder.jpg
            var fileName = Path.Combine(Path.GetDirectoryName(song.FullFileName), "folder.jpg");
            if (!File.Exists(fileName) &&
                options.MainSettings.CreateFolderThumb)
            {
              if (song.Pictures.Count > 0)
              {
                var indexFrontCover = -1;
                try
                {
                  indexFrontCover = song.Pictures
                    .Select((pic, i) => new { Pic = pic, Position = i }).First(m => m.Pic.Type == PictureType.FrontCover)
                    .Position;
                }
                catch (Exception)
                {
                  // ignored
                }

                if (indexFrontCover < 0)
                {
                  indexFrontCover = 0;
                }

                Util.SavePicture(song.Pictures[indexFrontCover], fileName);
                var miscfileevt = new GenericEvent
                {
                  Action = "miscfileschanged"
                };
                EventSystem.Publish(miscfileevt);
              }
            }

            ContainerLocator.Current.Resolve<IMusicDatabase>().UpdateSong(song, originalFileName);

            song.Status = 0;
            song.Changed = false;
          }
          else
          {
            song.Status = 2;
            song.StatusMsg = errorMessage;
          }
        }
      }
      catch (Exception ex)
      {
        song.Status = 2;
        song.StatusMsg = ex.Message;
        log.Error($"Save: Error Saving data for song {song.FileName}: {ex.Message} {ex.StackTrace}");
        return false;
      }
      log.Trace("<<<");
      return true;
    }

    /// <summary>
    ///   Rename the file if necessary
    ///   Called by Save and SaveAll
    /// </summary>
    /// <param name = "song"></param>
    /// <param name="newFileName"></param>
    private bool RenameFile(SongData song, out string newFileName)
    {
      newFileName = "";
      var originalFileName = Path.GetFileName(song.FullFileName);
      if (originalFileName != song.FileName)
      {
        var ext = Path.GetExtension(song.FileName);
        var filename = Path.GetFileNameWithoutExtension(song.FileName);
        var path = Path.GetDirectoryName(song.FullFileName);
        newFileName = Path.Combine(path, $"{Util.MakeValidFileName(filename)}{ext}");

        // Check, if the New file name already exists
        // Don't change the newfilename, when only the Case change happened in filename
        int i = 1;
        if (File.Exists(newFileName) && originalFileName.ToLowerInvariant() != song.FileName.ToLowerInvariant())
        {
          newFileName = Path.Combine(path, $"{filename} ({i}){ext}");
          while (File.Exists(newFileName))
          {
            i++;
            newFileName = Path.Combine(path, $"{filename} ({i}){ext}");
          }
        }

        File.Move(song.FullFileName, newFileName);
        log.Debug($"Save: Renaming Song: {song.FullFileName} Newname: {newFileName}");
        return true;
      }
      return false;
    }

    #endregion
  }
}
