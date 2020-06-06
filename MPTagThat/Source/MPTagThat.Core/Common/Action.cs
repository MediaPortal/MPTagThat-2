using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
