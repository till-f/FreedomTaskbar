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

  private readonly Timer _updateTimer;

  private readonly List<string> _excludedWindows = [/*"Settings", "BBar", "Windows Input Experience",*/ MainWindowTitle];

  public MainWindow()
  {
    InitializeComponent();

    MoveToSide(ESide.Right);

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

  private const int TaskbarWidth = 200;

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
    var taskBarButtons = WindowsStackPanel.Children.OfType<TaskbarButton>().ToList();

    foreach (var taskBarButton in taskBarButtons)
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

  private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
  {
    HideFromAltTabMenu();
  }

  private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
  {
    if (Keyboard.IsKeyDown(Key.LeftShift) 
        || Keyboard.IsKeyDown(Key.RightShift)
        || Keyboard.IsKeyDown(Key.LeftCtrl) 
        || Keyboard.IsKeyDown(Key.RightCtrl))
    {
      if (e.Key == Key.Right)
      {
        MoveToSide(ESide.Right);
      }
      if (e.Key == Key.Left)
      {
        MoveToSide(ESide.Left);
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
}

enum ESide { Left, Right }