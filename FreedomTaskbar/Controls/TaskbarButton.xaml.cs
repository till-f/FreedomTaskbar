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

  public event Action<(TaskbarButton TargetButton, string DroppedWindowHandle)>? WindowHandleDropped;

  private Point? _dragStartPos;

  public TaskbarButton(OsWindow window)
  {
    InitializeComponent();

    Window = window;

    window.IsActiveChanged += OnIsActiveChanged;
    
    InnerButton.Click += OnInnerButtonClicked;
    InnerButton.MouseEnter += OnInnerButtonMouseEntered;
    InnerButton.MouseLeave += OnInnerButtonMouseLeft;

    InnerButton.PreviewMouseLeftButtonDown += OnInnerButtonPreviewMouseLeftButtonDown;
    InnerButton.PreviewMouseLeftButtonUp += OnInnerButtonPreviewMouseLeftButtonUp;
    InnerButton.PreviewMouseMove += OnInnerButtonPreviewMouseMove;
    InnerButton.DragOver += OnInnerButtonDragOver;
    InnerButton.Drop += OnInnerButtonDrop;

    UpdateBackground();
  }

  private void OnInnerButtonPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
  {
    _dragStartPos = e.GetPosition(InnerButton);
  }

  private void OnInnerButtonPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
  {
    _dragStartPos = null;
  }

  private void OnInnerButtonPreviewMouseMove(object sender, MouseEventArgs e)
  {
    if (_dragStartPos == null) return;

    if (e.LeftButton != MouseButtonState.Pressed) return;

    var delta = e.GetPosition(InnerButton) - _dragStartPos.Value;
    if (Math.Abs(delta.X) > 4 || Math.Abs(delta.Y) > 4)
    {
      // reset _dragStartPos before calling DoDragDrop, otherwise this handler is called twice, starting a second drag in parallel.
      _dragStartPos = null;
      DragDrop.DoDragDrop(this, Window.RootHandle.ToString(), DragDropEffects.Move);
    }
  }

  private void OnInnerButtonDragOver(object sender, DragEventArgs e)
  {
    e.Handled = true;
    e.Effects = DragDropEffects.None;

    var handle = GetDraggedWindowHandled(e.Data);
    if (handle == null) return;

    e.Effects = DragDropEffects.Move;
  }

  private void OnInnerButtonDrop(object sender, DragEventArgs e)
  {
    var handle = GetDraggedWindowHandled(e.Data);
    if (handle == null) return;

    e.Handled = true;
    WindowHandleDropped?.Invoke((this, handle));
  }

  private string? GetDraggedWindowHandled(IDataObject data)
  {
    if (!data.GetDataPresent(DataFormats.StringFormat)) return null;

    var handleStr = data.GetData(DataFormats.StringFormat) as string;
    if (handleStr == Window.RootHandle.ToString()) return null;

    return handleStr;
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

    _dragStartPos = null;
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

  private void MenuItemClose_OnClick(object sender, RoutedEventArgs e)
  {
    Window.Close();
  }
  
  private void MenuItemKill_OnClick(object sender, RoutedEventArgs e)
  {
    Window.KillProcess();
  }
}