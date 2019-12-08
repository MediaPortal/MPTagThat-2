#region Copyright (C) 2020 Team MediaPortal
// Copyright (C) 2020 Team MediaPortal
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

using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

      if ((string)parameters[0] == "SaveAll")
      {
        NeedsCallback = true;
      }
    }

    #endregion

    #region Command Implementation

    public override bool Execute(SongData song)
    {
      if (!SaveTrack(song))
      {
        bErrors = true;
      }

      // returning false here, since we are setting the Track Status in Save Track
      // and don't want to have it changed again in calling routine
      return false;
    }

    public override bool PostProcess()
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
    private bool SaveTrack(SongData song)
    {
      try
      {
        if (song.Changed)
        {
          //Util.SendProgress($"Saving file {track.FullFileName}");
          log.Debug($"Save: Saving track: {song.FullFileName}");

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
            //CaseConversion.CaseConversion convert = new CaseConversion.CaseConversion(TracksGrid.MainForm, true);
            //convert.CaseConvert(track, rowIndex);
            //convert.Dispose();
          }

          var originalFileName = song.FullFileName; // Need the original filename for database update, in case a rename happens

          // Save the file 
          var errorMessage = "";
          if (Song.SaveFile(song, ref errorMessage))
          {
            if (RenameFile(song))
            {
              // rename was ok, so get the new file into the binding list
              string ext = Path.GetExtension(song.FileName);
              string newFileName = Path.Combine(Path.GetDirectoryName(song.FullFileName),
                                                $"{Path.GetFileNameWithoutExtension(song.FileName)}{ext}");

              song = Song.Create(newFileName);
            }

            // Check, if we need to create a folder.jpg
            if (!System.IO.File.Exists(Path.Combine(Path.GetDirectoryName(song.FullFileName), "folder.jpg")) &&
                options.MainSettings.CreateFolderThumb)
            {
              //Util.SavePicture(song);
            }

            // Update the Music Database
            //ServiceScope.Get<IMusicDatabase>().UpdateTrack(track, originalFileName);

            song.Status = 0;
            song.Changed = false;
            //TracksGrid.View.Rows[rowIndex].Cells[0].ToolTipText = "";
            //track.Changed = false;
            //TracksGrid.View.Rows[rowIndex].Tag = "";
            //Options.Songlist[rowIndex] = track;
            //TracksGrid.SetGridRowColors(rowIndex);
          }
          else
          {
            song.Status = 2;
            //TracksGrid.AddErrorMessage(TracksGrid.View.Rows[rowIndex], errorMessage);
          }
        }
      }
      catch (Exception ex)
      {
        //Options.Songlist[rowIndex].Status = 2;
        //TracksGrid.AddErrorMessage(TracksGrid.View.Rows[rowIndex], ex.Message);
        //log.Error("Save: Error Saving data for row {0}: {1} {2}", rowIndex, ex.Message, ex.StackTrace);
        return false;
      }
      return true;
    }

    /// <summary>
    ///   Rename the file if necessary
    ///   Called by Save and SaveAll
    /// </summary>
    /// <param name = "song"></param>
    private bool RenameFile(SongData song)
    {
      var originalFileName = Path.GetFileName(song.FullFileName);
      if (originalFileName != song.FileName)
      {
        var ext = Path.GetExtension(song.FileName);
        var filename = Path.GetFileNameWithoutExtension(song.FileName);
        var path = Path.GetDirectoryName(song.FullFileName);
        var newFileName = Path.Combine(path, $"{filename}{ext}");

        // Check, if the New file name already exists
        // Don't change the newfilename, when only the Case change happened in filename
        int i = 1;
        if (System.IO.File.Exists(newFileName) && originalFileName.ToLowerInvariant() != song.FileName.ToLowerInvariant())
        {
          newFileName = Path.Combine(path, $"{filename} ({i}){ext}");
          while (System.IO.File.Exists(newFileName))
          {
            i++;
            newFileName = Path.Combine(path, $"{filename} ({i}){ext}");
          }
        }

        System.IO.File.Move(song.FullFileName, newFileName);
        log.Debug($"Save: Renaming Song: {song.FullFileName} Newname: {newFileName}");
        return true;
      }
      return false;
    }

    #endregion
  }
}
