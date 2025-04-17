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

  [DllImport("user32.dll")]
  public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

  [DllImport("user32.dll")]
  public static extern IntPtr GetWindow(IntPtr hwnd, uint uCmd);
  
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


  public const uint GW_OWNER = 4;


  [StructLayout(LayoutKind.Sequential)]
  public struct WINDOWINFO
  {
    public uint cbSize;
    public RECT rcWindow;
    public RECT rcClient;
    public uint dwStyle;
    public uint dwExStyle;
    public uint dwWindowStatus;
    public uint cxWindowBorders;
    public uint cyWindowBorders;
    public ushort atomWindowType;
    public ushort wCreatorVersion;

    public WINDOWINFO(Boolean? filler) : this()
    {
      cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
    }
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct RECT
  {
    public int Left, Top, Right, Bottom;

    public RECT(int left, int top, int right, int bottom)
    {
      Left = left;
      Top = top;
      Right = right;
      Bottom = bottom;
    }

    public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

    public int X
    {
      get { return Left; }
      set { Right -= (Left - value); Left = value; }
    }

    public int Y
    {
      get { return Top; }
      set { Bottom -= (Top - value); Top = value; }
    }

    public int Height
    {
      get { return Bottom - Top; }
      set { Bottom = value + Top; }
    }

    public int Width
    {
      get { return Right - Left; }
      set { Right = value + Left; }
    }

    public System.Drawing.Point Location
    {
      get { return new System.Drawing.Point(Left, Top); }
      set { X = value.X; Y = value.Y; }
    }

    public System.Drawing.Size Size
    {
      get { return new System.Drawing.Size(Width, Height); }
      set { Width = value.Width; Height = value.Height; }
    }

    public static implicit operator System.Drawing.Rectangle(RECT r)
    {
      return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
    }

    public static implicit operator RECT(System.Drawing.Rectangle r)
    {
      return new RECT(r);
    }

    public static bool operator ==(RECT r1, RECT r2)
    {
      return r1.Equals(r2);
    }

    public static bool operator !=(RECT r1, RECT r2)
    {
      return !r1.Equals(r2);
    }

    public bool Equals(RECT r)
    {
      return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
    }

    public override bool Equals(object obj)
    {
      if (obj is RECT)
        return Equals((RECT)obj);
      else if (obj is System.Drawing.Rectangle)
        return Equals(new RECT((System.Drawing.Rectangle)obj));
      return false;
    }

    public override int GetHashCode()
    {
      return ((System.Drawing.Rectangle)this).GetHashCode();
    }

    public override string ToString()
    {
      return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
    }
  }
}