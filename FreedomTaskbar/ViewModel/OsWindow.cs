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
  public OsWindow(Win32Window w, IntPtr foregroundWindowHandle, IList<IntPtr> childWindowHandles)
  {
    Handle = w.Handle;
    Title = string.Empty;
    IsActive = false;

    RefreshInternal(foregroundWindowHandle, childWindowHandles, true);
  }

  public static readonly DependencyProperty HandleProperty = RegisterProperty(x => x.Handle);
  public IntPtr Handle
  {
    get => (IntPtr)GetValue(HandleProperty);
    set => SetValue(HandleProperty, value);
  }

  public event Action<string, string>? TitleChanged;
  public static readonly DependencyProperty TitleProperty = RegisterProperty(x => x.Title).OnChange(OnTitleChanged);
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

  public event Action<bool, bool>? IsActiveChanged;
  public static readonly DependencyProperty IsActiveProperty = RegisterProperty(x => x.IsActive).OnChange(OnIsActiveChanged);
  public bool IsActive
  {
    get => (bool)GetValue(IsActiveProperty);
    set => SetValue(IsActiveProperty, value);
  }

  public List<IntPtr> ChildWindows { get; } = new ();

  /// <summary>
  /// Queries the current state of the window from the OS and updates relevant properties.
  /// Note that <see cref="Icon"/> is not updated after initial construction.
  /// </summary>
  public void Refresh(IntPtr foregroundWindowHandle, IList<IntPtr> childWindowHandles)
  {
    RefreshInternal(foregroundWindowHandle, childWindowHandles, false);
  }

  public void RefreshInternal(IntPtr foregroundWindowHandle, IList<IntPtr> childWindowHandles, bool refreshIcon)
  {
    RefreshTitle();
    RefreshIsActive(foregroundWindowHandle, childWindowHandles);

    ChildWindows.Clear();
    ChildWindows.AddRange(childWindowHandles);

    if (refreshIcon)
    {
      RefreshIcon();
    }
  }

  private void RefreshTitle()
  {
    var length = Win32.GetWindowTextLength(Handle);
    if (length == 0)
    {
      Title = string.Empty;
    }

    var builder = new StringBuilder(length);
    Win32.GetWindowText(Handle, builder, length + 1);
    Title = builder.ToString();
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

  private void RefreshIsActive(IntPtr foregroundWindowHandle, IEnumerable<IntPtr> childWindowHandles)
  {
    IsActive = childWindowHandles.Contains(foregroundWindowHandle);
  }

  private static void OnTitleChanged(OsWindow sender, DependencyPropertyChangedEventArgs e)
  {
    sender.TitleChanged?.Invoke((string)e.OldValue, (string)e.NewValue);
  }

  private static void OnIsActiveChanged(OsWindow sender, DependencyPropertyChangedEventArgs e)
  {
    sender.IsActiveChanged?.Invoke((bool)e.OldValue, (bool)e.NewValue);
  }
}