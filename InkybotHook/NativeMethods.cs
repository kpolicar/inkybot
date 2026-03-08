using System;
using System.Runtime.InteropServices;

namespace InkybotHook
{
    public static class NativeMethods
    {
        // =============================================================
        // STRUCTS
        // =============================================================

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

        // =============================================================
        // WINDOW MESSAGES
        // =============================================================

        public const uint WM_NULL          = 0x0000;
        public const uint WM_INPUT         = 0x00FF;
        public const uint WM_KEYDOWN       = 0x0100;
        public const uint WM_KEYUP         = 0x0101;
        public const uint WM_CHAR          = 0x0102;
        public const uint WM_MOUSEMOVE     = 0x0200;
        public const uint WM_LBUTTONDOWN   = 0x0201;
        public const uint WM_LBUTTONUP     = 0x0202;
        public const uint WM_POINTERUPDATE = 0x0245;
        public const uint WM_POINTERDOWN   = 0x0246;
        public const uint WM_POINTERUP     = 0x0247;

        // =============================================================
        // VIRTUAL KEYS & MOUSE FLAGS
        // =============================================================

        public const int VK_LBUTTON  = 0x01;
        public const int MK_LBUTTON  = 0x0001;
        public const uint PM_REMOVE  = 0x0001;

        // =============================================================
        // RAW INPUT
        // =============================================================

        public const uint RID_INPUT                    = 0x10000003;
        public const uint RIM_TYPEMOUSE                = 0;
        public const ushort MOUSE_MOVE_ABSOLUTE        = 0x0001;
        public const ushort MOUSE_VIRTUAL_DESKTOP      = 0x0002;
        public const uint RI_MOUSE_LEFT_BUTTON_DOWN    = 0x0001;
        public const uint RI_MOUSE_LEFT_BUTTON_UP      = 0x0002;

        // =============================================================
        // SYSTEM METRICS
        // =============================================================

        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;

        // =============================================================
        // GDI
        // =============================================================

        public const int PS_SOLID = 0;
        public const int R2_NOT   = 6;

        // =============================================================
        // P/INVOKE — user32.dll
        // =============================================================

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetCapture(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetRawInputData(
            IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetRawInputDeviceList(
            [Out] RAWINPUTDEVICELIST[] pRawInputDeviceList, ref uint puiNumDevices, uint cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetRawInputDeviceList(
            IntPtr pRawInputDeviceList, ref uint puiNumDevices, uint cbSize);

        // =============================================================
        // P/INVOKE — kernel32.dll
        // =============================================================

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, UIntPtr size);

        // =============================================================
        // P/INVOKE — GDI (gdi32.dll + user32.dll)
        // =============================================================

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
    }
}
