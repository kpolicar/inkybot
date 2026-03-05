using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using static InkybotHook.NativeMethods;

namespace InkybotHook
{
    public partial class InjectionEntryPoint
    {
        // Synchronization event to ensure all hooks are installed before any hook logic runs
        private readonly System.Threading.ManualResetEventSlim _allHooksInstalled = new System.Threading.ManualResetEventSlim(false);
        // =========================================================
        // 1. DELEGATES
        // =========================================================
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GetCursorPosDelegate(out POINT lpPoint);
        private GetCursorPosDelegate _originalGetCursorPos;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GetCursorInfoDelegate(ref CURSORINFO pci);
        private GetCursorInfoDelegate _originalGetCursorInfo;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GetPointerInfoDelegate(uint pointerId, ref POINTER_INFO pointerInfo);
        private GetPointerInfoDelegate _originalGetPointerInfo;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool IsIconicDelegate(IntPtr hWnd);
        private IsIconicDelegate _originalIsIconic;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate bool PeekMessageWDelegate(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        private PeekMessageWDelegate _originalPeekMessageW;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate int GetMessageWDelegate(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        private GetMessageWDelegate _originalGetMessageW;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GetRawInputDataDelegate(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);
        private GetRawInputDataDelegate _originalGetRawInputData;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GetRawInputBufferDelegate(IntPtr pData, ref uint pcbSize, uint cbSizeHeader);
        private GetRawInputBufferDelegate _originalGetRawInputBuffer;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate short GetAsyncKeyStateDelegate(int vKey);
        private GetAsyncKeyStateDelegate _originalGetAsyncKeyState;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate short GetKeyStateDelegate(int nVirtKey);
        private GetKeyStateDelegate _originalGetKeyState;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool ScreenToClientDelegate(IntPtr hWnd, ref POINT lpPoint);
        private ScreenToClientDelegate _originalScreenToClient;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool ClientToScreenDelegate(IntPtr hWnd, ref POINT lpPoint);
        private ClientToScreenDelegate _originalClientToScreen;

        // =========================================================
        // 2. CONSTANTS & STRUCTS
        // =========================================================
        private const uint WM_NULL = 0x0000;
        private const uint WM_INPUT = 0x00FF;
        
        private const uint WM_MOUSEFIRST = 0x0200;
        private const uint WM_MOUSELAST = 0x020E;
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_MOUSEMOVE = 0x0200;
        private const uint WM_LBUTTONUP = 0x0202;

        private const uint WM_NCMOUSEFIRST = 0x00A0;
        private const uint WM_NCMOUSELAST = 0x00AD;

        private const uint WM_POINTERFIRST = 0x0240;
        private const uint WM_POINTERLAST = 0x0257;
        
        private const uint PM_REMOVE = 0x0001;

        private const uint RID_INPUT = 0x10000003;
        private const uint RIM_TYPEMOUSE = 0;
        private const int VK_LBUTTON = 0x01;
        private const int VK_RBUTTON = 0x02;

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
        public struct CURSORINFO
        {
            public uint cbSize;
            public uint flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTER_INFO
        {
            public uint pointerType;
            public uint pointerId;
            public uint frameId;
            public uint pointerFlags;
            public IntPtr sourceDevice;
            public IntPtr hwndTarget;
            public POINT ptPixelLocation;
            public POINT ptHimetricLocation;
            public POINT ptPixelLocationRaw;
            public POINT ptHimetricLocationRaw;
            public uint dwTime;
            public uint historyCount;
            public int InputData;
            public uint dwKeyStates;
            public ulong PerformanceCount;
            public uint ButtonChangeType;
        }

        // --- WIN32 RAW INPUT STRUCTS ---
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
            [FieldOffset(0)]
            public ushort usFlags;

            // Union for ulButtons / usButtonFlags & usButtonData
            [FieldOffset(4)]
            public uint ulButtons;

            [FieldOffset(4)]
            public ushort usButtonFlags;

            [FieldOffset(6)]
            public ushort usButtonData;

            [FieldOffset(8)]
            public uint ulRawButtons;

            [FieldOffset(12)]
            public int lLastX;

            [FieldOffset(16)]
            public int lLastY;

            [FieldOffset(20)]
            public uint ulExtraInformation;
        }

        // --- AUTOMATION STATE VARIABLES ---
        private enum ForgeState { Idle, Hover, ButtonDown, ButtonUp } 
        private volatile ForgeState _rawState = ForgeState.Idle;
        private volatile ForgeState _msgState = ForgeState.Idle;
        private volatile ForgeState _lastInjectedRawState = ForgeState.Idle;
        private int _lastClickTime = 0;
        private IntPtr _mainHwnd = IntPtr.Zero; // Tracks the game's window handle

        // captured raw input metadata (set from first real packet observed)
        private IntPtr _capturedDevice = IntPtr.Zero;      // first raw input device id we observed
        private uint _capturedPacketSize = 0;         // header.dwSize from the first observed packet
        private uint _rawInputHeaderSize = 0;         // cbSizeHeader (typically 24 on x64)
        private readonly object _deviceLock = new object();

        // native buffer for prepared fake packet (unmanaged)
        private IntPtr _nativeFakePacketPtr = IntPtr.Zero;
        private int _nativeFakePacketSize = 0; // exact size in bytes
        private readonly object _nativeBufLock = new object();
        private const int MAGIC_RAW_HANDLE = 0x1337;

        // Background thread to wake up the game
        private Thread _automationThread;
        private volatile bool _stopAutomationThread = false;

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, UIntPtr size);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        // =========================================================
        // 4. INSTALLATION
        // =========================================================
        private List<EasyHook.LocalHook> InstallMousePositionHooks()
        {
            var hooks = new List<EasyHook.LocalHook>();

            var getCursorPosHook = TryInstallHook<GetCursorPosDelegate>("GetCursorPos", new GetCursorPosDelegate(HookedGetCursorPos), out _originalGetCursorPos);
            if (getCursorPosHook != null) hooks.Add(getCursorPosHook);

            var getCursorInfoHook = TryInstallHook<GetCursorInfoDelegate>("GetCursorInfo", new GetCursorInfoDelegate(HookedGetCursorInfo), out _originalGetCursorInfo);
            if (getCursorInfoHook != null) hooks.Add(getCursorInfoHook);

            var getPointerInfoHook = TryInstallHook<GetPointerInfoDelegate>("GetPointerInfo", new GetPointerInfoDelegate(HookedGetPointerInfo), out _originalGetPointerInfo);
            if (getPointerInfoHook != null) hooks.Add(getPointerInfoHook);

            var peekMessageWHook = TryInstallHook<PeekMessageWDelegate>("PeekMessageW", new PeekMessageWDelegate(HookedPeekMessageW), out _originalPeekMessageW);
            if (peekMessageWHook != null) hooks.Add(peekMessageWHook);

            var getMessageWHook = TryInstallHook<GetMessageWDelegate>("GetMessageW", new GetMessageWDelegate(HookedGetMessageW), out _originalGetMessageW);
            if (getMessageWHook != null) hooks.Add(getMessageWHook);

            var getRawInputDataHook = TryInstallHook<GetRawInputDataDelegate>("GetRawInputData", new GetRawInputDataDelegate(HookedGetRawInputData), out _originalGetRawInputData);
            if (getRawInputDataHook != null) hooks.Add(getRawInputDataHook);

            var getRawInputBufferHook = TryInstallHook<GetRawInputBufferDelegate>("GetRawInputBuffer", new GetRawInputBufferDelegate(HookedGetRawInputBuffer), out _originalGetRawInputBuffer);
            if (getRawInputBufferHook != null) hooks.Add(getRawInputBufferHook);

            var getAsyncKeyStateHook = TryInstallHook<GetAsyncKeyStateDelegate>("GetAsyncKeyState", new GetAsyncKeyStateDelegate(HookedGetAsyncKeyState), out _originalGetAsyncKeyState);
            if (getAsyncKeyStateHook != null) hooks.Add(getAsyncKeyStateHook);

            var getKeyStateHook = TryInstallHook<GetKeyStateDelegate>("GetKeyState", new GetKeyStateDelegate(HookedGetKeyState), out _originalGetKeyState);
            if (getKeyStateHook != null) hooks.Add(getKeyStateHook);

            var isIconicHook = TryInstallHook<IsIconicDelegate>("IsIconic", new IsIconicDelegate(HookedIsIconic), out _originalIsIconic);
            if (isIconicHook != null) hooks.Add(isIconicHook);

            var screenToClientHook = TryInstallHook<ScreenToClientDelegate>("ScreenToClient", new ScreenToClientDelegate(HookedScreenToClient), out _originalScreenToClient);
            if (screenToClientHook != null) hooks.Add(screenToClientHook);

            var clientToScreenHook = TryInstallHook<ClientToScreenDelegate>("ClientToScreen", new ClientToScreenDelegate(HookedClientToScreen), out _originalClientToScreen);
            if (clientToScreenHook != null) hooks.Add(clientToScreenHook);

            // Start the background automation thread
            _stopAutomationThread = false;
            _automationThread = new Thread(AutomationThreadLoop) { IsBackground = true, Name = "Inkybot_AutomationThread" };
            _automationThread.Start();

            // Signal that all hooks are now installed
            _allHooksInstalled.Set();

            return hooks;
        }

        // =========================================================
        // 5. TIMER AUTOMATION (Now running in background)
        // =========================================================
        private void AutomationThreadLoop()
        {
            while (!_stopAutomationThread)
            {
                try
                {
                    if (IsCursorOverrideActive)
                    {
                        // 1. Manage state via timer (you can trigger this from IPC instead if you prefer)
                        int now = Environment.TickCount;
                        if (now - _lastClickTime >= 1000 && _rawState == ForgeState.Idle && _msgState == ForgeState.Idle)
                        {
                            _rawState = ForgeState.ButtonDown;
                            _msgState = ForgeState.Hover;
                            _lastClickTime = now;
                        }

                        // 2. Wake up the main thread if we have work to do
                        bool pendingRaw = (_rawState == ForgeState.ButtonDown && _lastInjectedRawState != ForgeState.ButtonDown) ||
                                          (_rawState == ForgeState.ButtonUp && _lastInjectedRawState != ForgeState.ButtonUp);
                        
                        bool pendingMsg = _msgState != ForgeState.Idle;

                        if ((pendingRaw || pendingMsg) && _mainHwnd != IntPtr.Zero)
                        {
                            // Spam a harmless message to force the game out of WaitMessage()
                            PostMessage(_mainHwnd, WM_NULL, IntPtr.Zero, IntPtr.Zero);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _server.ReportMessage($"[EXCEPTION in AutomationThreadLoop]\n{ex}");
                }

                // Sleep for ~1 frame (16ms) to avoid destroying CPU performance
                Thread.Sleep(16);
            }
        }

        // =========================================================
        // 6. IMPLEMENTATIONS
        // =========================================================

        private bool HookedGetCursorPos(out POINT lpPoint)
        {
            // Wait for all hooks to be installed
            _allHooksInstalled.Wait();
            try
            {
                if (IsCursorOverrideActive)
                {
                    lpPoint = GetFixedScreenPoint();
                    return true;
                }
                return _originalGetCursorPos(out lpPoint);
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in HookedGetCursorPos]\n{ex}");
                return _originalGetCursorPos(out lpPoint);
            }
        }

        private bool HookedGetCursorInfo(ref CURSORINFO pci)
        {
            _allHooksInstalled.Wait();
            bool calledOriginal = false;
            bool result = false;
            try
            {
                result = _originalGetCursorInfo(ref pci);
                calledOriginal = true;

                if (IsCursorOverrideActive && result)
                {
                    pci.ptScreenPos = GetFixedScreenPoint();
                }
                return result;
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in HookedGetCursorInfo]\n{ex}");
                if (!calledOriginal) return _originalGetCursorInfo(ref pci);
                return result;
            }
        }

        private bool HookedGetPointerInfo(uint pointerId, ref POINTER_INFO pointerInfo)
        {
            _allHooksInstalled.Wait();
            bool calledOriginal = false;
            bool result = false;
            try
            {
                result = _originalGetPointerInfo(pointerId, ref pointerInfo);
                calledOriginal = true;

                if (IsCursorOverrideActive && result)
                {
                    pointerInfo.ptPixelLocation = GetFixedScreenPoint();
                    pointerInfo.ptPixelLocationRaw = GetFixedScreenPoint();
                    return true;
                }
                return result;
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in HookedGetPointerInfo]\n{ex}");
                if (!calledOriginal) return _originalGetPointerInfo(pointerId, ref pointerInfo);
                return result;
            }
        }

        private bool HookedIsIconic(IntPtr hWnd) 
        {
            _allHooksInstalled.Wait();
            try { return false; }
            catch { return false; }
        }

        // --- THE MESSAGE PUMP HOOKS (BACKGROUND UI DISPATCHER) ---
        private bool HookedPeekMessageW(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            _allHooksInstalled.Wait();
            string step = "Init";
            bool calledOriginal = false;
            bool result = false;

            try
            {
                step = "Calling _originalPeekMessageW";
                result = _originalPeekMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
                calledOriginal = true;

                if (IsCursorOverrideActive)
                {
                    step = "Capturing main hWnd";
                    if (_mainHwnd == IntPtr.Zero && lpMsg.hwnd != IntPtr.Zero) 
                    {
                        _mainHwnd = lpMsg.hwnd;
                    }

                    step = "Filtering mouse message";
                    if (result) FilterMouseMessage(ref lpMsg);

                    // Check injection state
                    bool pendingRaw = (_rawState == ForgeState.ButtonDown && _lastInjectedRawState != ForgeState.ButtonDown) ||
                                      (_rawState == ForgeState.ButtonUp && _lastInjectedRawState != ForgeState.ButtonUp);
                    
                    bool pendingMsg = _msgState != ForgeState.Idle;

                    step = "Injecting automated message state";
                    if ((pendingRaw || pendingMsg) && (!result || lpMsg.message == WM_NULL))
                    {
                        IntPtr targetHwnd = (hWnd != IntPtr.Zero) ? hWnd : _mainHwnd;
                        if (targetHwnd == IntPtr.Zero) return result; 

                        lpMsg.hwnd = targetHwnd; 
                        POINT target = GetFixedScreenPoint();
                        lpMsg.pt = target; 
                        lpMsg.time = (uint)Environment.TickCount;

                        if (pendingRaw)
                        {
                            // Wake up GetRawInputBuffer
                            lpMsg.message = WM_INPUT;
                            lpMsg.wParam = IntPtr.Zero; 
                            lpMsg.lParam = (IntPtr)MAGIC_RAW_HANDLE;
                            return true;
                        }
                        else if (pendingMsg)
                        {
                            // Inject UI events
                            lpMsg.lParam = (IntPtr)((target.Y << 16) | (target.X & 0xFFFF));
                            lpMsg.wParam = (IntPtr)1; 

                            if (_msgState == ForgeState.Hover)
                            {
                                lpMsg.message = 0x0245; // WM_POINTERUPDATE
                                if ((wRemoveMsg & PM_REMOVE) != 0) _msgState = ForgeState.ButtonDown;
                            }
                            else if (_msgState == ForgeState.ButtonDown)
                            {
                                lpMsg.message = 0x0246; // WM_POINTERDOWN
                                if ((wRemoveMsg & PM_REMOVE) != 0) _msgState = ForgeState.ButtonUp;
                            }
                            else if (_msgState == ForgeState.ButtonUp)
                            {
                                lpMsg.message = 0x0247; // WM_POINTERUP
                                if ((wRemoveMsg & PM_REMOVE) != 0) _msgState = ForgeState.Idle;
                            }
                            return true;
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in HookedPeekMessageW | Step: {step}]\n{ex}");
                if (!calledOriginal) return _originalPeekMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
                return result;
            }
        }

        private int HookedGetMessageW(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
        {
            _allHooksInstalled.Wait();
            bool calledOriginal = false;
            int result = 0;
            try
            {
                result = _originalGetMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
                calledOriginal = true;

                if (IsCursorOverrideActive && result > 0)
                {
                    FilterMouseMessage(ref lpMsg);
                }
                return result;
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in HookedGetMessageW]\n{ex}");
                if (!calledOriginal) return _originalGetMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
                return result;
            }
        }

        private void FilterMouseMessage(ref MSG msg)
        {
            try
            {
                bool isStandardMouse = (msg.message >= WM_MOUSEFIRST && msg.message <= WM_MOUSELAST);
                bool isPointerOrTouch = (msg.message >= WM_POINTERFIRST && msg.message <= WM_POINTERLAST);
                bool isInputEvent = (msg.message == WM_INPUT);
                
                // If it is a physical mouse or touch event from the OS, completely shred it.
                // We do not want the physical mouse interfering with our injected bot clicks.
                if (isStandardMouse || isPointerOrTouch || isInputEvent)
                {
                    msg.message = WM_NULL; 
                }
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in FilterMouseMessage]\n{ex}");
            }
        }

        // --- THE RAW INPUT HOOKS (3D WORLD DISPATCHER) ---
        private uint HookedGetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            _allHooksInstalled.Wait();
            string step = "Init";
            bool calledOriginal = false;
            uint result = 0;

            try
            {
                // -------------------------------------------------------------
                // SYNTHETIC INJECTION (THE MAGIC HANDLE BYPASS)
                // -------------------------------------------------------------
                if (hRawInput == (IntPtr)MAGIC_RAW_HANDLE && uiCommand == RID_INPUT)
                {
                    if (_capturedDevice == IntPtr.Zero || _capturedPacketSize == 0) return unchecked((uint)-1);

                    // 1. Unity is asking for the size of the buffer needed
                    if (pData == IntPtr.Zero)
                    {
                        pcbSize = _capturedPacketSize;
                        return 0; // 0 means success for a size query
                    }

                    // 2. Unity's buffer is too small
                    if (pcbSize < _capturedPacketSize)
                    {
                        pcbSize = _capturedPacketSize;
                        return unchecked((uint)-1); 
                    }

                    // 3. Unity is providing the buffer, fill it with our synthetic data!
                    uint buttonFlag = 0;

                    if (_rawState == ForgeState.ButtonDown)
                    {
                        buttonFlag = (uint)RI_MOUSE_LEFT_BUTTON_DOWN;
                        _lastInjectedRawState = ForgeState.ButtonDown;
                        _rawState = ForgeState.ButtonUp; // Advance state machine
                    }
                    else if (_rawState == ForgeState.ButtonUp)
                    {
                        buttonFlag = (uint)RI_MOUSE_LEFT_BUTTON_UP;
                        _lastInjectedRawState = ForgeState.ButtonUp;
                        _rawState = ForgeState.Idle; // Advance state machine
                    }
                    else
                    {
                        return unchecked((uint)-1);
                    }

                    // Pack the unmanaged memory and copy it to Unity's pointer
                    if (EnsureNativePrepared(buttonFlag))
                    {
                        lock (_nativeBufLock)
                        {
                            CopyMemory(pData, _nativeFakePacketPtr, (UIntPtr)_capturedPacketSize);
                            return _capturedPacketSize; // Return bytes copied
                        }
                    }
                    return unchecked((uint)-1);
                }
                // -------------------------------------------------------------

                // If it's not our magic handle, let the OS handle the real hardware input
                step = "Calling _originalGetRawInputData";
                result = _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
                calledOriginal = true;

                if (!IsCursorOverrideActive) return result;
                if (pData == IntPtr.Zero || result == 0 || result == unchecked((uint)-1)) return result;
                if (uiCommand != RID_INPUT) return result;

                step = "Setting Header Size";
                if (_rawInputHeaderSize == 0 && cbSizeHeader != 0) _rawInputHeaderSize = cbSizeHeader;

                step = "Marshaling RAWINPUTHEADER";
                RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>(pData);

                if (header.dwType == RIM_TYPEMOUSE && header.hDevice != IntPtr.Zero)
                {
                    step = "Capturing device metadata";
                    lock (_deviceLock)
                    {
                        if (_capturedDevice == IntPtr.Zero) _capturedDevice = header.hDevice;
                        if (_capturedPacketSize == 0 && header.dwSize != 0) _capturedPacketSize = header.dwSize;
                    }
                }

                if (header.dwType == RIM_TYPEMOUSE)
                {
                    step = "Scrubbing RAWMOUSE struct";
                    if (header.dwSize != 0 && header.dwSize <= pcbSize)
                    {
                        long headerSize = (_rawInputHeaderSize != 0) ? _rawInputHeaderSize : 24;
                        IntPtr pMouse = new IntPtr(pData.ToInt64() + headerSize);

                        RAWMOUSE mouse = Marshal.PtrToStructure<RAWMOUSE>(pMouse);
                        
                        mouse.usButtonFlags = 0; 
                        mouse.lLastX = 0;
                        mouse.lLastY = 0;

                        Marshal.StructureToPtr(mouse, pMouse, false);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in HookedGetRawInputData | Step: {step}]\n{ex}");
                if (!calledOriginal) return _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
                return result;
            }
        }

        private uint HookedGetRawInputBuffer(IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            _allHooksInstalled.Wait();
            string step = "Init";
            bool calledOriginal = false;
            uint result = 0;

            try
            {
                step = "Calling _originalGetRawInputBuffer";
                result = _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);
                calledOriginal = true;

                if (!IsCursorOverrideActive) return result;
                if (pData == IntPtr.Zero) return result;

                step = "Setting header size";
                if (_rawInputHeaderSize == 0 && cbSizeHeader != 0) _rawInputHeaderSize = cbSizeHeader;

                step = "Scrubbing existing buffer";
                if (result > 0 && result != unchecked((uint)-1))
                {
                    ScrubRawInputBuffer(pData, (int)result);
                    return result;
                }

                step = "Checking for metadata";
                if (_capturedDevice == IntPtr.Zero || _capturedPacketSize == 0 || _rawInputHeaderSize == 0) return result;

                step = "Handling Injection State Machine";
                if (result == 0)
                {
                    if (_rawState == ForgeState.ButtonDown)
                    {
                        step = "Injecting ButtonDown - Prepare";
                        if (_lastInjectedRawState != ForgeState.ButtonDown)
                        {
                            if (!EnsureNativePrepared((uint)RI_MOUSE_LEFT_BUTTON_DOWN)) return result;

                            step = "Injecting ButtonDown - Copy Memory";
                            lock (_nativeBufLock)
                            {
                                if (_nativeFakePacketPtr == IntPtr.Zero || _nativeFakePacketSize == 0) return result;
                                if (pcbSize < (uint)_nativeFakePacketSize) return result;

                                CopyMemory(pData, _nativeFakePacketPtr, (UIntPtr)_nativeFakePacketSize);
                                _lastInjectedRawState = ForgeState.ButtonDown;
                                return 1;
                            }
                        }
                        else
                        {
                            _rawState = ForgeState.ButtonUp;
                        }
                    }
                    else if (_rawState == ForgeState.ButtonUp)
                    {
                        step = "Injecting ButtonUp - Prepare";
                        if (_lastInjectedRawState != ForgeState.ButtonUp)
                        {
                            if (!EnsureNativePrepared((uint)RI_MOUSE_LEFT_BUTTON_UP)) return result;

                            step = "Injecting ButtonUp - Copy Memory";
                            lock (_nativeBufLock)
                            {
                                if (_nativeFakePacketPtr == IntPtr.Zero || _nativeFakePacketSize == 0) return result;
                                if (pcbSize < (uint)_nativeFakePacketSize) return result;

                                CopyMemory(pData, _nativeFakePacketPtr, (UIntPtr)_nativeFakePacketSize);
                                _lastInjectedRawState = ForgeState.ButtonUp;
                                return 1;
                            }
                        }
                        else
                        {
                            _rawState = ForgeState.Idle;
                            _lastInjectedRawState = ForgeState.Idle;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in HookedGetRawInputBuffer | Step: {step}]\n{ex}");
                if (!calledOriginal) return _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);
                return result;
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
                    if (_capturedDevice == IntPtr.Zero || _capturedPacketSize == 0 || _rawInputHeaderSize == 0)
                        return false;
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

                    RAWMOUSE mouse = new RAWMOUSE
                    {
                        usFlags = 0, 
                        ulButtons = buttonFlag,
                        ulRawButtons = 0,
                        lLastX = 0,
                        lLastY = 0,
                        ulExtraInformation = 0
                    };

                    // Write the structs to our unmanaged memory buffer
                    Marshal.StructureToPtr(header, _nativeFakePacketPtr, false);

                    IntPtr pMouse = new IntPtr(_nativeFakePacketPtr.ToInt64() + _rawInputHeaderSize);
                    Marshal.StructureToPtr(mouse, pMouse, false);

                    // ========================================================
                    // DEBUG: READ AND DISPLAY THE RAW MEMORY BYTES
                    // ========================================================
                    byte[] debugBytes = new byte[_capturedPacketSize];
                    Marshal.Copy(_nativeFakePacketPtr, debugBytes, 0, (int)_capturedPacketSize);

                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.AppendLine($"Button Flag: {buttonFlag}");
                    sb.AppendLine($"Packet Size: {_capturedPacketSize} bytes");
                    sb.AppendLine($"Header Size: {_rawInputHeaderSize} bytes\n");
                    
                    for (int i = 0; i < debugBytes.Length; i++)
                    {
                        sb.Append($"{debugBytes[i]:X2} ");
                        // Add a newline every 16 bytes for standard hex dump formatting
                        if ((i + 1) % 16 == 0) sb.AppendLine();
                    }

                    // Pop the Win32 message box. 
                    MessageBox(IntPtr.Zero, sb.ToString(), "Injected Fake RAWINPUT Packet", 0);
                    // ========================================================

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

        private void ScrubRawInputBuffer(IntPtr pData, int packetCount)
        {
            try
            {
                IntPtr currentPtr = pData;
                for (int i = 0; i < packetCount; i++)
                {
                    RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>(currentPtr);

                    if (header.dwType == RIM_TYPEMOUSE)
                    {
                        try
                        {
                            long headerSize = (_rawInputHeaderSize != 0) ? _rawInputHeaderSize : 24;
                            IntPtr pMouse = new IntPtr(currentPtr.ToInt64() + headerSize);

                            RAWMOUSE mouse = Marshal.PtrToStructure<RAWMOUSE>(pMouse);
                            
                            mouse.usButtonFlags = 0; 
                            mouse.lLastX = 0;        
                            mouse.lLastY = 0;        

                            Marshal.StructureToPtr(mouse, pMouse, false); 
                        }
                        catch (Exception innerEx)
                        { 
                            _server.ReportMessage($"[EXCEPTION in ScrubRawInputBuffer per-packet scrub]\n{innerEx}");
                        }
                    }

                    if (header.dwSize == 0) break;

                    long nextPtr = currentPtr.ToInt64() + header.dwSize;
                    nextPtr = (nextPtr + 7) & ~7L; 
                    currentPtr = new IntPtr(nextPtr);
                }
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in ScrubRawInputBuffer loop]\n{ex}");
            }
        }

        // --- THE HARDWARE STATE HOOKS (MODIFIER BYPASS) ---
        private short HookedGetAsyncKeyState(int vKey)
        {
            _allHooksInstalled.Wait();
            try
            {
                if (IsCursorOverrideActive)
                {
                    if (vKey == 16 || vKey == 17 || vKey == 18 || vKey == 91 || vKey == 92 || vKey == 20) return 0;

                    if (vKey == VK_LBUTTON || vKey == VK_RBUTTON)
                    {
                        if (vKey == VK_LBUTTON && (_rawState == ForgeState.ButtonDown || _rawState == ForgeState.ButtonUp))
                            return unchecked((short)0x8000); 
                        return 0;
                    }
                }
                return _originalGetAsyncKeyState(vKey);
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
                if (IsCursorOverrideActive)
                {
                    if (nVirtKey == 16 || nVirtKey == 17 || nVirtKey == 18 || nVirtKey == 91 || nVirtKey == 92 || nVirtKey == 20) return 0;

                    if (nVirtKey == VK_LBUTTON || nVirtKey == VK_RBUTTON)
                    {
                        if (nVirtKey == VK_LBUTTON && (_rawState == ForgeState.ButtonDown || _rawState == ForgeState.ButtonUp))
                            return unchecked((short)0x8000);
                        return 0;
                    }
                }
                return _originalGetKeyState(nVirtKey);
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in HookedGetKeyState]\n{ex}");
                return _originalGetKeyState(nVirtKey);
            }
        }

        private bool HookedScreenToClient(IntPtr hWnd, ref POINT lpPoint)
        {
            _allHooksInstalled.Wait();
            try
            {
                if (IsCursorOverrideActive)
                {
                    lpPoint = GetFixedScreenPoint();
                }
                return _originalScreenToClient(hWnd, ref lpPoint);
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in HookedScreenToClient]\n{ex}");
                return _originalScreenToClient(hWnd, ref lpPoint);
            }
        }

        private bool HookedClientToScreen(IntPtr hWnd, ref POINT lpPoint)
        {
            _allHooksInstalled.Wait();
            bool calledOriginal = false;
            bool result = false;
            try
            {
                result = _originalClientToScreen(hWnd, ref lpPoint);
                calledOriginal = true;
                
                if (IsCursorOverrideActive && result)
                {
                    lpPoint = GetFixedScreenPoint();
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in HookedClientToScreen]\n{ex}");
                if (!calledOriginal) return _originalClientToScreen(hWnd, ref lpPoint);
                return result;
            }
        }

        // Call this on unload/uninject to free native buffer
        private void CleanupFakePacketResources()
        {
            _stopAutomationThread = true;
            FreeNativeFakePacket();
        }
    }
}