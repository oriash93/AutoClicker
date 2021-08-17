using System.Windows.Input;
using AutoClicker.Utils;

namespace AutoClicker.Commands
{
    public static class MainWindowCommands
    {
        public static readonly RoutedUICommand Start =
            AssemblyUtils.CreateCommand(typeof(MainWindowCommands), nameof(Start));

        public static readonly RoutedUICommand Stop =
            AssemblyUtils.CreateCommand(typeof(MainWindowCommands), nameof(Stop));

        public static readonly RoutedUICommand SaveSettings =
            AssemblyUtils.CreateCommand(typeof(MainWindowCommands), nameof(SaveSettings));

        public static readonly RoutedUICommand HotkeySettings =
            AssemblyUtils.CreateCommand(typeof(MainWindowCommands), nameof(HotkeySettings), new KeyGesture(Key.CapsLock, ModifierKeys.Control));

        public static readonly RoutedUICommand Exit =
            AssemblyUtils.CreateCommand(typeof(MainWindowCommands), nameof(Exit), new KeyGesture(Key.F4, ModifierKeys.Alt));

        public static readonly RoutedUICommand About =
            AssemblyUtils.CreateCommand(typeof(MainWindowCommands), nameof(About), new KeyGesture(Key.F1));
    }
}
