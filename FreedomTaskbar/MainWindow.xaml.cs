using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using FreedomTaskbar.Controls;
using FreedomTaskbar.ViewModel;
using Timer = System.Timers.Timer;

namespace FreedomTaskbar;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
  private readonly Timer _updateTimer;

  private readonly List<string> _excludedWindows = ["Settings", "BBar", "Windows Input Experience"];

  public MainWindow()
  {
    InitializeComponent();

    SetPosition();

    RefreshWindowList();

    _updateTimer = new Timer(200);
    _updateTimer.AutoReset = true;
    _updateTimer.Elapsed += OnUpdateTimeElapsed;
    _updateTimer.Start();
  }

  private void OnUpdateTimeElapsed(object? sender, ElapsedEventArgs e)
  {
    Dispatcher.InvokeAsync(RefreshWindowList);
  }

  private void SetPosition()
  {
    var width = 300;
    Top = 0;
    Left = SystemParameters.PrimaryScreenWidth - width;
    Height = SystemParameters.MaximizedPrimaryScreenHeight;
    Width = width;
  }

  private void RefreshWindowList()
  {
    var windows = Win32Utils.GetOpenWindows();

    WindowsStackPanel.Children.Clear();

    foreach (var window in windows)
    {
      if (_excludedWindows.Any(r => Regex.IsMatch(window.Title, r)))
      {
        continue;
      }

      WindowsStackPanel.Children.Add(new WindowButton(window));
    }
  }
}