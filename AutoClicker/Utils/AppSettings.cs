using System;
using System.Windows.Input;

namespace AutoClicker.Utils
{
    public static class AppSettings
    {
        public static Hotkey StartHotkey { get; private set; } = new Hotkey(Key.F6, Operation.Start);

        public static Hotkey StopHotkey { get; private set; } = new Hotkey(Key.F7, Operation.Stop);

        public static void SetStartHotKey(Key key)
        {
            StartHotkey = new Hotkey(key, Operation.Start);
            HotkeyChangedEventArgs args = new HotkeyChangedEventArgs
            {
                Hotkey = StartHotkey
            };
            HotKeyChangedEvent.Invoke(null, args);
        }

        public static void SetStopHotKey(Key key)
        {
            StopHotkey = new Hotkey(key, Operation.Stop);
            HotkeyChangedEventArgs args = new HotkeyChangedEventArgs
            {
                Hotkey = StopHotkey
            };
            HotKeyChangedEvent.Invoke(null, args);
        }

        public static event EventHandler<HotkeyChangedEventArgs> HotKeyChangedEvent;
    }
}
