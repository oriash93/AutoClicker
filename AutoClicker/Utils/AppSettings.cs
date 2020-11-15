using System;
using System.Windows.Input;
using AutoClicker.Enums;

namespace AutoClicker.Utils
{
    public static class AppSettings
    {
        // Extract default F6/F7 to Constants
        public static Hotkey StartHotkey { get; private set; } = new Hotkey(Key.F6);

        public static Hotkey StopHotkey { get; private set; } = new Hotkey(Key.F7);

        public static void SetStartHotKey(Key key)
        {
            StartHotkey = new Hotkey(key);
            HotkeyChangedEventArgs args = new HotkeyChangedEventArgs
            {
                Hotkey = StartHotkey,
                Operation = Operation.Start
            };
            HotKeyChangedEvent.Invoke(null, args);
        }

        public static void SetStopHotKey(Key key)
        {
            StopHotkey = new Hotkey(key);
            HotkeyChangedEventArgs args = new HotkeyChangedEventArgs
            {
                Hotkey = StopHotkey,
                Operation = Operation.Stop
            };
            HotKeyChangedEvent.Invoke(null, args);
        }

        public static event EventHandler<HotkeyChangedEventArgs> HotKeyChangedEvent;
    }
}
