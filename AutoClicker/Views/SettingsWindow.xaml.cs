using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using AutoClicker.Utils;

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

        public List<KeyMapping> KeyMapping { get; set; }

        #endregion Dependency Properties

        #region Lifetime

        public SettingsWindow()
        {
            DataContext = this;
            KeyMapping = KeyMappingUtils.KeyMapping;

            Title = Constants.SETTINGS_WINDOW_TITLE;
            SelectedStartKey = SettingsUtils.GetStartHotKey();
            SelectedStopKey = SettingsUtils.GetStopHotKey();

            InitializeComponent();
        }

        #endregion Lifetime

        #region Save Command

        private void SaveCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectedStartKey != SettingsUtils.GetStartHotKey())
            {
                SettingsUtils.SetStartHotKey(SelectedStartKey);
            }
            if (SelectedStopKey != SettingsUtils.GetStopHotKey())
            {
                SettingsUtils.SetStopHotKey(SelectedStopKey);
            }
        }

        #endregion Save Command

        #region Reset Command

        private void ResetCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SettingsUtils.LoadSettings();
            SelectedStartKey = SettingsUtils.GetStartHotKey();
            SelectedStopKey = SettingsUtils.GetStopHotKey();
        }

        #endregion Reset Command
    }
}
