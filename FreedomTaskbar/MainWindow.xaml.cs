using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Input;
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

  private const int TaskbarWidth = 200;

  private readonly Timer _refreshTimer = new (200);
  private readonly List<string> _excludedWindows = [];

  public MainWindow()
  {
    InitializeComponent();
  }

  private IEnumerable<TaskbarButton> TaskbarButtons => WindowsStackPanel.Children.OfType<TaskbarButton>();

  private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
  {
    HideFromAltTabMenu();
    MoveToSide(ESide.Right);
    RefreshWindowList();

    _refreshTimer.AutoReset = true;
    _refreshTimer.Elapsed -= RefreshTimer_OnElapsed;
    _refreshTimer.Elapsed += RefreshTimer_OnElapsed;
    _refreshTimer.Start();
  }

  private void RefreshTimer_OnElapsed(object? sender, ElapsedEventArgs e)
  {
    Dispatcher.InvokeAsync(RefreshWindowList);
  }

  private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
  {
    if (Keyboard.IsKeyDown(Key.LeftShift) 
        || Keyboard.IsKeyDown(Key.RightShift)
        || Keyboard.IsKeyDown(Key.LeftCtrl) 
        || Keyboard.IsKeyDown(Key.RightCtrl))
    {
      switch (e.Key)
      {
        case Key.Right:
          MoveToSide(ESide.Right);
          break;
        case Key.Left:
          MoveToSide(ESide.Left);
          break;
        case Key.Up:
          MoveButton(-1);
          break;
        case Key.Down:
          MoveButton(+1);
          break;
      }

    }
  }

  private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
  {
    // prevents closing window with Alt+F4 etc.
    e.Cancel = true;
  }

  private void MainWindow_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
  {
    if (e.OriginalSource == this && e.ChangedButton == MouseButton.Right)
    {
      MainContextMenu.IsOpen = true;
    }
  }

  private void MenuItemToLeft_OnClick(object sender, RoutedEventArgs e)
  {
    MoveToSide(ESide.Left);
  }

  private void MenuItemToRight_OnClick(object sender, RoutedEventArgs e)
  {
    MoveToSide(ESide.Right);
  }

  private void MenuItemExit_OnClick(object sender, RoutedEventArgs e)
  {
    Application.Current.Shutdown();
  }

  private void MoveToSide(ESide side)
  {
    int strangeWindowPaddingY = -16;

    Top = 0;
    Left = side switch
    {
      ESide.Left => 0,
      ESide.Right => SystemParameters.PrimaryScreenWidth - TaskbarWidth,
      _ => 0
    };
    Height = SystemParameters.MaximizedPrimaryScreenHeight + strangeWindowPaddingY;
    Width = TaskbarWidth;
  }

  private void RefreshWindowList()
  {
    var foregroundWindow = Win32.GetForegroundWindow();
    var newWindows = Win32Utils.GetOpenWindows().ToList();

    foreach (var taskBarButton in TaskbarButtons.ToList())
    {
      var newWindowsForExistingButton = newWindows.Where(it => it.RootHandle == taskBarButton.Window.RootHandle).ToList();

      if (newWindowsForExistingButton.Any(it => it.IsRootWindow))
      {
        // root window already has a taskbar button => refresh it and skip respective windows (remove from list)
        var childWindowHandles = newWindowsForExistingButton.Select(it => it.Handle).ToList();
        newWindows.RemoveAll(it => newWindowsForExistingButton.Contains(it));
        taskBarButton.Window.Refresh(foregroundWindow, childWindowHandles);
      }
      else
      {
        // root window for the taskbar button does not exist anymore => remove the button
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

  /// <summary>
  /// This hides the application from the Alt+Tab menu.
  /// It sets the "ToolWindow" flag without adding a caption etc. (keeping WindowStyle 'None').
  /// See https://www.csharp411.com/hide-form-from-alttab/ (Link is for WinForms, WPF requires the hack below).
  /// </summary>
  private void HideFromAltTabMenu()
  {
    try
    {
      var hWnd = Process.GetCurrentProcess().MainWindowHandle;
      Win32.WINDOWINFO wi = new();
      Win32.GetWindowInfo(hWnd, ref wi);

      var t = typeof(Window).GetNestedType("HwndStyleManager", BindingFlags.NonPublic);
      var m = t!.GetMethod("StartManaging", BindingFlags.Static | BindingFlags.NonPublic, [typeof(Window), typeof(int), typeof(int)]);
      using (var _ = (IDisposable)m!.Invoke(null, [this, (int)wi.dwStyle, (int)wi.dwExStyle])!)
      {
        var styleExProperty = typeof(Window).GetProperty("_StyleEx", BindingFlags.Instance | BindingFlags.NonPublic);
        var newStyleEx = (int)(wi.dwExStyle | 0x80);
        styleExProperty!.SetValue(this, newStyleEx);
      }
    }
    catch
    {
      Debug.WriteLine("Hack to hide FreedomTaskbar from Alt+Tab menu failed. The .NET codebase may have changed.");
    }
  }

  private void MoveButton(int delta)
  {
    var allButtons = TaskbarButtons.ToList();
    var activeWindow = TaskbarButtons.FirstOrDefault(it => it.Window.IsActive);
    if (activeWindow == null)
    {
      return;
    }

    var oldIdx = allButtons.IndexOf(activeWindow);
    if (oldIdx < 0)
    {
      return;
    }

    var newIdx = oldIdx + delta;
    if (newIdx < 0 || newIdx > allButtons.Count - 1)
    {
      return;
    }

    //WindowsStackPanel.Children.Remove(activeWindow);
    WindowsStackPanel.Children.Insert(allButtons.IndexOf(activeWindow), activeWindow);
  }
}

enum ESide { Left, Right }