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
            var screens = Screen.AllScreens;
            Log.Debug($"Total screens detected: {screens.Length}");

            // Need to do some special screen dimension calculation here to accomodate multiple monitors.
            // This works with horizontal, vertical and a combination of horizontal & vertical.
            // (e.g. 3 monitors total, 2 are side by side horizontally and the 3rd
            // is above/below the others) and vise versa.

            // Find the screen that has the largest X offset and then add the screen width to that.
            // This tells us the total maximum number of X pixels for the multi-monitor setup.
            var minX = screens.Min(s => s.Bounds.X);
            var maxX = screens.Max(s => s.Bounds.X);
            Width = Math.Abs(minX) + maxX + screens.First(s => s.Bounds.X == maxX).Bounds.Width;
            Log.Information($"Min Screen X: {minX}");
            Log.Information($"Max Screen X: {maxX}");

            // Find the screen that has the largest Y offset and then add the screen height to that.
            // This tells us the total maximum number of Y pixels for the multi-monitor setup.
            var minY = screens.Min(s => s.Bounds.Y);
            var maxY = screens.Max(s => s.Bounds.Y);
            Height = Math.Abs(minY) + maxY + screens.First(s => s.Bounds.Y == maxY).Bounds.Height;
            Log.Information($"Min Screen Y: {minY}");
            Log.Information($"Max Screen Y: {maxY}");

            Log.Information($"Set window size. Width: {Width}, Height: {Height}");

            WindowStyle = WindowStyle.None;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Top = minY;
            Left = minX;
            ResizeMode = ResizeMode.NoResize;

            Log.Information($"Setting mouse capture window position.");
            Log.Information($"Left: {Left}, Top: {Top}");

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

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Log.Information("Closing window to capture mouse coordinates.");
        }

        #endregion
    }
}
