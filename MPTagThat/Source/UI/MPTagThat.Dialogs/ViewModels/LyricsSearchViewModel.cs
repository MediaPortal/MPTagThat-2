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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CommonServiceLocator;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
using MPTagThat.Core.Lyrics;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.Dialogs.Models;
using Prism.Services.Dialogs;
using Syncfusion.UI.Xaml.Utility;
using WPFLocalizeExtension.Engine;

#endregion

namespace MPTagThat.Dialogs.ViewModels
{
  public class LyricsSearchViewModel : DialogViewModelBase, ILyricsSearch
  {
    #region Variables

    private readonly NLogLogger log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger)?.GetLogger;
    private readonly Options _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;
    private object _lock = new object();

    private const int NrOfCurrentSearchesAllowed = 6;
    private BackgroundWorker _bgWorkerLyrics;
    private LyricsController _lc;

    private Queue _lyricsQueue;
    private ManualResetEvent _eventStopThread;
    private Thread _lyricControllerThread;

    private List<SongData> _songs;

    private readonly string[] _strippedPrefixStrings = { "the ", "les ", "der " };
    private readonly string[] _titleBrackets = { "{}", "[]", "()" };

    // Strings that should be searched and replaced in final lyrics
    private readonly string _findStr = "<br>,<br />,<i>,<a>";
    private readonly string _replaceStr = " , , , ";

    private string _statusMsgTmp;

    #region Delegates

    public delegate void DelegateLyricFound(string artist, string title, string site, int row, string lyric);

    public delegate void DelegateLyricNotFound(string artist, string title, string site, int row, string message);

    public delegate void DelegateStatusUpdate(int nrOfLyricsToSearch, int nrOfLyricsSearched, int nrOfLyricsFound,int nrOfLyricsNotFound);

    public delegate void DelegateThreadException(string exception);

    public delegate void DelegateThreadFinished(string message, string site);

    public DelegateLyricFound _delegateLyricFound;
    public DelegateLyricNotFound _delegateLyricNotFound;
    public DelegateStatusUpdate _delegateStatusUpdate;
    public DelegateThreadException _delegateThreadException;
    public DelegateThreadFinished _delegateThreadFinished;

    #endregion

    #endregion

    #region Properties

    public Brush Background => (Brush)new BrushConverter().ConvertFromString(_options.MainSettings.BackGround);

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
    /// Binding for the Lyrics found
    /// </summary>
    private ObservableCollection<LyricsModel> _lyrics = new ObservableCollection<LyricsModel>();
    public ObservableCollection<LyricsModel> Lyrics
    {
      get => _lyrics;
      set => SetProperty(ref _lyrics, value);
    }

    /// <summary>
    /// The Binding for the Lyrics Search Sites
    /// </summary>
    private ObservableCollection<string> _lyricsSearchSites = new ObservableCollection<string>();
    public ObservableCollection<string> LyricsSearchSites
    {
      get => _lyricsSearchSites;
      set => SetProperty(ref _lyricsSearchSites, value);
    }

    /// <summary>
    /// The Selected Lyrics Search sites
    /// </summary>
    private ObservableCollection<string> _selectedLyricsSearchSites = new ObservableCollection<string>();
    public ObservableCollection<string> SelectedLyricsSearchSites
    {
      get => _selectedLyricsSearchSites;
      set => SetProperty(ref _selectedLyricsSearchSites, value);
    }

    /// <summary>
    /// The Binding for the Status Message
    /// </summary>
    private string _statusMsg;
    public string StatusMsg
    {
      get => _statusMsg;
      set => SetProperty(ref _statusMsg, value);
    }

    #endregion

    #region ctor

    public LyricsSearchViewModel()
    {
      Title = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "lyricsSearch_Title",
        LocalizeDictionary.Instance.Culture).ToString();

      BindingOperations.EnableCollectionSynchronization(Lyrics, _lock);
      
      // initialize delegates
      _delegateLyricFound = LyricFoundMethod;
      _delegateLyricNotFound = LyricNotFoundMethod;
      _delegateThreadFinished = ThreadFinishedMethod;
      _delegateThreadException = ThreadExceptionMethod;
      _delegateStatusUpdate = StatusUpdateMethod;

      // Commands
      SearchLyricsCommand = new BaseCommand(SearchLyrics);
      ApplyLyricsCommand = new BaseCommand(ApplyLyrics);
    }

    #endregion

    #region Commands

    /// <summary>
    /// The Search Lyrics Button has been pressed
    /// </summary>
    public ICommand SearchLyricsCommand { get; set; }
    private void SearchLyrics(object parm)
    {
      log.Trace(">>>");
      Lyrics.Clear();
      StopThread();
      DoSearchLyrics();
      _options.MainSettings.SelectedLyricSites = SelectedLyricsSearchSites.ToList();
      log.Trace("<<<");
    }

    /// <summary>
    /// Apply the Lyrics to the Song
    /// </summary>
    public ICommand ApplyLyricsCommand { get; set; }

    private void ApplyLyrics(object parm)
    {
      log.Trace(">>>");

      foreach (var lyric in Lyrics)
      {
        if (lyric.IsSelected)
        {
          log.Info($"Setting Lyrics from {lyric.Site} for {_songs[lyric.Row].FileName}");
          _songs[lyric.Row].LyricsFrames.Clear();
          _songs[lyric.Row].Lyrics = lyric.Lyric;
        }
      }

      _options.MainSettings.SelectedLyricSites = SelectedLyricsSearchSites.ToList();
      CloseDialog("true");

      log.Trace("<<<");
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Start the Lyrics controller Thread
    /// </summary>
    private void DoSearchLyrics()
    {
      log.Trace(">>>");
      if (SelectedLyricsSearchSites.Count == 0)
      {
        MessageBox.Show(LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "lyricsSearch_NoSites_Selected", LocalizeDictionary.Instance.Culture).ToString(),
          LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "message_Error_Title", LocalizeDictionary.Instance.Culture).ToString(), MessageBoxButton.OK);
        return;
      }

      _statusMsgTmp = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "lyricsSearch_Status",
        LocalizeDictionary.Instance.Culture).ToString();

      StatusMsg = string.Format(_statusMsgTmp, _lyricsSearchSites.Count, _songs.Count,  _songs.Count * _lyricsSearchSites.Count, 0,
        0, _songs.Count * _lyricsSearchSites.Count);

      log.Info($"Starting Lyrics Controller for {SelectedLyricsSearchSites.Count} sites");
      _eventStopThread = new ManualResetEvent(false);

      log.Debug($"Adding {_songs.Count} songs to the queue");
      _lyricsQueue = new Queue();
      var row = 0;
      foreach (var song in _songs)
      {
        var switchedArtist = SwitchArtist(song.Artist);
        var lyricsModel = new LyricsModel { ArtistAndTitle = $"{row+1:D2}. {switchedArtist} - {song.Title}", Site = "", Lyric = "", Row = row };
        Lyrics.Add(lyricsModel);
        row++;
        string[] lyricId = new string[] { song.Artist, song.Title };
        _lyricsQueue.Enqueue(lyricId);
      }

      log.Debug("Starting Async Worker Thread");
      _bgWorkerLyrics = new BackgroundWorker();
      _bgWorkerLyrics.DoWork += bgWorkerLyrics_DoWork;
      _bgWorkerLyrics.ProgressChanged += bgWorkerLyrics_ProgressChanged;
      _bgWorkerLyrics.RunWorkerCompleted += bgWorkerLyrics_RunWorkerCompleted;
      _bgWorkerLyrics.WorkerSupportsCancellation = true;
      _bgWorkerLyrics.RunWorkerAsync();
      log.Trace("<<<");
    }

    /// <summary>
    /// Lyrics Controller Thread 
    /// </summary>
    private void bgWorkerLyrics_DoWork(object sender, DoWorkEventArgs e)
    {
      log.Trace(">>>");
      IsBusy = true;

      if (_lyricsQueue.Count > 0)
      {
        // start running the lyricController
        _lc = new LyricsController(this, _eventStopThread, SelectedLyricsSearchSites.ToArray(), true, _findStr, _replaceStr)
        {
          NrOfLyricsToSearch = _lyricsQueue.Count
        };

        ThreadStart runLyricController = delegate { _lc.Run(); };
        _lyricControllerThread = new Thread(runLyricController);
        _lyricControllerThread.Start();

        _lc.StopSearches = false;


        int row = 0;
        while (_lyricsQueue.Count != 0)
        {
          if (_lc == null)
            return;

          if (_lc.NrOfCurrentSearches < NrOfCurrentSearchesAllowed && _lc.StopSearches == false)
          {
            string[] lyricId = (string[])_lyricsQueue.Dequeue();
            lyricId[0] = SwitchArtist(lyricId[0]);

            _lc.AddNewLyricSearch(lyricId[0], TrimTitle(lyricId[1]), GetStrippedPrefixArtist(lyricId[0], _strippedPrefixStrings),
              row);
            row++;
          }

          Thread.Sleep(100);
        }
      }
      else
      {
        ThreadFinished = new object[] { "", "", LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "lyricsSearch_NothingToSearch", LocalizeDictionary.Instance.Culture).ToString(), "" };
      }

      IsBusy = false;
      log.Trace("<<<");
    }

    private void bgWorkerLyrics_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) { }

    private void bgWorkerLyrics_ProgressChanged(object sender, ProgressChangedEventArgs e) { }

    // Stop worker thread if it is running.
    // Called when user presses Stop button of form is closed.
    private void StopThread()
    {
      if (_lc != null)
      {
        log.Debug("Stop all searches");
        _lc.StopSearches = true;
      }

      log.Debug("Stop all threads");
      _bgWorkerLyrics.CancelAsync();
      if (_lyricControllerThread != null && _lyricControllerThread.IsAlive) // thread is active
      {
        _eventStopThread.Set();
      }
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
      var lyricsModel = new LyricsModel { ArtistAndTitle = $"{row+1:D2}. {artist} - {title}", Site = site, Lyric = lyric, Row = row };

      log.Info($"{lyricsModel.Site} returned lyrics for {lyricsModel.ArtistAndTitle}");

      // is this the first site returning Lyrics?
      var firstSite = false;
      for (var i = 0; i < Lyrics.Count; i++)
      {
        if (Lyrics[i].ArtistAndTitle == lyricsModel.ArtistAndTitle && Lyrics[i].Site.Length == 0 && Lyrics[i].Row == lyricsModel.Row)
        {
          Lyrics[i].Site = lyricsModel.Site;
          Lyrics[i].Lyric = lyricsModel.Lyric;
          firstSite = true;
          break;
        }
      }

      if (!firstSite)
      {
        Lyrics.Add(lyricsModel);
      }
    }

    private void LyricNotFoundMethod(string artist, string title, string site, int row, string message)
    {
      log.Info($"{site} did not return lyrics for {artist} - {title}"); ;
    }

    private void ThreadFinishedMethod(string message, string site)
    {
      log.Info("All Searches Finished");
      StatusMsg = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "lyricsSearch_Finished",
        LocalizeDictionary.Instance.Culture).ToString();
      StopThread();
    }

    private void ThreadExceptionMethod(string s) { }

    private void StatusUpdateMethod(int nrOfLyricsToSearch, int nrOfLyricsSearched, int nrOfLyricsFound,int nrOfLyricsNotFound)
    {
      //Searching for Lyrics ...  {0} Sites  x {1} Songs  = {2} Songs to Search. Found: {3} Not Found: {4} Remaining: {5}
      var remaining = nrOfLyricsToSearch * _lyricsSearchSites.Count - nrOfLyricsSearched;
      StatusMsg = string.Format(_statusMsgTmp, _lyricsSearchSites.Count, _songs.Count,  nrOfLyricsToSearch * _lyricsSearchSites.Count, nrOfLyricsFound,
        nrOfLyricsNotFound, remaining);
    }

    #endregion

    #region Interface Implementation

    public object[] UpdateStatus
    {
      set
      {
        try
        {
          _delegateStatusUpdate.Invoke((int)value[0], (int)value[1], (int)value[2], (int)value[3]);
        }
        catch (InvalidOperationException) { }
      }
    }

    public object[] LyricFound
    {
      set
      {
        try
        {
          _delegateLyricFound.Invoke((string)value[0], (string)value[1], (string)value[2], (int)value[3], (string)value[4]);
        }
        catch (InvalidOperationException) { }
      }
    }

    public object[] LyricNotFound
    {
      set
      {
        try
        {
          _delegateLyricNotFound.Invoke((string)value[0], (string)value[1], (string)value[2], (int)value[3], (string)value[4]);
        }
        catch (InvalidOperationException) { }
      }
    }

    public object[] ThreadFinished
    {
      set
      {
        try
        {
          _delegateThreadFinished.Invoke((string)value[0], (string)value[1]);
        }
        catch (InvalidOperationException) { }
      }
    }
    
    public string ThreadException
    {
      set
      {
        try
        {
          _delegateThreadException.Invoke(value);
        }
        catch (InvalidOperationException) { }
      }
    }

    #endregion

    #region Overrides

    public override void OnDialogOpened(IDialogParameters parameters)
    {
      log.Trace(">>>");
      LyricsSearchSites.AddRange(_options.MainSettings.LyricSites);
      SelectedLyricsSearchSites.AddRange(_options.MainSettings.SelectedLyricSites);
      _songs = parameters.GetValue<List<SongData>>("songs");
      DoSearchLyrics();
      log.Trace("<<<");
    }

    public override void CloseDialog(string parameter)
    {
      StopThread();
      base.CloseDialog(parameter);
    }

    #endregion

    #region Event Handling

    public override void OnMessageReceived(GenericEvent msg)
    {
      switch (msg.Action.ToLower())
      {
        case "applychangesrequested":
          ApplyLyrics("");
          break;
      }
      base.OnMessageReceived(msg);
    }

    #endregion
  }
}
