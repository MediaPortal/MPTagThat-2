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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using CommonServiceLocator;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Lyrics;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Dialogs.Models;
using Prism.Services.Dialogs;
using Syncfusion.Data.Extensions;

namespace MPTagThat.Dialogs.ViewModels
{
  public class LyricsSearchViewModel : DialogViewModelBase, ILyricsSearch
  {
    #region Variables

    private readonly Options _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;
    private object _lock = new object();

    private Queue _lyricsQueue;
    private ManualResetEvent _eventStopThread;
    private Thread _lyricControllerThread;
    private List<string> _sitesToSearch;

    private List<SongData> _songs;
    private readonly string[] _strippedPrefixStrings = { "the ", "les " };
    private readonly string[] _titleBrackets = { "{}", "[]", "()" };


    #region Delegates

    public delegate void DelegateLyricFound(string artist, string title, string site, int row, string lyric);

    public delegate void DelegateLyricNotFound(string artist, string title, string site, int row, string message);

    public delegate void DelegateStringUpdate(string message, string site);

    public delegate void DelegateThreadException(string s);

    public delegate void DelegateThreadFinished(string message, string site);

    public DelegateLyricFound _delegateLyricFound;
    public DelegateLyricNotFound _delegateLyricNotFound;
    public DelegateStringUpdate _delegateStringUpdate;
    public DelegateThreadException _delegateThreadException;
    public DelegateThreadFinished _delegateThreadFinished;

    #endregion

    #endregion

    #region Properties

    public Brush Background => (Brush)new BrushConverter().ConvertFromString(_options.MainSettings.BackGround);
    
    /// <summary>
    /// Binding for the Albums found
    /// </summary>
    private ObservableCollection<LyricsModel> _lyrics = new ObservableCollection<LyricsModel>();
    public ObservableCollection<LyricsModel> Lyrics
    {
      get => _lyrics;
      set => SetProperty(ref _lyrics, value);
    }

    #endregion
    
    #region ctor

    public LyricsSearchViewModel()
    {
      BindingOperations.EnableCollectionSynchronization(Lyrics, _lock);
      _eventStopThread = new ManualResetEvent(false);
      _lyricsQueue = new Queue();
      _sitesToSearch = new List<string>();

      // initialize delegates
      _delegateLyricFound = LyricFoundMethod;
      _delegateLyricNotFound = LyricNotFoundMethod;
      //_delegateThreadFinished = ThreadFinishedMethod;
      //_delegateThreadException = ThreadExceptionMethod;

    }

    #endregion

    #region Private Methods

    private void DoSearchLyrics()
    {
      var row = 0;
      foreach (var song in _songs)
      {
        var switchedArtist = SwitchArtist(song.Artist);
        var lyricsModel = new LyricsModel {ArtistAndTitle = $"{switchedArtist} - {song.Title}", Site = "", Lyric = "", Row = row};
        Lyrics.Add(lyricsModel);
        row++;
        string[] lyricId = new string[] { song.Artist, song.Title };
        _lyricsQueue.Enqueue(lyricId);
      }

      _sitesToSearch.Add("Lyrics007");
      _sitesToSearch.Add("Lyricsmode");
      _sitesToSearch.Add("LyricsOnDemand");
      var lyricsController = new LyricsController(this, _eventStopThread, _sitesToSearch.ToArray(), true, false, "", "")
      {
        NrOfLyricsToSearch = _lyricsQueue.Count
      };

      ThreadStart runLyricController = delegate { lyricsController.Run(); };
      _lyricControllerThread = new Thread(runLyricController);
      _lyricControllerThread.Start();

      lyricsController.StopSearches = false;

      var lyr = (string[])_lyricsQueue.Dequeue();
      lyr[0] = SwitchArtist(lyr[0]);
      lyricsController.AddNewLyricSearch(lyr[0], TrimTitle(lyr[1]), GetStrippedPrefixArtist(lyr[0], _strippedPrefixStrings),
        0);

    }

    private string GetStrippedPrefixArtist(string artist, string[] strippedPrefixStringArray)
    {
      foreach (string s in strippedPrefixStringArray)
      {
        if (artist.Trim().ToLowerInvariant().StartsWith(s))
        {
          artist = artist.Substring(s.Length);
          break;
        }
      }
      return artist;
    }

    /// <summary>
    /// Switches the Artist, if it is separated with a "colon"
    /// </summary>
    /// <param name="artist"></param>
    /// <returns></returns>
    private string SwitchArtist(string artist)
    {
      int iPos = artist.IndexOf(',');
      if (iPos > 0)
      {
        artist = $"{artist.Substring(iPos + 2)} {artist.Substring(0, iPos)}";
      }
      return artist;
    }

    /// <summary>
    /// Cleans the title before submitting for Lyrics search
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    private string TrimTitle(string title)
    {
      foreach (string s in _titleBrackets)
      {
        if (title.Trim().EndsWith(s.Substring(1, 1)))
        {
          var startPos = title.LastIndexOf(s.Substring(0, 1), StringComparison.Ordinal);
          if (startPos > 0)
          {
            title = title.Substring(0, startPos).Trim();
          }
          break;
        }
      }
      return title;
    }

    private void LyricFoundMethod(string artist, string title, string site, int row, string lyric)
    {
      var lyricsModel = new LyricsModel {ArtistAndTitle = $"{artist} - {title}", Site = site, Lyric = lyric, Row = row};
      var found = false;
      for (var i = 0; i < Lyrics.Count; i++)
      {
        if (Lyrics[i].ArtistAndTitle == lyricsModel.ArtistAndTitle && Lyrics[i].Site.Length == 0 && Lyrics[i].Row == lyricsModel.Row)
        {
          Lyrics[i].Site = lyricsModel.Site;
          Lyrics[i].Lyric = lyricsModel.Lyric;
          found = true;
          break;
        }
      }

      if (!found)
      {
        Lyrics.Add(lyricsModel);
      }
    }

    private void LyricNotFoundMethod(string artist, string title, string site, int row, string message)
    {
      Console.WriteLine($"No Lyrics on {site} for Row {row}");
    }

    #endregion

    #region Interface Implementation

    public object[] UpdateString { get; set; }
    public object[] UpdateStatus { get; set; }
    public Object[] LyricFound
    {
      set
      {
        try
        {
          _delegateLyricFound.Invoke((string)value[0],(string)value[1],(string)value[2], (int)value[3],(string)value[4]); 
        }
        catch (InvalidOperationException) { }
      }
    }

    public Object[] LyricNotFound
    {
      set
      {
        try
        {
          _delegateLyricNotFound.Invoke((string)value[0],(string)value[1],(string)value[2], (int)value[3], (string)value[4]);
        }
        catch (InvalidOperationException) { }
      }
    }

    public object[] ThreadFinished { get; set; }
    public string ThreadException { get; set; }

    #endregion

    #region Overrides

    public override void OnDialogOpened(IDialogParameters parameters)
    {
      _songs = parameters.GetValue<List<SongData>>("songs");
      DoSearchLyrics();
    }

    public override void CloseDialog(string parameter)
    {
      ButtonResult result = ButtonResult.None;

      if (parameter?.ToLower() == "true")
        result = ButtonResult.OK;
      else if (parameter?.ToLower() == "false")
        result = ButtonResult.Cancel;

      CloseDialogWindow(new DialogResult(result));
    }


    #endregion

  }
}
