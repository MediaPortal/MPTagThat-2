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

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Prism.Services.Dialogs;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Utility;
using WPFLocalizeExtension.Engine;

#endregion


namespace MPTagThat.Dialogs.ViewModels
{
  public class FindReplaceViewModel : DialogViewModelBase
  {
    #region Variables

    private SfDataGrid _songGrid;
    private bool _stringFoundOnce;
    private bool _replaceAll;
    
    #endregion

    #region Properties

    private bool _ckMatchCase;
    public bool CkMatchCase 
    { 
      get => _ckMatchCase;
      set
      {
        SetProperty(ref _ckMatchCase, value);
        _songGrid.SearchHelper.AllowCaseSensitiveSearch = value;
      }
    }

    /// <summary>
    /// The Binding for the FindBuffer ComboBox
    /// </summary>
    private ObservableCollection<string> _findBuffer = new ObservableCollection<string>();
    public ObservableCollection<string> FindBuffer
    {
      get => _findBuffer;
      set
      {
        _findBuffer = value;
        RaisePropertyChanged("FindBuffer");
      }
    }

    /// <summary>
    /// The Binding for the Selected Text in the FindBuffer Combobox
    /// </summary>
    private string _selectedTextFindBuffer;

    public string SelectedTextFindBuffer
    {
      get => _selectedTextFindBuffer;
      set
      {
        SetProperty(ref _selectedTextFindBuffer, value);
        if (value.Length > 0)
        {
          _songGrid.SearchHelper.Search(value);
        }
      }
    }

    /// <summary>
    /// The Binding for the ReplaceBuffer ComboBox
    /// </summary>
    private ObservableCollection<string> _replaceBuffer = new ObservableCollection<string>();
    public ObservableCollection<string> ReplaceBuffer
    {
      get => _replaceBuffer;
      set
      {
        _replaceBuffer = value;
        RaisePropertyChanged("ReplaceBuffer");
      }
    }

    /// <summary>
    /// The Binding for the Selected Text in the ReplaceBuffer Combobox
    /// </summary>
    private string _selectedTextReplaceBuffer;

    public string SelectedTextReplaceBuffer
    {
      get => _selectedTextReplaceBuffer;
      set => SetProperty(ref _selectedTextReplaceBuffer, value);
    }


    private bool _isReplace;

    public bool IsReplace 
    {
      get => _isReplace; 
      set => SetProperty(ref _isReplace, value);
    }

    #endregion

    #region ctor

    public FindReplaceViewModel()
    {
      FindNextCommand = new BaseCommand(FindNext);
      ReplaceCommand = new BaseCommand(Replace);
      ReplaceAllCommand = new BaseCommand(ReplaceAll);
    }

    #endregion

    #region Commands

    public ICommand FindNextCommand { get; }

    private void FindNext(object param)
    {
      Find();
      MaintainFindReplaceBuffer();
    }

    private bool Find()
    {
      if (_songGrid.SearchHelper.FindNext(SelectedTextFindBuffer))
      {
        _stringFoundOnce = true;
        return true;
      }

      if (_replaceAll)
      {
        return false;
      }

      if (_stringFoundOnce)
      {
        MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "findReplace_NoMoreOccurencesFound",
          LocalizeDictionary.Instance.Culture).ToString(), "", MessageBoxButton.OK);
      }
      else
      {
        MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "findReplace_NotFound",
          LocalizeDictionary.Instance.Culture).ToString(), "", MessageBoxButton.OK);
      }

      return false;
    }


    public ICommand ReplaceCommand { get; }

    private void Replace(object param)
    {
      if (Find())
      {
        ReplaceValue();
      }

      MaintainFindReplaceBuffer();
    }

    public ICommand ReplaceAllCommand { get; }

    private void ReplaceAll(object param)
    {
      _replaceAll = true;

      while (Find())
      {
        ReplaceValue();
      }

      MaintainFindReplaceBuffer();
      _replaceAll = false;
    }

    private void ReplaceValue()
    {
      var index = _songGrid.SearchHelper.CurrentRowColumnIndex;
      if (index.RowIndex > -1)
      {
        if (!_songGrid.Columns[index.ColumnIndex].IsReadOnly)
        {
          var mappingName = _songGrid.Columns[index.ColumnIndex].MappingName;
          var record = _songGrid.View.Records.GetItemAt(index.RowIndex - 1);
          var cellValue = _songGrid.View.GetPropertyAccessProvider().GetValue(record, mappingName);
          if (cellValue != null)
          {
            var replacedString = (cellValue as string)?.Replace(SelectedTextFindBuffer, SelectedTextReplaceBuffer);
            _songGrid.View.GetPropertyAccessProvider().SetValue(record, mappingName, replacedString);
            _songGrid.View.Refresh();
          }
        }
      }
    }

    #endregion

    #region Private Methods

    private void MaintainFindReplaceBuffer()
    {
      if (!string.IsNullOrEmpty(SelectedTextFindBuffer))
      {
        if (FindBuffer.All(f => f != SelectedTextFindBuffer))
        {
          FindBuffer.Insert(0, SelectedTextFindBuffer);
        }
      }

      if (!string.IsNullOrEmpty(SelectedTextReplaceBuffer))
      {
        if (ReplaceBuffer.All(r => r != SelectedTextReplaceBuffer))
        {
          ReplaceBuffer.Insert(0, SelectedTextReplaceBuffer);
        }
      }
    }

    #endregion

    #region Overrides

    public override void OnDialogOpened(IDialogParameters parameters)
    {
      _songGrid = parameters.GetValue<SfDataGrid>("songgrid");
      var method = parameters.GetValue<string>("method");
      if (method == "Find")
      {
        IsReplace = false;
        Title = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "findReplace_HeaderFind",
          LocalizeDictionary.Instance.Culture).ToString();
      }
      else
      {
        Title = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "findReplace_HeaderReplace",
          LocalizeDictionary.Instance.Culture).ToString();
        IsReplace = true;
      }

      if (_options.FindBuffer != null)
      {
        FindBuffer.AddRange(_options.FindBuffer);
      }

      if (_options.ReplaceBuffer != null)
      {
        ReplaceBuffer.AddRange(_options.ReplaceBuffer);
      }
    }

    public override void CloseDialog(string parameter)
    {
      _options.FindBuffer = FindBuffer.ToList();
      _options.ReplaceBuffer = ReplaceBuffer.ToList();
      base.CloseDialog(parameter);
    }

    #endregion
  }
}
