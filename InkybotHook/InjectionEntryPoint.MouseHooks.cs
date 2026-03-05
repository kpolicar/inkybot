using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using static InkybotHook.NativeMethods;

namespace InkybotHook
{
    public partial class InjectionEntryPoint
    {
        private readonly System.Threading.ManualResetEventSlim _allHooksInstalled = new System.Threading.ManualResetEventSlim(false);

        // =========================================================
        // 1. DELEGATES (Stripped down to only what is needed for injection)
        // =========================================================
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate bool PeekMessageWDelegate(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        private PeekMessageWDelegate _originalPeekMessageW;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GetRawInputDataDelegate(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);
        private GetRawInputDataDelegate _originalGetRawInputData;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GetRawInputBufferDelegate(IntPtr pData, ref uint pcbSize, uint cbSizeHeader);
        private GetRawInputBufferDelegate _originalGetRawInputBuffer;

        // =========================================================
        // 2. CONSTANTS & STRUCTS
        // =========================================================
        private const uint WM_NULL = 0x0000;
        private const uint WM_INPUT = 0x00FF;
        private const uint RID_INPUT = 0x10000003;
        private const uint RIM_TYPEMOUSE = 0;
        
        // RAWINPUT button flags
        private const uint RI_MOUSE_LEFT_BUTTON_DOWN = 0x0001;
        private const uint RI_MOUSE_LEFT_BUTTON_UP = 0x0002;

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

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate short GetAsyncKeyStateDelegate(int vKey);
        private GetAsyncKeyStateDelegate _originalGetAsyncKeyState;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate short GetKeyStateDelegate(int nVirtKey);
        private GetKeyStateDelegate _originalGetKeyState;

        private const int VK_LBUTTON = 0x01;

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

        // --- AUTOMATION STATE VARIABLES ---
        private enum ForgeState { Idle, ButtonDown, ButtonUp } 
        private volatile ForgeState _rawState = ForgeState.Idle;
        private volatile ForgeState _lastInjectedRawState = ForgeState.Idle;
        private volatile ForgeState _lastPeekState = ForgeState.Idle;
        
        private int _lastStateChangeTime = 0;
        private IntPtr _mainHwnd = IntPtr.Zero; 

        private IntPtr _capturedDevice = IntPtr.Zero;      
        private uint _capturedPacketSize = 0;         
        private uint _rawInputHeaderSize = 0;         
        private readonly object _deviceLock = new object();

        private IntPtr _nativeFakePacketPtr = IntPtr.Zero;
        private int _nativeFakePacketSize = 0; 
        private readonly object _nativeBufLock = new object();
        private const int MAGIC_RAW_HANDLE = 0x1337;

        private Thread _automationThread;
        private volatile bool _stopAutomationThread = false;

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, UIntPtr size);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // =========================================================
        // 4. INSTALLATION
        // =========================================================
        private List<EasyHook.LocalHook> InstallMousePositionHooks()
        {
            var hooks = new List<EasyHook.LocalHook>();

            var peekMessageWHook = TryInstallHook<PeekMessageWDelegate>("PeekMessageW", new PeekMessageWDelegate(HookedPeekMessageW), out _originalPeekMessageW);
            if (peekMessageWHook != null) hooks.Add(peekMessageWHook);

            var getRawInputDataHook = TryInstallHook<GetRawInputDataDelegate>("GetRawInputData", new GetRawInputDataDelegate(HookedGetRawInputData), out _originalGetRawInputData);
            if (getRawInputDataHook != null) hooks.Add(getRawInputDataHook);

            var getRawInputBufferHook = TryInstallHook<GetRawInputBufferDelegate>("GetRawInputBuffer", new GetRawInputBufferDelegate(HookedGetRawInputBuffer), out _originalGetRawInputBuffer);
            if (getRawInputBufferHook != null) hooks.Add(getRawInputBufferHook);

            var getAsyncKeyStateHook = TryInstallHook<GetAsyncKeyStateDelegate>("GetAsyncKeyState", new GetAsyncKeyStateDelegate(HookedGetAsyncKeyState), out _originalGetAsyncKeyState);
            if (getAsyncKeyStateHook != null) hooks.Add(getAsyncKeyStateHook);

            var getKeyStateHook = TryInstallHook<GetKeyStateDelegate>("GetKeyState", new GetKeyStateDelegate(HookedGetKeyState), out _originalGetKeyState);
            if (getKeyStateHook != null) hooks.Add(getKeyStateHook);

            _stopAutomationThread = false;
            _automationThread = new Thread(AutomationThreadLoop) { IsBackground = true, Name = "Inkybot_AutomationThread" };
            _automationThread.Start();

            _allHooksInstalled.Set();
            return hooks;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;
        private const int MK_LBUTTON = 0x0001;

        private void AutomationThreadLoop()
        {
            while (!_stopAutomationThread)
            {
                try
                {
                    int now = Environment.TickCount;

                    // 1. Manage state machine with delays
                    if (_rawState == ForgeState.Idle && (now - _lastStateChangeTime >= 1000))
                    {
                        _rawState = ForgeState.ButtonDown;
                        _lastStateChangeTime = now;

                        // --- INJECT UI CLICK DOWN ---
                        if (_mainHwnd != IntPtr.Zero)
                        {
                            GetCursorPos(out POINT pt);
                            ScreenToClient(_mainHwnd, ref pt);
                            IntPtr lParam = (IntPtr)((pt.Y << 16) | (pt.X & 0xFFFF));
                            PostMessage(_mainHwnd, WM_LBUTTONDOWN, (IntPtr)MK_LBUTTON, lParam);
                        }
                    }
                    else if (_rawState == ForgeState.ButtonDown && (now - _lastStateChangeTime >= 50))
                    {
                        _rawState = ForgeState.ButtonUp;
                        _lastStateChangeTime = now;

                        // --- INJECT UI CLICK UP ---
                        if (_mainHwnd != IntPtr.Zero)
                        {
                            GetCursorPos(out POINT pt);
                            ScreenToClient(_mainHwnd, ref pt);
                            IntPtr lParam = (IntPtr)((pt.Y << 16) | (pt.X & 0xFFFF));
                            PostMessage(_mainHwnd, WM_LBUTTONUP, IntPtr.Zero, lParam);
                        }
                    }
                    else if (_rawState == ForgeState.ButtonUp && (now - _lastStateChangeTime >= 50))
                    {
                        _rawState = ForgeState.Idle;
                        _lastStateChangeTime = now;
                    }

                    // 2. Wake up the main thread for RAW INPUT injection
                    bool pendingRaw = (_rawState != ForgeState.Idle) && (_lastInjectedRawState != _rawState);

                    if (pendingRaw && _mainHwnd != IntPtr.Zero)
                    {
                        // Force the game loop to spin so our PeekMessage hook triggers
                        PostMessage(_mainHwnd, WM_NULL, IntPtr.Zero, IntPtr.Zero);
                    }
                }
                catch (Exception ex)
                {
                    _server.ReportMessage($"[EXCEPTION in AutomationThreadLoop]\n{ex}");
                }

                Thread.Sleep(16);
            }
        }

        // =========================================================
        // 6. IMPLEMENTATIONS
        // =========================================================

        private bool HookedPeekMessageW(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            _allHooksInstalled.Wait();
            bool result = false;

            try
            {
                result = _originalPeekMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);

                if (_mainHwnd == IntPtr.Zero && lpMsg.hwnd != IntPtr.Zero) 
                {
                    _mainHwnd = lpMsg.hwnd;
                }

                // Use our new _lastPeekState variable here
                bool pendingPeek = (_rawState != ForgeState.Idle) && (_lastPeekState != _rawState);

                if (pendingPeek && (!result || lpMsg.message == WM_NULL))
                {
                    lpMsg.hwnd = _mainHwnd; 
                    lpMsg.message = WM_INPUT;
                    lpMsg.wParam = IntPtr.Zero; 
                    lpMsg.lParam = (IntPtr)MAGIC_RAW_HANDLE;
                    lpMsg.time = (uint)Environment.TickCount;
                    
                    // CRITICAL FIX: Only mark as injected if the game is consuming the message!
                    // This prevents the infinite while(PeekMessage) deadlock.
                    if ((wRemoveMsg & 0x0001/*PM_REMOVE*/) != 0)
                    {
                        _server.ReportMessage($"Injecting Synthetic via PeekMessage! State: {_rawState}");
                        _lastPeekState = _rawState; 
                    }

                    return true;
                }

                return result;
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in HookedPeekMessageW]\n{ex}");
                return _originalPeekMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
            }
        }

        private uint HookedGetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            _allHooksInstalled.Wait();

            try
            {
                // -------------------------------------------------------------
                // SYNTHETIC INJECTION (THE MAGIC HANDLE BYPASS)
                // -------------------------------------------------------------
                if (hRawInput == (IntPtr)MAGIC_RAW_HANDLE && uiCommand == RID_INPUT)
                {
                    if (_capturedDevice == IntPtr.Zero || _capturedPacketSize == 0) return unchecked((uint)-1);

                    if (pData == IntPtr.Zero)
                    {
                        pcbSize = _capturedPacketSize;
                        return 0; 
                    }

                    if (pcbSize < _capturedPacketSize)
                    {
                        pcbSize = _capturedPacketSize;
                        return unchecked((uint)-1); 
                    }

                    uint buttonFlag = (_rawState == ForgeState.ButtonDown) ? RI_MOUSE_LEFT_BUTTON_DOWN : RI_MOUSE_LEFT_BUTTON_UP;

                    if (EnsureNativePrepared(buttonFlag))
                    {
                        lock (_nativeBufLock)
                        {
                            CopyMemory(pData, _nativeFakePacketPtr, (UIntPtr)_capturedPacketSize);
                            _lastInjectedRawState = _rawState; 
                            return _capturedPacketSize;
                        }
                    }
                    return unchecked((uint)-1);
                }

                // Let legitimate hardware input pass through untouched to capture metadata
                uint result = _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);

                if (pData != IntPtr.Zero && result > 0 && result != unchecked((uint)-1) && uiCommand == RID_INPUT)
                {
                    if (_rawInputHeaderSize == 0 && cbSizeHeader != 0) _rawInputHeaderSize = cbSizeHeader;

                    RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>(pData);
                    if (header.dwType == RIM_TYPEMOUSE && header.hDevice != IntPtr.Zero)
                    {
                        lock (_deviceLock)
                        {
                            if (_capturedDevice == IntPtr.Zero) _capturedDevice = header.hDevice;
                            if (_capturedPacketSize == 0 && header.dwSize != 0) _capturedPacketSize = header.dwSize;
                            
                            // Dump all RAWINPUTHEADER and RAWMOUSE fields for the captured packet
                            string dump;
                            try {
                                long headerSize = (_rawInputHeaderSize != 0) ? _rawInputHeaderSize : 24;
                                IntPtr pMouse = new IntPtr(pData.ToInt64() + headerSize);
                                RAWMOUSE mouse = Marshal.PtrToStructure<RAWMOUSE>(pMouse);
                                dump =
                                    $"[RAWINPUTHEADER] dwType={header.dwType} dwSize={header.dwSize} hDevice=0x{header.hDevice.ToInt64():X} wParam=0x{header.wParam.ToInt64():X}\n" +
                                    $"[RAWMOUSE] usFlags=0x{mouse.usFlags:X} ulButtons=0x{mouse.ulButtons:X} usButtonFlags=0x{mouse.usButtonFlags:X} usButtonData=0x{mouse.usButtonData:X} ulRawButtons=0x{mouse.ulRawButtons:X} lLastX={mouse.lLastX} lLastY={mouse.lLastY} ulExtraInformation=0x{mouse.ulExtraInformation:X}";
                            } catch (Exception ex) {
                                dump = $"[RAW PACKET DUMP ERROR] {ex}";
                            }
                            // Commenting out logging spam, uncomment if needed
                            // _server.ReportMessage(dump); 
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in HookedGetRawInputData]\n{ex}");
                // Fallback to calling original
                return _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
            }
        }

        private uint HookedGetRawInputBuffer(IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            _allHooksInstalled.Wait();

            try
            {
                uint result = _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);

                if (_rawInputHeaderSize == 0 && cbSizeHeader != 0) _rawInputHeaderSize = cbSizeHeader;

                // Only inject our fake buffer if the real buffer is empty
                if (result == 0 && _capturedDevice != IntPtr.Zero && pData != IntPtr.Zero)
                {
                    if (_rawState == ForgeState.ButtonDown || _rawState == ForgeState.ButtonUp)
                    {
                        if (_lastInjectedRawState == _rawState) return result;

                        uint buttonFlag = (_rawState == ForgeState.ButtonDown) ? RI_MOUSE_LEFT_BUTTON_DOWN : RI_MOUSE_LEFT_BUTTON_UP;

                        if (EnsureNativePrepared(buttonFlag))
                        {
                            lock (_nativeBufLock)
                            {
                                if (pcbSize >= (uint)_nativeFakePacketSize)
                                {
                                    CopyMemory(pData, _nativeFakePacketPtr, (UIntPtr)_nativeFakePacketSize);
                                    
                                    string dump;
                                    try {
                                        long headerSize = (_rawInputHeaderSize != 0) ? _rawInputHeaderSize : 24;
                                        IntPtr pMouse = new IntPtr(pData.ToInt64() + headerSize);
                                        RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>(pData);
                                        RAWMOUSE mouse = Marshal.PtrToStructure<RAWMOUSE>(pMouse);
                                        dump =
                                            $"[SYNTHETIC RAWINPUTHEADER] dwType={header.dwType} dwSize={header.dwSize} hDevice=0x{header.hDevice.ToInt64():X} wParam=0x{header.wParam.ToInt64():X}\n" +
                                            $"[SYNTHETIC RAWMOUSE] usFlags=0x{mouse.usFlags:X} ulButtons=0x{mouse.ulButtons:X} usButtonFlags=0x{mouse.usButtonFlags:X} usButtonData=0x{mouse.usButtonData:X} ulRawButtons=0x{mouse.ulRawButtons:X} lLastX={mouse.lLastX} lLastY={mouse.lLastY} ulExtraInformation=0x{mouse.ulExtraInformation:X}";
                                    } catch (Exception ex) {
                                        dump = $"[SYNTHETIC RAW PACKET DUMP ERROR] {ex}";
                                    }
                                    
                                    _server.ReportMessage(dump);
                                    _lastInjectedRawState = _rawState;
                                    return 1;
                                }
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in HookedGetRawInputBuffer]\n{ex}");
                return _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);
            }
        }

        // =========================================================
        // NATIVE STRUCT PACKING AND MEMORY MANAGEMENT
        // =========================================================

        private bool EnsureNativePrepared(uint buttonFlag)
        {
            try
            {
                lock (_deviceLock)
                {
                    if (_capturedDevice == IntPtr.Zero || _capturedPacketSize == 0 || _rawInputHeaderSize == 0) return false;
                }

                lock (_nativeBufLock)
                {
                    if (_nativeFakePacketPtr != IntPtr.Zero && _nativeFakePacketSize != _capturedPacketSize)
                    {
                        Marshal.FreeHGlobal(_nativeFakePacketPtr);
                        _nativeFakePacketPtr = IntPtr.Zero;
                    }

                    if (_nativeFakePacketPtr == IntPtr.Zero)
                    {
                        int alignedSize = (int)((_capturedPacketSize + 7) & ~7);
                        _nativeFakePacketPtr = Marshal.AllocHGlobal(alignedSize);
                        _nativeFakePacketSize = (int)_capturedPacketSize;
                        
                        byte[] zeros = new byte[alignedSize];
                        Marshal.Copy(zeros, 0, _nativeFakePacketPtr, alignedSize);
                    }

                    RAWINPUTHEADER header = new RAWINPUTHEADER
                    {
                        dwType = RIM_TYPEMOUSE,
                        dwSize = _capturedPacketSize,
                        hDevice = _capturedDevice,
                        wParam = IntPtr.Zero
                    };

                    // Note: lLastX and lLastY are hardcoded to 0 for a relative click at the current position
                    RAWMOUSE mouse = new RAWMOUSE
                    {
                        usFlags = 0, 
                        ulButtons = buttonFlag,
                        ulRawButtons = 0,
                        lLastX = 0,
                        lLastY = 0,
                        ulExtraInformation = 0
                    };

                    Marshal.StructureToPtr(header, _nativeFakePacketPtr, false);
                    IntPtr pMouse = new IntPtr(_nativeFakePacketPtr.ToInt64() + _rawInputHeaderSize);
                    Marshal.StructureToPtr(mouse, pMouse, false);

                    return true;
                }
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in EnsureNativePrepared]\n{ex}");
                return false;
            }
        }

        private void FreeNativeFakePacket()
        {
            try
            {
                lock (_nativeBufLock)
                {
                    if (_nativeFakePacketPtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(_nativeFakePacketPtr);
                        _nativeFakePacketPtr = IntPtr.Zero;
                        _nativeFakePacketSize = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in FreeNativeFakePacket]\n{ex}");
            }
        }

        private void CleanupFakePacketResources()
        {
            _stopAutomationThread = true;
            FreeNativeFakePacket();
        }

        private short HookedGetAsyncKeyState(int vKey)
        {
            _allHooksInstalled.Wait();
            try
            {
                short realState = _originalGetAsyncKeyState(vKey);

                // If the game is checking the Left Mouse Button, and our bot is currently in the "Down" state...
                if (vKey == VK_LBUTTON && _rawState == ForgeState.ButtonDown)
                {
                    // Force the "Key Down" bit (0x8000) to be true, regardless of physical mouse state
                    return (short)(realState | unchecked((short)0x8000)); 
                }

                return realState;
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in HookedGetAsyncKeyState]\n{ex}");
                return _originalGetAsyncKeyState(vKey);
            }
        }

        private short HookedGetKeyState(int nVirtKey)
        {
            _allHooksInstalled.Wait();
            try
            {
                short realState = _originalGetKeyState(nVirtKey);

                if (nVirtKey == VK_LBUTTON && _rawState == ForgeState.ButtonDown)
                {
                    return (short)(realState | unchecked((short)0x8000));
                }

                return realState;
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in HookedGetKeyState]\n{ex}");
                return _originalGetKeyState(nVirtKey);
            }
        }
    }
}