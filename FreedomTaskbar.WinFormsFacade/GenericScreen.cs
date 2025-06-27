namespace FreedomTaskbar.WinFormsFacade;

/// <summary>
/// This class avoids the need to reference System.Windows.Forms in the main FreedomTaskbar project.
/// </summary>
public class GenericScreen
{
  private GenericScreen(int screenIndex, Rectangle bounds, bool isPrimary)
  {
    ScreenIndex = screenIndex;
    Bounds = bounds;
    IsPrimary = isPrimary;
  }

  public int ScreenIndex { get; }

  public Rectangle Bounds { get; }

  public bool IsPrimary { get; }

  public static GenericScreen[] AllScreens
  {
    get
    {
      var idx = 0;
      var screens = Screen.AllScreens;
      var genericScreens = new GenericScreen[screens.Length];

      foreach (var screen in screens)
      {
        genericScreens[idx] = new GenericScreen(idx, screen.Bounds, screen.Primary);
        idx++;
      }

      return genericScreens;
    }
  }
}