using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace FreedomTaskbar;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
  private void App_OnStartup(object sender, StartupEventArgs e)
  {
    EnsureNotRunning();
  }

  private void EnsureNotRunning()
  {
    var alreadyRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()?.Location)).Length > 1;
    if (alreadyRunning)
    {
      Debug.WriteLine("Another instance is already running.");
      Shutdown(1);
    }
  }
}