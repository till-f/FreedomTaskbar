using FreedomTaskbar.Core;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FreedomTaskbar.WpfExtensions;
using System.Diagnostics;

namespace FreedomTaskbar.ViewModel;

using static DependencyPropertyRegistrar<OsWindow>;

public class OsWindow : DependencyObject
{
  public OsWindow(Win32Window rootWindow, IntPtr foregroundWindowHandle, IList<IntPtr> childWindowHandles)
  {
    RootHandle = rootWindow.Handle;
    Process = Win32Utils.GetProcessForWindowHandle(RootHandle);

    Title = string.Empty;
    IsActive = false;

    RefreshInternal(foregroundWindowHandle, childWindowHandles, true);
  }

  public IntPtr RootHandle { get; }

  public Process? Process { get; }

  public event Action<string, string>? TitleChanged;
  public static readonly DependencyPropertyKey TitleProperty = RegisterProperty(x => x.Title).OnChange(OnTitleChanged);
  public string Title
  {
    get => (string)GetValue(TitleProperty.DependencyProperty);
    private set => SetValue(TitleProperty, value);
  }

  public static readonly DependencyPropertyKey IconProperty = RegisterProperty(x => x.Icon);
  public ImageSource Icon
  {
    get => (ImageSource)GetValue(IconProperty.DependencyProperty);
    private set => SetValue(IconProperty, value);
  }

  public event Action<bool, bool>? IsActiveChanged;
  public static readonly DependencyPropertyKey IsActiveProperty = RegisterProperty(x => x.IsActive).OnChange(OnIsActiveChanged);
  public bool IsActive
  {
    get => (bool)GetValue(IsActiveProperty.DependencyProperty);
    private set => SetValue(IsActiveProperty, value);
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

  public void RefreshInternal(IntPtr foregroundWindowHandle, IList<IntPtr> childWindowHandles, bool initStaticProperties)
  {
    RefreshTitle();
    RefreshIsActive(foregroundWindowHandle, childWindowHandles);

    ChildWindows.Clear();
    ChildWindows.AddRange(childWindowHandles);

    if (initStaticProperties)
    {
      InitIcon();
    }
  }

  private void RefreshTitle()
  {
    var length = Win32.GetWindowTextLength(RootHandle);
    if (length == 0)
    {
      Title = string.Empty;
    }

    var builder = new StringBuilder(length);
    Win32.GetWindowText(RootHandle, builder, length + 1);
    Title = builder.ToString();
  }
  
  private void InitIcon()
  {
    try
    {
      var exePath = Process?.MainModule?.FileName;
      if (exePath == null) return;

      var icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
      if (icon == null) return;

      var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
      Icon = bitmapSource;
    }
    catch
    {
      // Win32Exception: "Access is denied." may be thrown when MainModule cannot be accessed for
      // elevated process. This can only be avoided by running FreedomTaskbar as administrator.
      Debug.WriteLine($"Failed to extract icon for window '{Title}'");
    }
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