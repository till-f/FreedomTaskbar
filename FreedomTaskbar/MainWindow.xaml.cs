using System.Windows;
using FreedomTaskbar.Core;

namespace FreedomTaskbar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var result = WinApi.EnumDesktopWindows(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }
    }
}