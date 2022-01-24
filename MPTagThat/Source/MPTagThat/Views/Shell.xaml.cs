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

using MPTagThat.Core;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Settings;
using System.IO;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Media;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.MediaChangeMonitor;
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.ViewModels;
using Prism.Ioc;

namespace MPTagThat.Views
{
  /// <summary>
  /// Interaction logic for Shell.xaml
  /// </summary>
  public partial class Shell : RibbonWindow
  {
    #region Variables

    private Options _options = ContainerLocator.Current.Resolve<ISettingsManager>().GetOptions;

    #endregion

    public Shell()
    {
      InitializeComponent();
      var vm = (ShellViewModel)DataContext;
      if (vm != null)
      {
        vm.MainDockingManager = this.MainDockingManager;
      }

      var stateFile = _options.ConfigDir + "\\DockingLayout.xml";

      if (File.Exists(stateFile))
      {
        BinaryFormatter formatter = new BinaryFormatter();
        MainDockingManager.LoadDockState(formatter, StorageFormat.Xml, stateFile);
      }
    }

    #region Event Handling

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      // Set the Background to be used in dialogs
      _options.BackGround = this.Background;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      GenericEvent evt = new GenericEvent
      {
        Action = "applicationclosing"
      };
      EventSystem.Publish(evt);

      ContainerLocator.Current.Resolve<ILogger>()?.GetLogger.Info("Saving Docking State");
      var stateFile = _options.ConfigDir + "\\DockingLayout.xml";
      BinaryFormatter formatter = new BinaryFormatter();

      // Only Save the state of the DockingManager, when the Tabs Tag is active
      if (_options.IsTagsTabActive)
      {
        MainDockingManager.SaveDockState(formatter, StorageFormat.Xml, stateFile);
      }

      ContainerLocator.Current.Resolve<IMediaChangeMonitor>()?.StopListening();
    }

    #endregion
  }
}
