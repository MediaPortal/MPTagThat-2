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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

#endregion

namespace MPTagThat.Core.Common.Behaviors
{
  public class ComboBoxCursorPositionBehavior : DependencyObject
  {
    public static void SetCursorPosition(DependencyObject dependencyObject, int i)
    {
      dependencyObject.SetValue(CursorPositionProperty, i);
    }

    public static int GetCursorPosition(DependencyObject dependencyObject)
    {
      return (int)dependencyObject.GetValue(CursorPositionProperty);
    }

    public static readonly DependencyProperty CursorPositionProperty =
                                       DependencyProperty.Register("CursorPosition"
                                                                   , typeof(int)
                                                                   , typeof(ComboBoxCursorPositionBehavior)
                                                                   , new FrameworkPropertyMetadata(default(int))
                                                                   {
                                                                     BindsTwoWayByDefault = true
                                                                       ,
                                                                     DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                                                                   }
                                                                   );

    public static readonly DependencyProperty TrackCaretIndexProperty =
                                                DependencyProperty.RegisterAttached(
                                                    "TrackCaretIndex",
                                                    typeof(bool),
                                                    typeof(ComboBoxCursorPositionBehavior),
                                                    new UIPropertyMetadata(false
                                                                            , OnTrackCaretIndex));

    public static void SetTrackCaretIndex(DependencyObject dependencyObject, bool i)
    {
      dependencyObject.SetValue(TrackCaretIndexProperty, i);
    }

    public static bool GetTrackCaretIndex(DependencyObject dependencyObject)
    {
      return (bool)dependencyObject.GetValue(TrackCaretIndexProperty);
    }

    private static void OnTrackCaretIndex(DependencyObject dependency, DependencyPropertyChangedEventArgs e)
    {
      var comboBox = (ComboBox)dependency;

      if (comboBox == null)
        return;
      bool oldValue = (bool)e.OldValue;
      bool newValue = (bool)e.NewValue;

      if (!oldValue && newValue) // If changed from false to true
      {
        comboBox.SelectionChanged += OnSelectionChanged;
      }
      else if (oldValue && !newValue) // If changed from true to false
      {
        comboBox.SelectionChanged -= OnSelectionChanged;
      }
    }

    private static void OnSelectionChanged(object sender, RoutedEventArgs e)
    {
      if (sender is ComboBox comboBox && comboBox.Template != null)
      {
        if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
        {
          textBox.SelectionChanged += TextBox_SelectionChanged;
        }

      }
    }

    private static void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
      if (sender is TextBox textBox)
      {
        SetCursorPosition(textBox.TemplatedParent, textBox.CaretIndex);
      }
    }
  }
}
