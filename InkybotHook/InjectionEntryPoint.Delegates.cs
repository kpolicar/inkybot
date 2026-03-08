using System;
using System.Runtime.InteropServices;
using static InkybotHook.NativeMethods;

namespace InkybotHook
{
    public partial class InjectionEntryPoint
    {
        // =============================================================
        // DELEGATE TYPES
        // =============================================================

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

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool ReleaseCaptureDelegate();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr GetCaptureDelegate();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GetCursorPosHookDelegate(out POINT lpPoint);

        // =============================================================
        // ORIGINAL FUNCTION POINTERS
        // =============================================================

        private PeekMessageWDelegate _originalPeekMessageW;
        private TranslateMessageDelegate _originalTranslateMessage;
        private DispatchMessageWDelegate _originalDispatchMessageW;
        private GetRawInputDataDelegate _originalGetRawInputData;
        private GetRawInputBufferDelegate _originalGetRawInputBuffer;
        private GetAsyncKeyStateDelegate _originalGetAsyncKeyState;
        private GetKeyStateDelegate _originalGetKeyState;
        private ReleaseCaptureDelegate _originalReleaseCapture;
        private GetCaptureDelegate _originalGetCapture;
        private GetCursorPosHookDelegate _originalGetCursorPos;
    }
}
