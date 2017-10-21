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

        #region Fields

        private Timer clickTimer;
        private int Interval => Milliseconds + Seconds * 1000 + Minutes * 60 * 1000 + Hours * 60 * 60 * 1000;
        private int Times => SelectedMouseAction == MouseAction.Single ? 1 : 2;

        #region Mouse Consts

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;

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
            Dispatcher.Invoke(() =>
            {
                InitMouseClick(xPosition, yPosition);
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

        private void InitMouseClick(int xPosition, int yPosition)
        {
            Dispatcher.Invoke(() =>
            {
                switch (SelectedMouseButton)
                {
                    case MouseButton.Left:
                        PerformMouseClick(xPosition, yPosition, MOUSEEVENTF_LEFTDOWN, MOUSEEVENTF_LEFTUP);
                        break;
                    case MouseButton.Right:
                        PerformMouseClick(xPosition, yPosition, MOUSEEVENTF_RIGHTDOWN, MOUSEEVENTF_RIGHTUP);
                        break;
                    case MouseButton.Middle:
                        PerformMouseClick(xPosition, yPosition, MOUSEEVENTF_MIDDLEDOWN, MOUSEEVENTF_MIDDLEUP);
                        break;
                }
            });
        }

        private void PerformMouseClick(int xpos, int ypos, int mouseDownAction, int mouseUpAction)
        {
            SetCursorPos(xpos, ypos);
            for (int i = 0; i < Times; ++i)
                mouse_event(mouseDownAction | mouseUpAction, xpos, ypos, 0, 0);
        }

        #endregion Helper Methods
    }
}