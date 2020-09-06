using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WindowsFormsApp
{
    internal static partial class Win32
    {
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

    }
}