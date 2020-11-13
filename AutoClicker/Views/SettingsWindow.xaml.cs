using System.Windows;
using System.Windows.Input;
using AutoClicker.Utils;

namespace AutoClicker.Views
{
    public partial class SettingsWindow : Window
    {
        #region Dependency Properties

        public Key SelectedKey
        {
            get => (Key)GetValue(SelectedKeyProperty);
            set => SetValue(SelectedKeyProperty, value);
        }

        public static readonly DependencyProperty SelectedKeyProperty =
            DependencyProperty.Register(nameof(SelectedKey), typeof(Key), typeof(SettingsWindow),
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

        #region Stop Command

        private void SaveCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            AppSettings.SetStartHotKey(Key.F4);
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        #endregion Stop Command

        #region Event Handlers

        #endregion Event Handlers
    }
}
