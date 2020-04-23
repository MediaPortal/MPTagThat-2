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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#endregion

namespace MPTagThat.Treeview.Model.Win32
{

  #region Public Enumerations

  /// <summary>
  ///   Available system image list sizes
  /// </summary>
  public enum SystemImageListSize
  {
    /// <summary>
    ///   System Large Icon Size (typically 32x32)
    /// </summary>
    LargeIcons = 0x0,
    /// <summary>
    ///   System Small Icon Size (typically 16x16)
    /// </summary>
    SmallIcons = 0x1,
    /// <summary>
    ///   System Extra Large Icon Size (typically 48x48).
    ///   Only available under XP; under other OS the
    ///   Large Icon ImageList is returned.
    /// </summary>
    ExtraLargeIcons = 0x2
  }

  /// <summary>
  ///   Flags controlling how the Image List item is 
  ///   drawn
  /// </summary>
  [Flags]
  public enum ImageListDrawItemConstants
  {
    /// <summary>
    ///   Draw item normally.
    /// </summary>
    ILD_NORMAL = 0x0,
    /// <summary>
    ///   Draw item transparently.
    /// </summary>
    ILD_TRANSPARENT = 0x1,
    /// <summary>
    ///   Draw item blended with 25% of the specified foreground colour
    ///   or the Highlight colour if no foreground colour specified.
    /// </summary>
    ILD_BLEND25 = 0x2,
    /// <summary>
    ///   Draw item blended with 50% of the specified foreground colour
    ///   or the Highlight colour if no foreground colour specified.
    /// </summary>
    ILD_SELECTED = 0x4,
    /// <summary>
    ///   Draw the icon's mask
    /// </summary>
    ILD_MASK = 0x10,
    /// <summary>
    ///   Draw the icon image without using the mask
    /// </summary>
    ILD_IMAGE = 0x20,
    /// <summary>
    ///   Draw the icon using the ROP specified.
    /// </summary>
    ILD_ROP = 0x40,
    /// <summary>
    ///   Preserves the alpha channel in dest. XP only.
    /// </summary>
    ILD_PRESERVEALPHA = 0x1000,
    /// <summary>
    ///   Scale the image to cx, cy instead of clipping it.  XP only.
    /// </summary>
    ILD_SCALE = 0x2000,
    /// <summary>
    ///   Scale the image to the current DPI of the display. XP only.
    /// </summary>
    ILD_DPISCALE = 0x4000
  }

  /// <summary>
  ///   Enumeration containing XP ImageList Draw State options
  /// </summary>
  [Flags]
  public enum ImageListDrawStateConstants
  {
    /// <summary>
    ///   The image state is not modified.
    /// </summary>
    ILS_NORMAL = (0x00000000),
    /// <summary>
    ///   Adds a glow effect to the icon, which causes the icon to appear to glow 
    ///   with a given color around the edges. (Note: does not appear to be
    ///   implemented)
    /// </summary>
    ILS_GLOW = (0x00000001),
    //The color for the glow effect is passed to the IImageList::Draw method in the crEffect member of IMAGELISTDRAWPARAMS. 
    /// <summary>
    ///   Adds a drop shadow effect to the icon. (Note: does not appear to be
    ///   implemented)
    /// </summary>
    ILS_SHADOW = (0x00000002),
    //The color for the drop shadow effect is passed to the IImageList::Draw method in the crEffect member of IMAGELISTDRAWPARAMS. 
    /// <summary>
    ///   Saturates the icon by increasing each color component 
    ///   of the RGB triplet for each pixel in the icon. (Note: only ever appears
    ///   to result in a completely unsaturated icon)
    /// </summary>
    ILS_SATURATE = (0x00000004),
    // The amount to increase is indicated by the frame member in the IMAGELISTDRAWPARAMS method. 
    /// <summary>
    ///   Alpha blends the icon. Alpha blending controls the transparency 
    ///   level of an icon, according to the value of its alpha channel. 
    ///   (Note: does not appear to be implemented).
    /// </summary>
    ILS_ALPHA = (0x00000008)
    //The value of the alpha channel is indicated by the frame member in the IMAGELISTDRAWPARAMS method. The alpha channel can be from 0 to 255, with 0 being completely transparent, and 255 being completely opaque. 
  }

  /// <summary>
  ///   Flags specifying the state of the icon to draw from the Shell
  /// </summary>
  [Flags]
  public enum ShellIconStateConstants
  {
    /// <summary>
    ///   Get icon in normal state
    /// </summary>
    ShellIconStateNormal = 0,
    /// <summary>
    ///   Put a link overlay on icon
    /// </summary>
    ShellIconStateLinkOverlay = 0x8000,
    /// <summary>
    ///   show icon in selected state
    /// </summary>
    ShellIconStateSelected = 0x10000,
    /// <summary>
    ///   get open icon
    /// </summary>
    ShellIconStateOpen = 0x2,
    /// <summary>
    ///   apply the appropriate overlays
    /// </summary>
    ShellIconAddOverlays = 0x000000020,
  }

  #endregion

  #region SystemImageList class

  /// <summary>
  ///   Summary description for SysImageList.
  /// </summary>
  public class SystemImageList : IDisposable
  {
    #region Constants

    private const int MAX_PATH = 260;

    private const int FILE_ATTRIBUTE_NORMAL = 0x80;
    private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;

    private const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
    private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
    private const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
    private const int FORMAT_MESSAGE_FROM_STRING = 0x400;
    private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
    private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
    private const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xFF;

    #endregion

    #region Fields

    private bool disposed;
    private IntPtr hIml = IntPtr.Zero;
    private IImageList iImageList;
    private SystemImageListSize size = SystemImageListSize.SmallIcons;

    #endregion

    #region Private Enumerations

    [Flags]
    private enum SHGetFileInfoConstants
    {
      SHGFI_ICON = 0x100, // get icon 
      SHGFI_DISPLAYNAME = 0x200, // get display name 
      SHGFI_TYPENAME = 0x400, // get type name 
      SHGFI_ATTRIBUTES = 0x800, // get attributes 
      SHGFI_ICONLOCATION = 0x1000, // get icon location 
      SHGFI_EXETYPE = 0x2000, // return exe type 
      SHGFI_SYSICONINDEX = 0x4000, // get system icon index 
      SHGFI_LINKOVERLAY = 0x8000, // put a link overlay on icon 
      SHGFI_SELECTED = 0x10000, // show icon in selected state 
      SHGFI_ATTR_SPECIFIED = 0x20000, // get only specified attributes 
      SHGFI_LARGEICON = 0x0, // get large icon 
      SHGFI_SMALLICON = 0x1, // get small icon 
      SHGFI_OPENICON = 0x2, // get open icon 
      SHGFI_SHELLICONSIZE = 0x4, // get shell size icon 
      SHGFI_PIDL = 0x8, // pszPath is a pidl 
      SHGFI_USEFILEATTRIBUTES = 0x10, // use passed dwFileAttribute 
      SHGFI_ADDOVERLAYS = 0x000000020, // apply the appropriate overlays
      SHGFI_OVERLAYINDEX = 0x000000040 // Get the index of the overlay
    }

    #endregion

    #region Private ImageList structures

    #region Nested type: IMAGEINFO

    [StructLayout(LayoutKind.Sequential)]
    private struct IMAGEINFO
    {
      public readonly IntPtr hbmImage;
      public readonly IntPtr hbmMask;
      public readonly int Unused1;
      public readonly int Unused2;
      public readonly RECT rcImage;
    }

    #endregion

    #region Nested type: IMAGELISTDRAWPARAMS

    [StructLayout(LayoutKind.Sequential)]
    private struct IMAGELISTDRAWPARAMS
    {
      public int cbSize;
      public IntPtr himl;
      public int i;
      public IntPtr hdcDst;
      public int x;
      public int y;
      public int cx;
      public int cy;
      public readonly int xBitmap; // x offest from the upperleft of bitmap
      public readonly int yBitmap; // y offset from the upperleft of bitmap
      public readonly int rgbBk;
      public int rgbFg;
      public int fStyle;
      public readonly int dwRop;
      public int fState;
      public int Frame;
      public int crEffect;
    }

    #endregion

    #region Nested type: POINT

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
      private readonly int x;
      private readonly int y;
    }

    #endregion

    #region Nested type: RECT

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
      private readonly int left;
      private readonly int top;
      private readonly int right;
      private readonly int bottom;
    }

    #endregion

    #region Nested type: SHFILEINFO

    [StructLayout(LayoutKind.Sequential)]
    private struct SHFILEINFO
    {
      public readonly IntPtr hIcon;
      public readonly int iIcon;
      public readonly int dwAttributes;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public readonly string szDisplayName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public readonly string szTypeName;
    }

    #endregion

    #endregion

    #region Constructors, Dispose, Destructor

    /// <summary>
    ///   Creates a Small Icons SystemImageList
    /// </summary>
    public SystemImageList()
    {
      create();
    }

    /// <summary>
    ///   Creates a SystemImageList with the specified size
    /// </summary>
    /// <param name = "size">Size of System ImageList</param>
    public SystemImageList(SystemImageListSize size)
    {
      this.size = size;
      create();
    }

    /// <summary>
    ///   Clears up any resources associated with the SystemImageList
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   Clears up any resources associated with the SystemImageList
    ///   when disposing is true.
    /// </summary>
    /// <param name = "disposing">Whether the object is being disposed</param>
    public virtual void Dispose(bool disposing)
    {
      if (!disposed)
      {
        if (disposing)
        {
          if (iImageList != null)
          {
            Marshal.ReleaseComObject(iImageList);
          }
          iImageList = null;
        }
      }
      disposed = true;
    }

    /// <summary>
    ///   Finalise for SysImageList
    /// </summary>
    ~SystemImageList()
    {
      Dispose(false);
    }

    #endregion

    #region Implementation

    [DllImport("shell32.dll")]
    private static extern Int32 SHParseDisplayName(
      [MarshalAs(UnmanagedType.LPWStr)] String pszName, // Pointer to a zero-terminated wide string that
      // contains the display name  to parse. 
      IntPtr pbc, // Optional bind context that controls the parsing
      // operation. This parameter is normally set to NULL.
      out IntPtr ppidl, // Address of a pointer to a variable of type
      // ITEMIDLIST that receives the item  identifier list for the object.
      UInt32 sfgaoIn, // ULONG value that specifies the attributes to query.
      out UInt32 psfgaoOut);

    // Pointer to a ULONG. On return, those attributes
    // that are true for the  object and were requested in sfgaoIn will be set. 
    [DllImport("shell32")]
    private static extern IntPtr SHGetFileInfo(
      string pszPath,
      int dwFileAttributes,
      ref SHFILEINFO psfi,
      uint cbFileInfo,
      uint uFlags);

    [DllImport("shell32")]
    private static extern IntPtr SHGetFileInfo(
      IntPtr pidl,
      int dwFileAttributes,
      ref SHFILEINFO psfi,
      uint cbFileInfo,
      uint uFlags);

    [DllImport("user32.dll")]
    private static extern int DestroyIcon(IntPtr hIcon);

    [DllImport("kernel32")]
    private static extern int FormatMessage(
      int dwFlags,
      IntPtr lpSource,
      int dwMessageId,
      int dwLanguageId,
      string lpBuffer,
      uint nSize,
      int argumentsLong);

    [DllImport("kernel32")]
    private static extern int GetLastError();

    [DllImport("comctl32")]
    private static extern int ImageList_Draw(
      IntPtr hIml,
      int i,
      IntPtr hdcDst,
      int x,
      int y,
      int fStyle);

    [DllImport("comctl32")]
    private static extern int ImageList_DrawIndirect(
      ref IMAGELISTDRAWPARAMS pimldp);

    [DllImport("comctl32")]
    private static extern int ImageList_GetIconSize(
      IntPtr himl,
      ref int cx,
      ref int cy);

    [DllImport("comctl32")]
    private static extern IntPtr ImageList_GetIcon(
      IntPtr himl,
      int i,
      int flags);

    /// <summary>
    ///   SHGetImageList is not exported correctly in XP.  See KB316931
    ///   http://support.microsoft.com/default.aspx?scid=kb;EN-US;Q316931
    ///   Apparently (and hopefully) ordinal 727 isn't going to change.
    /// </summary>
    [DllImport("shell32.dll", EntryPoint = "#727")]
    private static extern int SHGetImageList(
      int iImageList,
      ref Guid riid,
      ref IImageList ppv
      );

    [DllImport("shell32.dll", EntryPoint = "#727")]
    private static extern int SHGetImageListHandle(
      int iImageList,
      ref Guid riid,
      ref IntPtr handle
      );

    /// <summary>
    ///   Determines if the system is running Windows XP
    ///   or above
    /// </summary>
    /// <returns>True if system is running XP or above, False otherwise</returns>
    private bool isXpOrAbove()
    {
      bool ret = false;
      if (Environment.OSVersion.Version.Major > 5)
      {
        ret = true;
      }
      else if ((Environment.OSVersion.Version.Major == 5) &&
               (Environment.OSVersion.Version.Minor >= 1))
      {
        ret = true;
      }
      return ret;
      //return false;
    }

    /// <summary>
    ///   Creates the SystemImageList
    /// </summary>
    private void create()
    {
      // forget last image list if any:
      hIml = IntPtr.Zero;

      if (isXpOrAbove())
      {
        // Get the System IImageList object from the Shell:
        Guid iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
        int ret = SHGetImageList(
          (int)size,
          ref iidImageList,
          ref iImageList
          );
        // the image list handle is the IUnknown pointer, but 
        // using Marshal.GetIUnknownForObject doesn't return
        // the right value.  It really doesn't hurt to make
        // a second call to get the handle:
        SHGetImageListHandle((int)size, ref iidImageList, ref hIml);
      }
      else
      {
        // Prepare flags:
        SHGetFileInfoConstants dwFlags = SHGetFileInfoConstants.SHGFI_USEFILEATTRIBUTES |
                                         SHGetFileInfoConstants.SHGFI_SYSICONINDEX;
        if (size == SystemImageListSize.SmallIcons)
        {
          dwFlags |= SHGetFileInfoConstants.SHGFI_SMALLICON;
        }
        // Get image list
        SHFILEINFO shfi = new SHFILEINFO();
        uint shfiSize = (uint)Marshal.SizeOf(shfi.GetType());

        // Call SHGetFileInfo to get the image list handle
        // using an arbitrary file:
        hIml = SHGetFileInfo(
          ".txt",
          FILE_ATTRIBUTE_NORMAL,
          ref shfi,
          shfiSize,
          (uint)dwFlags);
        Debug.Assert((hIml != IntPtr.Zero), "Failed to create Image List");
      }
    }

    #region Private ImageList COM Interop (XP)

    [ComImportAttribute]
    [GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    //helpstring("Image List"),
    private interface IImageList
    {
      [PreserveSig]
      int Add(
        IntPtr hbmImage,
        IntPtr hbmMask,
        ref int pi);

      [PreserveSig]
      int ReplaceIcon(
        int i,
        IntPtr hicon,
        ref int pi);

      [PreserveSig]
      int SetOverlayImage(
        int iImage,
        int iOverlay);

      [PreserveSig]
      int Replace(
        int i,
        IntPtr hbmImage,
        IntPtr hbmMask);

      [PreserveSig]
      int AddMasked(
        IntPtr hbmImage,
        int crMask,
        ref int pi);

      [PreserveSig]
      int Draw(
        ref IMAGELISTDRAWPARAMS pimldp);

      [PreserveSig]
      int Remove(
        int i);

      [PreserveSig]
      int GetIcon(
        int i,
        int flags,
        ref IntPtr picon);

      [PreserveSig]
      int GetImageInfo(
        int i,
        ref IMAGEINFO pImageInfo);

      [PreserveSig]
      int Copy(
        int iDst,
        IImageList punkSrc,
        int iSrc,
        int uFlags);

      [PreserveSig]
      int Merge(
        int i1,
        IImageList punk2,
        int i2,
        int dx,
        int dy,
        ref Guid riid,
        ref IntPtr ppv);

      [PreserveSig]
      int Clone(
        ref Guid riid,
        ref IntPtr ppv);

      [PreserveSig]
      int GetImageRect(
        int i,
        ref RECT prc);

      [PreserveSig]
      int GetIconSize(
        ref int cx,
        ref int cy);

      [PreserveSig]
      int SetIconSize(
        int cx,
        int cy);

      [PreserveSig]
      int GetImageCount(
        ref int pi);

      [PreserveSig]
      int SetImageCount(
        int uNewCount);

      [PreserveSig]
      int SetBkColor(
        int clrBk,
        ref int pclr);

      [PreserveSig]
      int GetBkColor(
        ref int pclr);

      [PreserveSig]
      int BeginDrag(
        int iTrack,
        int dxHotspot,
        int dyHotspot);

      [PreserveSig]
      int EndDrag();

      [PreserveSig]
      int DragEnter(
        IntPtr hwndLock,
        int x,
        int y);

      [PreserveSig]
      int DragLeave(
        IntPtr hwndLock);

      [PreserveSig]
      int DragMove(
        int x,
        int y);

      [PreserveSig]
      int SetDragCursorImage(
        ref IImageList punk,
        int iDrag,
        int dxHotspot,
        int dyHotspot);

      [PreserveSig]
      int DragShowNolock(
        int fShow);

      [PreserveSig]
      int GetDragImage(
        ref POINT ppt,
        ref POINT pptHotspot,
        ref Guid riid,
        ref IntPtr ppv);

      [PreserveSig]
      int GetItemFlags(
        int i,
        ref int dwFlags);

      [PreserveSig]
      int GetOverlayImage(
        int iOverlay,
        ref int piIndex);
    } ;

    #endregion

    #endregion

    #region Properties

    /// <summary>
    ///   Gets the hImageList handle
    /// </summary>
    public IntPtr Handle
    {
      get { return hIml; }
    }

    /// <summary>
    ///   Gets/sets the size of System Image List to retrieve.
    /// </summary>
    public SystemImageListSize ImageListSize
    {
      get { return size; }
      set
      {
        size = value;
        create();
      }
    }

    /// <summary>
    ///   Returns the size of the Image List Icons.
    /// </summary>
    public Size Size
    {
      get
      {
        int cx = 0;
        int cy = 0;

        if (iImageList == null)
          ImageList_GetIconSize(hIml, ref cx, ref cy);
        else
          iImageList.GetIconSize(ref cx, ref cy);

        Size sz = new Size(cx, cy);

        return sz;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    ///   Return the index of the icon for the specified file, always using 
    ///   the cached version where possible.
    /// </summary>
    /// <param name = "fileName">Filename to get icon for</param>
    /// <returns>Index of the icon</returns>
    public int IconIndex(string fileName)
    {
      return IconIndex(fileName, false);
    }

    /// <summary>
    ///   Returns the index of the icon for the specified file
    /// </summary>
    /// <param name = "fileName">Filename to get icon for</param>
    /// <param name = "forceLoadFromDisk">If True, then hit the disk to get the icon,
    ///   otherwise only hit the disk if no cached icon is available.</param>
    /// <returns>Index of the icon</returns>
    public int IconIndex(
      string fileName,
      bool forceLoadFromDisk)
    {
      return IconIndex(
        fileName,
        forceLoadFromDisk,
        ShellIconStateConstants.ShellIconStateNormal);
    }

    /// <summary>
    ///   Returns the index of the icon for the specified file
    /// </summary>
    /// <param name = "fileName">Filename to get icon for</param>
    /// <param name = "forceLoadFromDisk">If True, then hit the disk to get the icon,
    ///   otherwise only hit the disk if no cached icon is available.</param>
    /// <param name = "iconState">Flags specifying the state of the icon
    ///   returned.</param>
    /// <returns>Index of the icon</returns>
    public int IconIndex(
      string fileName,
      bool forceLoadFromDisk,
      ShellIconStateConstants iconState
      )
    {
      SHGetFileInfoConstants dwFlags = SHGetFileInfoConstants.SHGFI_SYSICONINDEX |
                                       SHGetFileInfoConstants.SHGFI_DISPLAYNAME | SHGetFileInfoConstants.SHGFI_TYPENAME;
      int dwAttr = 0;
      if (size == SystemImageListSize.SmallIcons)
      {
        dwFlags |= SHGetFileInfoConstants.SHGFI_SMALLICON;
      }

      // We can choose whether to access the disk or not. If you don't
      // hit the disk, you may get the wrong icon if the icon is
      // not cached. Also only works for files.
      if (!forceLoadFromDisk)
      {
        dwFlags |= SHGetFileInfoConstants.SHGFI_USEFILEATTRIBUTES;
        dwAttr = FILE_ATTRIBUTE_NORMAL;
      }
      else
      {
        dwAttr = 0;
      }

      // sFileSpec can be any file. You can specify a
      // file that does not exist and still get the
      // icon, for example sFileSpec = "C:\PANTS.DOC"
      SHFILEINFO shfi = new SHFILEINFO();
      uint shfiSize = (uint)Marshal.SizeOf(shfi.GetType());
      IntPtr retVal = SHGetFileInfo(
        fileName, dwAttr, ref shfi, shfiSize,
        ((uint)(dwFlags) | (uint)iconState));

      if (retVal == IntPtr.Zero)
      {
        // Seems to be a special folder, so we need to try with the PIDL
        dwFlags |= SHGetFileInfoConstants.SHGFI_PIDL;
        IntPtr pidl = IntPtr.Zero;
        uint iAttribute;

        // Convert the folder name to the PIDL and get it's icon
        SHParseDisplayName(fileName, IntPtr.Zero, out pidl, 0, out iAttribute);
        if (pidl == IntPtr.Zero)
        {
          return 0;
        }
        retVal = SHGetFileInfo(pidl, dwAttr, ref shfi, shfiSize, ((uint)(dwFlags) | (uint)iconState));
        Marshal.FreeCoTaskMem(pidl);
        if (retVal != IntPtr.Zero)
        {
          return shfi.iIcon;
        }
        return 0;
      }
      return shfi.iIcon;
    }

    /// <summary>
    ///   Draws an image
    /// </summary>
    /// <param name = "hdc">Device context to draw to</param>
    /// <param name = "index">Index of image to draw</param>
    /// <param name = "x">X Position to draw at</param>
    /// <param name = "y">Y Position to draw at</param>
    public void DrawImage(
      IntPtr hdc,
      int index,
      int x,
      int y
      )
    {
      DrawImage(hdc, index, x, y, ImageListDrawItemConstants.ILD_TRANSPARENT);
    }

    /// <summary>
    ///   Draws an image using the specified flags
    /// </summary>
    /// <param name = "hdc">Device context to draw to</param>
    /// <param name = "index">Index of image to draw</param>
    /// <param name = "x">X Position to draw at</param>
    /// <param name = "y">Y Position to draw at</param>
    /// <param name = "flags">Drawing flags</param>
    public void DrawImage(
      IntPtr hdc,
      int index,
      int x,
      int y,
      ImageListDrawItemConstants flags
      )
    {
      if (iImageList == null)
      {
        int ret = ImageList_Draw(
          hIml,
          index,
          hdc,
          x,
          y,
          (int)flags);
      }
      else
      {
        IMAGELISTDRAWPARAMS pimldp = new IMAGELISTDRAWPARAMS();
        pimldp.hdcDst = hdc;
        pimldp.cbSize = Marshal.SizeOf(pimldp.GetType());
        pimldp.i = index;
        pimldp.x = x;
        pimldp.y = y;
        pimldp.rgbFg = -1;
        pimldp.fStyle = (int)flags;
        iImageList.Draw(ref pimldp);
      }
    }

    /// <summary>
    ///   Draws an image using the specified flags and specifies
    ///   the size to clip to (or to stretch to if ILD_SCALE
    ///   is provided).
    /// </summary>
    /// <param name = "hdc">Device context to draw to</param>
    /// <param name = "index">Index of image to draw</param>
    /// <param name = "x">X Position to draw at</param>
    /// <param name = "y">Y Position to draw at</param>
    /// <param name = "flags">Drawing flags</param>
    /// <param name = "cx">Width to draw</param>
    /// <param name = "cy">Height to draw</param>
    public void DrawImage(
      IntPtr hdc,
      int index,
      int x,
      int y,
      ImageListDrawItemConstants flags,
      int cx,
      int cy
      )
    {
      IMAGELISTDRAWPARAMS pimldp = new IMAGELISTDRAWPARAMS();
      pimldp.hdcDst = hdc;
      pimldp.cbSize = Marshal.SizeOf(pimldp.GetType());
      pimldp.i = index;
      pimldp.x = x;
      pimldp.y = y;
      pimldp.cx = cx;
      pimldp.cy = cy;
      pimldp.fStyle = (int)flags;
      if (iImageList == null)
      {
        pimldp.himl = hIml;
        int ret = ImageList_DrawIndirect(ref pimldp);
      }
      else
      {
        iImageList.Draw(ref pimldp);
      }
    }


    #endregion
  }

  #endregion

  #region SystemImageListHelper class

  /// <summary>
  ///   Helper Methods for Connecting SystemImageList to Common Controls
  /// </summary>
  public class SystemImageListHelper
  {
    #region Constants

    private const int LVM_FIRST = 0x1000;
    private const int LVM_SETIMAGELIST = (LVM_FIRST + 3);

    private const int LVSIL_NORMAL = 0;
    private const int LVSIL_SMALL = 1;
    private const int LVSIL_STATE = 2;

    private const int TV_FIRST = 0x1100;
    private const int TVM_SETIMAGELIST = (TV_FIRST + 9);

    private const int TVSIL_NORMAL = 0;
    private const int TVSIL_STATE = 2;

    #endregion

    #region Implementation

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(
      IntPtr hWnd,
      int wMsg,
      IntPtr wParam,
      IntPtr lParam);

    #endregion

 }

  #endregion
}
