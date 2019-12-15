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

using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.ComponentModel;
using System.Windows;

#endregion

namespace MPTagThat.Dialogs.ViewModels
{
  /// <summary>
  /// This is the base class to support showing of Dialogs
  /// </summary>
  public class DialogViewModelBase : BindableBase, IDialogAware
  {
    private DelegateCommand<string> _closeDialogCommand;
    public DelegateCommand<string> CloseDialogCommand =>
      _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

    private string _title;
    public string Title
    {
      get => _title; 
      set => SetProperty(ref _title, value);
    }

    public event Action<IDialogResult> RequestClose;

    public virtual void CloseDialog(string parameter)
    {
      ButtonResult result = ButtonResult.None;

      if (parameter?.ToLower() == "true")
        result = ButtonResult.OK;
      else if (parameter?.ToLower() == "false")
        result = ButtonResult.Cancel;

      
    }

    public void CloseDialogWindow(DialogResult result)
    {
      RequestClose?.Invoke(result);
    }


    public virtual bool CanCloseDialog()
    {
      return true;
    }

    public virtual void OnDialogClosed()
    {

    }

    public virtual void OnDialogOpened(IDialogParameters parameters)
    {

    }
  }
}
