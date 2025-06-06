﻿using System.Diagnostics;
using System.Drawing;

namespace FreedomTaskbar.Core;

public static class Win32Utils
{
  public static List<Win32Window> GetOpenWindows()
  {
    List<Win32Window> windows = [];

    Win32.EnumWindows(delegate (nint hWnd, int _)
    {
      var rootWindowHandle = GetRootWindow(hWnd);
      if (hWnd == rootWindowHandle && !IsApplicableForRootWindow(hWnd))
      {
        return true;
      }

      windows.Add(new Win32Window(hWnd, rootWindowHandle));
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

  public static Process? GetProcessForWindowHandle(IntPtr hWnd)
  {
    try
    {
      var processId = GetProcessIdForWindowHandle(hWnd);
      if (processId == null)
      {
        return null;
      }

      return Process.GetProcessById((int)processId);
    }
    catch (Exception)
    {
      return null;
    }
  }

  public static bool IsApplicableForRootWindow(IntPtr hWnd)
  {
    // Always skip invisible windows
    if (!Win32.IsWindowVisible(hWnd))
    {
      return false;
    }

    Win32.WINDOWINFO wi = new ();
    Win32.GetWindowInfo(hWnd, ref wi);

    // Skip Popup, Child, Tool and NoActive windows, if not explicitly marked as AppWindow
    return (wi.dwExStyle & Win32.WS_EX_APPWINDOW) != 0L ||
           ((wi.dwStyle & Win32.WS_POPUP) == 0L
            && (wi.dwStyle & Win32.WS_CHILD) == 0L
            && (wi.dwExStyle & Win32.WS_EX_TOOLWINDOW) == 0L
            && (wi.dwExStyle & Win32.WS_EX_NOACTIVATE) == 0L);
  }

  public static Win32.RECT GetWindowClientRect(IntPtr hWnd)
  {
    Win32.WINDOWINFO wi = new();
    Win32.GetWindowInfo(hWnd, ref wi);
    return wi.rcClient;
  }
}