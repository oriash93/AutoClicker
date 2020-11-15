using System.Windows.Input;
using AutoClicker.Utils;

namespace AutoClicker.Commands
{
    public static class SettingsWindowCommands
    {
        public static readonly RoutedUICommand Save = // TODO: should I rename it to "Set"?
            Utilities.CreateCommand(typeof(SettingsWindowCommands), nameof(Save));
    }
}
