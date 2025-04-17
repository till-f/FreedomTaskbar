using System.Runtime.InteropServices;
using System.Text;

namespace FreedomTaskbar.Core;


/// <summary>
/// Wrapper für Win32 API
/// https://learn.microsoft.com/de-de/windows/win32/api/winuser/
/// </summary>
public class Win32
{
  public delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

  [DllImport("User32.dll")]
  public static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

  [DllImport("User32.dll")]
  public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

  [DllImport("User32.dll")]
  public static extern int GetWindowTextLength(IntPtr hWnd);

  [DllImport("User32.dll")]
  public static extern bool IsWindowVisible(IntPtr hWnd);

  [DllImport("User32.dll")]
  public static extern IntPtr GetForegroundWindow();

  [DllImport("User32.dll")]
  public static extern bool SetForegroundWindow(IntPtr hWnd);

  [DllImport("User32.dll")]
  public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

  [DllImport("User32.dll")]
  public static extern IntPtr SetActiveWindow(IntPtr hWnd);

  [DllImport("User32.dll")]
  public static extern bool SwitchToThisWindow(IntPtr hWnd, bool isTabbedToWindow);
  
  [DllImport("User32.dll")]
  public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

  [DllImport("User32.dll")]
  public static extern IntPtr GetShellWindow();

  [DllImport("user32.dll", SetLastError = true)]
  public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

  /// <summary>
  /// Hides the window and activates another window.
  /// </summary>
  public const int SW_HIDE = 0;

  /// <summary>
  /// Activates and displays a window. If the window is minimized, maximized, or arranged, the system restores it to its original size and position. An application should specify this flag when displaying the window for the first time.
  /// </summary>
  public const int SW_SHOWNORMAL = 1;
  
  /// <summary>
  /// Activates the window and displays it as a minimized window.
  /// </summary>
  public const int SW_SHOWMINIMIZED = 2;

  /// <summary>
  /// Activates the window and displays it as a maximized window.
  /// </summary>
  public const int SW_SHOWMAXIMIZED = 3;

  /// <summary>
  /// Displays a window in its most recent size and position. This value is similar to SW_SHOWNORMAL, except that the window is not activated.
  /// </summary>
  public const int SW_SHOWNOACTIVATE = 4;

  /// <summary>
  /// Activates the window and displays it in its current size and position.
  /// </summary>
  public const int SW_SHOW = 5;

  /// <summary>
  /// Minimizes the specified window and activates the next top-level window in the Z order.
  /// </summary>
  public const int SW_MINIMIZE = 6;

  /// <summary>
  /// Displays the window as a minimized window. This value is similar to SW_SHOWMINIMIZED, except the window is not activated.
  /// </summary>
  public const int SW_SHOWMINNOACTIVE = 7;

  /// <summary>
  /// Displays the window in its current size and position. This value is similar to SW_SHOW, except that the window is not activated.
  /// </summary>
  public const int SW_SHOWNA = 8;

  /// <summary>
  /// Activates and displays the window. If the window is minimized, maximized, or arranged, the system restores it to its original size and position. An application should specify this flag when restoring a minimized window.
  /// </summary>
  public const int SW_RESTORE = 9;

  /// <summary>
  /// Sets the show state based on the SW_ value specified in the STARTUPINFO structure passed to the CreateProcess function by the program that started the application.
  /// </summary>
  public const int SW_SHOWDEFAULT = 10;

  /// <summary>
  /// Minimizes a window, even if the thread that owns the window is not responding. This flag should only be used when minimizing windows from a different thread.
  /// </summary>
  public const int SW_FORCEMINIMIZE = 11;
}