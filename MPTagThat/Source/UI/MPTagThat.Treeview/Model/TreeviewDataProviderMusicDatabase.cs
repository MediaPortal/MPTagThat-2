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

using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.MusicDatabase;
using MPTagThat.Core.Services.MusicDatabase.Indexes;
using MPTagThat.Dialogs.Views;
using Prism.Ioc;
using Syncfusion.Windows.Shared;

#endregion

namespace MPTagThat.Treeview.Model
{
  /// <summary>
  /// Data Provider for supporting Music Database in Tree views
  /// </summary>
  public class TreeviewDataProviderMusicDatabase : ITreeviewDataProvider
  {
    #region Enums

    private enum RootFolder
    {
      None,
      Artist,
      AlbumArtist,
      Genre
    }

    #endregion

    #region Variables

    private readonly NLogLogger log = ContainerLocator.Current.Resolve<ILogger>()?.GetLogger;

    /// <summary>
    ///   drive tree node (My computer) root collection
    /// </summary>
    private ObservableCollection<TreeItem> _rootCollection = new ObservableCollection<TreeItem>();

    /// <summary>
    /// The Current Root Folder
    /// </summary>
    private RootFolder _rootFolder = RootFolder.None;

    private NotificationView _notificationView;

    #endregion

    #region ITreeViewFolderBrowserDataProvider Members

    public void QueryContextMenuItems(TreeViewHelper helper, TreeItem node) { }

    public void RequestRoot(TreeViewHelper helper)
    {
      log.Trace(">>>");
      var musicDBNode = CreateTreeNode(helper, "Music Database", "Root", true, "Root");
      helper.TreeView.Nodes.Add(musicDBNode);
      var artistNode = CreateTreeNode(helper, "Artist", "Artist", true, "Artist");
      musicDBNode.Nodes.Add(artistNode);
      var albumArtistNode = CreateTreeNode(helper, "Album Artist", "AlbumArtist", true, "Artist");
      musicDBNode.Nodes.Add(albumArtistNode);
      var genreNode = CreateTreeNode(helper, "Genre", "Genre", true, "Genre");
      musicDBNode.Nodes.Add(genreNode);
      log.Trace("<<<");
    }

    public void RequestSubDirs(TreeViewHelper helper, TreeItem parent)
    {
      log.Trace(">>>");
      if (parent.Path.IsNullOrWhiteSpace())
      {
        return;
      }

      helper.TreeView.Cursor = Cursors.Wait;

      IEnumerable result = null;
      if (parent.IsSpecialFolder)
      {
        if (!ContainerLocator.Current.Resolve<IMusicDatabase>().DatabaseEngineStarted)
        {
          _notificationView = new NotificationView();
          _notificationView.Show();
        }
        switch (parent.Path.ToLower())
        {
          case "artist":
            _rootFolder = RootFolder.Artist;
            result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetArtists();
            break;

          case "albumartist":
            _rootFolder = RootFolder.AlbumArtist;
            result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetAlbumArtists();
            break;

          case "genre":
            _rootFolder = RootFolder.Genre;
            result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetGenres();
            break;
        }

        if (_notificationView != null)
        {
          _notificationView.Close();
        }

        if (result != null)
        {
          var type = "";
          foreach (var item in result)
          {
            var value = "";
            if (_rootFolder == RootFolder.Artist)
            {
              type = "Artist";
              value = (item as DistinctResult)?.Name;
            }
            else if (_rootFolder == RootFolder.AlbumArtist)
            {
              type = "AlbumArtist";
              value = (item as DistinctResult)?.Name;
            }
            else
            {
              type = "Genre";
              value = (item as DistinctResult)?.Genre;
            }

            var newNode = CreateTreeNode(helper, value, value, false, type);
            parent.Nodes.Add(newNode);
          }
        }
      }
      else
      {
        var isGenreArtistLevel = true;
        if (parent.Item == null)
        {
          helper.TreeView.Cursor = Cursors.Arrow;
          return;
        }

        if (_rootFolder == RootFolder.Artist)
        {
          result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetArtistAlbums(parent.Path);
        }
        else if (_rootFolder == RootFolder.AlbumArtist)
        {
          result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetAlbumArtistAlbums(parent.Path);
        }
        else if (_rootFolder == RootFolder.Genre)
        {
          string[] searchString = parent.Path.Split('\\');
          if (searchString.GetLength(0) == 1)
          {
            result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetGenreArtists(parent.Path);
          }
          else
          {
            isGenreArtistLevel = false;
            result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetGenreArtistAlbums(searchString[0], searchString[1]);
          }
        }

        if (result != null)
        {
          var type = "";
          foreach (var item in result)
          {
            var value = "";
            var path = "";
            if (_rootFolder == RootFolder.Artist || _rootFolder == RootFolder.AlbumArtist)
            {
              type = "Album";
              value = (item as DistinctResult)?.Album;
              path = value;
            }
            else
            {
              type = "Genre";
              if (isGenreArtistLevel)
              {
                value = (item as DistinctResult)?.Name;
                path = parent.Path + "\\" + value;
              }
              else
              {
                value = (item as DistinctResult)?.Album;
              }
            }

            var newNode = CreateTreeNode(helper, value, path, false, type);
            parent.Nodes.Add(newNode);
          }
        }
      }

      helper.TreeView.Cursor = Cursors.Arrow;
      log.Trace("<<<");
    }

    public ObservableCollection<TreeItem> RequestDriveCollection(TreeViewHelper helper, bool isNetwork)
    {
      return _rootCollection;
    }

    #endregion

    #region Internal Interface

    /// <summary>
    ///   Creates a new node and assigns a icon
    /// </summary>
    /// <param name = "helper"></param>
    /// <param name = "text"></param>
    /// <param name = "path"></param>
    /// <param name="isSpecialFolder"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    protected virtual TreeItem CreateTreeNode(TreeViewHelper helper, string text, string path,
      bool isSpecialFolder, object item)
    {
      return new TreeItem(text, isSpecialFolder) { Path = path, Item = item };
    }

    #endregion
  }
}
