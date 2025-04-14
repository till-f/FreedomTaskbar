using System.Windows;
using System.Windows.Controls;
using FreedomTaskbar.Controls;
using FreedomTaskbar.Core;
using FreedomTaskbar.ViewModel;

namespace FreedomTaskbar;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();

    RefreshWindowList();
  }

  public void RefreshWindowList()
  {
    var windows = Win32Utils.GetOpenWindows();

    foreach (var window in windows)
    {
      WindowsStackPanel.Children.Add(new WindowButton(window));
    }
  }
}