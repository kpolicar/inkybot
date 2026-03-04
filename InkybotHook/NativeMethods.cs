using System;
using System.Runtime.InteropServices;

namespace InkybotHook
{
    public static class NativeMethods
    {
        #region Constants

        public const uint WM_MOUSEMOVE     = 0x0200;
        public const uint WM_LBUTTONDOWN   = 0x0201;
        public const uint WM_LBUTTONUP     = 0x0202;
        public const uint WM_RBUTTONDOWN   = 0x0204;
        public const uint WM_RBUTTONUP     = 0x0205;
        public const uint WM_INPUT         = 0x00FF;
        public const uint WM_NULL          = 0x0000;
        public const uint WM_ACTIVATE      = 0x0006;
        public const uint WM_ACTIVATEAPP   = 0x001C;
        public const uint WM_KILLFOCUS     = 0x0008;
        public const uint WM_SETFOCUS      = 0x0007;
        public const uint WM_NCACTIVATE    = 0x0086;
        public const uint WM_MOUSEACTIVATE = 0x0021;
        public const uint WA_INACTIVE      = 0;
        public const uint WA_ACTIVE        = 1;
        public const uint RID_INPUT        = 0x10000003;
        public const uint RIM_TYPEMOUSE    = 0;
        public const int  CURSOR_SHOWING   = 0x00000001;
        public const int  GWLP_WNDPROC     = -4;

        // Sentinel bit ORed into wParam of bot-posted button messages.
        // MK_* virtual-key flags only use the low 7 bits, so bit 30 is safe.
        public const long BOT_INPUT_SENTINEL = 0x40000000L;

        #endregion

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GUITHREADINFO
        {
            public uint cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        }

        #endregion

        #region P/Invoke

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        public static extern IntPtr SetWindowLongPtrW(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
        public static extern IntPtr CallWindowProcW(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreatePen(int fnPenStyle, int nWidth, uint crColor);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern bool MoveToEx(IntPtr hdc, int X, int Y, IntPtr lpPoint);

        [DllImport("gdi32.dll")]
        public static extern bool LineTo(IntPtr hdc, int X, int Y);

        [DllImport("gdi32.dll")]
        public static extern bool Ellipse(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(uint crColor);

        [DllImport("gdi32.dll")]
        public static extern int SetROP2(IntPtr hdc, int fnDrawMode);

        [DllImport("gdi32.dll")]
        public static extern IntPtr GetStockObject(int fnObject);

        public const int PS_SOLID = 0;
        public const int R2_NOT    = 6; // inverted color — erases itself on second draw

        #endregion

        public static IntPtr MakeLParam(int x, int y)
        {
            return (IntPtr)((y << 16) | (x & 0xFFFF));
        }
    }
}
