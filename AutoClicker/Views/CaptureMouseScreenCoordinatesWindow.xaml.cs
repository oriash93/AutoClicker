using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using AutoClicker.Utils;
using Serilog;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Drawing.Point;
using WPFCursor = System.Windows.Forms.Cursor;

namespace AutoClicker.Views
{
    public partial class CaptureMouseScreenCoordinatesWindow : Window
    {
        #region Life Cycle

        public CaptureMouseScreenCoordinatesWindow()
        {
            DataContext = this;
            InitializeComponent();

            Log.Information("Opening capture mouse coordinates window");

            Title = Constants.CAPTURE_MOUSE_COORDINATES_WINDOW_TITLE;
            Width = 0;
            Height = 0;
            Top = 0;
            Left = 0;
            WindowStyle = WindowStyle.None;
            WindowStartupLocation = WindowStartupLocation.Manual;
            ResizeMode = ResizeMode.NoResize;

            var screens = Screen.AllScreens;
            Log.Debug($"Total number of screens detected={screens.Length}");

            // Need to do some special screen dimension calculation here to accomodate multiple monitors.
            // This works with horizontal, vertical and a combination of horizontal & vertical.
            // (e.g. 3 monitors total, 2 are side by side horizontally and the 3rd
            // is above/below the others) and vise versa.

            foreach (Screen screen in screens)
            {
                Log.Debug(screen.ToString());

                // Find the lowest X & Y screen values, it's possible for screens to have negative
                // values depending on how the multi monitor setup is configured
                if (screen.Bounds.X < Left)
                {
                    Left = screen.Bounds.X;
                }

                if (screen.Bounds.Y < Top)
                {
                    Top = screen.Bounds.Y;
                }

                Width += screen.Bounds.Width;
                Height += screen.Bounds.Height;
            }

            Log.Information($"Set window size, Width={Width}, Height={Height}");
            Log.Information($"Set window position, Left={Left}, Top={Top}");
        }

        #endregion

        #region Event Handlers

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Point position = WPFCursor.Position;
            LabelXCoordinate.Content = position.X;
            LabelYCoordinate.Content = position.Y;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            Point position = WPFCursor.Position;
            OnCoordinatesCaptured?.Invoke(this, position);
            Log.Information($"Captured mouse position at {position.X}, {position.Y}");
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
            Log.Debug($"Rendered window size, width={RenderSize.Width}, height={RenderSize.Height}");
            Log.Debug($"Rendered window position, left={Left}, top={Top}");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Log.Information("Closing capture mouse coordinates window");
        }

        #endregion
    }
}
