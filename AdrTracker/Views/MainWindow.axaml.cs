using AdrTracker.Logic;
using Avalonia.Controls;
using neXn.Ui.Avalonia;
using System.Linq;

namespace AdrTracker.Views
{
    public partial class MainWindow : Window
    {
        private readonly WindowDragHandler dragHandler;
        private readonly WindowManager windowManager;
        public MainWindow()
        {
            this.InitializeComponent();
            this.windowManager = new(this);
            this.dragHandler = new WindowDragHandler(this)
            {
                IsEnabled = true
            };

            this.dragHandler.PointerReleased += (s, e) =>
            {
                Globals.Configuration.RuntimeConfiguration.UserWindowLocation = new(this.Position.X, this.Position.Y);
                Globals.Configuration.Save();
            };
        }

        private void Window_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.windowManager.RestoreWindowLocation(Globals.Configuration.RuntimeConfiguration.UserWindowLocation);
        }

        private void ComboBox_PointerEntered(object sender, Avalonia.Input.PointerEventArgs e)
        {
            this.dragHandler.IsEnabled = false;
        }

        private void ComboBox_PointerExited(object sender, Avalonia.Input.PointerEventArgs e)
        {
            this.dragHandler.IsEnabled = true;
        }
    }
}