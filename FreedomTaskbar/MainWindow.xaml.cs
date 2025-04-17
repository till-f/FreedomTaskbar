using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using FreedomTaskbar.Controls;
using FreedomTaskbar.Core;
using FreedomTaskbar.ViewModel;
using Timer = System.Timers.Timer;

namespace FreedomTaskbar;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
  public const string MainWindowTitle = "Freedom Taskbar";

  private readonly Timer _updateTimer;

  private readonly List<string> _excludedWindows = ["Settings", "BBar", "Windows Input Experience", MainWindowTitle];

  public MainWindow()
  {
    InitializeComponent();

    SetPosition();

    RefreshWindowList();

    _updateTimer = new Timer(200);
    _updateTimer.AutoReset = true;
    _updateTimer.Elapsed += OnUpdateTimeElapsed;
    _updateTimer.Start();
  }

  private void OnUpdateTimeElapsed(object? sender, ElapsedEventArgs e)
  {
    Dispatcher.InvokeAsync(RefreshWindowList);
  }

  private void SetPosition()
  {
    int strangeWindowPaddingY = -16;

    var width = 200;
    Top = 0;
    Left = SystemParameters.PrimaryScreenWidth - width;
    Height = SystemParameters.MaximizedPrimaryScreenHeight + strangeWindowPaddingY;
    Width = width;
  }

  private void RefreshWindowList()
  {
    var foregroundWindow = Win32.GetForegroundWindow();
    var newWindows = Win32Utils.GetOpenWindows().Select(it => new OsWindow(it, foregroundWindow)).ToList();
    var taskBarButtons = WindowsStackPanel.Children.OfType<TaskbarButton>().ToList();

    foreach (var taskBarButton in taskBarButtons)
    {
      // count (and skip) windows that already have taskbar button
      var removedCount = newWindows.RemoveAll(it => it.Handle == taskBarButton.Window.Handle);
      if (removedCount == 0)
      {
        // window for the taskbar button does not exist anymore => remove the button
        WindowsStackPanel.Children.Remove(taskBarButton);
      }
    }

    // add new buttons for remaining windows that did not exist before
    foreach (var window in newWindows)
    {
      if (_excludedWindows.Any(r => Regex.IsMatch(window.Title, r)))
      {
        continue;
      }

      WindowsStackPanel.Children.Add(new TaskbarButton(window));
    }
  }
}