using System;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
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
            get => (int)GetValue(HoursProperty);
            set => SetValue(HoursProperty, value);
        }

        public static readonly DependencyProperty HoursProperty =
            DependencyProperty.Register(nameof(Hours), typeof(int), typeof(MainWindow),
                new PropertyMetadata(0));

        #endregion Hours

        #region Minutes

        public int Minutes
        {
            get => (int)GetValue(MinutesProperty);
            set => SetValue(MinutesProperty, value);
        }

        public static readonly DependencyProperty MinutesProperty =
            DependencyProperty.Register(nameof(Minutes), typeof(int), typeof(MainWindow),
                new PropertyMetadata(0));

        #endregion Minutes

        #region Seconds

        public int Seconds
        {
            get => (int)GetValue(SecondsProperty);
            set => SetValue(SecondsProperty, value);
        }

        public static readonly DependencyProperty SecondsProperty =
            DependencyProperty.Register(nameof(Seconds), typeof(int), typeof(MainWindow),
                new PropertyMetadata(0));

        #endregion Seconds

        #region Milliseconds

        public int Milliseconds
        {
            get => (int)GetValue(MillisecondsProperty);
            set => SetValue(MillisecondsProperty, value);
        }

        public static readonly DependencyProperty MillisecondsProperty =
            DependencyProperty.Register(nameof(Milliseconds), typeof(int), typeof(MainWindow),
                new PropertyMetadata(100));

        #endregion Milliseconds

        #region SelectedMouseButton

        public MouseButton SelectedMouseButton
        {
            get => (MouseButton)GetValue(SelectedMouseButtonProperty);
            set => SetValue(SelectedMouseButtonProperty, value);
        }

        public static readonly DependencyProperty SelectedMouseButtonProperty =
            DependencyProperty.Register(nameof(SelectedMouseButton), typeof(MouseButton), typeof(MainWindow),
                new PropertyMetadata(default(MouseButton)));

        #endregion SelectedMouseButton

        #region SelectedMouseAction

        public MouseAction SelectedMouseAction
        {
            get => (MouseAction)GetValue(SelectedMouseActionProperty);
            set => SetValue(SelectedMouseActionProperty, value);
        }

        public static readonly DependencyProperty SelectedMouseActionProperty =
            DependencyProperty.Register(nameof(SelectedMouseAction), typeof(MouseAction), typeof(MainWindow),
                new PropertyMetadata(default(MouseAction)));


        #endregion SelectedMouseAction

        #region SelectedRepeatMode

        public RepeatMode SelectedRepeatMode
        {
            get => (RepeatMode)GetValue(SelectedRepeatModeProperty);
            set => SetValue(SelectedRepeatModeProperty, value);
        }

        public static readonly DependencyProperty SelectedRepeatModeProperty =
            DependencyProperty.Register(nameof(SelectedRepeatMode), typeof(RepeatMode), typeof(MainWindow),
                new PropertyMetadata(default(RepeatMode)));

        #endregion SelectedRepeatMode

        #region SelectedLocationMode

        public LocationMode SelectedLocationMode
        {
            get => (LocationMode)GetValue(SelectedLocationModeProperty);
            set => SetValue(SelectedLocationModeProperty, value);
        }

        public static readonly DependencyProperty SelectedLocationModeProperty =
            DependencyProperty.Register(nameof(SelectedLocationMode), typeof(LocationMode), typeof(MainWindow),
                new PropertyMetadata(default(LocationMode)));

        #endregion SelectedLocationMode

        #region PickedXValue

        public int PickedXValue
        {
            get => (int)GetValue(PickedXValueProperty);
            set => SetValue(PickedXValueProperty, value);
        }

        public static readonly DependencyProperty PickedXValueProperty =
            DependencyProperty.Register(nameof(PickedXValue), typeof(int), typeof(MainWindow),
                new PropertyMetadata(0));

        #endregion PickedXValue

        #region PickedYValue

        public int PickedYValue
        {
            get => (int)GetValue(PickedYValueProperty);
            set => SetValue(PickedYValueProperty, value);
        }

        public static readonly DependencyProperty PickedYValueProperty =
            DependencyProperty.Register(nameof(PickedYValue), typeof(int), typeof(MainWindow),
                new PropertyMetadata(0));

        #endregion PickedYValue

        #region SelectedTimesToRepeat

        public int SelectedTimesToRepeat
        {
            get => (int)GetValue(SelectedTimesToRepeatProperty);
            set => SetValue(SelectedTimesToRepeatProperty, value);
        }

        public static readonly DependencyProperty SelectedTimesToRepeatProperty =
            DependencyProperty.Register(nameof(SelectedTimesToRepeat), typeof(int), typeof(MainWindow),
                new PropertyMetadata(0));

        #endregion SelectedTimesToRepeat

        #endregion Dependency Properties

        #region Fields

        private const string defaultTitle = "AutoClicker";
        private const string runningTitle = " - Running...";
        private const string aboutWindowContent = "AutoClicker v2.1 \n\nCreated by Ori Ashual \ngithub.com/oriash93"; // TODO: Current version

        private readonly Timer clickTimer;
        private int timesRepeated = 0;
        private int TimesToRepeat => SelectedRepeatMode == RepeatMode.Count ? SelectedTimesToRepeat : -1;
        private int Interval => Milliseconds + Seconds * 1000 + Minutes * 60 * 1000 + Hours * 60 * 60 * 1000;
        private int NumMouseActions => SelectedMouseAction == MouseAction.Single ? 1 : 2;
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

        #region Keyboard Consts

        private const int HOTKEY_ID = 9000;
        private const int WM_HOTKEY = 0x0312;

        private const uint MOD_NONE = 0x0000;
        private const uint F6_KEY = 0x75;
        private const uint F7_KEY = 0x76;

        #endregion Keyboard Consts

        private IntPtr _windowHandle;
        private HwndSource _source;

        #endregion Fields

        #region Lifetime

        public MainWindow()
        {
            clickTimer = new Timer();
            clickTimer.Elapsed += OnClickTimerElapsed;

            DataContext = this;
            Title = defaultTitle;
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(StartStopHooks);

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, F6_KEY);
            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, F7_KEY);
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(StartStopHooks);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);

            base.OnClosed(e);
        }

        #endregion Lifetime

        #region Commands

        #region Start Command

        private void StartCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            timesRepeated = 0;
            clickTimer.Interval = Interval;
            clickTimer.Start();
            Title += runningTitle;
        }

        private void StartCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = !clickTimer.Enabled && IsRepeatModeValid();

        private bool IsRepeatModeValid()
            => SelectedRepeatMode == RepeatMode.Infinite ||
                (SelectedRepeatMode == RepeatMode.Count && SelectedTimesToRepeat > 0);

        #endregion Start Command

        #region Stop Command

        private void StopCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            clickTimer.Stop();
            Title = defaultTitle;
        }

        private void StopCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = clickTimer.Enabled;

        #endregion Stop Command

        #region Exit Command

        private void ExitCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion Exit Command

        #region About Command

        private void AboutCommand_Execute(object sender, ExecutedRoutedEventArgs e)
            => MessageBox.Show(aboutWindowContent, "About", MessageBoxButton.OK, MessageBoxImage.Information);

        #endregion About Command

        #endregion Commands

        #region External Methods

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        static extern bool SetCursorPosition(int x, int y);

        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        static extern void ExecuteMouseEvent(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

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
                    Title = defaultTitle;
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
            for (int i = 0; i < NumMouseActions; ++i)
            {
                SetCursorPosition(xPos, yPos);
                ExecuteMouseEvent(mouseDownAction | mouseUpAction, xPos, yPos, 0, 0);
            }
        }

        private IntPtr StartStopHooks(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                int vkey = ((int)lParam >> 16) & 0xFFFF;
                if (vkey == F6_KEY && !clickTimer.Enabled && IsRepeatModeValid())
                {
                    StartCommand_Execute(null, null);
                }
                if (vkey == F7_KEY && clickTimer.Enabled)
                {
                    StopCommand_Execute(null, null);
                }
                handled = true;
            }
            return IntPtr.Zero;
        }

        #endregion Helper Methods
    }
}