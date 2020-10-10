using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WindowsFormsApp
{
    internal static partial class Win32
    {
        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.DLL")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point point);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("User32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        public class MouseOperations
        {
            [Flags]
            public enum MouseEventFlags
            {
                LeftDown = 0x00000002,
                LeftUp = 0x00000004,
                MiddleDown = 0x00000020,
                MiddleUp = 0x00000040,
                Move = 0x00000001,
                Absolute = 0x00008000,
                RightDown = 0x00000008,
                RightUp = 0x00000010
            }

            [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetCursorPos(int x, int y);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetCursorPos(out MousePoint lpMousePoint);

            [DllImport("user32.dll")]
            private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

            public static MousePoint GetCursorPosition() {
                MousePoint currentMousePoint;
                var gotPoint = GetCursorPos(out currentMousePoint);
                if (!gotPoint) currentMousePoint = new MousePoint(0, 0);
                return currentMousePoint;
            }

            public static void MouseEvent(MouseEventFlags value) {
                var position = GetCursorPosition();

                mouse_event
                    ((int) value,
                        position.X,
                        position.Y,
                        0,
                        0)
                    ;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MousePoint
            {
                public int X;
                public int Y;

                public MousePoint(int x, int y) {
                    X = x;
                    Y = y;
                }
            }
        }
    }
}
