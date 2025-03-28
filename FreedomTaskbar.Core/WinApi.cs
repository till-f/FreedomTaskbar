using System.Runtime.InteropServices;

namespace FreedomTaskbar.Core
{
  public class WinApi
  {
    [DllImport("User32.dll")] 
    public static extern bool EnumDesktopWindows(IntPtr hDesktop, IntPtr lpfn, IntPtr lParam);
  }
}
