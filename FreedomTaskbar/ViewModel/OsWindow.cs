using FreedomTaskbar.Core;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FreedomTaskbar.WpfExtensions;
using System.Diagnostics;
using FreedomTaskbar.WinFormsFacade;

namespace FreedomTaskbar.ViewModel;

using static DependencyPropertyRegistrar<OsWindow>;

public class OsWindow : DependencyObject
{
  private Win32.RECT? _posRestore;

  public OsWindow(Win32Window rootWindow, IntPtr foregroundWindowHandle, IList<IntPtr> childWindowHandles)
  {
    RootHandle = rootWindow.Handle;
    Process = Win32Utils.GetProcessForWindowHandle(RootHandle);

    Title = string.Empty;
    IsActive = false;

    RefreshInternal(foregroundWindowHandle, childWindowHandles, true);
  }

  public IntPtr RootHandle { get; }

  public Process? Process { get; }

  public string? ProcessExePath
  {
    get
    {
      try
      {
        return Process?.MainModule?.FileName;
      }
      catch
      {
        // Win32Exception: "Access is denied." may be thrown when MainModule cannot be accessed for
        // elevated process. This can only be avoided by running FreedomTaskbar as administrator.
        return null;
      }
    }
  }

  public event Action<string, string>? TitleChanged;
  public static readonly DependencyPropertyKey TitleProperty = RegisterProperty(x => x.Title).OnChange(OnTitleChanged);
  public string Title
  {
    get => (string)GetValue(TitleProperty.DependencyProperty);
    private set => SetValue(TitleProperty, value);
  }

  public static readonly DependencyPropertyKey IconProperty = RegisterProperty(x => x.Icon);
  public ImageSource Icon
  {
    get => (ImageSource)GetValue(IconProperty.DependencyProperty);
    private set => SetValue(IconProperty, value);
  }

  public event Action<bool, bool>? IsActiveChanged;
  public static readonly DependencyPropertyKey IsActiveProperty = RegisterProperty(x => x.IsActive).OnChange(OnIsActiveChanged);
  public bool IsActive
  {
    get => (bool)GetValue(IsActiveProperty.DependencyProperty);
    private set => SetValue(IsActiveProperty, value);
  }

  public void SetRestorePositionAndMaximize(GenericScreen screen)
  {
    Win32.GetWindowRect(RootHandle, out var currentPos);

    if (screen.IsPrimary)
    {
      // on the primary screen, always restore to the pseudo-maximized position.
      var primScreenArea = CalcPseudoMaximizedRect();
      _posRestore = primScreenArea;

      // un-maximize before moving to the pseudo-maximized position. otherwise the windows is moved to the wrong position when it was maximized on a secondary screen (ShrinkIfMaximize is triggered).
      Win32.ShowWindow(RootHandle, Win32.SW_SHOWNORMAL);
      Win32.SetWindowPos(RootHandle, IntPtr.Zero, primScreenArea.X, primScreenArea.Y, primScreenArea.Width, primScreenArea.Height, 0);
    }
    else
    {
      Win32.SetWindowPos(RootHandle, IntPtr.Zero, screen.Bounds.X, screen.Bounds.Y, currentPos.Width, currentPos.Height, 0);
      Win32.ShowWindow(RootHandle, Win32.SW_SHOWMAXIMIZED);
    }
  }

  /// <summary>
  /// Queries the current state of the window from the OS and updates relevant properties.
  /// Note that <see cref="Icon"/> is not updated after initial construction.
  /// </summary>
  public void Refresh(IntPtr foregroundWindowHandle, IList<IntPtr> childWindowHandles)
  {
    RefreshInternal(foregroundWindowHandle, childWindowHandles, false);
  }

  public void Close()
  {
    Win32.PostMessage(RootHandle, Win32.WM_CLOSE, 0, 0);
  }

  public void KillProcess()
  {
    var title = Title;
    var process = Process;
    if (process == null)
    {
      return;
    }

    Task.Run(() =>
    {
      try
      {
        process.Kill();
      }
      catch
      {
        Debug.WriteLine($"Failed to kill process for window '{title}'");
      }
    });

  }

  public void RefreshInternal(IntPtr foregroundWindowHandle, IList<IntPtr> childWindowHandles, bool initStaticProperties)
  {
    RefreshTitle();
    RefreshIsActive(foregroundWindowHandle, childWindowHandles);

    if (initStaticProperties)
    {
      InitIcon();
    }

    // this is a bit hacky at this place; consider to introduce an event so that logic can go into another class
    ShrinkIfMaximized();
  }

  private void RefreshTitle()
  {
    var length = Win32.GetWindowTextLength(RootHandle);
    if (length == 0)
    {
      Title = string.Empty;
    }

    var builder = new StringBuilder(length);
    Win32.GetWindowText(RootHandle, builder, length + 1);
    Title = builder.ToString();
  }
  
  private void InitIcon()
  {
    try
    {
      var exePath = ProcessExePath;
      if (exePath == null) return;

      var icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
      if (icon == null) return;

      var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
      Icon = bitmapSource;
    }
    catch
    {
      Debug.WriteLine($"Failed to extract icon for window '{Title}'");
    }
  }

  private void ShrinkIfMaximized()
  {
    var isMaximized = Win32.IsZoomed(RootHandle);
    if (!isMaximized) return;

    // skip if window is maximized on a different screen than primary screen (primary screen always has (0,0) as top-left coordinate)
    // the offset of 200 pixels accounts for Windows' weird border/offset around windows. the high value was chosen to ensure that
    // it works for high DPI displays as well. there is probably no more screen in use that's only 400 pixels wide or high.
    Win32.GetWindowRect(RootHandle, out var pos);
    if (pos.X < -200 || pos.X > 200 || pos.Y < -200 || pos.Y > 200)
    {
      // also forget _posRestore so that pseudo-maximisation is used again when maximized on the primary monitor later
      _posRestore = null;
      return;
    }

    // restore to previous size (not maximized)
    Win32.ShowWindow(RootHandle, Win32.SW_RESTORE);

    Win32.RECT newPos;
    if (_posRestore == null)
    {
      newPos = CalcPseudoMaximizedRect();

      Win32.GetWindowRect(RootHandle, out Win32.RECT wRect);
      _posRestore = wRect;
    }
    else
    {
      // a position is remembered; restore it
      newPos = _posRestore.Value;
      _posRestore = null;
    }

    // if Top/Left is below zero, the behavior of SetWindowPos is unexpected...
    var newX = newPos.Left < 0 ? 0 : newPos.Left;
    var newY = newPos.Top < 0 ? 0 : newPos.Top;

    Win32.SetWindowPos(RootHandle, 0, newX, newY, newPos.Width, newPos.Height, Win32.SWP_NOZORDER);
  }

  private Win32.RECT CalcPseudoMaximizedRect()
  {
    // no position is remembered; calculate position for pseudo-maximized window and remember previous size
    int width = (int)SystemParameters.MaximizedPrimaryScreenWidth - MainWindow.TaskbarWidth;
    int height = (int)SystemParameters.MaximizedPrimaryScreenHeight;
    int x = MainWindow.TaskbarSide == ESide.Right ? 0 : MainWindow.TaskbarWidth;
    int y = 0;

    // Apply adjustment for Windows' weird border/offset around windows
    // TODO: consider DPI and/or use this method instead of manually picked values:
    // Win32.AdjustWindowRectExForDpi(ref posMaximized, wi.dwStyle, false, wi.dwExStyle, 96);
    y += 5;
    width -= 16;
    height -= 26;
    return new Win32.RECT(x, y, x + width, y + height);
  }

  private void RefreshIsActive(IntPtr foregroundWindowHandle, IEnumerable<IntPtr> childWindowHandles)
  {
    // keep active state untouched if FreedomTaskbar itself is the active window
    if (Application.Current.MainWindow?.IsActive == true) return;

    IsActive = childWindowHandles.Contains(foregroundWindowHandle);
  }

  private static void OnTitleChanged(OsWindow sender, DependencyPropertyChangedEventArgs e)
  {
    sender.TitleChanged?.Invoke((string)e.OldValue, (string)e.NewValue);
  }

  private static void OnIsActiveChanged(OsWindow sender, DependencyPropertyChangedEventArgs e)
  {
    sender.IsActiveChanged?.Invoke((bool)e.OldValue, (bool)e.NewValue);
  }
}