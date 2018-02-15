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
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using MPTagThat.Services.Logging;
using Prism.Unity;
using Prism.Modularity;
using MPTagThat.Services.Settings;
using Prism.Regions;
using Syncfusion.Windows.Tools.Controls;

#endregion

namespace MPTagThat
{
  /// <summary>
  /// PRTSM Bootstrapper class
  /// </summary>
  public class Bootstrapper : UnityBootstrapper
  {
    #region Variables

    private string[] _commandLineArgs;
    private static StartupSettings _startupSettings;
    private static int _portable;
    private static string _startupFolder;

    #endregion

    #region ctor

    public Bootstrapper(string[] args)
    {
      _commandLineArgs = args;
    }

    #endregion

    #region Overrides

    /// <summary>
    /// Resolve our main Shell Winndow
    /// </summary>
    /// <returns></returns>
    protected override DependencyObject CreateShell()
    {
      return Container.Resolve<Shell>();
    }

    /// <summary>
    /// Initialize the Shell and show the Main Window
    /// </summary>
    protected override void InitializeShell()
    {
      var log = Container.Resolve<ILogger>().GetLogger;
      var settings = Container.Resolve<ISettingsManager>();

      settings.GetOptions = new Options();
      
      _portable = 0;
      _startupFolder = "";
      // Process Command line Arguments
      foreach (string arg in _commandLineArgs)
      {
        if (arg.ToLower().StartsWith("/folder="))
        {
          _startupFolder = arg.Substring(8);
        }
        else if (arg.ToLower() == "/portable")
        {
          _portable = 1;
        }
      }

      try
      {
        // Let's see, if we already have an instance of MPTagThat open
        using (var mmf = MemoryMappedFile.OpenExisting("MPTagThat"))
        {
          if (_startupFolder == string.Empty)
          {
            // Don't allow a second instance of MPTagThat running
            return;
          }

          byte[] buffer = Encoding.Default.GetBytes(_startupFolder);
          var messageWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "MPTagThat_IPC");

          // Create accessor to MMF
          using (var accessor = mmf.CreateViewAccessor(0, buffer.Length))
          {
            // Write to MMF
            accessor.WriteArray(0, buffer, 0, buffer.Length);
            messageWaitHandle.Set();

            // End exit this instance
            return;
          }
        }
      }
      catch (FileNotFoundException)
      {
        // The Memorymap does not exist, so MPTagThat is not yet running
      }

      log.Info("MPTagThat is starting...");

      // Read the Config file
      ReadConfig();

      settings.StartupSettings = _startupSettings;
      settings.GetOptions.InitOptions();

      // Move Init of Services, which we don't need immediately to a separate thread to increase startup performance
      Thread initService = new Thread(() => DoInitService(Container))
      {
        IsBackground = true,
        Name = "InitService"
      };
      initService.Start();

      Application.Current.MainWindow.Show();
    }

    /// <summary>
    /// Configure the Services
    /// </summary>
    protected override void ConfigureContainer()
    {
      ServiceLocator.SetLocatorProvider(() => new UnityServiceLocatorAdapter(Container));
      var logger = new NLogLogger("MPTagThat.log", Services.Logging.LogLevel.Debug, 0);
      Container.RegisterInstance<ILogger>(logger);
      var settings = new SettingsManager();
      Container.RegisterInstance<ISettingsManager>(settings);
      base.ConfigureContainer();
    }

    /// <summary>
    /// Override the Region adapter mappings to make use of our DockingManager Region Adapater
    /// </summary>
    /// <returns></returns>
    protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
    {
      RegionAdapterMappings regionAdapterMappings = base.ConfigureRegionAdapterMappings();
      if (regionAdapterMappings != null)
      {
        regionAdapterMappings.RegisterMapping(typeof(DockingManager), Container.Resolve<DockingManagerRegionAdapter>());
      }
      return regionAdapterMappings;
    }

    /// <summary>
    /// Module Catalog to hold all the modules, which are referenced from the Main Project
    /// </summary>
    protected override void ConfigureModuleCatalog()
    {
      ModuleCatalog catalog = (ModuleCatalog)ModuleCatalog;
      catalog.AddModule(typeof(Ribbon.RibbonModule));
      catalog.AddModule(typeof(Treeview.TreeviewModule));
      catalog.AddModule(typeof(SongGrid.SongGridModule));
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Init Service Thread
    /// </summary>
    private static void DoInitService(IUnityContainer container)
    {
     byte[] buffer = new byte[2048];
      var messageWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "MPTagThat_IPC");

      container.Resolve<ILogger>().GetLogger.Debug("Registering MemoryMappedFile");
      // Create named MMF
      using (var mmf = MemoryMappedFile.CreateOrOpen("MPTagThat", 2048))
      {
        // Create accessor to MMF
        using (var accessor = mmf.CreateViewAccessor(0, buffer.Length))
        {
          // Wait for the Message to be fired
          while (true)
          {
            container.Resolve<ILogger>().GetLogger.Debug("Wait for Startup Event to be fired");
            messageWaitHandle.WaitOne();
            container.Resolve<ILogger>().GetLogger.Debug("Startup Event fired");

            // Read from MMF
            accessor.ReadArray(0, buffer, 0, buffer.Length);

            string startupFolder = Encoding.Default.GetString(buffer).Trim(' ', '\x00');
            //SetCurrentFolder(startupFolder);
          }
        }
      }
    }

    /// <summary>
    /// Read the Config.xml file
    /// </summary>
    private static void ReadConfig()
    {
      _startupSettings = new StartupSettings();
      string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.xml");
      if (!File.Exists(configFile))
        return;

      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(configFile);

        // Check, if we got a config.xml
        if (doc.DocumentElement == null) return;
        string strRoot = doc.DocumentElement.Name;
        if (strRoot != "config") return;

        XmlNode portableNode = doc.DocumentElement.SelectSingleNode("/config/portable");
        if (portableNode != null)
        {
          if (_portable == 0)
          {
            // Only use the value from Config, if not overriden by an argument
            _portable = Convert.ToInt32(portableNode.InnerText);
          }
          _startupSettings.Portable = _portable != 0;
        }

        XmlNode maxSongsNode = doc.DocumentElement.SelectSingleNode("/config/MaximumNumberOfSongsInList");
        _startupSettings.MaxSongs = maxSongsNode != null ? Convert.ToInt32(maxSongsNode.InnerText) : 500;

        XmlNode ravenDebugNode = doc.DocumentElement.SelectSingleNode("/config/RavenDebug");
        _startupSettings.RavenDebug = ravenDebugNode != null && Convert.ToInt32(ravenDebugNode.InnerText) != 0;

        XmlNode ravenStudioNode = doc.DocumentElement.SelectSingleNode("/config/RavenStudio");
        _startupSettings.RavenStudio = ravenStudioNode != null && Convert.ToInt32(ravenStudioNode.InnerText) != 0;

        XmlNode ravenPortNode = doc.DocumentElement.SelectSingleNode("/config/RavenStudioPort");
        _startupSettings.RavenStudioPort = ravenPortNode != null ? Convert.ToInt32(ravenPortNode.InnerText) : 8080;

        XmlNode ravenDatabaseNode = doc.DocumentElement.SelectSingleNode("/config/MusicDatabaseFolder");
        var dbPath = ravenDatabaseNode?.InnerText ?? "%APPDATA%\\MPTagThat\\Databases";
        dbPath = CheckPath(dbPath);
        _startupSettings.DatabaseFolder = dbPath;

        XmlNode coverArtNode = doc.DocumentElement.SelectSingleNode("/config/CoverArtFolder");
        var coverArtPath = coverArtNode?.InnerText ?? "%APPDATA%\\MPTagThat\\CoverArt";
        coverArtPath = CheckPath(coverArtPath);
        _startupSettings.CoverArtFolder = coverArtPath;
      }
      catch (Exception)
      {
        // ignored
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="strPath"></param>
    /// <returns></returns>
    private static string CheckPath(string strPath)
    {
      string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      strPath = strPath.Replace("%APPDATA%", appData);
      strPath = strPath.Replace("%AppData%", appData);
      string commonData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
      strPath = strPath.Replace("%PROGRAMDATA%", commonData);
      strPath = strPath.Replace("%ProgramData%", commonData);

      // Check to see, if the location was specified with an absolute or relative path.
      // In case of relative path, prefix it with the startuppath   
      if (!Path.IsPathRooted(strPath))
      {
        strPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, strPath);
      }

      // Create the folder
      if (!Directory.Exists(strPath))
      {
        try
        {
          Directory.CreateDirectory(strPath);
        }
        catch (Exception)
        {
          // ignored
        }
      }

      // See if we got a slash at the end. If not add one.
      if (!strPath.EndsWith(@"\"))
      {
        strPath += @"\";
      }

      return strPath;
    }


    #endregion
  }
}
