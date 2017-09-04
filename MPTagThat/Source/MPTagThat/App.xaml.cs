using System.Windows;

namespace MPTagThat
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    /// <summary>
    /// Instantiate the Unity Bootstrapper
    /// </summary>
    /// <param name="e"></param>
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      Bootstrapper bootstrapper = new Bootstrapper();
      bootstrapper.Run();
    }
  }
}
