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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using Prism.Services.Dialogs;
using Syncfusion.UI.Xaml.Grid;
using WPFLocalizeExtension.Engine;
using SelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;

#endregion

namespace MPTagThat.Dialogs.ViewModels
{
  public class CaseConversionViewModel : DialogViewModelBase
  {
    #region Variables

    private List<SongData> _songs;
    private List<string> _selectedExceptions = new List<string>();

    #endregion

    #region Properties

    private ObservableCollection<string> _convertExceptions = new ObservableCollection<string>();

    public ObservableCollection<string> ConvertExceptions
    {
      get => _convertExceptions;
      set
      {
        _convertExceptions = value;
        RaisePropertyChanged("ConvertExceptions");
      }
    }

    private string _newException;

    public string NewException
    {
      get => _newException;
      set => SetProperty(ref _newException, value);
    }

    // Check Box Checked properties
    private bool _ckConvertTags;
    public bool CkConvertTags { get => _ckConvertTags; set => SetProperty(ref _ckConvertTags, value); }

    private bool _ckConvertFileName;
    public bool CkConvertFileName { get => _ckConvertFileName; set => SetProperty(ref _ckConvertFileName, value); }

    private bool _ckAllLowerCase;
    public bool CkAllLowerCase { get => _ckAllLowerCase; set => SetProperty(ref _ckAllLowerCase, value); }

    private bool _ckAllUpperCase;
    public bool CkAllUpperCase { get => _ckAllUpperCase; set => SetProperty(ref _ckAllUpperCase, value); }

    private bool _ckFirstLetterUpperCase;
    public bool CkFirstLetterUpperCase { get => _ckFirstLetterUpperCase; set => SetProperty(ref _ckFirstLetterUpperCase, value); }

    private bool _ckAllFirstLetterUpperCase;
    public bool CkAllFirstLetterUpperCase { get => _ckAllFirstLetterUpperCase; set => SetProperty(ref _ckAllFirstLetterUpperCase, value); }

    private bool _ckConvertArtist;
    public bool CkConvertArtist { get => _ckConvertArtist; set => SetProperty(ref _ckConvertArtist, value); }

    private bool _ckConvertAlbumArtist;
    public bool CkConvertAlbumArtist { get => _ckConvertAlbumArtist; set => SetProperty(ref _ckConvertAlbumArtist, value); }

    private bool _ckConvertAlbum;
    public bool CkConvertAlbum { get => _ckConvertAlbum; set => SetProperty(ref _ckConvertAlbum, value); }

    private bool _ckConvertTitle;
    public bool CkConvertTitle { get => _ckConvertTitle; set => SetProperty(ref _ckConvertTitle, value); }

    private bool _ckConvertComment;
    public bool CkConvertComment { get => _ckConvertComment; set => SetProperty(ref _ckConvertComment, value); }

    private bool _ckReplace20BySpace;
    public bool CkReplace20BySpace { get => _ckReplace20BySpace; set => SetProperty(ref _ckReplace20BySpace, value); }

    private bool _ckReplaceUnderScoreBySpace;
    public bool CkReplaceUnderScoreBySpace { get => _ckReplaceUnderScoreBySpace; set => SetProperty(ref _ckReplaceUnderScoreBySpace, value); }
    
    private bool _ckAlwaysUpperCaseFirstLetter;
    public bool CkAlwaysUpperCaseFirstLetter { get => _ckAlwaysUpperCaseFirstLetter; set => SetProperty(ref _ckAlwaysUpperCaseFirstLetter, value); }

    private bool _ckReplaceSpaceBy20;
    public bool CkReplaceSpaceBy20 { get => _ckReplaceSpaceBy20; set => SetProperty(ref _ckReplaceSpaceBy20, value); }

    private bool _ckReplaceSpaceByUnderscore;
    public bool CkReplaceSpaceByUnderscore { get => _ckReplaceSpaceByUnderscore; set => SetProperty(ref _ckReplaceSpaceByUnderscore, value); }

    #endregion


    #region ctor

    public CaseConversionViewModel()
    {
      Title = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "caseConversion_Header",
        LocalizeDictionary.Instance.Culture).ToString();
      CaseConversionCommand = new BaseCommand(CaseConversionApply);
      ExceptionAddCommand = new BaseCommand(ExceptionAdd);
      ExceptionRemoveCommand = new BaseCommand(ExceptionRemove);
      SelectionChangedCommand = new BaseCommand(SelectionChanged);

      ConvertExceptions.AddRange(_options.ConversionSettings.CaseConvExceptions);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Apply Button has been clicked
    /// </summary>
    public ICommand CaseConversionCommand { get; }

    private void CaseConversionApply(object param)
    {
      log.Trace(">>>");
      _options.ConversionSettings.ConvertTags = CkConvertTags;
      _options.ConversionSettings.ConvertFileName = CkConvertFileName;
      _options.ConversionSettings.ConvertAllLower = CkAllLowerCase;
      _options.ConversionSettings.ConvertAllUpper = CkAllUpperCase;
      _options.ConversionSettings.ConvertFirstUpper = CkFirstLetterUpperCase;
      _options.ConversionSettings.ConvertAllFirstUpper = CkAllFirstLetterUpperCase;
      _options.ConversionSettings.ConvertArtist = CkConvertArtist;
      _options.ConversionSettings.ConvertAlbumArtist = CkConvertAlbumArtist;
      _options.ConversionSettings.ConvertAlbum = CkConvertAlbum;
      _options.ConversionSettings.ConvertTitle = CkConvertTitle;
      _options.ConversionSettings.ConvertComment = CkConvertComment;
      _options.ConversionSettings.Replace20BySpace = CkReplace20BySpace;
      _options.ConversionSettings.ReplaceUnderscoreBySpace = CkReplaceUnderScoreBySpace;
      _options.ConversionSettings.ConvertAllWaysFirstUpper = CkAlwaysUpperCaseFirstLetter;
      _options.ConversionSettings.ReplaceSpaceBy20 = CkReplaceSpaceBy20;
      _options.ConversionSettings.ReplaceSpaceByUnderscore = CkReplaceSpaceByUnderscore;

      for (var i=0; i < _songs.Count; i++)
      {
        var song = _songs[i];
        CaseConversion.CaseConvert(ref song);
      }
      CloseDialog("true");
      log.Trace("<<<");
    }

    /// <summary>
    /// A new exception should be added to the list
    /// </summary>
    public ICommand ExceptionAddCommand { get; }

    private void ExceptionAdd(object param)
    {
      ConvertExceptions.Add(NewException);
      _options.ConversionSettings.CaseConvExceptions = ConvertExceptions.ToList();
    }

    public ICommand ExceptionRemoveCommand { get; }

    private void ExceptionRemove(object param)
    {
      // Prevent the Collection has been modified Error
      var selectionsTobeRemoved = new List<string>();
      foreach (var exc in _selectedExceptions)
      {
        selectionsTobeRemoved.Add(exc);
      }

      selectionsTobeRemoved.ForEach(exc => ConvertExceptions.Remove(exc));
      _options.ConversionSettings.CaseConvExceptions = ConvertExceptions.ToList();
    }

    public ICommand SelectionChangedCommand { get; }

    private void SelectionChanged(object param)
    {
      var parm = (SelectionChangedEventArgs) param;
      foreach (var added in parm.AddedItems)
      {
        _selectedExceptions.Add((string)added);
      }

      foreach (var removed in parm.RemovedItems)
      {
        _selectedExceptions.Remove((string) removed);
      }
    }

    #endregion

    #region Private Methods

    private void LoadParameters()
    {
      log.Trace(">>>");
      CkConvertTags = _options.ConversionSettings.ConvertTags;
      CkConvertFileName = _options.ConversionSettings.ConvertFileName;
      CkAllLowerCase = _options.ConversionSettings.ConvertAllLower;
      CkAllUpperCase = _options.ConversionSettings.ConvertAllUpper;
      CkFirstLetterUpperCase = _options.ConversionSettings.ConvertFirstUpper;
      CkAllFirstLetterUpperCase = _options.ConversionSettings.ConvertAllFirstUpper;
      CkConvertArtist = _options.ConversionSettings.ConvertArtist;
      CkConvertAlbumArtist = _options.ConversionSettings.ConvertAlbumArtist;
      CkConvertAlbum = _options.ConversionSettings.ConvertAlbum;
      CkConvertTitle = _options.ConversionSettings.ConvertTitle;
      CkConvertComment = _options.ConversionSettings.ConvertComment;
      CkReplace20BySpace = _options.ConversionSettings.Replace20BySpace;
      CkReplaceUnderScoreBySpace = _options.ConversionSettings.ReplaceUnderscoreBySpace;
      CkAlwaysUpperCaseFirstLetter = _options.ConversionSettings.ConvertAllWaysFirstUpper;
      CkReplaceSpaceBy20 = _options.ConversionSettings.ReplaceSpaceBy20;
      CkReplaceSpaceByUnderscore = _options.ConversionSettings.ReplaceSpaceByUnderscore;
      log.Trace("<<<");
    }

    #endregion

    #region Overrides

    public override void OnDialogOpened(IDialogParameters parameters)
    {
      _songs = parameters.GetValue<List<SongData>>("songs");
      LoadParameters();
    }

    #endregion
  }
}
