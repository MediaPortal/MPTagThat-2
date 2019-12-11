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
  public class DialogViewModelBase : BindableBase, IDialogAware, IDialogWindow
  {
    private DelegateCommand<string> _closeDialogCommand;
    public DelegateCommand<string> CloseDialogCommand =>
        _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

    private string _iconSource;
    public string IconSource
    {
      get { return _iconSource; }
      set { SetProperty(ref _iconSource, value); }
    }

    private string _title;
    public string Title
    {
      get { return _title; }
      set { SetProperty(ref _title, value); }
    }

    public object Content { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Window Owner { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public object DataContext { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IDialogResult Result { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Style Style { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event Action<IDialogResult> RequestClose;
    public event RoutedEventHandler Loaded;
    public event EventHandler Closed;
    public event CancelEventHandler Closing;

    public virtual void CloseDialog(string parameter)
    {

    }

    public virtual void RaiseRequestClose(IDialogResult dialogResult)
    {
      RequestClose?.Invoke(dialogResult);
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

    public void Close()
    {
      throw new NotImplementedException();
    }

    public void Show()
    {
      throw new NotImplementedException();
    }

    public bool? ShowDialog()
    {
      throw new NotImplementedException();
    }
  }

}
