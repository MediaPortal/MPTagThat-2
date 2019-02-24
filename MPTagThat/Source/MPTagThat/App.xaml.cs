using System;
using System.IO;
using System.Configuration;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Syncfusion.Windows.Tools.Controls;
using Unity;

namespace MPTagThat
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App
  {
    #region Variables

    private string[] _commandLineArgs;
    private static StartupSettings _startupSettings;
    private static int _portable;
    private static string _startupFolder;

    public App()
    {
      //Register Syncfusion license
      Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NzIzMjJAMzEzNjJlMzQyZTMwaFBNUzNxR285QU5QeW1Uamw1L3JxazRza3NXdThtL0toR2syVkdURFdiST0=");
    }

    #endregion

    #region Interfaces

    /// <summary>
    /// Resolve our main Shell Winndow
    /// </summary>
    /// <returns></returns>
    protected override Window CreateShell()
    {
      return Container.Resolve<Views.Shell>();
    }

    /// <summary>
    /// Configure the Services
    /// </summary>
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
      var logger = new NLogLogger("MPTagThat.log", LogLevel.Debug, 0);
      containerRegistry.RegisterInstance<ILogger>(logger);
      var settings = new SettingsManager();
      containerRegistry.RegisterInstance<ISettingsManager>(settings);
      CommonServiceLocator.ServiceLocator.SetLocatorProvider(() => new UnityServiceLocatorAdapter(Container.GetContainer()));

      // Must be the last thing here, since we are referring to the service 
      settings.StartupSettings = _startupSettings;
      settings.GetOptions.InitOptions();

    }

    /// <summary>
    /// Override the Region adapter mappings to make use of our DockingManager Region Adapater
    /// </summary>
    /// <returns></returns>
    protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
    {
      base.ConfigureRegionAdapterMappings(regionAdapterMappings);
      regionAdapterMappings.RegisterMapping(typeof(DockingManager), Container.Resolve<DockingManagerRegionAdapter>());
    }

    /// <summary>
    /// Module Catalog to hold all the modules, which are referenced from the Main Project
    /// </summary>
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
      moduleCatalog.AddModule(typeof(Ribbon.RibbonModule));
      moduleCatalog.AddModule(typeof(Treeview.TreeviewModule));
      moduleCatalog.AddModule(typeof(SongGrid.SongGridModule));
      moduleCatalog.AddModule(typeof(MiscFiles.MiscFilesModule));
    }

    /// <summary>
    /// Do startup related things
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
      _commandLineArgs = e.Args;     
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

      // Read the Config file
      ReadConfig();

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

      try
      {
        // We need to set the app.config file programmatically to point to the users APPDATA Folder
        var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        var appSettingssettings = configFile.AppSettings.Settings;
        var key = "Raven/WorkingDir";
        var value = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                    "\\MPTagthat\\Databases";
        if (appSettingssettings[key] == null)
        {
          appSettingssettings.Add(key, value);
        }
        else
        {
          appSettingssettings[key].Value = value;
        }

        configFile.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
      }
      catch (ConfigurationErrorsException)
      {
      }

      // Need to reset the Working directory, since when we called via the Explorer Context menu, it'll be different
      Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

      // Add our Bin and Bin\Bass Directory to the Path
      SetPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Bin"));
      SetPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Bin\Bass"));

      base.OnStartup(e);
    }

    /// <summary>
    /// Initialize the Shell and show the Main Window
    /// </summary>
    protected override void OnInitialized()
    {
      var log = Container.Resolve<ILogger>().GetLogger;
    
      log.Info("MPTagThat is starting...");
      
      // Move Init of Services, which we don't need immediately to a separate thread to increase startup performance
      Thread initService = new Thread(() => DoInitService(Container.GetContainer()))
      {
        IsBackground = true,
        Name = "InitService"
      };
      initService.Start();

      Current.MainWindow.Show();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Set the Path for the binaries
    /// </summary>
    /// <param name="path"></param>
    private static void SetPath(string path)
    {
      var newPath = $"{Environment.GetEnvironmentVariable("Path")};{path}";
      Environment.SetEnvironmentVariable("Path", newPath);
    }

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
