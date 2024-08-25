using System;
using AutoClicker.Enums;

namespace AutoClicker.Models
{
    public class HotkeyChangedEventArgs : EventArgs
    {
        public KeyMapping Hotkey { get; set; }
        public Operation Operation { get; set; }
        public bool IncludeModifiers { get; set; }
    }
}
