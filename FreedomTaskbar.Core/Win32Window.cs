namespace FreedomTaskbar.Core;

public class Win32Window(IntPtr handle)
{
  public IntPtr Handle { get; } = handle;
}