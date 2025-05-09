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
/// Feature backlog:
///  - Fix icon for some apps (use Win32 API to get icon instead of extracting from process main module file)
///  - Put windows of same process together (important: still allow to move buttons away freely)
///    - new windows are always added behind the last windows of the same process
///  - Pin apps and persist (on disk) the order/position for pinned apps:
///    - the position of the button for first window of a pinned app defines the position in relation to other pinned apps
///    - if last window of pinned app is closed, a button for the pinned app stays at position of the last closed window
///    - if first window for a pinned app is opened, the button for the pinned app is replaced by a button for the window
///  - Start apps not as admin even though the taskbar runs as admin (un-escalate privileges), e.g. when using SHIFT+click to start a 2nd instance
/// </summary>
public partial class MainWindow : Window
{
  public static ESide TaskbarSide { get; private set; } = ESide.Right;

  public const string MainWindowTitle = "Freedom Taskbar";
  public const int TaskbarWidth = 200;

  private readonly Timer _refreshTimer = new (200);
  private readonly List<string> _excludedWindows = [];

  public MainWindow()
  {
    InitializeComponent();
  }

  private IEnumerable<TaskbarButton> TaskbarButtons => TaskbarButtonsStackPanel.Children.OfType<TaskbarButton>();

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
    TaskbarSide = side;

    // TODO: consider DPI and/or use Win32 APIs to get rid of this hard-coded value
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
        TaskbarButtonsStackPanel.Children.Remove(taskBarButton);
        taskBarButton.WindowHandleDropped -= OnWindowHandleDropped;
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

      var tbb = new TaskbarButton(osWindow);
      tbb.WindowHandleDropped += OnWindowHandleDropped;
      TaskbarButtonsStackPanel.Children.Add(tbb);
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

  /// <summary>
  /// Moves the taskbar button for dropped window handle.
  /// If button was dragged down, it is inserted behind the drop target, otherwise it is inserted before.
  /// </summary>
  private void OnWindowHandleDropped((TaskbarButton TargetButton, string DroppedWindowHandle) e)
  {
    // move taskbar button for dropped window handle
    // if moved down, insert it behind (otherwise, it is inserted before)

    var sourceButton = TaskbarButtons.First(it => it.Window.RootHandle.ToString() == e.DroppedWindowHandle);
    var newIdx = TaskbarButtonsStackPanel.Children.IndexOf(e.TargetButton);

    TaskbarButtonsStackPanel.Children.Remove(sourceButton);
    TaskbarButtonsStackPanel.Children.Insert(newIdx, sourceButton);
  }
}

public enum ESide { Left, Right }