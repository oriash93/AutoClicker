using System.Timers;
using System.Windows;

namespace AutoClicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Dependency Properties

        #region Hours

        public int Hours
        {
            get { return (int)GetValue(HoursProperty); }
            set { SetValue(HoursProperty, value); }
        }

        public static readonly DependencyProperty HoursProperty =
            DependencyProperty.Register("Hours", typeof(int), typeof(MainWindow),
                new PropertyMetadata(0));

        #endregion Hours

        #region Minutes

        public int Minutes
        {
            get { return (int)GetValue(MinutesProperty); }
            set { SetValue(MinutesProperty, value); }
        }

        public static readonly DependencyProperty MinutesProperty =
            DependencyProperty.Register("Minutes", typeof(int), typeof(MainWindow),
                new PropertyMetadata(0));

        #endregion Minutes

        #region Seconds

        public int Seconds
        {
            get { return (int)GetValue(SecondsProperty); }
            set { SetValue(SecondsProperty, value); }
        }

        public static readonly DependencyProperty SecondsProperty =
            DependencyProperty.Register("Seconds", typeof(int), typeof(MainWindow),
                new PropertyMetadata(0));

        #endregion Seconds

        #region Milliseconds

        public int Milliseconds
        {
            get { return (int)GetValue(MillisecondsProperty); }
            set { SetValue(MillisecondsProperty, value); }
        }

        public static readonly DependencyProperty MillisecondsProperty =
            DependencyProperty.Register("Milliseconds", typeof(int), typeof(MainWindow),
                new PropertyMetadata(100));

        #endregion Milliseconds

        #region SelectedMouseButton

        public MouseButton SelectedMouseButton
        {
            get { return (MouseButton)GetValue(SelectedMouseButtonProperty); }
            set { SetValue(SelectedMouseButtonProperty, value); }
        }

        public static readonly DependencyProperty SelectedMouseButtonProperty =
            DependencyProperty.Register("SelectedMouseButton", typeof(MouseButton), typeof(MainWindow),
                new PropertyMetadata(default(MouseButton)));

        #endregion SelectedMouseButton

        #region SelectedMouseAction

        public MouseAction SelectedMouseAction
        {
            get { return (MouseAction)GetValue(SelectedMouseActionProperty); }
            set { SetValue(SelectedMouseActionProperty, value); }
        }

        public static readonly DependencyProperty SelectedMouseActionProperty =
            DependencyProperty.Register("SelectedMouseAction", typeof(MouseAction), typeof(MainWindow),
                new PropertyMetadata(default(MouseAction)));


        #endregion SelectedClickTypeSelectedMouseAction

        #region SelectedRepeatMode

        public RepeatMode SelectedRepeatMode
        {
            get { return (RepeatMode)GetValue(SelectedRepeatModeProperty); }
            set { SetValue(SelectedRepeatModeProperty, value); }
        }

        public static readonly DependencyProperty SelectedRepeatModeProperty =
            DependencyProperty.Register("SelectedRepeatMode", typeof(RepeatMode), typeof(MainWindow),
                new PropertyMetadata(default(RepeatMode)));

        #endregion SelectedRepeatMode

        #region SelectedLocationMode

        public LocationMode SelectedLocationMode
        {
            get { return (LocationMode)GetValue(SelectedLocationModeProperty); }
            set { SetValue(SelectedLocationModeProperty, value); }
        }

        public static readonly DependencyProperty SelectedLocationModeProperty =
            DependencyProperty.Register("SelectedLocationMode", typeof(LocationMode), typeof(MainWindow),
                new PropertyMetadata(default(LocationMode)));

        #endregion SelectedLocationMode

        #endregion Dependency Properties

        public int Interval
        {
            get
            {
                return Milliseconds + Seconds * 1000 + Minutes * 60 * 1000 + Hours * 60 * 60 * 1000;
            }
        }

        #region Fields

        private Timer clickTimer;

        #region Mouse Consts

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;
        public const int MOUSEEVENTF_MIDDLEUP = 0x0020;
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x0040;

        #endregion Mouse Consts

        #endregion Fields

        public MainWindow()
        {
            clickTimer = new Timer();
            clickTimer.Elapsed += OnClickTimerElapsed;

            DataContext = this;
            InitializeComponent();
        }

        #region External Methods

        //This is a replacement for Cursor.Position in WinForms
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        #endregion External Methods

        #region Events

        private void OnClickTimerElapsed(object sender, ElapsedEventArgs e)
        {
            int xPosition = 300;
            int yPosition = 300;
            int duration = 0;
            Dispatcher.Invoke(() =>
            {
                PerformMouseClick(SelectedMouseButton, xPosition, yPosition, duration);
            });
        }

        private void OnStartButtonClicked(object sender, RoutedEventArgs e)
        {
            clickTimer.Interval = Interval;
            clickTimer.Start();
        }

        private void OnStopButtonClicked(object sender, RoutedEventArgs e)
        {
            clickTimer.Stop();
        }

        #endregion Events

        #region Helper Methods

        private void PerformMouseClick(MouseButton selectedMouseButton, int xPosition, int yPosition, int duration)
        {
            Dispatcher.Invoke(() =>
            {
                switch (selectedMouseButton)
                {
                    case MouseButton.Left:
                        LeftMouseClick(xPosition, yPosition);
                        break;
                    case MouseButton.Right:
                        RightMouseClick(xPosition, yPosition);
                        break;
                    case MouseButton.Middle:
                        MiddleMouseClick(xPosition, yPosition);
                        break;
                }
            });
        }

        private static void LeftMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }

        private static void RightMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_RIGHTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, xpos, ypos, 0, 0);
        }

        private static void MiddleMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_MIDDLEDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_MIDDLEUP, xpos, ypos, 0, 0);
        }

        #endregion Helper Methods
    }
}