using FreedomTaskbar.Core;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FreedomTaskbar.WpfExtensions;

namespace FreedomTaskbar.ViewModel;

using static DependencyPropertyRegistrar<OsWindow>;

public class OsWindow : DependencyObject
{
  public OsWindow(Win32Window w, IntPtr? foregroundWindowHandle = null)
  {
    Title = string.Empty;
    IsForegroundWindow = false;
    Handle = w.Handle;

    RefreshIcon();

    RefreshState(foregroundWindowHandle);
  }

  public static readonly DependencyProperty HandleProperty = RegisterProperty(x => x.Handle);
  public IntPtr Handle
  {
    get => (IntPtr)GetValue(HandleProperty);
    set => SetValue(HandleProperty, value);
  }

  public static readonly DependencyProperty TitleProperty = RegisterProperty(x => x.Title);
  public string Title
  {
    get => (string)GetValue(TitleProperty);
    set => SetValue(TitleProperty, value);
  }

  public static readonly DependencyProperty IconProperty = RegisterProperty(x => x.Icon);
  public ImageSource Icon
  {
    get => (ImageSource)GetValue(IconProperty);
    set => SetValue(IconProperty, value);
  }

  public static readonly DependencyProperty IsForegroundWindowProperty = RegisterProperty(x => x.IsForegroundWindow);
  public bool IsForegroundWindow
  {
    get => (bool)GetValue(IsForegroundWindowProperty);
    set => SetValue(IsForegroundWindowProperty, value);
  }

  private void RefreshState(IntPtr? foregroundWindowHandle = null)
  {
    var length = Win32.GetWindowTextLength(Handle);
    if (length == 0)
    {
      Title = string.Empty;
      return;
    }

    var builder = new StringBuilder(length);
    Win32.GetWindowText(Handle, builder, length + 1);
    Title = builder.ToString();

    if (!foregroundWindowHandle.HasValue)
    {
      foregroundWindowHandle = Win32.GetForegroundWindow();
    }

    IsForegroundWindow = Handle == foregroundWindowHandle;
  }

  private void RefreshIcon()
  {
    var exePath = Win32Utils.GetExecutablePathForWindow(Handle);
    if (exePath == null) return;

    var icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
    if (icon == null) return;

    var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
    Icon = bitmapSource;
  }
}