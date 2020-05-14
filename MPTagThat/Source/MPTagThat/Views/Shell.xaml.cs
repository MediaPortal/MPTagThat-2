using CommonServiceLocator;
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
using MPTagThat.Core.Services.Settings.Setting;
using MPTagThat.ViewModels;

namespace MPTagThat.Views
{
  /// <summary>
  /// Interaction logic for Shell.xaml
  /// </summary>
  public partial class Shell : RibbonWindow
  {
    #region Variables

    private Options _options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;

    #endregion

    public Shell()
    {
      InitializeComponent();
      var vm = (ShellViewModel) DataContext;
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
      var dockcontrol = VisualUtils.FindDescendant(this, typeof(DockingManager)) as DockingManager;
      if (dockcontrol != null)
      {
        _options.MainSettings.BackGround = new BrushConverter().ConvertToString(dockcontrol.Background);
      }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      GenericEvent evt = new GenericEvent
      {
        Action = "applicationclosing"
      };
      EventSystem.Publish(evt);

      (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger).GetLogger.Info("Saving Docking State");
      var stateFile = _options.ConfigDir + "\\DockingLayout.xml";
      BinaryFormatter formatter = new BinaryFormatter();
      MainDockingManager.SaveDockState(formatter, StorageFormat.Xml, stateFile);
    }

    #endregion
  }
}
