using System.Windows;
using System.Windows.Input;
using AutoClicker.Utils;

namespace AutoClicker.Views
{
    public partial class SettingsWindow : Window
    {
        #region Dependency Properties

        public Key SelectedStartKey
        {
            get => (Key)GetValue(SelectedStartKeyProperty);
            set => SetValue(SelectedStartKeyProperty, value);
        }

        public static readonly DependencyProperty SelectedStartKeyProperty =
            DependencyProperty.Register(nameof(SelectedStartKey), typeof(Key), typeof(SettingsWindow));

        public Key SelectedStopKey
        {
            get => (Key)GetValue(SelectedStopKeyProperty);
            set => SetValue(SelectedStopKeyProperty, value);
        }

        public static readonly DependencyProperty SelectedStopKeyProperty =
            DependencyProperty.Register(nameof(SelectedStopKey), typeof(Key), typeof(SettingsWindow));

        #endregion Dependency Properties

        #region Lifetime

        public SettingsWindow()
        {
            DataContext = this;

            Title = Constants.SETTINGS_WINDOW_TITLE;
            SelectedStartKey = SettingsUtils.GetStartHotKey().Key;
            SelectedStopKey = SettingsUtils.GetStopHotKey().Key;

            InitializeComponent();
        }

        #endregion Lifetime

        #region Save Command

        private void SaveCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectedStartKey != SettingsUtils.GetStartHotKey().Key && IsKeyValid(SelectedStartKey))
            {
                SettingsUtils.SetStartHotKey(SelectedStartKey);
            }
            if (SelectedStopKey != SettingsUtils.GetStopHotKey().Key && IsKeyValid(SelectedStopKey))
            {
                SettingsUtils.SetStopHotKey(SelectedStopKey);
            }
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsKeyValid(SelectedStartKey) || IsKeyValid(SelectedStopKey);
        }

        #endregion Save Command

        #region Reset Command

        private void ResetCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SettingsUtils.LoadSettings();
            SelectedStartKey = SettingsUtils.GetStartHotKey().Key;
            SelectedStopKey = SettingsUtils.GetStopHotKey().Key;
        }

        #endregion Reset Command

        #region Helper Methods

        private bool IsKeyValid(Key key)
        {
            return key != Key.None;
        }

        #endregion Helper Methods
    }
}
