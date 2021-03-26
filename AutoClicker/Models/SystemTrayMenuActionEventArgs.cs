using System;
using AutoClicker.Enums;

namespace AutoClicker.Models
{
    public class SystemTrayMenuActionEventArgs : EventArgs
    {
        public SystemTrayMenuAction Action { get; set; }
    }
}
