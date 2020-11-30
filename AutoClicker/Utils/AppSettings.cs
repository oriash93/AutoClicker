using System;
using System.Windows.Input;
using AutoClicker.Enums;

namespace AutoClicker.Utils
{
    public static class AppSettings
    {
        public static Hotkey StartHotkey { get; private set; } = new Hotkey(Constants.DEFAULT_START_HOTKEY);

        public static Hotkey StopHotkey { get; private set; } = new Hotkey(Constants.DEFAULT_STOP_HOTKEY);

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
