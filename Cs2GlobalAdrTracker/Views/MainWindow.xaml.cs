using Cs2GlobalAdrTracker.Logic;
using Cs2GlobalAdrTracker.ViewModels;
using Serilog;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Cs2GlobalAdrTracker.Views
{
    public partial class MainWindow : Window
    {
        #region Remove from Alt+Tab Menu
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        #endregion

        public MainWindow()
        {
            this.InitializeComponent();
            ((MainWindowViewModel)this.DataContext).Instance = this;

            ContextMenu s = new();
            MenuItem mm = new() { Header = "Exit", Foreground = Brushes.Black };
            mm.Click += (o, e) => { this.Close(); };
            s.Items.Add(mm);

            this.ContextMenu = s;
            ((MainWindowViewModel)this.DataContext).ContextMenuTaskbar = s;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Globals.Configuration.RuntimeConfiguration.WindowStartupLocation != null)
            {
                this.Left = Globals.Configuration.RuntimeConfiguration.WindowStartupLocation.X;
                this.Top = Globals.Configuration.RuntimeConfiguration.WindowStartupLocation.Y;
            }
            else
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            _ = SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TOOLWINDOW);

            Log.Verbose("Window loaded");
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
                if (Globals.Configuration.RuntimeConfiguration.WindowStartupLocation == null)
                {
                    Globals.Configuration.RuntimeConfiguration.WindowStartupLocation = new();
                }

                Globals.Configuration.RuntimeConfiguration.WindowStartupLocation = new()
                {
                    X = this.Left,
                    Y = this.Top
                };
                Globals.Configuration.Save();

                Log.Verbose("Window relocated to coords:\nX: {Left}\nY: {Top}", this.Left, this.Top);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Log.Verbose("Window closing");
            ((MainWindowViewModel)this.DataContext)?.DisposeNotifyIcon();
        }
    }
}