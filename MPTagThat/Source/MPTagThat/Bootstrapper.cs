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
using Microsoft.Practices.Unity;
using Prism.Unity;
using Prism.Modularity;

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
      Application.Current.MainWindow.Show();
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
