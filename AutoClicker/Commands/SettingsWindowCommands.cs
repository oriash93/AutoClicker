using System.Windows.Input;
using AutoClicker.Utils;

namespace AutoClicker.Commands
{
    public static class SettingsWindowCommands
    {
        public static readonly RoutedUICommand Save =
            Utilities.CreateCommand(typeof(SettingsWindowCommands), nameof(Save));

        public static readonly RoutedUICommand Reset =
            Utilities.CreateCommand(typeof(SettingsWindowCommands), nameof(Reset));
    }
}
