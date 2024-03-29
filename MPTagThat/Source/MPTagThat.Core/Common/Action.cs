﻿#region Copyright (C) 2022 Team MediaPortal
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


namespace MPTagThat.Core.Common
{
  public class Action
  {
    #region enums

    public enum ActionType
    {
      // Attention: The IDs refer to the ID in the Keymap
      INVALID = 0,
      SAVE = 1,
      FILENAME2TAG = 3,
      TAG2FILENAME = 4,
      SELECTALL = 5,
      COPY = 6,
      PASTE = 7,
      SCRIPTEXECUTE = 8,
      TREEREFRESH = 9,
      FOLDERDELETE = 10,
      CASECONVERSION_BATCH = 11,
      REFRESH = 12,
      OPTIONS = 13,
      DELETE = 14,
      NEXTFILE = 17,
      PREVFILE = 18,
      ORGANISE = 20,
      IDENTIFYFILE = 21,
      GETCOVERART = 22,
      GETLYRICS = 23,
      EXIT = 24,
      HELP = 25,
      TAGFROMINTERNET = 26,
      REMOVECOMMENT = 28,
      REMOVEPICTURE = 29,
      SAVEALL = 30,
      VALIDATEMP3 = 32,
      FIXMP3 = 33,
      FIND = 34,
      REPLACE = 35,
      REPLAYGAIN = 38,
      MusicBrainzInfo = 39,

      // Actions which don't have a Key assigned
      CASECONVERSION = 60,
      DELETEALLTAGS = 61,
      DELETEV1TAGS = 62,
      DELETEV2TAGS = 63,
      BPM = 64,
      AUTONUMBER = 65,
      NUMBERONCLICK = 66,
      CONVERT = 67,
      CONVERTCANCEL = 68,
      ADDCONVERSION = 69,
      DATABASEQUERY = 70,
      RIP = 71,
      RIPCANCEL = 72,
      GNUDBQUERY = 73,
      SWITCHDATABASE = 74,
      DATABASESTATUS = 75,
      DATABASETREEVIEWREFRESH = 76,
      CHECKARTISTS = 77,
      CHECKALBUMS = 78,
      SCANCHECKDATABASE = 79,
      APPLYSELECTEDTAGCHECKERITEM = 80,
      TOGGLEIGNORESELECTEDTAGCHECKERITEM = 81,
    }

    #endregion

    #region Variables

    #endregion

    #region Properties

    public ActionType ID { get; set; }

    #endregion

    #region public Methods

    public static string ActionToCommand(ActionType action)
    {
      var checkSelections = false;
      return ActionToCommand(action, ref checkSelections);
    }

    public static string ActionToCommand(ActionType action, ref bool checkSelections)
    {
      switch (action)
      {
        case ActionType.SAVE:
          return "Save";

        case ActionType.SAVEALL:
          return "SaveAll";

        case ActionType.IDENTIFYFILE:
          checkSelections = true;
          return "IdentifySong";

        case Action.ActionType.GETCOVERART:
          checkSelections = true;
          return "GetCoverArt";

        case Action.ActionType.GETLYRICS:
          checkSelections = true;
          return "GetLyrics";

        case Action.ActionType.REMOVECOMMENT:
          checkSelections = true;
          return "RemoveComments";

        case Action.ActionType.REMOVEPICTURE:
          checkSelections = true;
          return "RemoveCoverArt";

        case Action.ActionType.VALIDATEMP3:
          checkSelections = true;
          return "ValidateMP3File";

        case Action.ActionType.FIXMP3:
          checkSelections = true;
          return "FixMP3File";

        case Action.ActionType.REPLAYGAIN:
          checkSelections = true;
          return "ReplayGain";

        case Action.ActionType.CASECONVERSION_BATCH:
          checkSelections = true;
          return "CaseConversion";

        case Action.ActionType.DELETEALLTAGS:
          checkSelections = true;
          return "DeleteAllTags";

        case Action.ActionType.DELETEV1TAGS:
          checkSelections = true;
          return "DeleteV1Tags";

        case Action.ActionType.DELETEV2TAGS:
          checkSelections = true;
          return "DeleteV2Tags";

        case Action.ActionType.BPM:
          checkSelections = true;
          return "Bpm";

        case Action.ActionType.AUTONUMBER:
          checkSelections = true;
          return "AutoNumber";

        case Action.ActionType.MusicBrainzInfo:
          checkSelections = true;
          return "MusicBrainzInfo";

        default:
          return "";
      }
    }

    #endregion

  }
}
