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

#region 

using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using CommonServiceLocator;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Core.Utils;
using WPFLocalizeExtension.Engine;

#endregion

namespace MPTagThat.Core.Common.Song
{
  public static class CaseConversion
  {
    #region Variables

    private static NLogLogger log;
    private static Options _options;

    private static readonly TextInfo _textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
    private static string _strExcep;

    #endregion

    #region ctor

    static CaseConversion()
    {
      log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger)?.GetLogger;
      _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;
    }

    #endregion

    #region Public Methods

    public static void CaseConvert(ref SongData song)
    {
      log.Trace(">>>");
      bool bErrors = false;
      // Convert the Filename
      if (_options.ConversionSettings.ConvertFileName)
      {
        var fileName = ConvertCase(Path.GetFileNameWithoutExtension(song.FileName));

        // Now check the length of the filename
        if (fileName.Length > 255)
        {
          log.Debug($"Filename too long: {fileName}");
          song.Status = 2;
          song.StatusMsg = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings",
            "tagAndRename_NameTooLong", LocalizeDictionary.Instance.Culture).ToString();
          bErrors = true;
        }

        if (!bErrors)
        {
          // Now that we have a correct Filename
          if (fileName != Path.GetFileNameWithoutExtension(song.FileName))
          {
            song.FileName = $"{fileName}{Path.GetExtension(song.FileName)}";
            song.Changed = true;
          }
        }
      }

      // Convert the Tags
      if (_options.ConversionSettings.ConvertTags)
      {
        string strConv;
        var bChanged = false;
        if (_options.ConversionSettings.ConvertArtist)
        {
          strConv = song.Artist;
          bChanged = (strConv = ConvertCase(strConv)) != song.Artist;
          if (bChanged)
          {
            song.Artist = strConv;
            song.Changed = true;
          }
        }

        if (_options.ConversionSettings.ConvertAlbumArtist)
        {
          strConv = song.AlbumArtist;
          bChanged = (strConv = ConvertCase(strConv)) != song.AlbumArtist;
          if (bChanged)
          {
            song.AlbumArtist = strConv;
            song.Changed = true;
          }
        }

        if (_options.ConversionSettings.ConvertAlbum)
        {
          strConv = song.Album;
          bChanged = (strConv = ConvertCase(strConv)) != song.Album;
          if (bChanged)
          {
            song.Album = strConv;
            song.Changed = true;
          }
        }

        if (_options.ConversionSettings.ConvertTitle)
        {
          strConv = song.Title;
          bChanged = (strConv = ConvertCase(strConv)) != song.Title;
          if (bChanged)
          {
            song.Title = strConv;
            song.Changed = true;
          }
        }

        if (_options.ConversionSettings.ConvertComment)
        {
          strConv = song.Comment;
          bChanged = (strConv = ConvertCase(strConv)) != song.Comment;
          if (bChanged)
          {
            song.Comment = strConv;
            song.Changed = true;
          }
        }
      }
      log.Trace("<<<");
    }

    #endregion

    #region Private Methods

    private static string ConvertCase(string strText)
    {
      if (string.IsNullOrEmpty(strText))
      {
        return string.Empty;
      }

      if (_options.ConversionSettings.Replace20BySpace)
      {
        strText = strText.Replace("%20", " ");
      }

      if (_options.ConversionSettings.ReplaceSpaceBy20)
      {
        strText = strText.Replace(" ", "%20");
      }

      if (_options.ConversionSettings.ReplaceUnderscoreBySpace)
      {
        strText = strText.Replace("_", " ");
      }

      if (_options.ConversionSettings.ReplaceSpaceByUnderscore)
      {
        strText = strText.Replace(" ", "_");
      }

      if (_options.ConversionSettings.ConvertAllLower)
      {
        strText = strText.ToLowerInvariant();
      }
      else if (_options.ConversionSettings.ConvertAllUpper)
        strText = strText.ToUpperInvariant();
      else if (_options.ConversionSettings.ConvertFirstUpper)
      {
        // Go to Lowercase first, in case that everything is already uppercase
        strText = strText.ToLowerInvariant();
        strText = strText.Substring(0, 1).ToUpperInvariant() + strText.Substring(1);
      }
      else if (_options.ConversionSettings.ConvertAllFirstUpper)
      {
        // Go to Lowercase first, in case that everything is already uppercase
        strText = strText.ToLowerInvariant();
        strText = _textInfo.ToTitleCase(strText);
      }

      if (_options.ConversionSettings.ConvertAllWaysFirstUpper)
      {
        strText = strText.Substring(0, 1).ToUpperInvariant() + strText.Substring(1);
      }

      // Handle the Exceptions
      foreach (var excep in _options.ConversionSettings.CaseConvExceptions)
      {
        _strExcep = Regex.Escape(excep);
        strText = Regex.Replace(strText, @"(\W|^)" + _strExcep + @"(\W|$)", RegexReplaceCallback,
          RegexOptions.Singleline | RegexOptions.IgnoreCase);
      }

      return strText;
    }

    /// <summary>
    ///   Callback Method for every Match of the Regex
    /// </summary>
    /// <param name = "match"></param>
    /// <returns></returns>
    private static string RegexReplaceCallback(Match match)
    {
      _strExcep = _strExcep.Replace(@"\\", "\x0001").Replace(@"\", "").Replace("\x0001", @"\");
      return Util.ReplaceEx(match.Value, _strExcep, _strExcep);
    }

    #endregion
  }
}
