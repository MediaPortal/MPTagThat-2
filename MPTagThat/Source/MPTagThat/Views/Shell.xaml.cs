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

      var stateFile = _options.ConfigDir + "\\DockingLayout.xml";

      if (File.Exists(stateFile))
      {
        BinaryFormatter formatter = new BinaryFormatter();
        MainDockingManager.LoadDockState(formatter, StorageFormat.Xml, stateFile);
      }

      EventSystem.Subscribe<GenericEvent>(OnMessageReceived);
    }

    #region Event Handling

    private void OnMessageReceived(GenericEvent msg)
    {
      switch (msg.Action.ToLower())
      {
        case "resetdockstate":
          MainDockingManager.ResetState();
          break;

        case "deletedockstate":
          if (File.Exists($"{_options.ConfigDir}\\DockingLayout.xml"))
          {
            File.Delete($"{_options.ConfigDir}\\DockingLayout.xml");
          }
          if (File.Exists($"{_options.ConfigDir}\\Default_DockingLayout.xml"))
          {
            BinaryFormatter formatter = new BinaryFormatter();
            MainDockingManager.LoadDockState(formatter, StorageFormat.Xml, $"{_options.ConfigDir}\\Default_DockingLayout.xml");
          }
          break;
      }
    }

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
