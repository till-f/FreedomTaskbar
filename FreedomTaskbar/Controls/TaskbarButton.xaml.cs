using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FreedomTaskbar.Core;
using FreedomTaskbar.ViewModel;
using FreedomTaskbar.WpfExtensions;

namespace FreedomTaskbar.Controls;

using static DependencyPropertyRegistrar<TaskbarButton>;

/// <summary>
/// Interaction logic for TaskButton.xaml
/// </summary>
public partial class TaskbarButton : UserControl
{
  public static readonly SolidColorBrush ColorInactive = new (Color.FromRgb(80, 80, 80));
  public static readonly SolidColorBrush ColorInactiveHover = new (Color.FromRgb(128, 128, 128));
  public static readonly SolidColorBrush ColorActive = new (Color.FromRgb(80, 140, 80));
  public static readonly SolidColorBrush ColorActiveHover = new (Color.FromRgb(90, 180, 90));

  public TaskbarButton(OsWindow window)
  {
    InitializeComponent();

    Window = window;

    window.IsActiveChanged += OnIsActiveChanged;
    
    InnerButton.Click += OnInnerButtonClicked;
    InnerButton.MouseEnter += OnInnerButtonMouseEntered;
    InnerButton.MouseLeave += OnInnerButtonMouseLeft;

    UpdateBackground();
  }

  public static readonly DependencyProperty WindowProperty = RegisterProperty(x => x.Window);
  public OsWindow Window
  {
    get => (OsWindow)GetValue(WindowProperty);
    set => SetValue(WindowProperty, value);
  }

  private void OnInnerButtonClicked(object sender, RoutedEventArgs e)
  {
    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
    {
      var si = Window.Process?.MainModule?.FileName;
      if (si != null && si.ToLower().EndsWith(".exe"))
      {
        Process.Start(si);
      }                   
    }
    else
    {
      Win32.SwitchToThisWindow(Window.RootHandle, true);
    }
  }

  private void OnIsActiveChanged(bool oldValue, bool newValue)
  {
    UpdateBackground();
  }

  private void OnInnerButtonMouseLeft(object sender, MouseEventArgs e)
  {
    UpdateBackground();
  }

  private void OnInnerButtonMouseEntered(object sender, MouseEventArgs e)
  {
    UpdateBackground();
  }

  private void UpdateBackground()
  {
    var isHovered = InnerButton.IsMouseOver;

    if (Window.IsActive)
    {
      InnerButton.Background = isHovered ? ColorActiveHover : ColorActive;
    }
    else
    {
      InnerButton.Background = isHovered ? ColorInactiveHover : ColorInactive;
    }
  }
}