using System.Windows.Input;

namespace AutoClicker.Commands
{
    public static class MainWindowCommands
    {
        public static readonly RoutedUICommand Start =
            CommandUtils.CreateCommand(typeof(MainWindowCommands), nameof(Start));

        public static readonly RoutedUICommand Stop =
            CommandUtils.CreateCommand(typeof(MainWindowCommands), nameof(Stop));

        public static readonly RoutedUICommand Toggle =
            CommandUtils.CreateCommand(typeof(MainWindowCommands), nameof(Toggle));

        public static readonly RoutedUICommand SaveSettings =
            CommandUtils.CreateCommand(typeof(MainWindowCommands), nameof(SaveSettings));

        public static readonly RoutedUICommand HotkeySettings =
            CommandUtils.CreateCommand(typeof(MainWindowCommands), nameof(HotkeySettings), new KeyGesture(Key.CapsLock, ModifierKeys.Control));

        public static readonly RoutedUICommand Exit =
            CommandUtils.CreateCommand(typeof(MainWindowCommands), nameof(Exit), new KeyGesture(Key.F4, ModifierKeys.Alt));

        public static readonly RoutedUICommand About =
            CommandUtils.CreateCommand(typeof(MainWindowCommands), nameof(About), new KeyGesture(Key.F1));

        public static readonly RoutedUICommand CaptureMouseScreenCoordinates =
            CommandUtils.CreateCommand(typeof(MainWindowCommands), nameof(CaptureMouseScreenCoordinates));
    }
}
