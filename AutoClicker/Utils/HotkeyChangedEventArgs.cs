using System;

namespace AutoClicker.Utils
{
    public class HotkeyChangedEventArgs : EventArgs
    {
        public Hotkey Hotkey { get; set; }
    }
}
