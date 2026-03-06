using System;
using System.Runtime.InteropServices;
using static InkybotHook.NativeMethods;

namespace InkybotHook
{
    public partial class AdvancedInjectionEntryPoint
    {
        // =========================================================
        // DELEGATE TYPES
        // =========================================================
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GetCursorPosDelegate(out POINT lpPoint);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool IsIconicDelegate(IntPtr hWnd);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate bool PeekMessageWDelegate(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool TranslateMessageDelegate(ref MSG lpMsg);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate IntPtr DispatchMessageWDelegate(ref MSG lpMsg);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GetRawInputDataDelegate(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GetRawInputBufferDelegate(IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate short GetAsyncKeyStateDelegate(int vKey);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate short GetKeyStateDelegate(int nVirtKey);

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        // =========================================================
        // ORIGINAL FUNCTION POINTERS (set during hook installation)
        // =========================================================
        private GetCursorPosDelegate _originalGetCursorPos;
        private IsIconicDelegate _originalIsIconic;
        private PeekMessageWDelegate _originalPeekMessageW;
        private TranslateMessageDelegate _originalTranslateMessage;
        private DispatchMessageWDelegate _originalDispatchMessageW;
        private GetRawInputDataDelegate _originalGetRawInputData;
        private GetRawInputBufferDelegate _originalGetRawInputBuffer;
        private GetAsyncKeyStateDelegate _originalGetAsyncKeyState;
        private GetKeyStateDelegate _originalGetKeyState;

        // =========================================================
        // P/INVOKE DECLARATIONS (used across hook implementations)
        // =========================================================
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, UIntPtr size);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetCursorPos")]
        private static extern bool GetCursorPosNative(out POINT lpPoint);

        [DllImport("user32.dll", EntryPoint = "ScreenToClient")]
        private static extern bool ScreenToClientNative(IntPtr hWnd, ref POINT lpPoint);

        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8) return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    }
}
