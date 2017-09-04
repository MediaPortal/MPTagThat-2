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

using System.Windows;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using MPTagThat.Services.Logging;
using NLog;
using Prism.Unity;
using Prism.Modularity;
using ILogger = MPTagThat.Services.Logging.ILogger;
using MPTagThat.Services.Settings;

#endregion

namespace MPTagThat
{
  /// <summary>
  /// PRTSM Bootstrapper class
  /// </summary>
  public class Bootstrapper : UnityBootstrapper
  {
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
      var settings = Container.Resolve<ISettingsManager>("SettingsManager");
      Application.Current.MainWindow.Show();
    }

    /// <summary>
    /// Configure the Services
    /// </summary>
    protected override void ConfigureContainer()
    {
      var logger = new NLogLogger("MPTagThat.log", LogLevel.Debug, 0);
      Container.RegisterInstance<ILogger>(logger);
      Container.RegisterType<ISettingsManager, SettingsManager>("SettingsManager");
      base.ConfigureContainer();
    }

    /// <summary>
    /// Module Catalog to hold all the modules, which are referenced from the Main Project
    /// </summary>
    protected override void ConfigureModuleCatalog()
    {
      ModuleCatalog catalog = (ModuleCatalog)ModuleCatalog;
      // Need to add the module catalogs here
    }
  }
}
