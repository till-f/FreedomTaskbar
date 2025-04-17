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
  public WindowButton(OsWindow osWindow)
  {
    InitializeComponent();

    OsWindow = osWindow;

    InnerButton.Click += OnInnerButtonClicked;
  }

  public static readonly DependencyProperty OsWindowProperty = RegisterProperty(x => x.OsWindow);
  public OsWindow OsWindow
  {
    get => (OsWindow)GetValue(OsWindowProperty);
    set => SetValue(OsWindowProperty, value);
  }

  private void OnInnerButtonClicked(object sender, RoutedEventArgs e)
  {
    Win32.SwitchToThisWindow(OsWindow.Handle, true);
  }
}