using System;
using System.Windows.Input;

namespace AutoClicker.Commands
{
    public static class CommandUtils
    {
        public static RoutedUICommand CreateCommand(Type windowType, string commandName, KeyGesture keyGesture = null)
            => keyGesture == null
                ? new RoutedUICommand(commandName, commandName, windowType)
                : new RoutedUICommand(commandName, commandName, windowType, [keyGesture]);
    }
}
