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
    var newWindows = Win32Utils.GetOpenWindows().ToList();
    var taskBarButtons = WindowsStackPanel.Children.OfType<TaskbarButton>().ToList();

    foreach (var taskBarButton in taskBarButtons)
    {
      var newWindowsForExistingButton = newWindows.Where(it => it.RootHandle == taskBarButton.Window.Handle).ToList();

      if (newWindowsForExistingButton.Any())
      {
        var childWindowHandles = newWindowsForExistingButton.Select(it => it.Handle).ToList();

        // button already exists => refresh and skip respective windows (remove from list)
        // skip windows that already have taskbar button
        newWindows.RemoveAll(it => newWindowsForExistingButton.Contains(it));
        taskBarButton.Window.Refresh(foregroundWindow, childWindowHandles);
      }
      else
      {
        // window for the taskbar button does not exist anymore => remove the button
        WindowsStackPanel.Children.Remove(taskBarButton);
      }
    }

    // add new buttons for remaining windows that did not exist before
    foreach (var rootWindow in newWindows.Where(it => it.IsRootWindow))
    {
      var childWindowHandles = newWindows.Where(it => it.RootHandle == rootWindow.Handle).Select(it => it.Handle).ToList();
      var osWindow = new OsWindow(rootWindow, foregroundWindow, childWindowHandles);

      if (_excludedWindows.Any(r => Regex.IsMatch(osWindow.Title, r)))
      {
        continue;
      }

      WindowsStackPanel.Children.Add(new TaskbarButton(osWindow));
    }
  }
}