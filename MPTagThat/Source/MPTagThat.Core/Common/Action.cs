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
      CASECONVERSION = 11,
      CASECONVERSION_BATCH = 12,
      REFRESH = 13,
      OPTIONS = 14,
      DELETE = 15,
      NEXTFILE = 16,
      PREVFILE = 17,
      ORGANISE = 18,
      IDENTIFYFILE = 19,
      GETCOVERART = 20,
      GETLYRICS = 21,
      EXIT = 22,
      HELP = 23,
      TAGFROMINTERNET = 24,
      REMOVECOMMENT = 25,
      REMOVEPICTURE = 26,
      SAVEALL = 27,
      VALIDATEMP3 = 28,
      FIXMP3 = 29,
      FIND = 30,
      REPLACE = 31,
      REPLAYGAIN = 32,
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
          return "IdentifyFiles";

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

        default:
          return "";
      }
    }

    #endregion

  }
}
