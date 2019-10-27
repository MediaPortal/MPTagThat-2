using CommonServiceLocator;
using MPTagThat.Core;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Settings;
using System.IO;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
using System.Runtime.Serialization.Formatters.Binary;

namespace MPTagThat.Views
{
  /// <summary>
  /// Interaction logic for Shell.xaml
  /// </summary>
  public partial class Shell : RibbonWindow
  {
    public Shell()
    {
      InitializeComponent();

      var options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;
      var stateFile = options.ConfigDir + "\\DockingLayout.xml";

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
          var options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;
          if (File.Exists($"{options.ConfigDir}\\DockingLayout.xml"))
          {
            File.Delete($"{options.ConfigDir}\\DockingLayout.xml");
          }
          if (File.Exists($"{options.ConfigDir}\\Default_DockingLayout.xml"))
          {
            BinaryFormatter formatter = new BinaryFormatter();
            MainDockingManager.LoadDockState(formatter, StorageFormat.Xml, $"{options.ConfigDir}\\Default_DockingLayout.xml");
          }
          break;
      }
    }

    #endregion

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      var options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;
      var stateFile = options.ConfigDir + "\\DockingLayout.xml";
      BinaryFormatter formatter = new BinaryFormatter();
      MainDockingManager.SaveDockState(formatter, StorageFormat.Xml, stateFile);
    }
  }
}
