using System.Windows.Input;
using AutoClicker.Utils;

namespace AutoClicker.Commands
{
    public static class MainWindowCommands
    {
        public static readonly RoutedUICommand Start =
            Utilities.CreateCommand(typeof(MainWindowCommands), nameof(Start), new KeyGesture(Key.F6));

        public static readonly RoutedUICommand Stop =
            Utilities.CreateCommand(typeof(MainWindowCommands), nameof(Stop), new KeyGesture(Key.F7));

        public static readonly RoutedUICommand HotkeySettings =
            Utilities.CreateCommand(typeof(MainWindowCommands), nameof(HotkeySettings), new KeyGesture(Key.CapsLock, ModifierKeys.Control));

        public static readonly RoutedUICommand Exit =
            Utilities.CreateCommand(typeof(MainWindowCommands), nameof(Exit), new KeyGesture(Key.F4, ModifierKeys.Alt));

        public static readonly RoutedUICommand About =
            Utilities.CreateCommand(typeof(MainWindowCommands), nameof(About), new KeyGesture(Key.F1));
    }
}
