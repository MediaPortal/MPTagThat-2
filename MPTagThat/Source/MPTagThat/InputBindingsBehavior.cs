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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MPTagThat
{
  public class InputBindingsBehavior
  {
    public static readonly DependencyProperty InputBindingsProperty = DependencyProperty.RegisterAttached(
        "InputBindings", typeof(IEnumerable<InputBinding>), typeof(InputBindingsBehavior), new PropertyMetadata(null, new PropertyChangedCallback(Callback)));

    public static void SetInputBindings(UIElement element, IEnumerable<InputBinding> value)
    {
      element.SetValue(InputBindingsProperty, value);
    }
    public static IEnumerable<InputBinding> GetInputBindings(UIElement element)
    {
      return (IEnumerable<InputBinding>)element.GetValue(InputBindingsProperty);
    }

    private static void Callback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      UIElement uiElement = (UIElement)d;
      uiElement.InputBindings.Clear();
      IEnumerable<InputBinding> inputBindings = e.NewValue as IEnumerable<InputBinding>;
      if (inputBindings != null)
      {
        foreach (InputBinding inputBinding in inputBindings)
          uiElement.InputBindings.Add(inputBinding);
      }
    }
  }
}
