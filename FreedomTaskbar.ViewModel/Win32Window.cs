using FreedomTaskbar.Core;
using System.Text;
using System.Windows;

namespace FreedomTaskbar.ViewModel;

using static DependencyPropertyRegistrar<Win32Window>;

public class Win32Window : DependencyObject
{
  public Win32Window(IntPtr handle, bool isForegroundWindow = false)
  {
    Title = string.Empty;
    Handle = handle;
    IsForegroundWindow = isForegroundWindow;

    RefreshTitle();
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