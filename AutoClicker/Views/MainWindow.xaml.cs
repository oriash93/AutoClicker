using System;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoClicker.Enums;
using AutoClicker.Models;
using AutoClicker.Utils;
using Serilog;
using MathNet.Numerics.Distributions;
using CheckBox = System.Windows.Controls.CheckBox;
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
        public AutoClickerSettings AutoClickerSettings
        {
            get { return (AutoClickerSettings)GetValue(CurrentSettingsProperty); }
            set { SetValue(CurrentSettingsProperty, value); }
        }

        public static readonly DependencyProperty CurrentSettingsProperty =
           DependencyProperty.Register(nameof(AutoClickerSettings), typeof(AutoClickerSettings), typeof(MainWindow),
               new UIPropertyMetadata(SettingsUtils.CurrentSettings.AutoClickerSettings));

        private int timesRepeated = 0;
        private Timer clickTimer;
        private double cpsMean = 1.0;
        private double cpsSDev = 0.25;
        private MathNet.Numerics.Distributions.Normal cpsDistribution;

        private readonly Uri runningIconUri =
            new Uri(Constants.RUNNING_ICON_RESOURCE_PATH, UriKind.Relative);

        private NotifyIcon systemTrayIcon;
        private SystemTrayMenu systemTrayMenu;
        private AboutWindow aboutWindow = null;
        private SettingsWindow settingsWindow = null;
        private CaptureMouseScreenCoordinatesWindow captureMouseCoordinatesWindow;

        private ImageSource _defaultIcon;
        private IntPtr _mainWindowHandle;
        private HwndSource _source;

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

            SettingsUtils.HotKeyChangedEvent += SettingsUtils_HotKeyChangedEvent;
            SettingsUtils_HotKeyChangedEvent(this, new HotkeyChangedEventArgs()
            {
                Hotkey = SettingsUtils.CurrentSettings.HotkeySettings.StartHotkey,
                Operation = Operation.Start
            });
            SettingsUtils_HotKeyChangedEvent(this, new HotkeyChangedEventArgs()
            {
                Hotkey = SettingsUtils.CurrentSettings.HotkeySettings.StopHotkey,
                Operation = Operation.Stop
            });
            SettingsUtils_HotKeyChangedEvent(this, new HotkeyChangedEventArgs()
            {
                Hotkey = SettingsUtils.CurrentSettings.HotkeySettings.ToggleHotkey,
                Operation = Operation.Toggle
            });

            _defaultIcon = Icon;

            RadioButtonSelectedLocationMode_CurrentLocation.Checked += RadioButtonSelectedLocationMode_CurrentLocationOnChecked;

            InitializeSystemTrayMenu();
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(StartStopHooks);

            SettingsUtils.HotKeyChangedEvent -= SettingsUtils_HotKeyChangedEvent;
            UnregisterHotkey(Constants.START_HOTKEY_ID);
            UnregisterHotkey(Constants.STOP_HOTKEY_ID);
            UnregisterHotkey(Constants.TOGGLE_HOTKEY_ID);

            systemTrayIcon.Click -= SystemTrayIcon_Click;
            systemTrayIcon.Dispose();

            systemTrayMenu.SystemTrayMenuActionEvent -= SystemTrayMenu_SystemTrayMenuActionEvent;
            systemTrayMenu.Dispose();

            Log.Information("Application closing");
            Log.Debug("==================================================");

            base.OnClosed(e);
        }

        #endregion Life Cycle

        #region Commands

        private void StartCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            int interval = CalculateInterval();
            cpsMean = (interval <= 0) ? 0.5 : 1000 / (double)interval;
            cpsDistribution = new Normal(cpsMean, cpsSDev);
            clickTimer.Interval = (int)Math.Round(1000 / cpsDistribution.Sample());
            timesRepeated = 0;

            Log.Information("Starting operation, cps_avg={cps}, cps_std_dev={dev}, interval={interval}ms", cpsMean, cpsSDev, clickTimer.Interval);
            clickTimer.Start();

            Icon = new BitmapImage(runningIconUri);
            Title += Constants.MAIN_WINDOW_TITLE_RUNNING;
            systemTrayIcon.Text += Constants.MAIN_WINDOW_TITLE_RUNNING;
        }

        private void StartCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanStartOperation();
        }

        private void StopCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Information("Stopping operation");
            clickTimer.Stop();

            ResetTitle();
            Icon = _defaultIcon;
        }

        private void StopCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = clickTimer.Enabled;
        }

        private void ToggleCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (clickTimer.Enabled)
                StopCommand_Execute(sender, e);
            else
                StartCommand_Execute(sender, e);
        }

        private void ToggleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanStartOperation() | clickTimer.Enabled;
        }

        private void SaveSettingsCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Information("Saving Settings");
            SettingsUtils.SetApplicationSettings(AutoClickerSettings);
        }

        private void HotkeySettingsCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (settingsWindow == null)
            {
                settingsWindow = new SettingsWindow();
                settingsWindow.Closed += (o, args) => settingsWindow = null;
            }

            settingsWindow.Show();
        }

        private void ExitCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            Exit();
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        private void AboutCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (aboutWindow == null)
            {
                aboutWindow = new AboutWindow();
                aboutWindow.Closed += (o, args) => aboutWindow = null;
            }

            aboutWindow.Show();
        }

        private void CaptureMouseScreenCoordinatesCommand_Execute(
            object sender,
            ExecutedRoutedEventArgs e
        )
        {
            if (captureMouseCoordinatesWindow == null)
            {
                captureMouseCoordinatesWindow = new CaptureMouseScreenCoordinatesWindow();
                captureMouseCoordinatesWindow.Closed += (o, args) => captureMouseCoordinatesWindow = null;
                captureMouseCoordinatesWindow.OnCoordinatesCaptured += (o, point) =>
                {
                    TextBoxPickedXValue.Text = point.X.ToString();
                    TextBoxPickedYValue.Text = point.Y.ToString();
                    RadioButtonSelectedLocationMode_PickedLocation.IsChecked = true;
                };
            }

            captureMouseCoordinatesWindow.Show();
        }

        #endregion Commands

        #region Helper Methods

        private int CalculateInterval()
        {
            return AutoClickerSettings.Milliseconds
                + (AutoClickerSettings.Seconds * 1000)
                + (AutoClickerSettings.Minutes * 60 * 1000)
                + (AutoClickerSettings.Hours * 60 * 60 * 1000);
        }

        private bool IsIntervalValid()
        {
            return CalculateInterval() > 0;
        }

        private int DistributedInterval()
        {
            /* Instead of constant if/else checking to avoid zero division, just always skew my 1 ms.
             * Shouldn't affect CPS meaningfully in practice. */
            return 1 + (int)Math.Round(1000 / cpsDistribution.Sample());
        }

        private bool CanStartOperation()
        {
            return !clickTimer.Enabled && IsRepeatModeValid() && IsIntervalValid();
        }

        private int GetTimesToRepeat()
        {
            return AutoClickerSettings.SelectedRepeatMode == RepeatMode.Count ? AutoClickerSettings.SelectedTimesToRepeat : -1;
        }

        private Point GetSelectedPosition()
        {
            return AutoClickerSettings.SelectedLocationMode == LocationMode.CurrentLocation ?
                MouseCursor.Position : new Point(AutoClickerSettings.PickedXValue, AutoClickerSettings.PickedYValue);
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
            return AutoClickerSettings.SelectedMouseAction == MouseAction.Single ? 1 : 2;
        }

        private bool IsRepeatModeValid()
        {
            return AutoClickerSettings.SelectedRepeatMode == RepeatMode.Infinite
                || (AutoClickerSettings.SelectedRepeatMode == RepeatMode.Count && AutoClickerSettings.SelectedTimesToRepeat > 0);
        }

        private void ResetTitle()
        {
            Title = Constants.MAIN_WINDOW_TITLE_DEFAULT;
            if (systemTrayIcon != null)
            {
                systemTrayIcon.Text = Constants.MAIN_WINDOW_TITLE_DEFAULT;
            }
        }

        private void InitializeSystemTrayMenu()
        {
            systemTrayIcon = new NotifyIcon
            {
                Visible = true,
                Icon = AssemblyUtils.GetApplicationIcon()
            };

            systemTrayIcon.Click += SystemTrayIcon_Click;
            systemTrayIcon.Text = Constants.MAIN_WINDOW_TITLE_DEFAULT;
            systemTrayMenu = new SystemTrayMenu();
            systemTrayMenu.SystemTrayMenuActionEvent += SystemTrayMenu_SystemTrayMenuActionEvent;
        }

        private void ReRegisterHotkey(int hotkeyId, KeyMapping hotkey)
        {
            UnregisterHotkey(hotkeyId);
            RegisterHotkey(hotkeyId, hotkey);
        }

        private void RegisterHotkey(int hotkeyId, KeyMapping hotkey)
        {
            Log.Information("RegisterHotkey with hotkeyId {HotkeyId} and hotkey {Hotkey}", hotkeyId, hotkey.DisplayName);
            User32ApiUtils.RegisterHotKey(_mainWindowHandle, hotkeyId, Constants.MOD_NONE, hotkey.VirtualKeyCode);
        }

        private void UnregisterHotkey(int hotkeyId)
        {
            Log.Information("UnregisterHotkey with hotkeyId {HotkeyId}", hotkeyId);
            if (User32ApiUtils.UnregisterHotKey(_mainWindowHandle, hotkeyId))
                return;
            Log.Warning("No hotkey registered on {HotkeyId}", hotkeyId);
        }

        #endregion Helper Methods

        #region Event Handlers

        private void OnClickTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                InitMouseClick();
                timesRepeated++;
                clickTimer.Interval = DistributedInterval();

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
                switch (AutoClickerSettings.SelectedMouseButton)
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
                var setCursorPos = User32ApiUtils.SetCursorPosition(xPos, yPos);
                if (!setCursorPos)
                {
                    Log.Error($"Could not set the mouse cursor.");
                }

                User32ApiUtils.ExecuteMouseEvent(mouseDownAction | mouseUpAction, xPos, yPos, 0, 0);
            }
        }

        private IntPtr StartStopHooks(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            int hotkeyId = wParam.ToInt32();
            if (msg == Constants.WM_HOTKEY && hotkeyId == Constants.START_HOTKEY_ID || hotkeyId == Constants.STOP_HOTKEY_ID || hotkeyId == Constants.TOGGLE_HOTKEY_ID)
            {
                int virtualKey = ((int)lParam >> 16) & 0xFFFF;
                if (virtualKey == SettingsUtils.CurrentSettings.HotkeySettings.StartHotkey.VirtualKeyCode && CanStartOperation())
                {
                    StartCommand_Execute(null, null);
                }
                if (virtualKey == SettingsUtils.CurrentSettings.HotkeySettings.StopHotkey.VirtualKeyCode && clickTimer.Enabled)
                {
                    StopCommand_Execute(null, null);
                }
                if (virtualKey == SettingsUtils.CurrentSettings.HotkeySettings.ToggleHotkey.VirtualKeyCode && CanStartOperation() | clickTimer.Enabled)
                {
                    ToggleCommand_Execute(null, null);
                }
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void SettingsUtils_HotKeyChangedEvent(object sender, HotkeyChangedEventArgs e)
        {
            Log.Information("HotKeyChangedEvent with operation {Operation} and hotkey {Hotkey}", e.Operation, e.Hotkey.DisplayName);
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
                case Operation.Toggle:
                    ReRegisterHotkey(Constants.TOGGLE_HOTKEY_ID, e.Hotkey);
                    toggleButton.Content = $"{Constants.MAIN_WINDOW_TOGGLE_BUTTON_CONTENT} ({e.Hotkey.DisplayName})";
                    break;
                default:
                    Log.Warning("Operation {Operation} not supported!", e.Operation);
                    throw new NotSupportedException($"Operation {e.Operation} not supported!");
            }
        }

        private void SystemTrayIcon_Click(object sender, EventArgs e)
        {
            systemTrayMenu.IsOpen = true;
            systemTrayMenu.Focus();
        }

        private void SystemTrayMenu_SystemTrayMenuActionEvent(object sender, SystemTrayMenuActionEventArgs e)
        {
            switch (e.Action)
            {
                case SystemTrayMenuAction.Show:
                    Show();
                    break;
                case SystemTrayMenuAction.Hide:
                    Hide();
                    break;
                case SystemTrayMenuAction.Exit:
                    Exit();
                    break;
                default:
                    Log.Warning("Action {Action} not supported!", e.Action);
                    throw new NotSupportedException($"Action {e.Action} not supported!");
            }
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (aboutWindow == null)
            {
                aboutWindow = new AboutWindow();
                aboutWindow.Closed += (o, args) => aboutWindow = null;
            }

            aboutWindow.Show();
        }

        private void MinimizeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            systemTrayMenu.ToggleMenuItemsVisibility(true);
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Exit();
        }

        private void RadioButtonSelectedLocationMode_CurrentLocationOnChecked(
            object sender,
            RoutedEventArgs e
        )
        {
            TextBoxPickedXValue.Text = string.Empty;
            TextBoxPickedYValue.Text = string.Empty;
        }

        #endregion Event Handlers

        private void TopMostCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            Topmost = checkbox.IsChecked.Value;
        }
    }
}