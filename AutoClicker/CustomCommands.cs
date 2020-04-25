using System.Windows.Input;

namespace AutoClicker
{
    public static class CustomCommands
    {
        public static readonly RoutedUICommand Start = CreateCommand(nameof(Start), new KeyGesture(Key.F6));

        public static readonly RoutedUICommand Stop = CreateCommand(nameof(Stop), new KeyGesture(Key.F7));

        public static readonly RoutedUICommand Exit = CreateCommand(nameof(Exit), new KeyGesture(Key.F4, ModifierKeys.Alt));

        public static readonly RoutedUICommand About = CreateCommand(nameof(About), new KeyGesture(Key.F1));

        private static RoutedUICommand CreateCommand(string commandName, KeyGesture keyGesture = null)
            => new RoutedUICommand(commandName, commandName, typeof(CustomCommands), new InputGestureCollection() { keyGesture });
    }
}
