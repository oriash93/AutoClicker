using System;
using System.Windows;
using System.Windows.Input;
using AutoClicker.Enums;
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
            Operation operation = (Operation)e.Parameter;
            switch (operation)
            {
                case Operation.Start:
                    AppSettings.SetStartHotKey(SelectedStartKey);
                    break;
                case Operation.Stop:
                    AppSettings.SetStopHotKey(SelectedStopKey);
                    break;
                default:
                    throw new NotSupportedException("Operation not supported!");
            }
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            Operation operation = (Operation)e.Parameter;
            switch (operation)
            {
                case Operation.Start:
                    e.CanExecute = SelectedStartKey != Key.None;
                    break;
                case Operation.Stop:
                    e.CanExecute = SelectedStopKey != Key.None;
                    break;
                default:
                    throw new NotSupportedException("Operation not supported!");
            }
        }

        #endregion Save Command
    }
}
