using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace InkybotHook
{
    
    public class InjectionEntryPoint: EasyHook.IEntryPoint
    {
        ServerInterface _server = null;

        Queue<string> _messageQueue = new Queue<string>();

        private readonly HashSet<string> _loggedFirstCalls = new HashSet<string>();

        // Fixed cursor position always reported to the game
        private POINT _overridePoint = new POINT { X = 50, Y = 50 };

        private void LogFirstCall(string hookName)
        {
            if (!_loggedFirstCalls.Contains(hookName))
            {
                _loggedFirstCalls.Add(hookName);
                QueueMessage("[EasyHook:Target] First call intercepted: " + hookName);
            }
        }

        private void QueueMessage(string message)
        {
            lock (_messageQueue)
            {
                _messageQueue.Enqueue(message);
            }
        }

        public InjectionEntryPoint(
            EasyHook.RemoteHooking.IContext context,
            string channelName) {
            try
            {
                _server = EasyHook.RemoteHooking.IpcConnectClient<ServerInterface>(channelName);
                // If Ping fails then the Run method will be not be called
                _server.Ping();
            }
            catch (Exception e)
            {
                // IPC connection failed - Run() will not be called by EasyHook
                throw new Exception("[EasyHook:Target] Failed to connect IPC channel '" + channelName + "': " + e.Message, e);
            }
        }

        public void Run(
            EasyHook.RemoteHooking.IContext context,
            string channelName)
        {
            EasyHook.LocalHook getCursorPosHook = null;
            EasyHook.LocalHook isIconicHook = null;
            EasyHook.LocalHook getMessageWHook = null;
            EasyHook.LocalHook peekMessageWHook = null;
            EasyHook.LocalHook getRawInputDataHook = null;
            EasyHook.LocalHook getCursorInfoHook = null;
            EasyHook.LocalHook setCursorPosHook = null;
            EasyHook.LocalHook getRawInputBufferHook = null;
            EasyHook.LocalHook getForegroundWindowHook = null;
            EasyHook.LocalHook getActiveWindowHook = null;
            EasyHook.LocalHook getFocusHook = null;
            EasyHook.LocalHook getPhysicalCursorPosHook = null;
            EasyHook.LocalHook clipCursorHook = null;
            EasyHook.LocalHook getMessageAHook = null;
            EasyHook.LocalHook peekMessageAHook = null;
            EasyHook.LocalHook screenToClientHook = null;
            EasyHook.LocalHook clientToScreenHook = null;
            EasyHook.LocalHook getGuiThreadInfoHook = null;
            EasyHook.LocalHook getCaptureHook = null;
            EasyHook.LocalHook getAsyncKeyStateHook = null;
            EasyHook.LocalHook getKeyStateHook = null;
            EasyHook.LocalHook getKeyboardStateHook = null;

            try
            {
                // Injection is now complete and the server interface is connected
                _server.IsInstalled(EasyHook.RemoteHooking.GetCurrentProcessId());

                // ---- GetCursorPos hook ----
                try
                {
                    var targetFunction = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetCursorPos");
                    getCursorPosHook = EasyHook.LocalHook.Create(
                        targetFunction,
                        new GetCursorPosDelegate(HookedGetCursorPos),
                        this);
                    _originalGetCursorPos = Marshal.GetDelegateForFunctionPointer<GetCursorPosDelegate>(targetFunction);
                    getCursorPosHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] GetCursorPos hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] GetCursorPos hook FAILED: " + e.Message);
                    getCursorPosHook = null;
                }

                // ---- IsIconic hook ----
                try
                {
                    var isIconicTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "IsIconic");
                    isIconicHook = EasyHook.LocalHook.Create(
                        isIconicTarget,
                        new IsIconicDelegate(HookedIsIconic),
                        this);
                    _originalIsIconic = Marshal.GetDelegateForFunctionPointer<IsIconicDelegate>(isIconicTarget);
                    isIconicHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] IsIconic hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] IsIconic hook FAILED: " + e.Message);
                    isIconicHook = null;
                }

                // ---- GetMessageW hook (WM_MOUSEMOVE interception) ----
                try
                {
                    var getMessageWTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetMessageW");
                    getMessageWHook = EasyHook.LocalHook.Create(
                        getMessageWTarget,
                        new GetMessageWDelegate(HookedGetMessageW),
                        this);
                    _originalGetMessageW = Marshal.GetDelegateForFunctionPointer<GetMessageWDelegate>(getMessageWTarget);
                    getMessageWHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] GetMessageW hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] GetMessageW hook FAILED: " + e.Message);
                    getMessageWHook = null;
                }

                // ---- PeekMessageW hook (WM_MOUSEMOVE interception) ----
                try
                {
                    var peekMessageWTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "PeekMessageW");
                    peekMessageWHook = EasyHook.LocalHook.Create(
                        peekMessageWTarget,
                        new PeekMessageWDelegate(HookedPeekMessageW),
                        this);
                    _originalPeekMessageW = Marshal.GetDelegateForFunctionPointer<PeekMessageWDelegate>(peekMessageWTarget);
                    peekMessageWHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] PeekMessageW hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] PeekMessageW hook FAILED: " + e.Message);
                    peekMessageWHook = null;
                }

                // ---- GetRawInputData hook ----
                try
                {
                    var getRawInputDataTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetRawInputData");
                    getRawInputDataHook = EasyHook.LocalHook.Create(
                        getRawInputDataTarget,
                        new GetRawInputDataDelegate(HookedGetRawInputData),
                        this);
                    _originalGetRawInputData = Marshal.GetDelegateForFunctionPointer<GetRawInputDataDelegate>(getRawInputDataTarget);
                    getRawInputDataHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] GetRawInputData hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] GetRawInputData hook FAILED: " + e.Message);
                    getRawInputDataHook = null;
                }

                // ---- GetCursorInfo hook ----
                try
                {
                    var getCursorInfoTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetCursorInfo");
                    getCursorInfoHook = EasyHook.LocalHook.Create(
                        getCursorInfoTarget,
                        new GetCursorInfoDelegate(HookedGetCursorInfo),
                        this);
                    _originalGetCursorInfo = Marshal.GetDelegateForFunctionPointer<GetCursorInfoDelegate>(getCursorInfoTarget);
                    getCursorInfoHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] GetCursorInfo hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] GetCursorInfo hook FAILED: " + e.Message);
                    getCursorInfoHook = null;
                }

                // ---- SetCursorPos hook ----
                try
                {
                    var setCursorPosTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "SetCursorPos");
                    setCursorPosHook = EasyHook.LocalHook.Create(
                        setCursorPosTarget,
                        new SetCursorPosDelegate(HookedSetCursorPos),
                        this);
                    _originalSetCursorPos = Marshal.GetDelegateForFunctionPointer<SetCursorPosDelegate>(setCursorPosTarget);
                    setCursorPosHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] SetCursorPos hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] SetCursorPos hook FAILED: " + e.Message);
                    setCursorPosHook = null;
                }

                // ---- GetRawInputBuffer hook ----
                try
                {
                    var getRawInputBufferTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetRawInputBuffer");
                    getRawInputBufferHook = EasyHook.LocalHook.Create(
                        getRawInputBufferTarget,
                        new GetRawInputBufferDelegate(HookedGetRawInputBuffer),
                        this);
                    _originalGetRawInputBuffer = Marshal.GetDelegateForFunctionPointer<GetRawInputBufferDelegate>(getRawInputBufferTarget);
                    getRawInputBufferHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] GetRawInputBuffer hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] GetRawInputBuffer hook FAILED: " + e.Message);
                    getRawInputBufferHook = null;
                }

                // ---- GetPhysicalCursorPos hook ----
                try
                {
                    var getPhysicalCursorPosTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetPhysicalCursorPos");
                    getPhysicalCursorPosHook = EasyHook.LocalHook.Create(
                        getPhysicalCursorPosTarget,
                        new GetCursorPosDelegate(HookedGetPhysicalCursorPos),
                        this);
                    _originalGetPhysicalCursorPos = Marshal.GetDelegateForFunctionPointer<GetCursorPosDelegate>(getPhysicalCursorPosTarget);
                    getPhysicalCursorPosHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] GetPhysicalCursorPos hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] GetPhysicalCursorPos hook FAILED: " + e.Message);
                    getPhysicalCursorPosHook = null;
                }

                // ---- ClipCursor hook ----
                try
                {
                    var clipCursorTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "ClipCursor");
                    clipCursorHook = EasyHook.LocalHook.Create(
                        clipCursorTarget,
                        new ClipCursorDelegate(HookedClipCursor),
                        this);
                    _originalClipCursor = Marshal.GetDelegateForFunctionPointer<ClipCursorDelegate>(clipCursorTarget);
                    clipCursorHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] ClipCursor hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] ClipCursor hook FAILED: " + e.Message);
                    clipCursorHook = null;
                }

                // ---- ScreenToClient hook ----
                try
                {
                    var screenToClientTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "ScreenToClient");
                    screenToClientHook = EasyHook.LocalHook.Create(
                        screenToClientTarget,
                        new ScreenToClientDelegate(HookedScreenToClient),
                        this);
                    _originalScreenToClient = Marshal.GetDelegateForFunctionPointer<ScreenToClientDelegate>(screenToClientTarget);
                    screenToClientHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] ScreenToClient hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] ScreenToClient hook FAILED: " + e.Message);
                    screenToClientHook = null;
                }

                // ---- ClientToScreen hook ----
                try
                {
                    var clientToScreenTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "ClientToScreen");
                    clientToScreenHook = EasyHook.LocalHook.Create(
                        clientToScreenTarget,
                        new ClientToScreenDelegate(HookedClientToScreen),
                        this);
                    _originalClientToScreen = Marshal.GetDelegateForFunctionPointer<ClientToScreenDelegate>(clientToScreenTarget);
                    clientToScreenHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] ClientToScreen hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] ClientToScreen hook FAILED: " + e.Message);
                    clientToScreenHook = null;
                }

                // ---- GetMessageA hook (ANSI variant) ----
                try
                {
                    var getMessageATarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetMessageA");
                    getMessageAHook = EasyHook.LocalHook.Create(
                        getMessageATarget,
                        new GetMessageWDelegate(HookedGetMessageA),
                        this);
                    _originalGetMessageA = Marshal.GetDelegateForFunctionPointer<GetMessageWDelegate>(getMessageATarget);
                    getMessageAHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] GetMessageA hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] GetMessageA hook FAILED: " + e.Message);
                    getMessageAHook = null;
                }

                // ---- PeekMessageA hook (ANSI variant) ----
                try
                {
                    var peekMessageATarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "PeekMessageA");
                    peekMessageAHook = EasyHook.LocalHook.Create(
                        peekMessageATarget,
                        new PeekMessageWDelegate(HookedPeekMessageA),
                        this);
                    _originalPeekMessageA = Marshal.GetDelegateForFunctionPointer<PeekMessageWDelegate>(peekMessageATarget);
                    peekMessageAHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] PeekMessageA hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] PeekMessageA hook FAILED: " + e.Message);
                    peekMessageAHook = null;
                }

                // ---- GetForegroundWindow hook ----
                try
                {
                    var getForegroundWindowTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetForegroundWindow");
                    getForegroundWindowHook = EasyHook.LocalHook.Create(
                        getForegroundWindowTarget,
                        new GetForegroundWindowDelegate(HookedGetForegroundWindow),
                        this);
                    _originalGetForegroundWindow = Marshal.GetDelegateForFunctionPointer<GetForegroundWindowDelegate>(getForegroundWindowTarget);
                    getForegroundWindowHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] GetForegroundWindow hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] GetForegroundWindow hook FAILED: " + e.Message);
                    getForegroundWindowHook = null;
                }

                // ---- GetActiveWindow hook ----
                try
                {
                    var getActiveWindowTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetActiveWindow");
                    getActiveWindowHook = EasyHook.LocalHook.Create(
                        getActiveWindowTarget,
                        new GetActiveWindowDelegate(HookedGetActiveWindow),
                        this);
                    _originalGetActiveWindow = Marshal.GetDelegateForFunctionPointer<GetActiveWindowDelegate>(getActiveWindowTarget);
                    getActiveWindowHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] GetActiveWindow hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] GetActiveWindow hook FAILED: " + e.Message);
                    getActiveWindowHook = null;
                }

                // ---- GetFocus hook ----
                try
                {
                    var getFocusTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetFocus");
                    getFocusHook = EasyHook.LocalHook.Create(
                        getFocusTarget,
                        new GetFocusDelegate(HookedGetFocus),
                        this);
                    _originalGetFocus = Marshal.GetDelegateForFunctionPointer<GetFocusDelegate>(getFocusTarget);
                    getFocusHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] GetFocus hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] GetFocus hook FAILED: " + e.Message);
                    getFocusHook = null;
                }

                // ---- GetGUIThreadInfo hook ----
                try
                {
                    var getGuiThreadInfoTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetGUIThreadInfo");
                    getGuiThreadInfoHook = EasyHook.LocalHook.Create(
                        getGuiThreadInfoTarget,
                        new GetGUIThreadInfoDelegate(HookedGetGUIThreadInfo),
                        this);
                    _originalGetGUIThreadInfo = Marshal.GetDelegateForFunctionPointer<GetGUIThreadInfoDelegate>(getGuiThreadInfoTarget);
                    getGuiThreadInfoHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] GetGUIThreadInfo hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] GetGUIThreadInfo hook FAILED: " + e.Message);
                    getGuiThreadInfoHook = null;
                }

                // ---- GetCapture hook ----
                try
                {
                    var getCaptureTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetCapture");
                    getCaptureHook = EasyHook.LocalHook.Create(
                        getCaptureTarget,
                        new GetCaptureDelegate(HookedGetCapture),
                        this);
                    _originalGetCapture = Marshal.GetDelegateForFunctionPointer<GetCaptureDelegate>(getCaptureTarget);
                    getCaptureHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] GetCapture hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] GetCapture hook FAILED: " + e.Message);
                    getCaptureHook = null;
                }

                // ---- GetAsyncKeyState hook ----
                try
                {
                    var getAsyncKeyStateTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetAsyncKeyState");
                    getAsyncKeyStateHook = EasyHook.LocalHook.Create(
                        getAsyncKeyStateTarget,
                        new GetAsyncKeyStateDelegate(HookedGetAsyncKeyState),
                        this);
                    _originalGetAsyncKeyState = Marshal.GetDelegateForFunctionPointer<GetAsyncKeyStateDelegate>(getAsyncKeyStateTarget);
                    getAsyncKeyStateHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] GetAsyncKeyState hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] GetAsyncKeyState hook FAILED: " + e.Message);
                    getAsyncKeyStateHook = null;
                }

                // ---- GetKeyState hook ----
                try
                {
                    var getKeyStateTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetKeyState");
                    getKeyStateHook = EasyHook.LocalHook.Create(
                        getKeyStateTarget,
                        new GetKeyStateDelegate(HookedGetKeyState),
                        this);
                    _originalGetKeyState = Marshal.GetDelegateForFunctionPointer<GetKeyStateDelegate>(getKeyStateTarget);
                    getKeyStateHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] GetKeyState hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] GetKeyState hook FAILED: " + e.Message);
                    getKeyStateHook = null;
                }

                // ---- GetKeyboardState hook ----
                try
                {
                    var getKeyboardStateTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetKeyboardState");
                    getKeyboardStateHook = EasyHook.LocalHook.Create(
                        getKeyboardStateTarget,
                        new GetKeyboardStateDelegate(HookedGetKeyboardState),
                        this);
                    _originalGetKeyboardState = Marshal.GetDelegateForFunctionPointer<GetKeyboardStateDelegate>(getKeyboardStateTarget);
                    getKeyboardStateHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    _server.ReportMessage("[EasyHook:Target] GetKeyboardState hook installed successfully");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook:Target] GetKeyboardState hook FAILED: " + e.Message);
                    getKeyboardStateHook = null;
                }

                _server.SetState(HookState.HooksInstalled);
            }
            catch (Exception e)
            {
                _server.ReportMessage("[EasyHook:Target] Unexpected error during hook setup: " + e.ToString());
                _server.SetState(HookState.Failed);
                return;
            }

            _server.SetState(HookState.Running);

            try
            {
                // Loop until IPC fails
                while (!_server.ShutdownFlag)
                {
                    EnsureWndProcSubclassed();
                    System.Threading.Thread.Sleep(50);

                    string[] queued = null;

                    lock (_messageQueue)
                    {
                        queued = _messageQueue.ToArray();
                        _messageQueue.Clear();
                    }

                    if (queued != null && queued.Length > 0)
                    {
                        _server.ReportMessages(queued);
                    }
                    else
                    {
                        _server.Ping();
                    }
                }

                _server.ReportMessage("[EasyHook:Target] Shutdown flag received, cleaning up hooks");
            }
            catch
            {
                // Ping() or ReportMessages() will raise an exception if host is unreachable
                // Can't log via _server since it's disconnected - this is expected on app exit
            }

            // Remove hooks
            try
            {
                getCursorPosHook?.Dispose();
                isIconicHook?.Dispose();
                getMessageWHook?.Dispose();
                peekMessageWHook?.Dispose();
                getRawInputDataHook?.Dispose();
                getCursorInfoHook?.Dispose();
                setCursorPosHook?.Dispose();
                getRawInputBufferHook?.Dispose();
                getForegroundWindowHook?.Dispose();
                getActiveWindowHook?.Dispose();
                getFocusHook?.Dispose();
                getPhysicalCursorPosHook?.Dispose();
                clipCursorHook?.Dispose();
                getMessageAHook?.Dispose();
                peekMessageAHook?.Dispose();
                screenToClientHook?.Dispose();
                clientToScreenHook?.Dispose();
                getGuiThreadInfoHook?.Dispose();
                getCaptureHook?.Dispose();
                getAsyncKeyStateHook?.Dispose();
                getKeyStateHook?.Dispose();
                getKeyboardStateHook?.Dispose();
                RestoreWndProcSubclass();
                EasyHook.LocalHook.Release();
                _server.ReportMessage("[EasyHook:Target] Hooks disposed and released");
                _server.SetState(HookState.Disposed);
            }
            catch
            {
                // Host may already be gone, swallow
            }
        }

        #region Native structs and constants

        const uint WM_MOUSEMOVE = 0x0200;
        const uint WM_INPUT = 0x00FF;
        const uint WM_NULL = 0x0000;
        const uint WM_ACTIVATE = 0x0006;
        const uint WM_ACTIVATEAPP = 0x001C;
        const uint WM_KILLFOCUS = 0x0008;
        const uint WM_SETFOCUS = 0x0007;
        const uint WM_NCACTIVATE = 0x0086;
        const uint WM_MOUSEACTIVATE = 0x0021;
        const uint WA_INACTIVE = 0;
        const uint WA_ACTIVE = 1;
        const uint RID_INPUT = 0x10000003;
        const uint RIM_TYPEMOUSE = 0;
        const int CURSOR_SHOWING = 0x00000001;
        const int GWLP_WNDPROC = -4;

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

        [DllImport("user32.dll")]
        static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        static extern IntPtr SetWindowLongPtrW(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
        static extern IntPtr CallWindowProcW(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        static IntPtr MakeLParam(int x, int y)
        {
            return (IntPtr)((y << 16) | (x & 0xFFFF));
        }

        #endregion

        #region Window procedure subclassing

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private WndProcDelegate _subclassedWndProcDelegate;
        private IntPtr _originalWndProc = IntPtr.Zero;
        private IntPtr _subclassedWindowHandle = IntPtr.Zero;

        private bool IsCursorOverrideActive()
        {
            return true;
        }

        private void EnsureWndProcSubclassed()
        {
            try
            {
                if (_server == null)
                {
                    QueueMessage("[EasyHook:Target] EnsureWndProcSubclassed: server is null, skipping");
                    return;
                }

                if (_server.targetHwnd == IntPtr.Zero)
                {
                    QueueMessage("[EasyHook:Target] EnsureWndProcSubclassed: targetHwnd is Zero, skipping");
                    return;
                }

                var targetHandle = _server.targetHwnd;

                if (_subclassedWindowHandle == targetHandle && _originalWndProc != IntPtr.Zero)
                {
                    // Already subclassed for this handle — no action needed
                    return;
                }

                if (_subclassedWindowHandle != IntPtr.Zero && _originalWndProc != IntPtr.Zero)
                {
                    QueueMessage("[EasyHook:Target] EnsureWndProcSubclassed: target handle changed (old=0x" + _subclassedWindowHandle.ToString("X") + ", new=0x" + targetHandle.ToString("X") + "), restoring previous subclass");
                    RestoreWndProcSubclass();
                }

                QueueMessage("[EasyHook:Target] EnsureWndProcSubclassed: installing WndProc subclass on hwnd=0x" + targetHandle.ToString("X"));
                _subclassedWndProcDelegate = HookedWndProc;
                var newWndProcPointer = Marshal.GetFunctionPointerForDelegate(_subclassedWndProcDelegate);
                var originalWndProc = SetWindowLongPtrW(targetHandle, GWLP_WNDPROC, newWndProcPointer);

                if (originalWndProc == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != 0)
                        QueueMessage("[EasyHook:Target] Failed to subclass WndProc on hwnd=0x" + targetHandle.ToString("X") + ", SetWindowLongPtrW error: " + error);
                    else
                        QueueMessage("[EasyHook:Target] SetWindowLongPtrW returned Zero with no error on hwnd=0x" + targetHandle.ToString("X") + " (may already be subclassed or handle invalid)");
                    return;
                }

                _originalWndProc = originalWndProc;
                _subclassedWindowHandle = targetHandle;
                QueueMessage("[EasyHook:Target] WndProc subclass installed on hwnd=0x" + targetHandle.ToString("X") + ", original WndProc=0x" + originalWndProc.ToString("X"));
            }
            catch (Exception e)
            {
                QueueMessage("[EasyHook:Target] Error subclassing WndProc: " + e.Message);
            }
        }

        private void RestoreWndProcSubclass()
        {
            try
            {
                if (_subclassedWindowHandle == IntPtr.Zero || _originalWndProc == IntPtr.Zero)
                    return;

                SetWindowLongPtrW(_subclassedWindowHandle, GWLP_WNDPROC, _originalWndProc);
            }
            catch
            {
            }
            finally
            {
                _subclassedWindowHandle = IntPtr.Zero;
                _originalWndProc = IntPtr.Zero;
                _subclassedWndProcDelegate = null;
            }
        }

        private IntPtr HookedWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            LogFirstCall("WndProc");
            string step = "checking override active";
            try
            {
                if (IsCursorOverrideActive())
                {
                    if (msg == WM_MOUSEMOVE)
                    {
                        step = "rewriting WM_MOUSEMOVE lParam";
                        var clientPt = new POINT { X = _overridePoint.X, Y = _overridePoint.Y };
                        if (_originalScreenToClient != null)
                            _originalScreenToClient(hWnd, ref clientPt);
                        lParam = MakeLParam(clientPt.X, clientPt.Y);
                    }
                    else if (msg == WM_INPUT)
                    {
                        step = "converting WM_INPUT to WM_MOUSEMOVE";
                        var clientPt = new POINT { X = _overridePoint.X, Y = _overridePoint.Y };
                        if (_originalScreenToClient != null)
                            _originalScreenToClient(hWnd, ref clientPt);
                        lParam = MakeLParam(clientPt.X, clientPt.Y);
                        msg = WM_MOUSEMOVE;
                        wParam = IntPtr.Zero;
                    }
                }
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] WndProc error at '{step}': {e.Message}");
            }

            step = "calling original WndProc";
            try
            {
                if (_originalWndProc != IntPtr.Zero)
                    return CallWindowProcW(_originalWndProc, hWnd, msg, wParam, lParam);
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] WndProc error at '{step}': {e.Message}");
            }

            return IntPtr.Zero;
        }

        #endregion

        #region GetCursorPos hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool GetCursorPosDelegate(out POINT lpPoint);
        private GetCursorPosDelegate _originalGetCursorPos;
        
        public bool HookedGetCursorPos(out POINT lpPoint)
        {
            LogFirstCall("GetCursorPos");
            try
            {
                lpPoint.X = _overridePoint.X;
                lpPoint.Y = _overridePoint.Y;
                return true;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetCursorPos error: {e.Message}");
                lpPoint = new POINT();
                return false;
            }
        }

        #endregion

        #region IsIconic hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool IsIconicDelegate(IntPtr hWnd);
        private IsIconicDelegate _originalIsIconic;

        public bool HookedIsIconic(IntPtr hWnd)
        {
            LogFirstCall("IsIconic");
            return false;
        }

        #endregion

        #region GetMessageW hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int GetMessageWDelegate(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        private GetMessageWDelegate _originalGetMessageW;

        public int HookedGetMessageW(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
        {
            LogFirstCall("GetMessageW");
            string step = "calling original";
            try
            {
                int result = _originalGetMessageW(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
                if (result != 0)
                {
                    step = "filtering message";
                    FilterMessage(ref lpMsg);
                }
                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetMessageW error at '{step}': {e.Message}");
                lpMsg = new MSG();
                return 0;
            }
        }

        #endregion

        #region PeekMessageW hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool PeekMessageWDelegate(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        private PeekMessageWDelegate _originalPeekMessageW;

        public bool HookedPeekMessageW(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            LogFirstCall("PeekMessageW");
            string step = "calling original";
            try
            {
                bool result = _originalPeekMessageW(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
                if (result)
                {
                    step = "filtering message";
                    FilterMessage(ref lpMsg);
                }
                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] PeekMessageW error at '{step}': {e.Message}");
                lpMsg = new MSG();
                return false;
            }
        }

        #endregion

        #region GetRawInputData hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint GetRawInputDataDelegate(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);
        private GetRawInputDataDelegate _originalGetRawInputData;

        public uint HookedGetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            LogFirstCall("GetRawInputData");
            string step = "calling original";
            try
            {
                uint result = _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
                step = "checking if mouse RID_INPUT";
                if (pData != IntPtr.Zero && uiCommand == RID_INPUT)
                {
                    step = "reading RAWINPUTHEADER dwType";
                    uint dwType = (uint)Marshal.ReadInt32(pData, 0);
                    if (dwType == RIM_TYPEMOUSE)
                    {
                        step = "zeroing lLastX/lLastY in RAWMOUSE";
                        int headerSize = Marshal.SizeOf(typeof(RAWINPUTHEADER));
                        int lLastXOffset = headerSize + 12;
                        int lLastYOffset = headerSize + 16;
                        Marshal.WriteInt32(pData, lLastXOffset, 0);
                        Marshal.WriteInt32(pData, lLastYOffset, 0);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetRawInputData error at '{step}': {e.Message}");
                return 0;
            }
        }

        #endregion

        #region GetCursorInfo hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool GetCursorInfoDelegate(ref CURSORINFO pci);
        private GetCursorInfoDelegate _originalGetCursorInfo;

        public bool HookedGetCursorInfo(ref CURSORINFO pci)
        {
            LogFirstCall("GetCursorInfo");
            string step = "calling original";
            try
            {
                bool result = _originalGetCursorInfo(ref pci);
                step = "overriding cursor position";
                if (result)
                {
                    pci.ptScreenPos.X = _overridePoint.X;
                    pci.ptScreenPos.Y = _overridePoint.Y;
                }
                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetCursorInfo error at '{step}': {e.Message}");
                return false;
            }
        }

        #endregion

        #region Shared message filter

        /// <summary>
        /// Filters messages when fixed position is active:
        /// - WM_MOUSEMOVE: replaces coordinates with fixed position
        /// - WM_KILLFOCUS / WM_ACTIVATE(WA_INACTIVE) / WM_ACTIVATEAPP(0) / WM_NCACTIVATE(0):
        ///   converted to no-ops so the game thinks it still has focus
        /// </summary>
        private void FilterMessage(ref MSG lpMsg)
        {
            switch (lpMsg.message)
            {
                case WM_MOUSEMOVE:
                {
                    var clientPt = new POINT { X = _overridePoint.X, Y = _overridePoint.Y };
                    if (_originalScreenToClient != null)
                        _originalScreenToClient(lpMsg.hwnd, ref clientPt);
                    lpMsg.lParam = MakeLParam(clientPt.X, clientPt.Y);
                    lpMsg.pt = new POINT { X = _overridePoint.X, Y = _overridePoint.Y };
                    break;
                }
                case WM_KILLFOCUS:
                    // Suppress — change to WM_SETFOCUS so the game thinks it gained focus
                    lpMsg.message = WM_SETFOCUS;
                    lpMsg.wParam = IntPtr.Zero;
                    break;

                case WM_ACTIVATE:
                {
                    uint loWord = (uint)((long)lpMsg.wParam & 0xFFFF);
                    if (loWord == WA_INACTIVE)
                    {
                        // Change WA_INACTIVE to WA_ACTIVE so the game thinks it's active
                        lpMsg.wParam = (IntPtr)WA_ACTIVE;
                    }
                    break;
                }

                case WM_ACTIVATEAPP:
                    if (lpMsg.wParam == IntPtr.Zero)
                    {
                        // wParam=0 means deactivating — fake it to activated
                        lpMsg.wParam = (IntPtr)1;
                    }
                    break;

                case WM_NCACTIVATE:
                    if (lpMsg.wParam == IntPtr.Zero)
                    {
                        // wParam=FALSE means deactivating — fake it to TRUE
                        lpMsg.wParam = (IntPtr)1;
                    }
                    break;
            }
        }

        #endregion

        #region ScreenToClient / ClientToScreen hooks

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool ScreenToClientDelegate(IntPtr hWnd, ref POINT lpPoint);
        private ScreenToClientDelegate _originalScreenToClient;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool ClientToScreenDelegate(IntPtr hWnd, ref POINT lpPoint);
        private ClientToScreenDelegate _originalClientToScreen;

        public bool HookedScreenToClient(IntPtr hWnd, ref POINT lpPoint)
        {
            LogFirstCall("ScreenToClient");
            string step = "checking override active";
            try
            {
                if (IsCursorOverrideActive() && _server.targetHwnd != IntPtr.Zero && hWnd == _server.targetHwnd)
                {
                    step = "creating fixed client point";
                    var clientPt = new POINT { X = _overridePoint.X, Y = _overridePoint.Y };
                    step = "calling original ScreenToClient for translation";
                    if (_originalScreenToClient != null)
                        _originalScreenToClient(hWnd, ref clientPt);
                    lpPoint = clientPt;
                    return true;
                }
                step = "calling original ScreenToClient (passthrough)";
                return _originalScreenToClient != null && _originalScreenToClient(hWnd, ref lpPoint);
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] ScreenToClient error at '{step}': {e.Message}");
                return false;
            }
        }

        public bool HookedClientToScreen(IntPtr hWnd, ref POINT lpPoint)
        {
            LogFirstCall("ClientToScreen");
            string step = "checking override active";
            try
            {
                if (IsCursorOverrideActive() && _server.targetHwnd != IntPtr.Zero && hWnd == _server.targetHwnd)
                {
                    step = "setting fixed screen point";
                    lpPoint = new POINT { X = _overridePoint.X, Y = _overridePoint.Y };
                    return true;
                }
                step = "calling original ClientToScreen (passthrough)";
                return _originalClientToScreen != null && _originalClientToScreen(hWnd, ref lpPoint);
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] ClientToScreen error at '{step}': {e.Message}");
                return false;
            }
        }

        #endregion

        #region GetForegroundWindow hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool GetGUIThreadInfoDelegate(uint idThread, ref GUITHREADINFO pgui);
        private GetGUIThreadInfoDelegate _originalGetGUIThreadInfo;

        public bool HookedGetGUIThreadInfo(uint idThread, ref GUITHREADINFO pgui)
        {
            LogFirstCall("GetGUIThreadInfo");
            string step = "init";
            try
            {
                step = "setting cbSize";
                if (pgui.cbSize == 0)
                    pgui.cbSize = (uint)Marshal.SizeOf(typeof(GUITHREADINFO));

                step = "calling original GetGUIThreadInfo";
                bool result = false;
                if (_originalGetGUIThreadInfo != null)
                    result = _originalGetGUIThreadInfo(idThread, ref pgui);

                step = "checking override active";
                if (IsCursorOverrideActive() && _server.targetHwnd != IntPtr.Zero)
                {
                    step = "spoofing GUI thread info handles";
                    pgui.hwndActive = _server.targetHwnd;
                    pgui.hwndFocus = _server.targetHwnd;
                    pgui.hwndCapture = _server.targetHwnd;
                    if (pgui.hwndMenuOwner == IntPtr.Zero)
                        pgui.hwndMenuOwner = _server.targetHwnd;
                    if (pgui.hwndMoveSize == IntPtr.Zero)
                        pgui.hwndMoveSize = _server.targetHwnd;
                    return true;
                }
                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetGUIThreadInfo error at '{step}': {e.Message}");
                return false;
            }
        }

        #endregion

        #region Keyboard/Capture hooks

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr GetCaptureDelegate();
        private GetCaptureDelegate _originalGetCapture;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate short GetAsyncKeyStateDelegate(int vKey);
        private GetAsyncKeyStateDelegate _originalGetAsyncKeyState;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate short GetKeyStateDelegate(int nVirtKey);
        private GetKeyStateDelegate _originalGetKeyState;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool GetKeyboardStateDelegate(IntPtr lpKeyState);
        private GetKeyboardStateDelegate _originalGetKeyboardState;

        public IntPtr HookedGetCapture()
        {
            LogFirstCall("GetCapture");
            string step = "checking override active";
            try
            {
                if (IsCursorOverrideActive() && _server.targetHwnd != IntPtr.Zero)
                    return _server.targetHwnd;
                step = "calling original GetCapture";
                return _originalGetCapture != null ? _originalGetCapture() : IntPtr.Zero;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetCapture error at '{step}': {e.Message}");
                return IntPtr.Zero;
            }
        }

        public short HookedGetAsyncKeyState(int vKey)
        {
            LogFirstCall("GetAsyncKeyState");
            try
            {
                if (_originalGetAsyncKeyState != null)
                    return _originalGetAsyncKeyState(vKey);
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetAsyncKeyState error: {e.Message}");
            }
            return 0;
        }

        public short HookedGetKeyState(int nVirtKey)
        {
            LogFirstCall("GetKeyState");
            try
            {
                if (_originalGetKeyState != null)
                    return _originalGetKeyState(nVirtKey);
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetKeyState error: {e.Message}");
            }
            return 0;
        }

        public bool HookedGetKeyboardState(IntPtr lpKeyState)
        {
            LogFirstCall("GetKeyboardState");
            string step = "calling original";
            try
            {
                if (_originalGetKeyboardState != null)
                {
                    bool result = _originalGetKeyboardState(lpKeyState);
                    if (result)
                        return true;
                }
                step = "providing zeroed fallback state";
                if (IsCursorOverrideActive() && lpKeyState != IntPtr.Zero)
                {
                    for (int i = 0; i < 256; i++)
                        Marshal.WriteByte(lpKeyState, i, 0);
                    return true;
                }
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetKeyboardState error at '{step}': {e.Message}");
            }
            return false;
        }

        #endregion

        #region GetForegroundWindow hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr GetForegroundWindowDelegate();
        private GetForegroundWindowDelegate _originalGetForegroundWindow;

        public IntPtr HookedGetForegroundWindow()
        {
            LogFirstCall("GetForegroundWindow");
            string step = "checking targetHwnd";
            try
            {
                if (_server.targetHwnd != IntPtr.Zero)
                    return _server.targetHwnd;
                step = "calling original GetForegroundWindow";
                return _originalGetForegroundWindow();
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetForegroundWindow error at '{step}': {e.Message}");
                return IntPtr.Zero;
            }
        }

        #endregion

        #region GetActiveWindow hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr GetActiveWindowDelegate();
        private GetActiveWindowDelegate _originalGetActiveWindow;

        public IntPtr HookedGetActiveWindow()
        {
            LogFirstCall("GetActiveWindow");
            string step = "checking targetHwnd";
            try
            {
                if (_server.targetHwnd != IntPtr.Zero)
                    return _server.targetHwnd;
                step = "calling original GetActiveWindow";
                return _originalGetActiveWindow();
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetActiveWindow error at '{step}': {e.Message}");
                return IntPtr.Zero;
            }
        }

        #endregion

        #region GetFocus hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr GetFocusDelegate();
        private GetFocusDelegate _originalGetFocus;

        public IntPtr HookedGetFocus()
        {
            LogFirstCall("GetFocus");
            string step = "checking targetHwnd";
            try
            {
                if (_server.targetHwnd != IntPtr.Zero)
                    return _server.targetHwnd;
                step = "calling original GetFocus";
                return _originalGetFocus();
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetFocus error at '{step}': {e.Message}");
                return IntPtr.Zero;
            }
        }

        #endregion

        #region SetCursorPos hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool SetCursorPosDelegate(int X, int Y);
        private SetCursorPosDelegate _originalSetCursorPos;

        public bool HookedSetCursorPos(int X, int Y)
        {
            LogFirstCall("SetCursorPos");
            try
            {
                return true;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] SetCursorPos error: {e.Message}");
                return false;
            }
        }

        #endregion

        #region GetRawInputBuffer hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint GetRawInputBufferDelegate(IntPtr pData, ref uint pcbSize, uint cbSizeHeader);
        private GetRawInputBufferDelegate _originalGetRawInputBuffer;

        public uint HookedGetRawInputBuffer(IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            LogFirstCall("GetRawInputBuffer");
            string step = "calling original";
            try
            {
                uint result = _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);
                step = "checking if buffer has mouse data";
                if (pData != IntPtr.Zero && result > 0)
                {
                    int headerSize = Marshal.SizeOf(typeof(RAWINPUTHEADER));
                    IntPtr current = pData;

                    for (uint i = 0; i < result; i++)
                    {
                        step = $"reading RAWINPUTHEADER[{i}]";
                        uint dwType = (uint)Marshal.ReadInt32(current, 0);
                        uint dwSize = (uint)Marshal.ReadInt32(current, 4);

                        if (dwType == RIM_TYPEMOUSE)
                        {
                            step = $"zeroing mouse delta[{i}]";
                            int lLastXOffset = headerSize + 12;
                            int lLastYOffset = headerSize + 16;
                            Marshal.WriteInt32(current, lLastXOffset, 0);
                            Marshal.WriteInt32(current, lLastYOffset, 0);
                        }

                        long aligned = ((long)dwSize + 7) & ~7L;
                        current = new IntPtr(current.ToInt64() + aligned);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetRawInputBuffer error at '{step}': {e.Message}");
                return 0;
            }
        }

        #endregion

        #region GetPhysicalCursorPos hook

        private GetCursorPosDelegate _originalGetPhysicalCursorPos;

        public bool HookedGetPhysicalCursorPos(out POINT lpPoint)
        {
            LogFirstCall("GetPhysicalCursorPos");
            try
            {
                lpPoint.X = _overridePoint.X;
                lpPoint.Y = _overridePoint.Y;
                return true;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetPhysicalCursorPos error: {e.Message}");
                lpPoint = new POINT();
                return false;
            }
        }

        #endregion

        #region ClipCursor hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool ClipCursorDelegate(IntPtr lpRect);
        private ClipCursorDelegate _originalClipCursor;

        public bool HookedClipCursor(IntPtr lpRect)
        {
            LogFirstCall("ClipCursor");
            try
            {
                return true;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] ClipCursor error: {e.Message}");
                return false;
            }
        }

        #endregion

        #region GetMessageA / PeekMessageA hooks (ANSI variants)

        private GetMessageWDelegate _originalGetMessageA;
        private PeekMessageWDelegate _originalPeekMessageA;

        public int HookedGetMessageA(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
        {
            LogFirstCall("GetMessageA");
            string step = "calling original";
            try
            {
                int result = _originalGetMessageA(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
                if (result != 0)
                {
                    step = "filtering message";
                    FilterMessage(ref lpMsg);
                }
                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetMessageA error at '{step}': {e.Message}");
                lpMsg = new MSG();
                return 0;
            }
        }

        public bool HookedPeekMessageA(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            LogFirstCall("PeekMessageA");
            string step = "calling original";
            try
            {
                bool result = _originalPeekMessageA(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
                if (result)
                {
                    step = "filtering message";
                    FilterMessage(ref lpMsg);
                }
                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] PeekMessageA error at '{step}': {e.Message}");
                lpMsg = new MSG();
                return false;
            }
        }

        #endregion
    }
}
