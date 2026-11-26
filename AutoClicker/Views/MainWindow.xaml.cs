using System;
using System.Collections.Generic;
using System.IO;
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
using DrawingIcon = System.Drawing.Icon;
using FormsMouseEventArgs = System.Windows.Forms.MouseEventArgs;
using MouseAction = AutoClicker.Enums.MouseAction;
using MouseButton = AutoClicker.Enums.MouseButton;
using MouseButtons = System.Windows.Forms.MouseButtons;
using MouseCursor = System.Windows.Forms.Cursor;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using Point = System.Drawing.Point;
using SystemInformation = System.Windows.Forms.SystemInformation;
using Timer = System.Timers.Timer;
using Validation = System.Windows.Controls.Validation;

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
        private int clickInterval = 0;
        private int clickIntervalOffset = 0;
        private int positionOffsetPixels;
        private Point positionOffsetOrigin;
        private int heldMouseUpAction;
        private readonly Timer clickTimer;
        private bool IsOperationRunning => clickTimer.Enabled || heldMouseUpAction != 0;
        private readonly Uri runningIconUri = new Uri(Constants.RUNNING_ICON_RESOURCE_URI);
        // CommandManager only keeps a weak reference to RequerySuggested handlers
        private readonly EventHandler requerySuggestedHandler;

        private NotifyIcon systemTrayIcon;
        private DrawingIcon defaultTrayIcon;
        private DrawingIcon runningTrayIcon;
        private SystemTrayMenu systemTrayMenu;
        private AboutWindow aboutWindow = null;
        private SettingsWindow settingsWindow = null;
        private CaptureMouseScreenCoordinatesWindow captureMouseCoordinatesWindow;

        private ImageSource _defaultIcon;
        private nint _mainWindowHandle;
        private HwndSource _source;

        #region Life Cycle

        public MainWindow()
        {
            clickTimer = new Timer();
            clickTimer.Elapsed += OnClickTimerElapsed;

            DataContext = this;
            ResetTitle();
            InitializeComponent();

            requerySuggestedHandler = (sender, e) => UpdateIntervalError();
            CommandManager.RequerySuggested += requerySuggestedHandler;
            UpdateIntervalError();
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

            InitializeSystemTrayMenu();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (IsOperationRunning)
                StopOperation();
            clickTimer.Dispose();

            _source.RemoveHook(StartStopHooks);

            SettingsUtils.HotkeyChangedEvent -= SettingsUtils_HotkeyChangedEvent;
            CommandManager.RequerySuggested -= requerySuggestedHandler;

            foreach (int hotkeyId in Constants.ALL_HOTKEY_IDS)
            {
                DeregisterHotkey(hotkeyId);
            }

            DisposeSystemTrayMenu();

            Log.Information("Application closing");
            LogUtils.Close();

            base.OnClosed(e);
        }

        #endregion Life Cycle

        #region Commands

        private void StartCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            clickInterval = (int)IntervalUtils.CalculateInterval(AutoClickerSettings);
            clickIntervalOffset = clickInterval > 0 && AutoClickerSettings.IsRandomOffsetEnabled
                ? AutoClickerSettings.RandomOffsetMilliseconds : 0;
            positionOffsetPixels = AutoClickerSettings.IsPositionOffsetEnabled ? AutoClickerSettings.PositionOffsetPixels : 0;
            if (positionOffsetPixels > 0)
                positionOffsetOrigin = GetSelectedPosition();

            Log.Information("Starting operation, interval={Interval}ms, offset={Offset}ms", clickInterval, clickIntervalOffset);
            timesRepeated = 0;

            Icon = new BitmapImage(runningIconUri);
            Title += Constants.MAIN_WINDOW_TITLE_RUNNING;
            systemTrayIcon.Text += Constants.MAIN_WINDOW_TITLE_RUNNING;
            systemTrayIcon.Icon = runningTrayIcon;

            if (clickInterval == 0)
            {
                (int mouseDownAction, int mouseUpAction) = GetMouseActions();
                MoveCursor(GetClickPosition());
                Win32ApiUtils.ExecuteMouseEvent(mouseDownAction, 0, 0, 0, 0);
                heldMouseUpAction = mouseUpAction;
            }
            else
            {
                clickTimer.Interval = clickInterval;
                clickTimer.Start();
                PerformRepeatedClick();
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private void StartCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanStartOperation();
        }

        private void StopCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            StopOperation();
        }

        private void StopCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsOperationRunning;
        }

        private void ToggleCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (IsOperationRunning)
                StopCommand_Execute(sender, e);
            else
                StartCommand_Execute(sender, e);
        }

        private void ToggleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsOperationRunning || CanStartOperation();
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

        private bool CanStartOperation()
        {
            return !IsOperationRunning
                && IntervalUtils.GetIntervalError(AutoClickerSettings) == null
                && (!AutoClickerSettings.IsPositionOffsetEnabled
                    || (PositionOffsetTextBox != null && !Validation.GetHasError(PositionOffsetTextBox)))
                && (IntervalUtils.CalculateInterval(AutoClickerSettings) == 0 || IsRepeatModeValid());
        }

        private void UpdateIntervalError()
        {
            string error = IntervalUtils.GetIntervalError(AutoClickerSettings);
            IntervalErrorTextBlock.Text = error;
            IntervalErrorTextBlock.Visibility = error == null ? Visibility.Hidden : Visibility.Visible;
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

        private Point GetClickPosition()
        {
            if (positionOffsetPixels == 0)
                return GetSelectedPosition();

            return new Point(GetRandomizedCoordinate(positionOffsetOrigin.X), GetRandomizedCoordinate(positionOffsetOrigin.Y));
        }

        private int GetRandomizedCoordinate(int origin)
        {
            long minimum = Math.Max(int.MinValue, (long)origin - positionOffsetPixels);
            long maximum = Math.Min(int.MaxValue, (long)origin + positionOffsetPixels);
            return (int)Random.Shared.NextInt64(minimum, maximum + 1);
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

        private void PerformRepeatedClick()
        {
            (int mouseDownAction, int mouseUpAction) = GetMouseActions();
            if (mouseDownAction != 0)
            {
                Point position = GetClickPosition();
                int numberOfActions = GetNumberOfMouseActions();
                for (int i = 0; i < numberOfActions; ++i)
                {
                    MoveCursor(position);
                    Win32ApiUtils.ExecuteMouseEvent(mouseDownAction, 0, 0, 0, 0);
                    Win32ApiUtils.ExecuteMouseEvent(mouseUpAction, 0, 0, 0, 0);
                }
            }
            timesRepeated++;

            int timesToRepeat = GetTimesToRepeat();
            if (timesToRepeat > 0 && timesRepeated >= timesToRepeat)
            {
                Log.Information("Completed {TimesRepeated} repetitions", timesRepeated);
                StopOperation();
            }
            else if (clickIntervalOffset > 0)
            {
                clickTimer.Interval = IntervalUtils.GetRandomizedInterval(clickInterval, clickIntervalOffset);
            }
        }

        private void StopOperation()
        {
            Log.Information("Stopping operation");
            clickTimer.Stop();
            if (heldMouseUpAction != 0)
            {
                Win32ApiUtils.ExecuteMouseEvent(heldMouseUpAction, 0, 0, 0, 0);
                heldMouseUpAction = 0;
            }

            ResetTitle();
            Icon = _defaultIcon;
            systemTrayIcon.Icon = defaultTrayIcon;

            CommandManager.InvalidateRequerySuggested();
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
            defaultTrayIcon = AssemblyUtils.GetApplicationIcon();
            using (Stream runningIconStream = Application.GetResourceStream(runningIconUri).Stream)
            {
                runningTrayIcon = new DrawingIcon(runningIconStream, SystemInformation.SmallIconSize);
            }

            systemTrayIcon = new NotifyIcon
            {
                Visible = true,
                Icon = defaultTrayIcon,
                Text = Constants.MAIN_WINDOW_TITLE_DEFAULT
            };
            systemTrayIcon.MouseClick += SystemTrayIcon_MouseClick;
            systemTrayIcon.MouseDoubleClick += SystemTrayIcon_MouseDoubleClick;
            systemTrayMenu = new SystemTrayMenu();
            systemTrayMenu.SystemTrayMenuActionEvent += SystemTrayMenu_SystemTrayMenuActionEvent;
        }

        private void DisposeSystemTrayMenu()
        {
            systemTrayIcon.MouseClick -= SystemTrayIcon_MouseClick;
            systemTrayIcon.MouseDoubleClick -= SystemTrayIcon_MouseDoubleClick;
            systemTrayIcon.Dispose();
            defaultTrayIcon.Dispose();
            runningTrayIcon.Dispose();

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
                    Win32ApiUtils.RegisterHotkey(_mainWindowHandle, item.Item1, item.Item2, hotkey.VirtualKeyCode);
                }
            }
            else
            {
                Win32ApiUtils.RegisterHotkey(_mainWindowHandle, hotkeyIdsToModifiers.ElementAt(0).Item1, hotkeyIdsToModifiers.ElementAt(0).Item2, hotkey.VirtualKeyCode);
            }
        }

        private void DeregisterHotkey(int hotkeyId)
        {
            Log.Debug("DeregisterHotkey with hotkeyId={HotkeyId}", hotkeyId);
            if (Win32ApiUtils.DeregisterHotkey(_mainWindowHandle, hotkeyId))
                return;
            Log.Debug("No hotkey registered on {HotkeyId}", hotkeyId);
        }

        #endregion Helper Methods

        #region Event Handlers

        private void OnClickTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                // A tick may already be queued when the operation stops
                if (!clickTimer.Enabled)
                    return;

                PerformRepeatedClick();
            });
        }

        private (int Down, int Up) GetMouseActions()
        {
            return AutoClickerSettings.SelectedMouseButton switch
            {
                MouseButton.Left => (Constants.MOUSEEVENTF_LEFTDOWN, Constants.MOUSEEVENTF_LEFTUP),
                MouseButton.Right => (Constants.MOUSEEVENTF_RIGHTDOWN, Constants.MOUSEEVENTF_RIGHTUP),
                MouseButton.Middle => (Constants.MOUSEEVENTF_MIDDLEDOWN, Constants.MOUSEEVENTF_MIDDLEUP),
                MouseButton.LeftAndRight => (Constants.MOUSEEVENTF_LEFTDOWN | Constants.MOUSEEVENTF_RIGHTDOWN,
                    Constants.MOUSEEVENTF_LEFTUP | Constants.MOUSEEVENTF_RIGHTUP),
                _ => (0, 0)
            };
        }

        private void MoveCursor(Point position)
        {
            if (!Win32ApiUtils.SetCursorPosition(position.X, position.Y))
            {
                Log.Error("Failed to set the mouse cursor!");
            }
        }

        private nint StartStopHooks(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            if (msg == Constants.WM_HOTKEY && Constants.ALL_HOTKEY_IDS.Contains(wParam.ToInt32()))
            {
                int virtualKey = ((int)lParam >> 16) & 0xFFFF;
                if (virtualKey == SettingsUtils.CurrentSettings.HotkeySettings.StartHotkey.VirtualKeyCode && CanStartOperation())
                {
                    StartCommand_Execute(null, null);
                }
                if (virtualKey == SettingsUtils.CurrentSettings.HotkeySettings.StopHotkey.VirtualKeyCode && IsOperationRunning)
                {
                    StopCommand_Execute(null, null);
                }
                if (virtualKey == SettingsUtils.CurrentSettings.HotkeySettings.ToggleHotkey.VirtualKeyCode
                    && (IsOperationRunning || CanStartOperation()))
                {
                    ToggleCommand_Execute(null, null);
                }
                handled = true;
            }
            return nint.Zero;
        }

        private void SettingsUtils_HotkeyChangedEvent(object sender, HotkeyChangedEventArgs e)
        {
            Log.Information("HotkeyChangedEvent with operation={Operation}, hotkey={Hotkey}, includeModifiers={IncludeModifiers}", e.Operation, e.Hotkey.DisplayName, e.IncludeModifiers);
            switch (e.Operation)
            {
                case Operation.Start:
                    ReRegisterHotkey(Constants.START_HOTKEY_IDS, e.Hotkey, e.IncludeModifiers);
                    startButton.Content = $"{Constants.MAIN_WINDOW_START_BUTTON_CONTENT} ({e.Hotkey.LayoutDisplayName})";
                    break;
                case Operation.Stop:
                    ReRegisterHotkey(Constants.STOP_HOTKEY_IDS, e.Hotkey, e.IncludeModifiers);
                    stopButton.Content = $"{Constants.MAIN_WINDOW_STOP_BUTTON_CONTENT} ({e.Hotkey.LayoutDisplayName})";
                    break;
                case Operation.Toggle:
                    ReRegisterHotkey(Constants.TOGGLE_HOTKEY_IDS, e.Hotkey, e.IncludeModifiers);
                    toggleButton.Content = $"{Constants.MAIN_WINDOW_TOGGLE_BUTTON_CONTENT} ({e.Hotkey.LayoutDisplayName})";
                    break;
                default:
                    Log.Warning("Operation {Operation} not supported!", e.Operation);
                    throw new NotSupportedException($"Operation {e.Operation} not supported!");
            }
        }

        private void SystemTrayIcon_MouseClick(object sender, FormsMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            systemTrayMenu.IsOpen = true;
            systemTrayMenu.Focus();
        }

        private void SystemTrayIcon_MouseDoubleClick(object sender, FormsMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            systemTrayMenu.ToggleMenuItemsVisibility(false);
            Show();
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
            Activate();
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

        #endregion Event Handlers

        private void TopMostCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            Topmost = checkbox.IsChecked.Value;
        }
    }
}
