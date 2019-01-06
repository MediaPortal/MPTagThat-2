#region Copyright (C) 2017 Team MediaPortal
// Copyright (C) 2017 Team MediaPortal
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
using System;
using System.Windows.Input;

namespace MPTagThat.Core.Common
{
  public class RelayCommand : ICommand
  {
    // Event that fires when the enabled/disabled state of the cmd changes
    public event EventHandler CanExecuteChanged;

    // Delegate for method to call when the cmd needs to be executed        
    private readonly Action<object> _targetExecuteMethod;

    // Delegate for method that determines if cmd is enabled/disabled        
    private readonly Predicate<object> _targetCanExecuteMethod;

    public bool CanExecute(object parameter)
    {
      return _targetCanExecuteMethod == null || _targetCanExecuteMethod(parameter);
    }

    public void Execute(object parameter)
    {
      // Call the delegate if it's not null
      _targetExecuteMethod?.Invoke(parameter);
    }

    public RelayCommand(Action<object> executeMethod, Predicate<object> canExecuteMethod = null)
    {
      _targetExecuteMethod = executeMethod;
      _targetCanExecuteMethod = canExecuteMethod;
    }

    public void RaiseCanExecuteChanged()
    {
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
  }
}
