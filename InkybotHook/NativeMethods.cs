using System;
using System.Runtime.InteropServices;

namespace InkybotHook
{
    public static class NativeMethods
    {
        #region Constants

        public const uint WM_LBUTTONDBLCLK = 0x0203;
        public const uint WM_RBUTTONDBLCLK = 0x0206;
        public const uint WM_NCLBUTTONDBLCLK = 0x00A3;
        public const uint WM_NCRBUTTONDBLCLK = 0x00A6;
        public const uint WM_MOUSEMOVE     = 0x0200;
        public const uint WM_LBUTTONDOWN   = 0x0201;
        public const uint WM_LBUTTONUP     = 0x0202;
        public const uint WM_RBUTTONDOWN   = 0x0204;
        public const uint WM_RBUTTONUP     = 0x0205;
        public const uint WM_NCLBUTTONDOWN = 0x00A1;
        public const uint WM_NCLBUTTONUP   = 0x00A2;
        public const uint WM_NCRBUTTONDOWN = 0x00A4;
        public const uint WM_NCRBUTTONUP   = 0x00A5;
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
        public const uint INPUT_MOUSE = 0;
        public const uint MOUSEEVENTF_MOVE = 0x0001;
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;
        public const long MK_LBUTTON = 0x0001L;
        public const long MK_RBUTTON = 0x0002L;

        // Raw input mouse button transition flags (RAWMOUSE.usButtonFlags)
        public const ushort RI_MOUSE_LEFT_BUTTON_DOWN   = 0x0001;
        public const ushort RI_MOUSE_LEFT_BUTTON_UP     = 0x0002;
        public const ushort RI_MOUSE_RIGHT_BUTTON_DOWN  = 0x0004;
        public const ushort RI_MOUSE_RIGHT_BUTTON_UP    = 0x0008;

        // Sentinel bit ORed into wParam of bot-posted button messages.
        // MK_* virtual-key flags only use the low 7 bits, so bit 30 is safe.
        public const long BOT_INPUT_SENTINEL = 0x40000000L;

        // WM_POINTER messages (Win8+)
        public const uint WM_POINTERDOWN       = 0x0246;
        public const uint WM_POINTERUP         = 0x0247;
        public const uint WM_POINTERUPDATE     = 0x0245;
        public const uint WM_POINTERENTER      = 0x0249;
        public const uint WM_POINTERLEAVE      = 0x024A;
        public const uint WM_POINTERCAPTURECHANGED = 0x024C;

        // Child window hit-test flags
        public const uint CWP_ALL             = 0x0000;
        public const uint CWP_SKIPINVISIBLE   = 0x0001;
        public const uint CWP_SKIPDISABLED    = 0x0002;
        public const uint CWP_SKIPTRANSPARENT = 0x0004;

        // SendMessageTimeout flags
        public const uint SMTO_NORMAL          = 0x0000;
        public const uint SMTO_BLOCK           = 0x0001;
        public const uint SMTO_ABORTIFHUNG     = 0x0002;

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

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
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

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessageW(ref MSG lpMsg);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessageA(ref MSG lpMsg);

        [DllImport("user32.dll", EntryPoint = "SendMessageW")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern IntPtr SendMessageA(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessageTimeoutW")]
        public static extern IntPtr SendMessageTimeoutW(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

        [DllImport("user32.dll", EntryPoint = "SendMessageTimeoutA")]
        public static extern IntPtr SendMessageTimeoutA(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

        [DllImport("user32.dll", EntryPoint = "SendNotifyMessageW")]
        public static extern bool SendNotifyMessageW(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendNotifyMessageA")]
        public static extern bool SendNotifyMessageA(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessageCallbackW")]
        public static extern bool SendMessageCallbackW(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, IntPtr lpResultCallBack, IntPtr dwData);

        [DllImport("user32.dll", EntryPoint = "SendMessageCallbackA")]
        public static extern bool SendMessageCallbackA(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, IntPtr lpResultCallBack, IntPtr dwData);

        [DllImport("user32.dll", EntryPoint = "CallWindowProcA")]
        public static extern IntPtr CallWindowProcA(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "DefWindowProcW")]
        public static extern IntPtr DefWindowProcW(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "DefWindowProcA")]
        public static extern IntPtr DefWindowProcA(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetCapture(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll")]
        public static extern IntPtr ChildWindowFromPointEx(IntPtr hwndParent, POINT pt, uint uFlags);

        [DllImport("user32.dll")]
        public static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, ref POINT lpPoints, uint cPoints);

        [DllImport("user32.dll", EntryPoint = "GetPointerType", SetLastError = true)]
        public static extern bool GetPointerType(uint pointerId, out uint pointerType);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern uint GetMessagePos();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

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

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);
        public delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);
    }
}
