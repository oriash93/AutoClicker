using System;
using System.Runtime.InteropServices;

namespace AutoClicker.Utils
{
    public static class User32ApiUtils
    {
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        internal static extern bool SetCursorPosition(int x, int y);

        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        internal static extern void ExecuteMouseEvent(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll", EntryPoint = "RegisterHotKey")]
        internal static extern bool RegisterHotkey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll", EntryPoint = "UnregisterHotKey")]
        internal static extern bool DeregisterHotkey(IntPtr hWnd, int id);
    }
}
