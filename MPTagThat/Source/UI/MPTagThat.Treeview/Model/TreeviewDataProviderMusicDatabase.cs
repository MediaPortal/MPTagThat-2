#region Copyright (C) 2022 Team MediaPortal
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

#region

using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.MusicDatabase;
using Prism.Ioc;
using Syncfusion.UI.Xaml.TreeView;
using Syncfusion.UI.Xaml.TreeView.Engine;
using Syncfusion.Windows.Shared;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

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
      Album,
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

    /// <summary>
    /// The string to hold the current selected Item
    /// </summary>
    private string _savedSelectedNode = null;

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

    /// <summary>
    /// Clear the Selected node
    /// </summary>
    public void ClearSelectedDatabaseNode()
    {
      _savedSelectedNode = null;
    }

    public void RequestRoot(TreeViewHelper helper, TreeViewNode parent)
    {
      log.Trace(">>>");
      var items = new List<TreeItem>();
      var albumArtistNode = CreateTreeNode(helper, "Album Artist", "AlbumArtist", true, "Artist");
      items.Add(albumArtistNode);
      var artistNode = CreateTreeNode(helper, "Artist", "Artist", true, "Artist");
      items.Add(artistNode);
      var albumNode = CreateTreeNode(helper, "Album", "Album", true, "Album");
      items.Add(albumNode);
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
      if ((musicItem.Path == "Album") && node.Level == 2)
      {
        node.HasChildNodes = false;
        return;
      }
      if ((musicItem.Path.StartsWith("Artist") || musicItem.Path.StartsWith("AlbumArtist")) && node.Level == 4)
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
      var init = false;

      List<string> result = null;
      if (musicItem.IsSpecialFolder)
      {
        init = _savedSelectedNode == null;

        // In case of Refreshing the Database Treeview, open the right Node
        var rootPath = musicItem.Path.ToLower();
        if (_savedSelectedNode != null)
        {
          if (_savedSelectedNode.Split('\\')[0].ToLower() != rootPath)
          {
            rootPath = _savedSelectedNode.Split('\\')[0].ToLower();
            var rootNode = helper.Model.TreeView.Nodes[0];
            foreach (var treeNode in rootNode.ChildNodes)
            {
              if ((treeNode.Content as TreeItem).Name == _savedSelectedNode.Split('\\')[0])
              {
                treeNode.IsExpanded = true;
                node = treeNode;
              }
              else
              {
                treeNode.IsExpanded = false;
              }
            }
          }
        }

        switch (rootPath)
        {
          case "artist":
            _rootFolder = RootFolder.Artist;
            result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetArtists();
            break;

          case "albumartist":
            _rootFolder = RootFolder.AlbumArtist;
            result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetAlbumArtists();
            break;

          case "album":
            _rootFolder = RootFolder.Album;
            result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetAlbums();
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
            else if (_rootFolder == RootFolder.Album)
            {
              type = "Album";
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

          // Artists and Albumrtists should have also shown the first letter in the tree view for easy navigation
          if (_rootFolder == RootFolder.AlbumArtist || _rootFolder == RootFolder.Artist)
          {
            var savedfirstLetter = "";
            TreeViewNode firstLetterNode = null;
            foreach (var item in nodesArray)
            {
              var firstLetter = item.Name.Substring(0, 1).ToUpperInvariant();
              if (firstLetter != savedfirstLetter)
              {
                savedfirstLetter = firstLetter;
                firstLetterNode = new TreeViewNode { Content = CreateTreeNode(helper, firstLetter, type, true, type) };
                firstLetterNode.HasChildNodes = true;
                node.ChildNodes.Add(firstLetterNode);
              }
              var itemPath = item.Path.Split('\\').ToList();
              itemPath.Insert(1, firstLetter);
              item.Path = string.Join("\\", itemPath);
              firstLetterNode.ChildNodes.Add(new TreeViewNode { Content = item });
            }
          }
          else
          {
            node.PopulateChildNodes(nodesArray);
          }
        }
      }

      if (!init)
      {
        if (musicItem.Item == null)
        {
          helper.Model.Cursor = Cursors.Arrow;
          return;
        }

        var childNode = node;
        var searchString = musicItem.Path.Split('\\');
        if (_savedSelectedNode != null && searchString.Length == 1)
        {
          if (_savedSelectedNode == musicItem.Path)
          {
            _savedSelectedNode = null;
            return;
          }
          searchString = _savedSelectedNode.Split('\\');
          childNode = FindChildNode(node, searchString);
        }
        else if (searchString.Length > 1)
        {
          _savedSelectedNode = musicItem.Path;
        }

        var genreLevel = 1;
        if (_rootFolder == RootFolder.Artist)
        {
          result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetArtistAlbums(searchString[2]);
        }
        else if (_rootFolder == RootFolder.AlbumArtist)
        {
          result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetAlbumArtistAlbums(searchString[2]);
        }
        else if (_rootFolder == RootFolder.Genre)
        {
          if (searchString.GetLength(0) == 2)
          {
            result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetGenreArtists(searchString[1]);
            genreLevel = 2;
          }
          else
          {
            result = ContainerLocator.Current.Resolve<IMusicDatabase>().GetGenreArtistAlbums(searchString[1], searchString[2]);
            genreLevel = 3;
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
              switch (genreLevel)
              {
                case 1:
                  type = "Genre";
                  break;
                case 2:
                  type = "Artist";
                  break;
                case 3:
                  type = "Album";
                  break;

              }
              value = item;
            }

            var path = $@"{(childNode.Content as TreeItem).Path}\{value}";
            var newNode = CreateTreeNode(helper, value, path, false, type);
            items.Add(newNode);
          }

          // Sort the Result, as we cannot rely on database sorting
          var nodesArray = items.ToArray();
          Array.Sort(nodesArray,
            (p1, p2) => string.Compare(p1.Name, p2.Name));

          childNode.PopulateChildNodes(nodesArray);
          childNode.IsExpanded = true;
          helper.Model.TreeView.BringIntoView(childNode, false, true, ScrollToPosition.MakeVisible);
        }
      }

      helper.Model.Cursor = Cursors.Arrow;
      log.Trace("<<<");
    }


    /// <summary>
    /// Find a Child Node in a Node Collection
    /// </summary>
    /// <param name="node"></param>
    /// <param name="Path"></param>
    /// <returns></returns>
    private TreeViewNode FindChildNode(TreeViewNode node, string[] searchString)
    {
      TreeViewNode childNode = null;

      if (searchString.Length > 1)
      {
        searchString = searchString.Skip(1).ToArray(); // shrink array
      }

      foreach (var child in node.ChildNodes)
      {
        if ((child.Content as TreeItem).Name == searchString[0])
        {
          childNode = child;
          if (searchString.Length > 1)
          {
            searchString = searchString.Skip(1).ToArray(); // shrink array
            childNode = FindChildNode(childNode, searchString);
          }
          break;
        }
      }

      if (childNode == null)
      {
        childNode = node;
      }

      return childNode;
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
      return new TreeItem(text, isSpecialFolder) { Path = path, Item = item, HasChildNodes = true };
    }

    #endregion
  }
}
