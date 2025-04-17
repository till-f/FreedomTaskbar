using System.Diagnostics;

namespace FreedomTaskbar.Core;

public static class Win32Utils
{
  public static List<Win32Window> GetOpenWindows()
  {
    var shellWindow = Win32.GetShellWindow();

    List<Win32Window> windows = [];

    Win32.EnumWindows(delegate (nint hWnd, int _)
    {
      if (hWnd == shellWindow)
      {
        return true;
      }
      
      if (!Win32.IsWindowVisible(hWnd))
      {
        return true;
      }

      if (!IsShownInTaskBar(hWnd))
      {
        return true;
      }

      var length = Win32.GetWindowTextLength(hWnd);
      if (length == 0)
      {
        return true;
      }

      windows.Add(new Win32Window(hWnd, GetRootWindow(hWnd)));
      return true;
    }, 0);

    return windows;
  }

  public static IntPtr GetRootWindow(IntPtr hWnd)
  {
    var parent = hWnd;

    while (parent != 0)
    {
      hWnd = parent;
      parent = Win32.GetWindow(hWnd, Win32.GW_OWNER);
    }

    return hWnd;
  }

  public static int? GetProcessIdForWindowHandle(IntPtr hWnd)
  {
    var result = Win32.GetWindowThreadProcessId(hWnd, out uint processId);
    if (result == 0)
    {
      return null;
    }

    return (int)processId;
  }

  public static string? GetExecutablePathForWindow(IntPtr hWnd)
  {
    try
    {
      var processId = GetProcessIdForWindowHandle(hWnd);
      if (processId == null)
      {
        return null;
      }

      var process = Process.GetProcessById((int)processId);
      return process.MainModule?.FileName;
    }
    catch (Exception)
    {
      return null;
    }
  }

  public static bool IsShownInTaskBar(IntPtr hWnd)
  {
    ulong WS_POPUP   = 0x80000000L;
    ulong WS_CHILD   = 0x40000000L;
    ulong WS_VISIBLE = 0x10000000L;

    ulong WS_EX_APPWINDOW = 0x00040000L;
    ulong WS_EX_TOOLWINDOW = 0x00000080L;
    ulong WS_EX_NOACTIVATE = 0x08000000L;

    Win32.WINDOWINFO wi = new ();
    Win32.GetWindowInfo(hWnd, ref wi);

    // Extended styles
    var isAppWindow = (wi.dwExStyle & WS_EX_APPWINDOW) != 0L;
    var isNotToolWindow = (wi.dwExStyle & WS_EX_TOOLWINDOW) == 0L;
    var isNotInactiveWindow = (wi.dwExStyle & WS_EX_NOACTIVATE) == 0L;

    var condition1 = isAppWindow || (isNotToolWindow && isNotInactiveWindow);
    if (condition1) return true;

    // Basic styles
    var isNotPopupWindow = (wi.dwStyle & WS_POPUP) == 0L;
    var isNotChildWindow = (wi.dwStyle & WS_CHILD) == 0L;
    var isVisible = (wi.dwStyle & WS_VISIBLE) != 0L;

    var condition2 = isNotPopupWindow && isNotChildWindow && isVisible;
    return condition2;
  }
}