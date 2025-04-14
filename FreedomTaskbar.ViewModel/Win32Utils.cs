using System.Text;
using FreedomTaskbar.Core;

namespace FreedomTaskbar.ViewModel;

public static class Win32Utils
{
  public static List<Win32Window> GetOpenWindows()
  {
    var shellWindow = Win32.GetShellWindow();
    var foregroundWindow = Win32.GetForegroundWindow();

    List<Win32Window> windows = [];

    Win32.EnumWindows(delegate (IntPtr hWnd, int _)
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

      windows.Add(new Win32Window(hWnd, hWnd == foregroundWindow));
      return true;
    }, 0);

    return windows;
  }
}