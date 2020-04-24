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

#region

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

#endregion

namespace MPTagThat.Core.Utils
{
  /// <summary>
  /// Summary description for ShellIcon.  Get a small or large Icon with an easy C# function call
  /// that returns a 32x32 or 16x16 System.Drawing.Icon depending on which function you call
  /// either GetSmallIcon(string fileName) or GetLargeIcon(string fileName)
  /// </summary>
  public static class ShellIcon
  {
    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
      public IntPtr hIcon;
      public IntPtr iIcon;
      public uint dwAttributes;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
      public string szDisplayName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
      public string szTypeName;
    };

    class Win32
    {
      public const uint SHGFI_ICON = 0x100;
      public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
      public const uint SHGFI_SMALLICON = 0x1; // 'Small icon
      public const uint SHGFI_PIDL = 0x8; // pszPath is a pidl

      [DllImport("shell32")]
      public static extern IntPtr SHGetFileInfo(string pszPath, int dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

      [DllImport("shell32")]
      public static extern IntPtr SHGetFileInfo(IntPtr pidl, int dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

      [DllImport("shell32.dll")]
      public static extern Int32 SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] String pszName, IntPtr pbc, out IntPtr ppidl, UInt32 sfgaoIn, out UInt32 psfgaoOut);

      [DllImport("User32.dll")]
      public static extern int DestroyIcon(IntPtr hIcon);

    }

    public static Icon GetSmallIcon(string fileName)
    {
      return GetIcon(fileName, Win32.SHGFI_SMALLICON);
    }

    public static BitmapImage GetSmallIcon(string fileName, bool imagesource)
    {
      var icon = GetIcon(fileName, Win32.SHGFI_SMALLICON);
      if (icon == null)
      {
        return null;
      }
      return ToImageSource(icon);
    }

    public static Icon GetLargeIcon(string fileName)
    {
      return GetIcon(fileName, Win32.SHGFI_LARGEICON);
    }

    private static Icon GetIcon(string fileName, uint flags)
    {
      SHFILEINFO shinfo = new SHFILEINFO();
      IntPtr retVal = Win32.SHGetFileInfo(fileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | flags);
      if (retVal == IntPtr.Zero)
      {
        // Seems to be a special folder, so we need to try with the PIDL
        flags |= Win32.SHGFI_ICON | Win32.SHGFI_PIDL;
        IntPtr pidl = IntPtr.Zero;
        uint iAttribute;

        // Convert the folder name to the PIDL and get it's icon
        Win32.SHParseDisplayName(fileName, IntPtr.Zero, out pidl, 0, out iAttribute);
        if (pidl == IntPtr.Zero)
        {
          return null;
        }
        retVal = Win32.SHGetFileInfo(pidl, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
        Marshal.FreeCoTaskMem(pidl);
        if (retVal == IntPtr.Zero)
        {
          return null;
        }
      }
      
      Icon icon = (Icon)System.Drawing.Icon.FromHandle(shinfo.hIcon).Clone();
      Win32.DestroyIcon(shinfo.hIcon);
      return icon;
    }

    public static BitmapImage ToImageSource(this Icon icon)
    {
      var bitmap = icon.ToBitmap();
      using (var memory = new MemoryStream())
      {
        bitmap.Save(memory, ImageFormat.Png);
        memory.Position = 0;

        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memory;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        bitmapImage.Freeze();

        return bitmapImage;
      }
    }
  }
}
