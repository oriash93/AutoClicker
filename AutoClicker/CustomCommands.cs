using System.Windows.Input;

namespace AutoClicker
{
    public class CustomCommands
    {
        public static readonly RoutedUICommand Start =
            new RoutedUICommand
            ("Start", "Start", typeof(CustomCommands), new InputGestureCollection() { new KeyGesture(Key.F6) });

        public static readonly RoutedUICommand Stop =
            new RoutedUICommand
            ("Stop", "Stop", typeof(CustomCommands), new InputGestureCollection() { new KeyGesture(Key.F7) });

        public static readonly RoutedUICommand Exit =
            new RoutedUICommand
            ("Exit", "Exit", typeof(CustomCommands), new InputGestureCollection() { new KeyGesture(Key.F4, ModifierKeys.Alt) });

        public static readonly RoutedUICommand About =
            new RoutedUICommand("About", "About", typeof(CustomCommands));
    }
}
