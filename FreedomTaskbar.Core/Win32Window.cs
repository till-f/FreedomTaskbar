namespace FreedomTaskbar.Core;

public class Win32Window(IntPtr handle, int processId)
{
  public IntPtr Handle { get; } = handle;

  public int ProcessId { get; } = processId;
}