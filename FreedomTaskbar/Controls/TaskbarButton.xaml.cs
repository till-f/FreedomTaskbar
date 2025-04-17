using System.Windows;
using System.Windows.Controls;
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
  public TaskbarButton(OsWindow window)
  {
    InitializeComponent();

    Window = window;

    InnerButton.Click += OnInnerButtonClicked;
  }

  public static readonly DependencyProperty WindowProperty = RegisterProperty(x => x.Window);
  public OsWindow Window
  {
    get => (OsWindow)GetValue(WindowProperty);
    set => SetValue(WindowProperty, value);
  }

  private void OnInnerButtonClicked(object sender, RoutedEventArgs e)
  {
    Win32.SwitchToThisWindow(Window.Handle, true);
    //Win32.SetActiveWindow(Window.Handle);
    //Win32.SetForegroundWindow(OsWindow.Handle);
  }
}