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

using LiteDB;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.MusicDatabase;
using MPTagThat.Core.Utils;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Controls.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using WPFLocalizeExtension.Engine;
using Action = MPTagThat.Core.Common.Action;
using Application = System.Windows.Forms.Application;

#endregion

namespace MPTagThat.TagChecker.ViewModels
{
  public class TagCheckerViewModel : BindableBase
  {
    #region Variables

    private readonly IRegionManager _regionManager;
    private readonly IDialogService _dialogService;
    private readonly NLogLogger log;
    private string _currentItemType = "artists";

    #endregion

    #region ctor

    public TagCheckerViewModel(IRegionManager regionManager, IDialogService dialogService)
    {
      _regionManager = regionManager;
      _dialogService = dialogService;
      log = ContainerLocator.Current.Resolve<ILogger>()?.GetLogger;
      log.Trace(">>>");
      EventSystem.Subscribe<GenericEvent>(OnMessageReceived);
      log.Trace("<<<");
    }


    #endregion


    #region Properties

    /// <summary>
    /// Reference to Item Grid. Set from code behind
    /// </summary>
    public SfDataGrid ItemsGrid { get; set; }

    /// <summary>
    /// The Items in the Grid
    /// </summary>

    private ObservableCollection<TagCheckerData> _items = new ObservableCollection<TagCheckerData>();
    public ObservableCollection<TagCheckerData> Items
    {
      get => _items;
      set => SetProperty(ref _items, value);
    }

    /// <summary>
    /// The selected Items in the Grid
    /// </summary>
    private ObservableCollection<object> _selectedItems = new ObservableCollection<object>();
    public ObservableCollection<object> SelectedItems
    {
      get => _selectedItems;
      set => SetProperty(ref _selectedItems, value);
    }

    /// <summary>
    /// Binding for Wait Cursor
    /// </summary>
    private bool _isBusy;
    public bool IsBusy
    {
      get => _isBusy;
      set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// The Binding for the Artist Auto Complete
    /// </summary>
    private ObservableCollection<string> _autoCompleteArtists = new ObservableCollection<string>();
    public ObservableCollection<string> AutoCompleteArtists
    {
      get => _autoCompleteArtists;
      set => SetProperty(ref _autoCompleteArtists, value);
    }

    #endregion

    #region Commands

    #region Common Methods

    private void ApplySelectedItems()
    {
      var query = "dbview:";
      if (_currentItemType.ToLower() == "artists")
      {
        query += $"Artist = \"%original%\" OR AlbumArtist = \"%original%\"";
      }
      else
      {
        query += $"Album = \"%original%\"";
      }

      IsBusy = true;

      var items = SelectedItems.Cast<TagCheckerData>().ToList();
      foreach (var item in items)
      {
        Application.DoEvents();
        var errors = false;
        var dbQuery = query.Replace("%original%", item.OriginalItem);
        var result = ContainerLocator.Current.Resolve<IMusicDatabase>().ExecuteQuery(dbQuery);

        if (result != null)
        {
          log.Debug($"Query returned {result.Count} songs");
          foreach (var songitem in result)
          {
            Application.DoEvents();
            var song = Song.Create(songitem.FullFileName);
            if (song != null)
            {
              if (_currentItemType.ToLower() == "artists")
              {
                if (song.Artist.Trim() == item.OriginalItem.Trim())
                {
                  song.Artist = item.ChangedItem;
                }
                if (song.AlbumArtist.Trim() == item.OriginalItem.Trim())
                {
                  song.AlbumArtist = item.ChangedItem;
                }
                song.Changed = true;
              }
              var errormsg = "";
              if (Song.SaveFile(song, ref errormsg))
              {
                ContainerLocator.Current.Resolve<IMusicDatabase>().UpdateSong(song, song.FullFileName);
              }
              else
              {
                log.Info($"Update of song {song.FullFileName} failed with error {errormsg}");
              }
            }
            else
            {
              errors = true;
            }
          }
        }

        if (!errors)
        {
          item.Status = ItemStatus.Applied;
          item.Changed = false;
          ContainerLocator.Current.Resolve<IMusicDatabase>().UpdateTagCheckerItem(item, _currentItemType);
        }
      }
      IsBusy = false;
    }

    /// <summary>
    /// Toggle the "Ignore" Status Flag in the Items Database
    /// </summary>
    private void ToggleIgnoreSelectedItems()
    {
      IsBusy = true;
      // Set the UpdateProperty to false
      _items.Select(p => { p.UpdateChangedProperty = false; return p; }).ToList();

      var items = SelectedItems.Cast<TagCheckerData>().ToList();
      foreach (var item in items)
      {
        Application.DoEvents();
        if (item.Status == ItemStatus.Ignored)
        {
          item.Status = ItemStatus.NoMatch;
        }
        else
        {
          item.Status = ItemStatus.Ignored;
        }
      }

      ContainerLocator.Current.Resolve<IMusicDatabase>().UpdateTagCheckerDatabase(ref items, _currentItemType);

      // Set the UpdateProperty to true
      _items.Select(p => { p.UpdateChangedProperty = true; return p; }).ToList();
      IsBusy = false;
    }

    /// <summary>
    /// Callback from the View when a Text is changed, it should check the database for autocompletion
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void TextChanged(object sender, TextCompositionEventArgs e)
    {
      // Auto Complete Support for Artist / AlbumArtist
      var autoComplete = (SfTextBoxExt)sender;
      if (autoComplete.Text.Length > 1)
      {
        var filter = autoComplete.Text + e.Text;

        AutoCompleteArtists.Clear();
        AutoCompleteArtists.AddRange(ContainerLocator.Current.Resolve<IMusicDatabase>()?.SearchAutocompleteArtists(filter));
        autoComplete.AutoCompleteSource = AutoCompleteArtists;
      }
    }

    #endregion

    #region Check Artists

    /// <summary>
    /// Get the Entries from the Tag Checker collection
    /// </summary>
    private void CheckArtists()
    {
      ItemsGrid.Columns[2].HeaderText = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "tagChecker_OriginalArtist", LocalizeDictionary.Instance.Culture).ToString();
      ItemsGrid.Columns[3].HeaderText = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "tagChecker_ChangedArtist", LocalizeDictionary.Instance.Culture).ToString();

      // Set the initial filter to hide ignored and already applied changes
      ItemsGrid.Columns[1].FilterPredicates.Add(new FilterPredicate() { FilterType = FilterType.NotEquals, FilterValue = "Ignored" });
      ItemsGrid.Columns[1].FilterPredicates.Add(new FilterPredicate() { FilterType = FilterType.NotEquals, FilterValue = "Applied" });

      IsBusy = true;
      Application.DoEvents();
      _currentItemType = "artists";
      _items.Clear();
      var items = ContainerLocator.Current.Resolve<IMusicDatabase>().GetTagCheckerItems(_currentItemType);
      _items.AddRange(items);

      // Set the UpdateProperty to true
      _items.Select(p => { p.UpdateChangedProperty = true; return p; }).ToList();
      IsBusy = false;
    }

    /// <summary>
    /// Add Unique Album Artists and Artists fom the Database to the Grid
    /// </summary>
    private void ScanDatabase()
    {
      IsBusy = true;
      _items.Clear();
      var artists = ContainerLocator.Current.Resolve<IMusicDatabase>().GetArtists();
      var albumArtists = ContainerLocator.Current.Resolve<IMusicDatabase>().GetAlbumArtists();

      // Union the 2 lists
      var result = artists.Union(albumArtists).ToList();

      var i = 0;
      foreach (var artist in result)
      {
        i++;
        if (i % 100 == 0)
        {
          Application.DoEvents();
        }
        var item = new TagCheckerData { OriginalItem = artist };
        CheckArtistInMusicBrainz(ref item);
        _items.Add(item);
      }

      ContainerLocator.Current.Resolve<IMusicDatabase>().AddItemsToTagCheckerDatabase(ref _items, _currentItemType);
      IsBusy = false;
    }

    /// <summary>
    /// Check the Item against the MusicBrainz Database
    /// </summary>
    /// <param name="item"></param>
    private void CheckArtistInMusicBrainz(ref TagCheckerData item)
    {

      //TODO: Add Selectzion to the UI
      var artistType = "SortName";
      var ignoreCase = true;

      var searchList = new List<string>() { " feat ", " feat. ", " ft ", " ft. ", " vs ", " vs. " };
      var foundArtists = new List<string>();
      var artistSplit = new List<string>();
      var sqlStmt = "select {0} from artist where name like '{1}' or sortname like '{1}'";

      var artist = item.OriginalItem.Trim();

      // Do we have Artists separated with a semi colon?
      if (artist.IndexOf(";", StringComparison.InvariantCulture) > 1)
      {
        artistSplit.AddRange(artist.Split(';'));
      }
      else if (artist.IndexOf("&", StringComparison.InvariantCulture) > 1)
      {
        // Do we have Artists separated with an ampersand?
        // In this case add the original string to match something like "Sony & Cher"
        // And then also split it to get individual names
        artistSplit.Add(artist);
        artistSplit.AddRange(artist.Split('&'));
      }
      else
      {
        // Does the artist contain a string specified in the searchList above?
        foreach (var searchstring in searchList)
        {
          var index = artist.IndexOf(searchstring, StringComparison.InvariantCultureIgnoreCase);
          if (index > 0)
          {
            artistSplit.Add(artist.Substring(0, index));
            artistSplit.Add(artist.Substring(index + searchstring.Length));
            break;
          }
        }
      }

      // If none of the above matched, use the original content of the field
      if (artistSplit.Count == 0)
      {
        artistSplit.Add(artist);
      }

      // Lookup the artist in the database
      bool foundSomeArtists = false;
      foreach (var searchArtist in artistSplit)
      {
        bool multiArtists = false;
        if (searchArtist.Contains("&"))
        {
          multiArtists = true;
        }
        var tmpArtist = Util.RemoveInvalidChars(searchArtist);
        var sql = string.Format(sqlStmt, artistType, tmpArtist);
        var returnedArtists = ContainerLocator.Current.Resolve<IMusicDatabase>().GetAutoCorrectArtists(sql);
        if (returnedArtists.Count > 0)
        {
          foundArtists.Add(returnedArtists[0]);
          foundSomeArtists = true;
          if (item.Status == ItemStatus.NoMatch)
          {
            item.Status = ItemStatus.FullMatch;
          }
          // Don't search any further on the splitted values, if we have a multi artist search
          if (multiArtists)
          {
            break;
          }
        }
        else
        {
          if (artistSplit.Count > 1)
          {
            item.Status = ItemStatus.PartialMatch;
          }
          // Only add non multi artists here
          if (!multiArtists)
          {
            foundArtists.Add(searchArtist);
          }
        }
      }

      if (foundSomeArtists)
      {
        if (item.Status != ItemStatus.PartialMatch)
        {
          item.Status = ItemStatus.FullMatch;
        }
        var result = "";
        for (int i = 0; i < foundArtists.Count; i++)
        {
          result += foundArtists[i];
          if (i < foundArtists.Count - 1)
          {
            result += ";";
          }
        }

        item.ChangedItem = result;
        if (item.OriginalItem == item.ChangedItem)
        {
          item.Changed = false;
        }
        else
        {
          if (!ignoreCase && item.OriginalItem.ToLower() != item.ChangedItem.ToLower())
          {
            item.Status = ItemStatus.FullMatchChanged;
            item.Changed = true;
          }
        }
      }
    }

    #endregion

    #endregion

    #region Event Handling

    /// <summary>
    /// General Message Handler
    /// </summary>
    /// <param name="msg"></param>
    private void OnMessageReceived(GenericEvent msg)
    {
      switch (msg.Action.ToLower())
      {
        case "command":

          var command = (Action.ActionType)msg.MessageData["command"];
          if (command == Action.ActionType.CHECKARTISTS)
          {
            CheckArtists();
            return;
          }
          else if (command == Action.ActionType.SCANCHECKDATABASE)
          {
            ScanDatabase();
            return;
          }
          else if (command == Action.ActionType.APPLYSELECTEDTAGCHECKERITEM)
          {
            ApplySelectedItems();
            return;
          }
          else if (command == Action.ActionType.TOGGLEIGNORESELECTEDTAGCHECKERITEM)
          {
            ToggleIgnoreSelectedItems();
            return;
          }

          break;
      }
    }

    #endregion

  }
}
