using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using Serilog;

namespace AutoClicker.Utils
{

    /// <summary>
    /// This class has been copied from StackOverflow where the OP copied it form somewhere else...
    /// </summary>
    public static class GlobalMouseHook
    {
        public static bool IsActive { get; private set; }

        public static event EventHandler MouseAction = delegate { };

        public static int MilisBetweenEvents { get; private set; }

        public static void Start(int milisBetweenEvents)
        {
            Start();
            MilisBetweenEvents = milisBetweenEvents;
            _timer = new Stopwatch();
            _timer.Start();
        }
        public static void Start()
        {
            _hookID = SetHook(_proc);
            IsActive = true;
            Log.Information("GloablMouseHook started");
        }
        public static void stop()
        {
            UnhookWindowsHookEx(_hookID);
            IsActive = false;
            if (MilisBetweenEvents > 0)
                _timer.Stop();
            Log.Information("GloablMouseHook stopped");
        }

        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static Stopwatch _timer = new Stopwatch();

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                IntPtr hook = SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle("user32"), 0);
                if (hook == IntPtr.Zero)
                {
                    throw new System.ComponentModel.Win32Exception();
                }
                return hook;
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && MouseMessages.WM_MOUSEMOVE == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                if (MilisBetweenEvents > 0)
                {
                    if (_timer.ElapsedMilliseconds > MilisBetweenEvents)
                    {
                        _timer.Restart();
                        MouseAction(null, new EventArgs());
                    }
                }
                else
                    MouseAction(null, new EventArgs());
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        /// <summary>
        /// Struct representing a point.
        /// Thanks StackOverflow again :)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData, flags, time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            // NOTE: If you need error handling
            // bool success = GetCursorPos(out lpPoint);
            // if (!success)

            return lpPoint;
        }
    }
}
