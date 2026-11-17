using System.Windows.Input;

namespace AutoClicker.Commands
{
    public static class SettingsWindowCommands
    {
        public static readonly RoutedUICommand Save =
            CommandUtils.CreateCommand(typeof(SettingsWindowCommands), nameof(Save));

        public static readonly RoutedUICommand Reset =
            CommandUtils.CreateCommand(typeof(SettingsWindowCommands), nameof(Reset));
    }
}
