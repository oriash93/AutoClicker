using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AutoClicker.Models;
using AutoClicker.Utils;
using Serilog;

namespace AutoClicker.Views
{
    public partial class SettingsWindow : Window
    {
        #region Dependency Properties

        public KeyMapping SelectedStartKey
        {
            get => (KeyMapping)GetValue(SelectedStartKeyProperty);
            set => SetValue(SelectedStartKeyProperty, value);
        }

        public static readonly DependencyProperty SelectedStartKeyProperty =
            DependencyProperty.Register(nameof(SelectedStartKey), typeof(KeyMapping), typeof(SettingsWindow));

        public KeyMapping SelectedStopKey
        {
            get => (KeyMapping)GetValue(SelectedStopKeyProperty);
            set => SetValue(SelectedStopKeyProperty, value);
        }

        public static readonly DependencyProperty SelectedStopKeyProperty =
            DependencyProperty.Register(nameof(SelectedStopKey), typeof(KeyMapping), typeof(SettingsWindow));

        public KeyMapping SelectedToggleKey
        {
            get => (KeyMapping)GetValue(SelectedToggleKeyProperty);
            set => SetValue(SelectedToggleKeyProperty, value);
        }

        public static readonly DependencyProperty SelectedToggleKeyProperty =
            DependencyProperty.Register(nameof(SelectedToggleKey), typeof(KeyMapping), typeof(SettingsWindow));

        public List<KeyMapping> KeyMapping { get; set; }

        #endregion Dependency Properties

        #region Life Cycle

        public SettingsWindow()
        {
            DataContext = this;
            KeyMapping = KeyMappingUtils.KeyMapping;

            Title = Constants.SETTINGS_WINDOW_TITLE;
            SelectedStartKey = SettingsUtils.CurrentSettings.HotkeySettings.StartHotkey;
            SelectedStopKey = SettingsUtils.CurrentSettings.HotkeySettings.StopHotkey;
            SelectedToggleKey = SettingsUtils.CurrentSettings.HotkeySettings.ToggleHotkey;

            InitializeComponent();
        }

        #endregion Life Cycle

        #region Commands

        private void SaveCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectedStartKey != SettingsUtils.CurrentSettings.HotkeySettings.StartHotkey)
            {
                SettingsUtils.SetStartHotKey(SelectedStartKey);
            }
            if (SelectedStopKey != SettingsUtils.CurrentSettings.HotkeySettings.StopHotkey)
            {
                SettingsUtils.SetStopHotKey(SelectedStopKey);
            }
            if (SelectedStopKey != SettingsUtils.CurrentSettings.HotkeySettings.ToggleHotkey)
            {
                SettingsUtils.SetToggleHotKey(SelectedToggleKey);
            }
        }

        private void ResetCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SettingsUtils.Reset();
            SelectedStartKey = SettingsUtils.CurrentSettings.HotkeySettings.StartHotkey;
            SelectedStopKey = SettingsUtils.CurrentSettings.HotkeySettings.StopHotkey;
            SelectedToggleKey = SettingsUtils.CurrentSettings.HotkeySettings.ToggleHotkey;
        }

        #endregion Commands

        #region Helper Methods

        private void StartKeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            KeyMapping newKeyMapping = GetNewKeyMapping(e.Key);
            if (newKeyMapping == null)
            {
                Log.Error("No Matching key for {Key}", e.Key);
                return;
            }

            e.Handled = true;
            SelectedStartKey = newKeyMapping;
        }

        private void StopKeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            KeyMapping newKeyMapping = GetNewKeyMapping(e.Key);
            if (newKeyMapping == null)
            {
                Log.Error("No Matching key for {Key}", e.Key);
                return;
            }

            e.Handled = true;
            SelectedStopKey = newKeyMapping;
        }
        private void toggleKeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            KeyMapping newKeyMapping = GetNewKeyMapping(e.Key);
            if (newKeyMapping == null)
            {
                Log.Error("No Matching key for {Key}", e.Key);
                return;
            }

            e.Handled = true;
            SelectedToggleKey = newKeyMapping;
        }

        private KeyMapping GetNewKeyMapping(Key key)
        {
            int virtualKeyCode = KeyInterop.VirtualKeyFromKey(key);
            Log.Debug("GetNewKeyMapping with virtualKeyCode {VirtualKeyCode}", virtualKeyCode);
            return KeyMapping.FirstOrDefault(keyMapping => keyMapping.VirtualKeyCode == virtualKeyCode);
        }

        #endregion Helper Methods
    }
}
