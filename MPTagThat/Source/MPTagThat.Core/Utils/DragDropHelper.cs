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

using System;
using System.Windows;

#endregion

namespace MPTagThat.Core.Utils
{
  /// <summary>
  /// IDragDropTarget Interface
  /// </summary>
  public interface IDragDropTarget
  {
    void OnFileDrop(string[] filepaths);
    void OnHtmlDrop(object html);
  }

  /// <summary>
  /// FileDragDropHelper
  /// </summary>
  public class DragDropHelper
  {
    public static bool GetIsDragDropEnabled(DependencyObject obj)
    {
      return (bool)obj.GetValue(IsDragDropEnabledProperty);
    }

    public static void SetIsDragDropEnabled(DependencyObject obj, bool value)
    {
      obj.SetValue(IsDragDropEnabledProperty, value);
    }

    public static bool GetDragDropTarget(DependencyObject obj)
    {
      return (bool)obj.GetValue(DragDropTargetProperty);
    }

    public static void SetDragDropTarget(DependencyObject obj, bool value)
    {
      obj.SetValue(DragDropTargetProperty, value);
    }

    public static readonly DependencyProperty IsDragDropEnabledProperty =
            DependencyProperty.RegisterAttached("IsDragDropEnabled", typeof(bool), typeof(DragDropHelper), new PropertyMetadata(OnDragDropEnabled));

    public static readonly DependencyProperty DragDropTargetProperty =
            DependencyProperty.RegisterAttached("DragDropTarget", typeof(object), typeof(DragDropHelper), null);

    private static void OnDragDropEnabled(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue == e.OldValue) return;
      var control = d as UIElement;
      if (control != null)
      {
        control.Drop += OnDrop;
      }
    }

    private static void OnDrop(object _sender, DragEventArgs _dragEventArgs)
    {
      DependencyObject d = _sender as DependencyObject;
      if (d == null) return;
      Object target = d.GetValue(DragDropTargetProperty);
      IDragDropTarget dropTarget = target as IDragDropTarget;
      if (dropTarget != null)
      {
        if (_dragEventArgs.Data.GetDataPresent(DataFormats.Html))
        {
          dropTarget.OnHtmlDrop((object)_dragEventArgs.Data.GetData(DataFormats.Html));
        }
        else if (_dragEventArgs.Data.GetDataPresent(DataFormats.FileDrop))
        {
          dropTarget.OnFileDrop((string[])_dragEventArgs.Data.GetData(DataFormats.FileDrop));
        }
      }
      else
      {
        throw new Exception("DragDropTarget object must be of type IDragDropTarget");
      }
    }
  }
}
