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

        private bool _hasLoggedFirstCursorIntercept = false;

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

        [DllImport("user32.dll")]
        static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        static IntPtr MakeLParam(int x, int y)
        {
            return (IntPtr)((y << 16) | (x & 0xFFFF));
        }

        #endregion

        #region GetCursorPos hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool GetCursorPosDelegate(out POINT lpPoint);
        private GetCursorPosDelegate _originalGetCursorPos;
        
        public bool HookedGetCursorPos(out POINT lpPoint)
        {
            try
            {
                // Call the original function
                if (_server.point.X == -1 && _server.point.Y == -1)
                    return _originalGetCursorPos(out lpPoint);

                lpPoint.X = _server.point.X;
                lpPoint.Y = _server.point.Y;

                if (!_hasLoggedFirstCursorIntercept)
                {
                    _hasLoggedFirstCursorIntercept = true;
                    try
                    {
                        _server.ReportMessage($"[EasyHook:Target] First cursor position intercept: ({lpPoint.X}, {lpPoint.Y})");
                    }
                    catch { /* IPC may fail, don't crash target */ }
                }

                return true;
            }
            catch (Exception e)
            {
                _server.ReportMessage("[EasyHook:Target] Error reading cursor point from server: " + e.Message);
                // Fallback to original to avoid crashing the target process
                return _originalGetCursorPos(out lpPoint);
            }
        }

        #endregion

        #region IsIconic hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool IsIconicDelegate(IntPtr hWnd);
        private IsIconicDelegate _originalIsIconic;

        public bool HookedIsIconic(IntPtr hWnd)
        {
            return false;
        }

        #endregion

        #region GetMessageW hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int GetMessageWDelegate(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        private GetMessageWDelegate _originalGetMessageW;

        public int HookedGetMessageW(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
        {
            int result = _originalGetMessageW(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
            try
            {
                if (result != 0 && (_server.point.X != -1 || _server.point.Y != -1))
                {
                    FilterMessage(ref lpMsg);
                }
            }
            catch { /* never crash the target */ }
            return result;
        }

        #endregion

        #region PeekMessageW hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool PeekMessageWDelegate(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        private PeekMessageWDelegate _originalPeekMessageW;

        public bool HookedPeekMessageW(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            bool result = _originalPeekMessageW(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
            try
            {
                if (result && (_server.point.X != -1 || _server.point.Y != -1))
                {
                    FilterMessage(ref lpMsg);
                }
            }
            catch { /* never crash the target */ }
            return result;
        }

        #endregion

        #region GetRawInputData hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint GetRawInputDataDelegate(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);
        private GetRawInputDataDelegate _originalGetRawInputData;

        public uint HookedGetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            uint result = _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
            try
            {
                // Only process when we have data, the command is RID_INPUT, and fixed position is active
                if (pData != IntPtr.Zero && uiCommand == RID_INPUT &&
                    (_server.point.X != -1 || _server.point.Y != -1))
                {
                    // Read dwType from RAWINPUTHEADER at offset 0
                    uint dwType = (uint)Marshal.ReadInt32(pData, 0);
                    if (dwType == RIM_TYPEMOUSE)
                    {
                        // Zero out lLastX and lLastY in RAWMOUSE (follows RAWINPUTHEADER)
                        int headerSize = Marshal.SizeOf(typeof(RAWINPUTHEADER));
                        // RAWMOUSE layout: usFlags(2) + pad(2) + ulButtons(4) + ulRawButtons(4) + lLastX(4) + lLastY(4)
                        int lLastXOffset = headerSize + 12;
                        int lLastYOffset = headerSize + 16;
                        Marshal.WriteInt32(pData, lLastXOffset, 0);
                        Marshal.WriteInt32(pData, lLastYOffset, 0);
                    }
                }
            }
            catch { /* never crash the target */ }
            return result;
        }

        #endregion

        #region GetCursorInfo hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool GetCursorInfoDelegate(ref CURSORINFO pci);
        private GetCursorInfoDelegate _originalGetCursorInfo;

        public bool HookedGetCursorInfo(ref CURSORINFO pci)
        {
            bool result = _originalGetCursorInfo(ref pci);
            try
            {
                if (result && (_server.point.X != -1 || _server.point.Y != -1))
                {
                    pci.ptScreenPos.X = _server.point.X;
                    pci.ptScreenPos.Y = _server.point.Y;
                }
            }
            catch { /* never crash the target */ }
            return result;
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
                    var clientPt = new POINT { X = _server.point.X, Y = _server.point.Y };
                    ScreenToClient(lpMsg.hwnd, ref clientPt);
                    lpMsg.lParam = MakeLParam(clientPt.X, clientPt.Y);
                    lpMsg.pt = new POINT { X = _server.point.X, Y = _server.point.Y };
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

        #region GetForegroundWindow hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr GetForegroundWindowDelegate();
        private GetForegroundWindowDelegate _originalGetForegroundWindow;

        public IntPtr HookedGetForegroundWindow()
        {
            try
            {
                if ((_server.point.X != -1 || _server.point.Y != -1) && _server.targetHwnd != IntPtr.Zero)
                    return _server.targetHwnd;
            }
            catch { /* never crash the target */ }
            return _originalGetForegroundWindow();
        }

        #endregion

        #region GetActiveWindow hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr GetActiveWindowDelegate();
        private GetActiveWindowDelegate _originalGetActiveWindow;

        public IntPtr HookedGetActiveWindow()
        {
            try
            {
                if ((_server.point.X != -1 || _server.point.Y != -1) && _server.targetHwnd != IntPtr.Zero)
                    return _server.targetHwnd;
            }
            catch { /* never crash the target */ }
            return _originalGetActiveWindow();
        }

        #endregion

        #region GetFocus hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr GetFocusDelegate();
        private GetFocusDelegate _originalGetFocus;

        public IntPtr HookedGetFocus()
        {
            try
            {
                if ((_server.point.X != -1 || _server.point.Y != -1) && _server.targetHwnd != IntPtr.Zero)
                    return _server.targetHwnd;
            }
            catch { /* never crash the target */ }
            return _originalGetFocus();
        }

        #endregion

        #region SetCursorPos hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool SetCursorPosDelegate(int X, int Y);
        private SetCursorPosDelegate _originalSetCursorPos;

        public bool HookedSetCursorPos(int X, int Y)
        {
            try
            {
                // When fixed position is active, suppress the game's cursor warp
                if (_server.point.X != -1 || _server.point.Y != -1)
                    return true;
            }
            catch { /* never crash the target */ }
            return _originalSetCursorPos(X, Y);
        }

        #endregion

        #region GetRawInputBuffer hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint GetRawInputBufferDelegate(IntPtr pData, ref uint pcbSize, uint cbSizeHeader);
        private GetRawInputBufferDelegate _originalGetRawInputBuffer;

        public uint HookedGetRawInputBuffer(IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            uint result = _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);
            try
            {
                if (pData != IntPtr.Zero && result > 0 &&
                    (_server.point.X != -1 || _server.point.Y != -1))
                {
                    int headerSize = Marshal.SizeOf(typeof(RAWINPUTHEADER));
                    IntPtr current = pData;

                    for (uint i = 0; i < result; i++)
                    {
                        // Read the RAWINPUTHEADER to get dwType and dwSize
                        uint dwType = (uint)Marshal.ReadInt32(current, 0);
                        uint dwSize = (uint)Marshal.ReadInt32(current, 4);

                        if (dwType == RIM_TYPEMOUSE)
                        {
                            // Zero out lLastX and lLastY
                            int lLastXOffset = headerSize + 12;
                            int lLastYOffset = headerSize + 16;
                            Marshal.WriteInt32(current, lLastXOffset, 0);
                            Marshal.WriteInt32(current, lLastYOffset, 0);
                        }

                        // Advance to next RAWINPUT (aligned to 8 bytes on x64)
                        long aligned = ((long)dwSize + 7) & ~7L;
                        current = new IntPtr(current.ToInt64() + aligned);
                    }
                }
            }
            catch { /* never crash the target */ }
            return result;
        }

        #endregion

        #region GetPhysicalCursorPos hook

        private GetCursorPosDelegate _originalGetPhysicalCursorPos;

        public bool HookedGetPhysicalCursorPos(out POINT lpPoint)
        {
            try
            {
                if (_server.point.X == -1 && _server.point.Y == -1)
                    return _originalGetPhysicalCursorPos(out lpPoint);

                lpPoint.X = _server.point.X;
                lpPoint.Y = _server.point.Y;
                return true;
            }
            catch
            {
                return _originalGetPhysicalCursorPos(out lpPoint);
            }
        }

        #endregion

        #region ClipCursor hook

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool ClipCursorDelegate(IntPtr lpRect);
        private ClipCursorDelegate _originalClipCursor;

        public bool HookedClipCursor(IntPtr lpRect)
        {
            try
            {
                // When fixed position is active, suppress cursor clipping
                if (_server.point.X != -1 || _server.point.Y != -1)
                    return true;
            }
            catch { /* never crash the target */ }
            return _originalClipCursor(lpRect);
        }

        #endregion

        #region GetMessageA / PeekMessageA hooks (ANSI variants)

        private GetMessageWDelegate _originalGetMessageA;
        private PeekMessageWDelegate _originalPeekMessageA;

        public int HookedGetMessageA(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
        {
            int result = _originalGetMessageA(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
            try
            {
                if (result != 0 && (_server.point.X != -1 || _server.point.Y != -1))
                {
                    FilterMessage(ref lpMsg);
                }
            }
            catch { /* never crash the target */ }
            return result;
        }

        public bool HookedPeekMessageA(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            bool result = _originalPeekMessageA(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
            try
            {
                if (result && (_server.point.X != -1 || _server.point.Y != -1))
                {
                    FilterMessage(ref lpMsg);
                }
            }
            catch { /* never crash the target */ }
            return result;
        }

        #endregion
    }
}
