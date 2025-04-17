namespace FreedomTaskbar.Core;

public class Win32Window(IntPtr handle, IntPtr rootHandle)
{
  public IntPtr Handle { get; } = handle;

  public IntPtr RootHandle { get; } = rootHandle;

  public bool IsRootWindow => Handle == RootHandle;
}