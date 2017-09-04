using System;
using System.IO;
using System.Configuration;
using System.Windows;

namespace MPTagThat
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App
  {
    /// <summary>
    /// Instantiate the Unity Bootstrapper
    /// </summary>
    /// <param name="e"></param>
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      try
      {
        // We need to set the app.config file programmatically to point to the users APPDATA Folder
        var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        var settings = configFile.AppSettings.Settings;
        var key = "Raven/WorkingDir";
        var value = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                    "\\MPTagthat\\Databases";
        if (settings[key] == null)
        {
          settings.Add(key, value);
        }
        else
        {
          settings[key].Value = value;
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

      // Call Bootstrapper with command line arguments
      Bootstrapper bootstrapper = new Bootstrapper(e.Args);
      bootstrapper.Run();
    }

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

    #endregion
  }
}
