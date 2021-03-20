using System;
using AutoClicker.Enums;

namespace AutoClicker.Utils
{
    public class HotkeyChangedEventArgs : EventArgs
    {
        public KeyMapping Hotkey { get; set; }
        public Operation Operation { get; set; }
    }
}
