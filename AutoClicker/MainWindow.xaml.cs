using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using MouseCursor = System.Windows.Forms.Cursor;
using Point = System.Drawing.Point;

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

        #region PickedXValue

        public int PickedXValue
        {
            get { return (int)GetValue(PickedXValueProperty); }
            set { SetValue(PickedXValueProperty, value); }
        }

        public static readonly DependencyProperty PickedXValueProperty =
            DependencyProperty.Register("PickedXValue", typeof(int), typeof(MainWindow),
                new PropertyMetadata(0));

        #endregion PickedXValue

        #region PickedYValue

        public int PickedYValue
        {
            get { return (int)GetValue(PickedYValueProperty); }
            set { SetValue(PickedYValueProperty, value); }
        }

        public static readonly DependencyProperty PickedYValueProperty =
            DependencyProperty.Register("PickedYValue", typeof(int), typeof(MainWindow),
                new PropertyMetadata(0));

        #endregion PickedYValue

        #region SelectedTimesToRepeat

        public int SelectedTimesToRepeat
        {
            get { return (int)GetValue(SelectedTimesToRepeatProperty); }
            set { SetValue(SelectedTimesToRepeatProperty, value); }
        }

        public static readonly DependencyProperty SelectedTimesToRepeatProperty =
            DependencyProperty.Register("SelectedTimesToRepeat", typeof(int), typeof(MainWindow),
                new PropertyMetadata(0));

        #endregion SelectedTimesToRepeat

        #endregion Dependency Properties

        #region Fields

        private Timer clickTimer;
        private int Interval => Milliseconds + Seconds * 1000 + Minutes * 60 * 1000 + Hours * 60 * 60 * 1000;
        private int MouseActions => SelectedMouseAction == MouseAction.Single ? 1 : 2;
        private int TimesToRepeat => SelectedRepeatMode == RepeatMode.Count ? SelectedTimesToRepeat : -1;
        private int timesRepeated = 0;
        private Point CurrentCursorPosition => MouseCursor.Position;
        private Point SelectedPosition => SelectedLocationMode == LocationMode.CurrentLocation ?
                            CurrentCursorPosition :
                            new Point(PickedXValue, PickedYValue);

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

        #region Commands

        #region Start Command

        private void StartCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            timesRepeated = 0;
            clickTimer.Interval = Interval;
            clickTimer.Start();
        }

        private void StartCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !clickTimer.Enabled &&
                (SelectedRepeatMode == RepeatMode.Infinite ||
                (SelectedRepeatMode == RepeatMode.Count && SelectedTimesToRepeat > 0));
        }

        #endregion Start Command

        #region Stop Command

        private void StopCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            clickTimer.Stop();
        }

        private void StopCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = clickTimer.Enabled;
        }

        #endregion Stop Command

        #region Exit Command

        private void ExitCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion Exit Command

        #region About Command

        private void AboutCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show(
                "Created by Ori Ashual\n" +
                "github.com/oriash93",
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        #endregion About Command

        #endregion Commands

        #region External Methods

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        static extern bool SetCursorPosition(int x, int y);

        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        static extern void ExecuteMouseEvent(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        #endregion External Methods

        #region Helper Methods

        private void OnClickTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                timesRepeated++;
                InitMouseClick();

                if (timesRepeated == TimesToRepeat)
                {
                    clickTimer.Stop();
                }
            });
        }

        private void InitMouseClick()
        {
            Dispatcher.Invoke(() =>
            {
                switch (SelectedMouseButton)
                {
                    case MouseButton.Left:
                        PerformMouseClick(MOUSEEVENTF_LEFTDOWN, MOUSEEVENTF_LEFTUP, SelectedPosition.X, SelectedPosition.Y);
                        break;
                    case MouseButton.Right:
                        PerformMouseClick(MOUSEEVENTF_RIGHTDOWN, MOUSEEVENTF_RIGHTUP, SelectedPosition.X, SelectedPosition.Y);
                        break;
                    case MouseButton.Middle:
                        PerformMouseClick(MOUSEEVENTF_MIDDLEDOWN, MOUSEEVENTF_MIDDLEUP, SelectedPosition.X, SelectedPosition.Y);
                        break;
                }
            });
        }

        private void PerformMouseClick(int mouseDownAction, int mouseUpAction, int xPos, int yPos)
        {
            for (int i = 0; i < MouseActions; ++i)
            {
                SetCursorPosition(xPos, yPos);
                ExecuteMouseEvent(mouseDownAction | mouseUpAction, xPos, yPos, 0, 0);
            }
        }

        #endregion Helper Methods
    }
}