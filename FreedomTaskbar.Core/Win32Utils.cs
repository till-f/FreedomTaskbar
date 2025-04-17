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

      var length = Win32.GetWindowTextLength(hWnd);
      if (length == 0)
      {
        return true;
      }

      windows.Add(new Win32Window(hWnd));
      return true;
    }, 0);

    return windows;
  }

  public static string? GetExecutablePathForWindow(IntPtr hWnd)
  {
    try
    {
      var result = Win32.GetWindowThreadProcessId(hWnd, out uint processId);
      if (result == 0)
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
}