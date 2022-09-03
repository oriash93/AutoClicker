using AutoClicker.Utils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
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
            
            Title = Constants.CAPTURE_MOUSE_COORDINATES_WINDOW_TITLE;
            var screens = Screen.AllScreens;

            // Need to do some special screen dimension calculation here to accomodate multiple monitors.
            // This works with horizontal, vertical and a combination of horizontal & vertical.
            // (e.g. 3 monitors total, 2 are side by side horizontally and the 3rd
            // is above/below the others) and vise versa.

            // Find the screen that has the largest X offset and then add the screen width to that.
            // This tells us the total maximum number of X pixels for the multi-monitor setup.
            var maxX = screens.Max(s => s.Bounds.X);
            Width = maxX + screens.First(s => s.Bounds.X == maxX).Bounds.Width;

            // Find the screen that has the largest Y offset and then add the screen height to that.
            // This tells us the total maximum number of Y pixels for the multi-monitor setup.
            var maxY = screens.Max(s => s.Bounds.Y);
            Height = maxY + screens.First(s => s.Bounds.Y == maxY).Bounds.Height;

            WindowStyle = WindowStyle.None;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Top = 0;
            Left = 0;
            ResizeMode = ResizeMode.NoResize;
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

        #endregion
    }
}
