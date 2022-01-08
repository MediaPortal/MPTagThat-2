﻿#region Copyright (C) 2020 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.MusicDatabase;
using MPTagThat.Dialogs.Views;
using Prism.Ioc;
using Syncfusion.UI.Xaml.TreeView.Engine;
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

    #endregion

    #region ITreeViewFolderBrowserDataProvider Members

    public void QueryContextMenuItems(TreeViewHelper helper, TreeViewNode node) { }

    /// <summary>
    /// Create the Root Collection when building the Tree View
    /// </summary>
    /// <param name="helper"></param>
    public virtual void CreateRootNode(TreeViewHelper helper)
    {
      log.Trace(">>>");
      var musicDBNode = CreateTreeNode(helper, "Music Database", "Root", true, "Root");

      musicDBNode.IsRoot = true;
      helper.Model.Nodes.Add(musicDBNode);
      log.Trace("<<<");
    }

    public void RequestRoot(TreeViewHelper helper, TreeViewNode parent)
    {
      log.Trace(">>>");
      var items = new List<TreeItem>();
      var albumArtistNode = CreateTreeNode(helper, "Album Artist", "AlbumArtist", true, "Artist");
      items.Add(albumArtistNode);
      var artistNode = CreateTreeNode(helper, "Artist", "Artist", true, "Artist");
      items.Add(artistNode);
      var genreNode = CreateTreeNode(helper, "Genre", "Genre", true, "Genre");
      items.Add(genreNode);
      parent.PopulateChildNodes(items);
      log.Trace("<<<");
    }

    public void RequestSubDirs(TreeViewHelper helper, TreeViewNode node)
    {
      log.Trace(">>>");

      var musicItem = node.Content as TreeItem;
      if (musicItem == null)
      {
        return;
      }

      if (musicItem.Path.IsNullOrWhiteSpace())
      {
        return;
      }

      // Check on the Level of the node, so that we don't allow infinite expansions
      if ((musicItem.Path.StartsWith("Artist") || musicItem.Path.StartsWith("AlbumArtist")) && node.Level == 3)
      {
        node.HasChildNodes = false;
        return;
      } 
      if (musicItem.Path.StartsWith("Genre") && node.Level == 4)
      {
        node.HasChildNodes = false;
        return;
      }

      helper.Model.Cursor = Cursors.Wait;

      List<string> result = null;
      if (musicItem.IsSpecialFolder)
      {
        switch (musicItem.Path.ToLower())
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

        if (result != null)
        {
          var items = new List<TreeItem>();
          var type = "";
          foreach (var item in result)
          {
            var value = "";
            if (_rootFolder == RootFolder.Artist)
            {
              type = "Artist";
              value = item;
              
            }
            else if (_rootFolder == RootFolder.AlbumArtist)
            {
              type = "AlbumArtist";
              value = item;
            }
            else
            {
              type = "Genre";
              value = item;
            }
            var path = $@"{type}\{value}";
            var newNode = CreateTreeNode(helper, value, path, false, type);
            items.Add(newNode);
          }

          // Sort the Result, as we cannot rely on database sorting
          var nodesArray = items.ToArray();
          Array.Sort(nodesArray,
            (p1, p2) => string.Compare(p1.Name, p2.Name));

          node.PopulateChildNodes(nodesArray);
        }
      }
      else
      {
        if (musicItem.Item == null)
        {
          helper.Model.Cursor = Cursors.Arrow;
          return;
        }

        var searchString = musicItem.Path.Split('\\');

        if (_rootFolder == RootFolder.Artist)
        {
          result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetArtistAlbums(searchString[1]);
        }
        else if (_rootFolder == RootFolder.AlbumArtist)
        {
          result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetAlbumArtistAlbums(searchString[1]);
        }
        else if (_rootFolder == RootFolder.Genre)
        {
          if (searchString.GetLength(0) == 2)
          {
            result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetGenreArtists(searchString[1]);
          }
          else
          {
            result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetGenreArtistAlbums(searchString[1], searchString[2]);
          }
        }

        if (result != null)
        {
          var items = new List<TreeItem>();
          var type = "";
          foreach (var item in result)
          {
            var value = "";
            if (_rootFolder == RootFolder.Artist || _rootFolder == RootFolder.AlbumArtist)
            {
              type = "Album";
              value = item;
            }
            else
            {
              type = "Genre";
              value = item;
            }

            var path = $@"{musicItem.Path}\{value}";
            var newNode = CreateTreeNode(helper, value, path, false, type);
            items.Add(newNode);
          }

          // Sort the Result, as we cannot rely on database sorting
          var nodesArray = items.ToArray();
          Array.Sort(nodesArray,
            (p1, p2) => string.Compare(p1.Name, p2.Name));

          node.PopulateChildNodes(nodesArray);
        }
      }

      helper.Model.Cursor = Cursors.Arrow;
      log.Trace("<<<");
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
      return new TreeItem(text, isSpecialFolder) { Path = path, Item = item, HasChildNodes = true};
    }

    #endregion
  }
}
