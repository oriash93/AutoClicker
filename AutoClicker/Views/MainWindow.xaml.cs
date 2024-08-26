using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Timer clickTimer;
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
            _defaultIcon = Icon;

            SettingsUtils.HotkeyChangedEvent += SettingsUtils_HotkeyChangedEvent;
            SettingsUtils_HotkeyChangedEvent(this, new HotkeyChangedEventArgs()
            {
                Hotkey = SettingsUtils.CurrentSettings.HotkeySettings.StartHotkey,
                Operation = Operation.Start
            });
            SettingsUtils_HotkeyChangedEvent(this, new HotkeyChangedEventArgs()
            {
                Hotkey = SettingsUtils.CurrentSettings.HotkeySettings.StopHotkey,
                Operation = Operation.Stop
            });
            SettingsUtils_HotkeyChangedEvent(this, new HotkeyChangedEventArgs()
            {
                Hotkey = SettingsUtils.CurrentSettings.HotkeySettings.ToggleHotkey,
                Operation = Operation.Toggle
            });

            RadioButtonSelectedLocationMode_CurrentLocation.Checked += RadioButtonSelectedLocationMode_CurrentLocationOnChecked;

            InitializeSystemTrayMenu();
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(StartStopHooks);

            SettingsUtils.HotkeyChangedEvent -= SettingsUtils_HotkeyChangedEvent;

            foreach (int hotkeyId in Constants.ALL_HOTKEY_IDS)
            {
                DeregisterHotkey(hotkeyId);
            }

            RadioButtonSelectedLocationMode_CurrentLocation.Checked -= RadioButtonSelectedLocationMode_CurrentLocationOnChecked;

            DisposeSystemTrayMenu();

            Log.Information("Application closing");
            Log.Debug("==================================================");
            Log.CloseAndFlush();

            base.OnClosed(e);
        }

        #endregion Life Cycle

        #region Commands

        private void StartCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            int interval = CalculateInterval();
            Log.Information("Starting operation, interval={Interval}ms", interval);

            timesRepeated = 0;
            clickTimer.Interval = interval;
            clickTimer.Start();

            Icon = new BitmapImage(runningIconUri);
            Title += Constants.MAIN_WINDOW_TITLE_RUNNING;
            systemTrayIcon.Text += Constants.MAIN_WINDOW_TITLE_RUNNING;

            CommandManager.InvalidateRequerySuggested();
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

            CommandManager.InvalidateRequerySuggested();
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

        private void CaptureMouseScreenCoordinatesCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (captureMouseCoordinatesWindow == null)
            {
                captureMouseCoordinatesWindow = new CaptureMouseScreenCoordinatesWindow();
                captureMouseCoordinatesWindow.OnCoordinatesCaptured += CaptureMouseCoordinatesWindow_OnCoordinatesCaptured;
                captureMouseCoordinatesWindow.Closed += (o, args) =>
                {
                    captureMouseCoordinatesWindow.OnCoordinatesCaptured -= CaptureMouseCoordinatesWindow_OnCoordinatesCaptured;
                    captureMouseCoordinatesWindow = null;
                };
            }

            captureMouseCoordinatesWindow.Show();
        }

        private void CaptureMouseCoordinatesWindow_OnCoordinatesCaptured(object sender, Point point)
        {
            TextBoxPickedXValue.Text = point.X.ToString();
            TextBoxPickedYValue.Text = point.Y.ToString();
            RadioButtonSelectedLocationMode_PickedLocation.IsChecked = true;
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
                Icon = AssemblyUtils.GetApplicationIcon(),
                Text = Constants.MAIN_WINDOW_TITLE_DEFAULT
            };
            systemTrayIcon.Click += SystemTrayIcon_Click;

            systemTrayMenu = new SystemTrayMenu();
            systemTrayMenu.SystemTrayMenuActionEvent += SystemTrayMenu_SystemTrayMenuActionEvent;
        }

        private void DisposeSystemTrayMenu()
        {
            systemTrayIcon.Click -= SystemTrayIcon_Click;
            systemTrayIcon.Dispose();

            systemTrayMenu.SystemTrayMenuActionEvent -= SystemTrayMenu_SystemTrayMenuActionEvent;
            systemTrayMenu.Dispose();
        }

        private void ReRegisterHotkey(IEnumerable<int> hotkeyIds, KeyMapping hotkey, bool includeModifiers)
        {
            foreach (int hotkeyId in hotkeyIds)
            {
                DeregisterHotkey(hotkeyId);
            }
            RegisterHotkey(hotkeyIds, hotkey, includeModifiers);
        }

        private void RegisterHotkey(IEnumerable<int> hotkeyIds, KeyMapping hotkey, bool includeModifiers)
        {
            Log.Information("RegisterHotkey with hotkey={Hotkey}, includeModifiers={IncludeModifiers}", hotkey.DisplayName, includeModifiers);
            IEnumerable<(int, int)> hotkeyIdsToModifiers = Enumerable.Zip(hotkeyIds, Constants.MODIFIERS, (first, second) => ValueTuple.Create(first, second));
            if (includeModifiers)
            {
                foreach ((int, int) item in hotkeyIdsToModifiers)
                {
                    User32ApiUtils.RegisterHotkey(_mainWindowHandle, item.Item1, item.Item2, hotkey.VirtualKeyCode);
                }
            }
            else
            {
                User32ApiUtils.RegisterHotkey(_mainWindowHandle, hotkeyIdsToModifiers.ElementAt(0).Item1, hotkeyIdsToModifiers.ElementAt(0).Item2, hotkey.VirtualKeyCode);
            }
        }

        private void DeregisterHotkey(int hotkeyId)
        {
            Log.Information("DeregisterHotkey with hotkeyId={HotkeyId}", hotkeyId);
            if (User32ApiUtils.DeregisterHotkey(_mainWindowHandle, hotkeyId))
                return;
            Log.Debug("No hotkey registered on {HotkeyId}", hotkeyId);
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
                bool setCursorPos = User32ApiUtils.SetCursorPosition(xPos, yPos);
                if (!setCursorPos)
                {
                    Log.Error("Failed to set the mouse cursor!");
                }

                User32ApiUtils.ExecuteMouseEvent(mouseDownAction | mouseUpAction, xPos, yPos, 0, 0);
            }
        }

        private IntPtr StartStopHooks(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            int hotkeyId = wParam.ToInt32();
            if (msg == Constants.WM_HOTKEY && Constants.ALL_HOTKEY_IDS.Contains(hotkeyId))
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

        private void SettingsUtils_HotkeyChangedEvent(object sender, HotkeyChangedEventArgs e)
        {
            Log.Information("HotkeyChangedEvent with operation={Operation}, hotkey={Hotkey}, includeModifiers={IncludeModifiers}", e.Operation, e.Hotkey.DisplayName, e.IncludeModifiers);
            switch (e.Operation)
            {
                case Operation.Start:
                    ReRegisterHotkey(Constants.START_HOTKEY_IDS, e.Hotkey, e.IncludeModifiers);
                    startButton.Content = $"{Constants.MAIN_WINDOW_START_BUTTON_CONTENT} ({e.Hotkey.DisplayName})";
                    break;
                case Operation.Stop:
                    ReRegisterHotkey(Constants.STOP_HOTKEY_IDS, e.Hotkey, e.IncludeModifiers);
                    stopButton.Content = $"{Constants.MAIN_WINDOW_STOP_BUTTON_CONTENT} ({e.Hotkey.DisplayName})";
                    break;
                case Operation.Toggle:
                    ReRegisterHotkey(Constants.TOGGLE_HOTKEY_IDS, e.Hotkey, e.IncludeModifiers);
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

        private void RadioButtonSelectedLocationMode_CurrentLocationOnChecked(object sender, RoutedEventArgs e)
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