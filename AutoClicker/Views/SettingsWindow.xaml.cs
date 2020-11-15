using System.Windows;
using System.Windows.Controls;
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
            DependencyProperty.Register(nameof(SelectedStartKey), typeof(Key), typeof(SettingsWindow),
                new PropertyMetadata(default(Key)));

        public Key SelectedStopKey
        {
            get => (Key)GetValue(SelectedStopKeyProperty);
            set => SetValue(SelectedStopKeyProperty, value);
        }

        public static readonly DependencyProperty SelectedStopKeyProperty =
            DependencyProperty.Register(nameof(SelectedStopKey), typeof(Key), typeof(SettingsWindow),
                new PropertyMetadata(default(Key)));

        #endregion Dependency Properties

        #region Lifetime

        public SettingsWindow()
        {
            DataContext = this;

            Title = Constants.SETTINGS_WINDOW_TITLE;
            InitializeComponent();
        }

        #endregion Lifetime

        #region Save Command

        private void SaveCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            Button source = (Button)e.Source;
            if (source.Name == nameof(saveStartHotkeyButton)) // TODO: find a BETTER way to tell apart
            {
                AppSettings.SetStartHotKey(SelectedStartKey);
            }
            else if (source.Name == nameof(saveStopHotkeyButton))
            {
                AppSettings.SetStopHotKey(SelectedStopKey);
            }
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // TODO: find a way to tell apart
            e.CanExecute = SelectedStartKey != Key.None;
        }

        #endregion Save Command
    }
}
