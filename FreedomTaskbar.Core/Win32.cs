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
  public static extern IntPtr SetFocus(IntPtr hWnd);

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
  
  public const int SW_HIDE = 0;
  public const int SW_SHOWNORMAL = 1;
  public const int SW_SHOWMINIMIZED = 2;
  public const int SW_SHOWMAXIMIZED = 3;
  public const int SW_SHOWNOACTIVATE = 4;
  public const int SW_SHOW = 5;
  public const int SW_MINIMIZE = 6;
  public const int SW_SHOWMINNOACTIVE = 7;
  public const int SW_SHOWNA = 8;
  public const int SW_RESTORE = 9;
  public const int SW_SHOWDEFAULT = 10;
  public const int SW_FORCEMINIMIZE = 11;


  public const uint GW_OWNER = 4;

  public const ulong WS_POPUP = 0x80000000L;
  public const ulong WS_CHILD = 0x40000000L;
  public const ulong WS_VISIBLE = 0x10000000L;
  public const ulong WS_EX_APPWINDOW = 0x00040000L;
  public const ulong WS_EX_TOOLWINDOW = 0x00000080L;
  public const ulong WS_EX_NOACTIVATE = 0x08000000L;

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

    public override bool Equals(object? obj)
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