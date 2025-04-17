using FreedomTaskbar.Core;
using FreedomTaskbar.FrameworkExtensions;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace FreedomTaskbar.ViewModel;

using static DependencyPropertyRegistrar<OsWindow>;

public class OsWindow : DependencyObject
{
  public OsWindow(IntPtr handle, bool isForegroundWindow = false)
  {
    Title = string.Empty;
    Handle = handle;
    IsForegroundWindow = isForegroundWindow;

    RefreshTitle();

    var icon = System.Drawing.Icon.ExtractAssociatedIcon();
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

  private void RefreshTitle()
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
  }
}