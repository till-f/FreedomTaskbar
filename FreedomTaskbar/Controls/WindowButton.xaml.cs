using System.Windows;
using System.Windows.Controls;
using FreedomTaskbar.Core;
using FreedomTaskbar.ViewModel;

namespace FreedomTaskbar.Controls;

using static DependencyPropertyRegistrar<WindowButton>;

/// <summary>
/// Interaction logic for TaskButton.xaml
/// </summary>
public partial class WindowButton : UserControl
{
  public WindowButton(Win32Window win32Window)
  {
    InitializeComponent();

    Win32Window = win32Window;

    InnerButton.Click += OnInnerButtonClicked;
  }

  public static readonly DependencyProperty Win32WindowProperty = RegisterProperty(x => x.Win32Window);
  public Win32Window Win32Window
  {
    get => (Win32Window)GetValue(Win32WindowProperty);
    set => SetValue(Win32WindowProperty, value);
  }

  private void OnInnerButtonClicked(object sender, RoutedEventArgs e)
  {
    Win32.SwitchToThisWindow(Win32Window.Handle, true);
  }
}