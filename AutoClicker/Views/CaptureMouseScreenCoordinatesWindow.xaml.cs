using AutoClicker.Utils;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Serilog;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Drawing.Point;

namespace AutoClicker.Views
{
    /// <summary>
    /// Interaction logic for CaptureMouseScreenCoordinatesWindow.xaml
    /// </summary>
    public partial class CaptureMouseScreenCoordinatesWindow : Window
    {
        #region Life Cycle

        public CaptureMouseScreenCoordinatesWindow()
        {
            DataContext = this;
            InitializeComponent();

            Log.Information("Opening window to capture mouse coordinates.");
            
            Title = Constants.CAPTURE_MOUSE_COORDINATES_WINDOW_TITLE;
            Width = 0;
            Height = 0;
            WindowStyle = WindowStyle.None;
            WindowStartupLocation = WindowStartupLocation.Manual;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;

            var screens = Screen.AllScreens;
            Log.Debug($"Total screens detected: {screens.Length}");
            
            // Need to do some special screen dimension calculation here to accomodate multiple monitors.
            // This works with horizontal, vertical and a combination of horizontal & vertical.
            // (e.g. 3 monitors total, 2 are side by side horizontally and the 3rd
            // is above/below the others) and vise versa.

            var minX = 0;
            var minY = 0;
            foreach (var screen in screens)
            {
                Log.Information(screen.ToString());

                // Find the lowest X & Y screen values, it's possible for screens to have negative
                // values depending on how the multi monitor setup is configured
                minX = screen.Bounds.X < minX ? screen.Bounds.X : minX;
                minY = screen.Bounds.Y < minY ? screen.Bounds.Y : minY;

                Width += screen.Bounds.Width;
                Height += screen.Bounds.Height;
            }
            Log.Information($"Min Screen X: {minX}");
            Log.Information($"Min Screen Y: {minY}");
            Log.Information($"Set window size. Width: {Width}, Height: {Height}");

            Top = minY;
            Left = minX;
            
            Log.Information($"Set window position. Left: {Left}, Top: {Top}");

            Log.Information("Opened window to capture mouse coordinates.");
        }

        #endregion

        #region Event Handlers

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var position = System.Windows.Forms.Cursor.Position;
            LabelXCoordinate.Content = position.X;
            LabelYCoordinate.Content = position.Y;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            var position = System.Windows.Forms.Cursor.Position;
            OnCoordinatesCaptured?.Invoke(this, position);
            Log.Information($"Captured mouse position: {position.X}, {position.Y}");
            Close();
        }

        public event EventHandler<Point> OnCoordinatesCaptured;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            Log.Information($"Rendered window size: Width: {RenderSize.Width}, Height: {RenderSize.Height}");
            Log.Information($"Rendered window position: Left:{Left}, Height: {Height}");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Log.Information("Closing window to capture mouse coordinates.");
        }

        #endregion
    }
}
