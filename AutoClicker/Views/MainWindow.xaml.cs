using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using AutoClicker.Enums;
using AutoClicker.Utils;
using Serilog;
using MouseAction = AutoClicker.Enums.MouseAction;
using MouseButton = AutoClicker.Enums.MouseButton;
using MouseCursor = System.Windows.Forms.Cursor;
using NotifyIcon = System.Windows.Forms.NotifyIcon;

using Point = System.Drawing.Point;
using Timer = System.Timers.Timer;

namespace AutoClicker.Views
{
    public partial class MainWindow : Window
    {
        #region Dependency Properties

        public int Hours
        {
            get => (int)GetValue(HoursProperty);
            set => SetValue(HoursProperty, value);
        }

        public static readonly DependencyProperty HoursProperty =
            DependencyProperty.Register(nameof(Hours), typeof(int), typeof(MainWindow));

        public int Minutes
        {
            get => (int)GetValue(MinutesProperty);
            set => SetValue(MinutesProperty, value);
        }

        public static readonly DependencyProperty MinutesProperty =
            DependencyProperty.Register(nameof(Minutes), typeof(int), typeof(MainWindow));

        public int Seconds
        {
            get => (int)GetValue(SecondsProperty);
            set => SetValue(SecondsProperty, value);
        }

        public static readonly DependencyProperty SecondsProperty =
            DependencyProperty.Register(nameof(Seconds), typeof(int), typeof(MainWindow));

        public int Milliseconds
        {
            get => (int)GetValue(MillisecondsProperty);
            set => SetValue(MillisecondsProperty, value);
        }

        public static readonly DependencyProperty MillisecondsProperty =
            DependencyProperty.Register(nameof(Milliseconds), typeof(int), typeof(MainWindow));

        public MouseButton SelectedMouseButton
        {
            get => (MouseButton)GetValue(SelectedMouseButtonProperty);
            set => SetValue(SelectedMouseButtonProperty, value);
        }

        public static readonly DependencyProperty SelectedMouseButtonProperty =
            DependencyProperty.Register(nameof(SelectedMouseButton), typeof(MouseButton), typeof(MainWindow));

        public MouseAction SelectedMouseAction
        {
            get => (MouseAction)GetValue(SelectedMouseActionProperty);
            set => SetValue(SelectedMouseActionProperty, value);
        }

        public static readonly DependencyProperty SelectedMouseActionProperty =
            DependencyProperty.Register(nameof(SelectedMouseAction), typeof(MouseAction), typeof(MainWindow));
        public RepeatMode SelectedRepeatMode
        {
            get => (RepeatMode)GetValue(SelectedRepeatModeProperty);
            set => SetValue(SelectedRepeatModeProperty, value);
        }

        public static readonly DependencyProperty SelectedRepeatModeProperty =
            DependencyProperty.Register(nameof(SelectedRepeatMode), typeof(RepeatMode), typeof(MainWindow));

        public LocationMode SelectedLocationMode
        {
            get => (LocationMode)GetValue(SelectedLocationModeProperty);
            set => SetValue(SelectedLocationModeProperty, value);
        }

        public static readonly DependencyProperty SelectedLocationModeProperty =
            DependencyProperty.Register(nameof(SelectedLocationMode), typeof(LocationMode), typeof(MainWindow));

        public int PickedXValue
        {
            get => (int)GetValue(PickedXValueProperty);
            set => SetValue(PickedXValueProperty, value);
        }

        public static readonly DependencyProperty PickedXValueProperty =
            DependencyProperty.Register(nameof(PickedXValue), typeof(int), typeof(MainWindow));

        public int PickedYValue
        {
            get => (int)GetValue(PickedYValueProperty);
            set => SetValue(PickedYValueProperty, value);
        }

        public static readonly DependencyProperty PickedYValueProperty =
            DependencyProperty.Register(nameof(PickedYValue), typeof(int), typeof(MainWindow));

        public int SelectedTimesToRepeat
        {
            get => (int)GetValue(SelectedTimesToRepeatProperty);
            set => SetValue(SelectedTimesToRepeatProperty, value);
        }

        public static readonly DependencyProperty SelectedTimesToRepeatProperty =
            DependencyProperty.Register(nameof(SelectedTimesToRepeat), typeof(int), typeof(MainWindow));

        #endregion Dependency Properties

        #region Fields

        private int timesRepeated = 0;
        private readonly Timer clickTimer;
        private NotifyIcon systemTrayIcon;
        private AboutWindow aboutWindow = null;
        private SettingsWindow settingsWindow = null;

        private IntPtr _mainWindowHandle;
        private HwndSource _source;

        #endregion Fields

        #region Life Cycle

        public MainWindow()
        {
            clickTimer = new Timer();
            clickTimer.Elapsed += OnClickTimerElapsed;

            DataContext = this;
            ResetTitle();
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _mainWindowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_mainWindowHandle);
            _source.AddHook(StartStopHooks);

            SettingsUtils.HotKeyChangedEvent += OnAppSettingsHotKeyChanged;
            RegisterHotkey(Constants.START_HOTKEY_ID, SettingsUtils.CurrentSettings.StartHotkey);
            RegisterHotkey(Constants.STOP_HOTKEY_ID, SettingsUtils.CurrentSettings.StopHotkey);

            InitializeSystemTrayIcon();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized && SettingsUtils.CurrentSettings.MinimizeToTray)
            {
                Hide();
            }

            base.OnStateChanged(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(StartStopHooks);

            SettingsUtils.HotKeyChangedEvent -= OnAppSettingsHotKeyChanged;
            UnregisterHotkey(Constants.START_HOTKEY_ID);
            UnregisterHotkey(Constants.STOP_HOTKEY_ID);

            systemTrayIcon.Click -= OnSystemTrayIconClick;

            base.OnClosed(e);
        }

        #endregion Life Cycle

        #region Commands

        #region Start Command

        private void StartCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            int interval = CalculateInterval();
            Log.Information("Starting operation, interval={Interval}ms", interval);

            timesRepeated = 0;
            clickTimer.Interval = interval;
            clickTimer.Start();
            Title += Constants.MAIN_WINDOW_TITLE_RUNNING;
        }

        private void StartCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanStart();
        }

        #endregion Start Command

        #region Stop Command

        private void StopCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Information("Stopping operation");
            clickTimer.Stop();
            ResetTitle();
        }

        private void StopCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = clickTimer.Enabled;
        }

        #endregion Stop Command

        #region HotkeySettings Command

        private void HotkeySettingsCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (settingsWindow == null)
            {
                settingsWindow = new SettingsWindow();
                settingsWindow.Closed += (o, args) => settingsWindow = null;
            }

            settingsWindow.Show();
        }

        #endregion HotkeySettings Command

        #region Exit Command

        private void ExitCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion Exit Command

        #region About Command

        private void AboutCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (aboutWindow == null)
            {
                aboutWindow = new AboutWindow();
                aboutWindow.Closed += (o, args) => aboutWindow = null;
            }

            aboutWindow.Show();
        }

        #endregion About Command

        #endregion Commands

        #region Helper Methods

        private int CalculateInterval()
        {
            return Milliseconds + (Seconds * 1000) + (Minutes * 60 * 1000) + (Hours * 60 * 60 * 1000);
        }

        private bool IsIntervalValid()
        {
            return CalculateInterval() > 0;
        }

        private bool CanStart()
        {
            return !clickTimer.Enabled && IsRepeatModeValid() && IsIntervalValid();
        }

        private int GetTimesToRepeat()
        {
            return SelectedRepeatMode == RepeatMode.Count ? SelectedTimesToRepeat : -1;
        }

        private Point GetSelectedPosition()
        {
            return SelectedLocationMode == LocationMode.CurrentLocation ? MouseCursor.Position : new Point(PickedXValue, PickedYValue);
        }

        private int GetSelectedXPosition()
        {
            return GetSelectedPosition().X;
        }

        private int GetSelectedYPosition()
        {
            return GetSelectedPosition().Y;
        }

        private int GetNumberOfMouseActions()
        {
            return SelectedMouseAction == MouseAction.Single ? 1 : 2;
        }

        private bool IsRepeatModeValid()
        {
            return SelectedRepeatMode == RepeatMode.Infinite || (SelectedRepeatMode == RepeatMode.Count && SelectedTimesToRepeat > 0);
        }

        private void ResetTitle()
        {
            Title = Constants.MAIN_WINDOW_TITLE_DEFAULT;
        }

        private void ReRegisterHotkey(int hotkeyId, KeyMapping hotkey)
        {
            UnregisterHotkey(hotkeyId);
            RegisterHotkey(hotkeyId, hotkey);
        }

        private void RegisterHotkey(int hotkeyId, KeyMapping hotkey)
        {
            Log.Information("RegisterHotkey with hotkeyId {HotkeyId} and hotkey {Hotkey}", hotkeyId, hotkey.DisplayName);
            User32Api.RegisterHotKey(_mainWindowHandle, hotkeyId, Constants.MOD_NONE, hotkey.VirtualKeyCode);
        }

        private void UnregisterHotkey(int hotkeyId)
        {
            Log.Information("UnregisterHotkey with hotkeyId {HotkeyId}", hotkeyId);
            if (User32Api.UnregisterHotKey(_mainWindowHandle, hotkeyId))
                return;
            Log.Warning("No hotkey registered on {HotkeyId}", hotkeyId);
            throw new InvalidOperationException($"No hotkey registered on {hotkeyId}");
        }

        #endregion Helper Methods

        #region Event Handlers

        private void OnClickTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                InitMouseClick();
                timesRepeated++;

                if (timesRepeated == GetTimesToRepeat())
                {
                    clickTimer.Stop();
                    ResetTitle();
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
                        PerformMouseClick(Constants.MOUSEEVENTF_LEFTDOWN, Constants.MOUSEEVENTF_LEFTUP, GetSelectedXPosition(), GetSelectedYPosition());
                        break;
                    case MouseButton.Right:
                        PerformMouseClick(Constants.MOUSEEVENTF_RIGHTDOWN, Constants.MOUSEEVENTF_RIGHTUP, GetSelectedXPosition(), GetSelectedYPosition());
                        break;
                    case MouseButton.Middle:
                        PerformMouseClick(Constants.MOUSEEVENTF_MIDDLEDOWN, Constants.MOUSEEVENTF_MIDDLEUP, GetSelectedXPosition(), GetSelectedYPosition());
                        break;
                }
            });
        }

        private void PerformMouseClick(int mouseDownAction, int mouseUpAction, int xPos, int yPos)
        {
            for (int i = 0; i < GetNumberOfMouseActions(); ++i)
            {
                User32Api.SetCursorPosition(xPos, yPos);
                User32Api.ExecuteMouseEvent(mouseDownAction | mouseUpAction, xPos, yPos, 0, 0);
            }
        }

        private IntPtr StartStopHooks(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            int hotkeyId = wParam.ToInt32();
            if (msg == Constants.WM_HOTKEY && hotkeyId == Constants.START_HOTKEY_ID || hotkeyId == Constants.STOP_HOTKEY_ID)
            {
                int virtualKey = ((int)lParam >> 16) & 0xFFFF;
                if (virtualKey == SettingsUtils.CurrentSettings.StartHotkey.VirtualKeyCode && CanStart())
                {
                    StartCommand_Execute(null, null);
                }
                if (virtualKey == SettingsUtils.CurrentSettings.StopHotkey.VirtualKeyCode && clickTimer.Enabled)
                {
                    StopCommand_Execute(null, null);
                }
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void OnAppSettingsHotKeyChanged(object sender, HotkeyChangedEventArgs e)
        {
            Log.Information("OnAppSettingsHotKeyChanged with operation {Operation} and hotkey {Hotkey}", e.Operation, e.Hotkey.DisplayName);
            switch (e.Operation)
            {
                case Operation.Start:
                    ReRegisterHotkey(Constants.START_HOTKEY_ID, e.Hotkey);
                    startButton.Content = $"{Constants.MAIN_WINDOW_START_BUTTON_CONTENT} ({e.Hotkey.DisplayName})";
                    break;
                case Operation.Stop:
                    ReRegisterHotkey(Constants.STOP_HOTKEY_ID, e.Hotkey);
                    stopButton.Content = $"{Constants.MAIN_WINDOW_STOP_BUTTON_CONTENT} ({e.Hotkey.DisplayName})";
                    break;
                default:
                    Log.Warning("Operation {Operation} not supported!", e.Operation);
                    throw new NotSupportedException($"Operation {e.Operation} not supported!");
            }
        }

        private void InitializeSystemTrayIcon()
        {
            systemTrayIcon = new NotifyIcon
            {
                Visible = true,
                Icon = Utilities.GetApplicationIcon()
            };

            systemTrayIcon.Click += OnSystemTrayIconClick;
        }

        private void OnSystemTrayIconClick(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Normal && !IsVisible)
            {
                Show();
                WindowState = WindowState.Normal;
            }
        }

        private void OnMenuItemCheckChanged(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                SettingsUtils.CurrentSettings.MinimizeToTray = menuItem.IsChecked;
            }
        }

        #endregion Event Handlers
    }
}