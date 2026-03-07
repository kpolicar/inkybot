using System;
using System.Runtime.InteropServices;

namespace InkybotHook
{
    public static class NativeMethods
    {
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

        [StructLayout(LayoutKind.Explicit)]
        public struct RAWMOUSE
        {
            [FieldOffset(0)] public ushort usFlags;
            [FieldOffset(4)] public uint ulButtons;
            [FieldOffset(4)] public ushort usButtonFlags;
            [FieldOffset(6)] public ushort usButtonData;
            [FieldOffset(8)] public uint ulRawButtons;
            [FieldOffset(12)] public int lLastX;
            [FieldOffset(16)] public int lLastY;
            [FieldOffset(20)] public uint ulExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTMOUSE
        {
            public RAWINPUTHEADER header;
            public RAWMOUSE mouse;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUT
        {
            public RAWINPUTHEADER header;
            public RAWMOUSE mouse;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICELIST
        {
            public IntPtr hDevice;
            public uint dwType;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        public const uint WM_LBUTTONDOWN = 0x0201;
        public const uint WM_LBUTTONUP = 0x0202;
        public const int MK_LBUTTON = 0x0001;
        #endregion

        #region P/Invoke

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

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
        public static extern int SetROP2(IntPtr hdc, int fnDrawMode);

        [DllImport("gdi32.dll")]
        public static extern IntPtr GetStockObject(int fnObject);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetRawInputData(
            IntPtr hRawInput,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetRawInputDeviceList(
            [Out] RAWINPUTDEVICELIST[] pRawInputDeviceList,
            ref uint puiNumDevices,
            uint cbSize);

        #endregion

        #region Constants

        public const int PS_SOLID = 0;
        public const int R2_NOT   = 6;

        public const int RID_INPUT = 0x10000003;
        public const int RIM_TYPEMOUSE = 0;
        public const uint RI_MOUSE_LEFT_BUTTON_DOWN   = 0x0001;
        public const uint RI_MOUSE_LEFT_BUTTON_UP     = 0x0002;
        public const uint RI_MOUSE_RIGHT_BUTTON_DOWN  = 0x0004;
        public const uint RI_MOUSE_RIGHT_BUTTON_UP    = 0x0008;
        public const uint RI_MOUSE_MIDDLE_BUTTON_DOWN = 0x0010;
        public const int RI_MOUSE_MIDDLE_BUTTON_UP   = 0x0020;
        public const uint RI_MOUSE_BUTTON_1_DOWN      = RI_MOUSE_LEFT_BUTTON_DOWN;
        public const uint RI_MOUSE_BUTTON_1_UP        = RI_MOUSE_LEFT_BUTTON_UP;
        public const uint RI_MOUSE_BUTTON_2_DOWN      = RI_MOUSE_RIGHT_BUTTON_DOWN;
        public const uint RI_MOUSE_BUTTON_2_UP        = RI_MOUSE_RIGHT_BUTTON_UP;
        public const uint RI_MOUSE_BUTTON_3_DOWN      = RI_MOUSE_MIDDLE_BUTTON_DOWN;
        public const uint RI_MOUSE_BUTTON_3_UP        = RI_MOUSE_MIDDLE_BUTTON_UP;
        public const uint RI_MOUSE_WHEEL              = 0x0400;
        public const uint RI_MOUSE_HWHEEL             = 0x0800;

        
        // =========================================================
        // 2. CONSTANTS & STRUCTS
        // =========================================================
        public const uint WM_NULL = 0x0000;
        public const uint WM_INPUT = 0x00FF;
        public const int GWLP_WNDPROC = -4;
        public const int VK_LBUTTON = 0x01;

        #endregion
    }
}
