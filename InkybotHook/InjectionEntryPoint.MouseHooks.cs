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
        // 1. DELEGATES 
        // =========================================================
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate bool PeekMessageWDelegate(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        private PeekMessageWDelegate _originalPeekMessageW;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool TranslateMessageDelegate(ref MSG lpMsg);
        private TranslateMessageDelegate _originalTranslateMessage;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate IntPtr DispatchMessageWDelegate(ref MSG lpMsg);
        private DispatchMessageWDelegate _originalDispatchMessageW;

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
        private delegate IntPtr SetCaptureDelegate(IntPtr hWnd);
        private SetCaptureDelegate _originalSetCapture;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool ReleaseCaptureDelegate();
        private ReleaseCaptureDelegate _originalReleaseCapture;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr GetCaptureDelegate();
        private GetCaptureDelegate _originalGetCapture;

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        // =========================================================
        // 2. STATE & DICTIONARIES
        // =========================================================
        private readonly Dictionary<IntPtr, IntPtr> _originalWndProcs = new Dictionary<IntPtr, IntPtr>();
        private readonly Dictionary<IntPtr, WndProcDelegate> _wndProcDelegates = new Dictionary<IntPtr, WndProcDelegate>();
        private readonly object _wndProcLock = new object();

        private enum ForgeState { Idle, ButtonDown, ButtonUp } 
        private volatile ForgeState _rawState = ForgeState.Idle;
        private volatile ForgeState _lastInjectedRawState = ForgeState.Idle;
        
        private int _lastStateChangeTime = 0;
        private IntPtr _mainHwnd = IntPtr.Zero; 

        private uint _capturedPointerId = 0;
        private IntPtr _capturedDevice = IntPtr.Zero;      
        private uint _capturedPacketSize = 0;        
        private uint _rawInputHeaderSize = 0;        
        private readonly object _deviceLock = new object();

        private IntPtr _nativeFakePacketPtr = IntPtr.Zero;
        private int _nativeFakePacketSize = 0; 
        private readonly object _nativeBufLock = new object();

        private Queue<MSG> _syntheticMessages = new Queue<MSG>();
        private readonly object _queueLock = new object();

        private volatile int _targetScreenX;
        private volatile int _targetScreenY;

        private Thread _clickThread;
        private volatile bool _stopClickThread = false;

        // =========================================================
        // 3. CONSTANTS & NATIVE IMPORTS
        // =========================================================
        private const int MAGIC_RAW_HANDLE = 0x1337;
        private const int MAGIC_RAW_MOVE_HANDLE = 0x1338;
        private const ushort MOUSE_MOVE_ABSOLUTE = 0x0001;
        private const ushort MOUSE_VIRTUAL_DESKTOP = 0x0002;
        private const uint WM_NULL = 0x0000;
        private const uint WM_INPUT = 0x00FF;
        private const uint WM_MOUSEMOVE = 0x0200;
        private const uint WM_POINTERUPDATE = 0x0245;
        private const uint WM_POINTERDOWN = 0x0246;
        private const uint WM_POINTERUP = 0x0247;
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;
        private const int MK_LBUTTON = 0x0001;
        private const int GWLP_WNDPROC = -4;

        private const uint RID_INPUT = 0x10000003;
        private const uint RIM_TYPEMOUSE = 0;
        private const uint RI_MOUSE_LEFT_BUTTON_DOWN = 0x0001;
        private const uint RI_MOUSE_LEFT_BUTTON_UP = 0x0002;
        private const int VK_LBUTTON = 0x01;

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, UIntPtr size);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8) return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        
        // =========================================================
        // 4. INSTALLATION & AUTOMATION LOOP
        // =========================================================
        private List<EasyHook.LocalHook> InstallHooks()
        {
            var hooks = new List<EasyHook.LocalHook>();

            hooks.Add(TryInstallHook<PeekMessageWDelegate>("PeekMessageW", new PeekMessageWDelegate(HookedPeekMessageW), out _originalPeekMessageW));
            hooks.Add(TryInstallHook<TranslateMessageDelegate>("TranslateMessage", new TranslateMessageDelegate(HookedTranslateMessage), out _originalTranslateMessage));
            hooks.Add(TryInstallHook<DispatchMessageWDelegate>("DispatchMessageW", new DispatchMessageWDelegate(HookedDispatchMessageW), out _originalDispatchMessageW));
            hooks.Add(TryInstallHook<GetRawInputDataDelegate>("GetRawInputData", new GetRawInputDataDelegate(HookedGetRawInputData), out _originalGetRawInputData));
            hooks.Add(TryInstallHook<GetRawInputBufferDelegate>("GetRawInputBuffer", new GetRawInputBufferDelegate(HookedGetRawInputBuffer), out _originalGetRawInputBuffer));
            hooks.Add(TryInstallHook<GetAsyncKeyStateDelegate>("GetAsyncKeyState", new GetAsyncKeyStateDelegate(HookedGetAsyncKeyState), out _originalGetAsyncKeyState));
            hooks.Add(TryInstallHook<GetKeyStateDelegate>("GetKeyState", new GetKeyStateDelegate(HookedGetKeyState), out _originalGetKeyState));
            hooks.Add(TryInstallHook<SetCaptureDelegate>("SetCapture", new SetCaptureDelegate(HookedSetCapture), out _originalSetCapture));
            hooks.Add(TryInstallHook<ReleaseCaptureDelegate>("ReleaseCapture", new ReleaseCaptureDelegate(HookedReleaseCapture), out _originalReleaseCapture));
            hooks.Add(TryInstallHook<GetCaptureDelegate>("GetCapture", new GetCaptureDelegate(HookedGetCapture), out _originalGetCapture));

            hooks.RemoveAll(item => item == null);

            ProbeRawInputDevices();

            _stopClickThread = false;
            _clickThread = new Thread(ClickProcessorLoop) { IsBackground = true, Name = "Inkybot_ClickThread" };
            _clickThread.Start();

            _allHooksInstalled.Set();
            return hooks;
        }

        private void ClickProcessorLoop()
        {
            while (!_stopClickThread)
            {
                try
                {
                    int now = Environment.TickCount;

                    if (_rawState == ForgeState.Idle && _server.clickRequested)
                    {
                        // Consume the click request
                        int screenX = _server.clickScreenX;
                        int screenY = _server.clickScreenY;
                        _server.clickRequested = false;

                        _targetScreenX = screenX;
                        _targetScreenY = screenY;

                        _rawState = ForgeState.ButtonDown;
                        _lastStateChangeTime = now;

                        if (_mainHwnd != IntPtr.Zero)
                        {
                            if (_originalSetCapture != null)
                                _originalSetCapture(_mainHwnd);

                            POINT screenPt = new POINT { X = screenX, Y = screenY };

                            POINT clientPt = screenPt;
                            ScreenToClient(_mainHwnd, ref clientPt);

                            IntPtr clientLParam = (IntPtr)((uint)((clientPt.Y << 16) | (clientPt.X & 0xFFFF)));
                            IntPtr screenLParam = (IntPtr)((uint)((screenPt.Y << 16) | (screenPt.X & 0xFFFF)));

                            lock (_queueLock)
                            {
                                uint activePointerId = _capturedPointerId == 0 ? 1 : _capturedPointerId;
                                IntPtr pointerWParamDown = (IntPtr)((0x0016 << 16) | activePointerId);

                                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_INPUT, wParam = IntPtr.Zero, lParam = (IntPtr)MAGIC_RAW_MOVE_HANDLE, time = (uint)now, pt = screenPt });
                                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_POINTERUPDATE, wParam = pointerWParamDown, lParam = screenLParam, time = (uint)now, pt = screenPt });
                                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_MOUSEMOVE, wParam = IntPtr.Zero, lParam = clientLParam, time = (uint)now, pt = screenPt });
                                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_INPUT, wParam = IntPtr.Zero, lParam = (IntPtr)MAGIC_RAW_HANDLE, time = (uint)now, pt = screenPt });
                                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_POINTERDOWN, wParam = pointerWParamDown, lParam = screenLParam, time = (uint)now, pt = screenPt });
                                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_LBUTTONDOWN, wParam = (IntPtr)MK_LBUTTON, lParam = clientLParam, time = (uint)now, pt = clientPt });
                            }
                            PostMessage(_mainHwnd, WM_NULL, IntPtr.Zero, IntPtr.Zero);
                        }
                    }
                    else if (_rawState == ForgeState.ButtonDown && (now - _lastStateChangeTime >= 50))
                    {
                        _rawState = ForgeState.ButtonUp;
                        _lastStateChangeTime = now;

                        if (_mainHwnd != IntPtr.Zero)
                        {
                            if (_originalReleaseCapture != null)
                                _originalReleaseCapture();

                            POINT screenPt = new POINT { X = _targetScreenX, Y = _targetScreenY };

                            POINT clientPt = screenPt;
                            ScreenToClient(_mainHwnd, ref clientPt);

                            IntPtr clientLParam = (IntPtr)((uint)((clientPt.Y << 16) | (clientPt.X & 0xFFFF)));
                            IntPtr screenLParam = (IntPtr)((uint)((screenPt.Y << 16) | (screenPt.X & 0xFFFF)));

                            lock (_queueLock)
                            {
                                uint activePointerId = _capturedPointerId == 0 ? 1 : _capturedPointerId;
                                IntPtr pointerWParamUp = (IntPtr)((0x0002 << 16) | activePointerId);

                                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_INPUT, wParam = IntPtr.Zero, lParam = (IntPtr)MAGIC_RAW_MOVE_HANDLE, time = (uint)now, pt = screenPt });
                                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_POINTERUPDATE, wParam = pointerWParamUp, lParam = screenLParam, time = (uint)now, pt = screenPt });
                                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_MOUSEMOVE, wParam = IntPtr.Zero, lParam = clientLParam, time = (uint)now, pt = screenPt });
                                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_INPUT, wParam = IntPtr.Zero, lParam = (IntPtr)MAGIC_RAW_HANDLE, time = (uint)now, pt = screenPt });
                                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_POINTERUP, wParam = pointerWParamUp, lParam = screenLParam, time = (uint)now, pt = screenPt });
                                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_LBUTTONUP, wParam = IntPtr.Zero, lParam = clientLParam, time = (uint)now, pt = clientPt });
                            }
                            PostMessage(_mainHwnd, WM_NULL, IntPtr.Zero, IntPtr.Zero);
                        }
                    }
                    else if (_rawState == ForgeState.ButtonUp && (now - _lastStateChangeTime >= 50))
                    {
                        _rawState = ForgeState.Idle;
                        _lastStateChangeTime = now;
                    }
                }
                catch (Exception ex)
                {
                    _server.ReportMessage($"[EXCEPTION in ClickProcessorLoop]\n{ex}");
                }

                Thread.Sleep(1);
            }
        }

        // =========================================================
        // 5. IMPLEMENTATIONS
        // =========================================================
        private bool HookedPeekMessageW(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            _allHooksInstalled.Wait();
            try
            {
                bool result = _originalPeekMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);

                if (lpMsg.hwnd != IntPtr.Zero) 
                {
                    lock (_wndProcLock)
                    {
                        // 1. Subclass every new UnityWndClass window dynamically
                        if (!_originalWndProcs.ContainsKey(lpMsg.hwnd))
                        {
                            System.Text.StringBuilder windowText = new System.Text.StringBuilder(256);
                            System.Text.StringBuilder className = new System.Text.StringBuilder(256);
                            GetWindowText(lpMsg.hwnd, windowText, windowText.Capacity);
                            GetClassName(lpMsg.hwnd, className, className.Capacity);

                            if (className.ToString() == "UnityWndClass")
                            {
                                WndProcDelegate newDelegate = new WndProcDelegate(HookedWndProc);
                                _wndProcDelegates[lpMsg.hwnd] = newDelegate;
                                IntPtr orig = SetWindowLongPtr(lpMsg.hwnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(newDelegate));
                                _originalWndProcs[lpMsg.hwnd] = orig;

                                string wName = string.IsNullOrEmpty(windowText.ToString()) ? "[No Name]" : windowText.ToString();
                                _server.ReportMessage($"[LOG] Subclassed HWND: 0x{lpMsg.hwnd.ToInt64():X} | Name: '{wName}' | Class: '{className}'");
                            }
                        }

                        // 2. Lock onto the window actively receiving hardware inputs
                        if (lpMsg.message == WM_INPUT || lpMsg.message == WM_MOUSEMOVE || (lpMsg.message >= WM_POINTERUPDATE && lpMsg.message <= WM_POINTERUP))
                        {
                            if (_mainHwnd != lpMsg.hwnd)
                            {
                                _mainHwnd = lpMsg.hwnd;
                                _server.ReportMessage($"[LOG] Locked onto active Input HWND: 0x{_mainHwnd.ToInt64():X}");
                            }

                            // --- NEW: HIJACK THE REAL POINTER ID ---
                            if (lpMsg.message >= WM_POINTERUPDATE && lpMsg.message <= WM_POINTERUP)
                            {
                                uint extractedPointerId = (uint)(lpMsg.wParam.ToInt64() & 0xFFFF);
                                if (_capturedPointerId != extractedPointerId)
                                {
                                    _capturedPointerId = extractedPointerId;
                                    _server.ReportMessage($"[LOG] Hijacked real OS Pointer ID: {_capturedPointerId}");
                                }
                            }
                        }
                    }
                }

                // If the game's queue is empty (or it's just a WM_NULL wakeup), feed it our synthetic messages
                lock (_queueLock)
                {
                    if (_syntheticMessages.Count > 0 && (!result || lpMsg.message == WM_NULL) && _mainHwnd != IntPtr.Zero)
                    {
                        if ((wRemoveMsg & 0x0001 /* PM_REMOVE */) != 0)
                        {
                            lpMsg = _syntheticMessages.Dequeue(); // Pop it out
                            _server.ReportMessage($"[LOG] Injected from Queue -> MSG: 0x{lpMsg.message:X}");
                        }
                        else
                        {
                            lpMsg = _syntheticMessages.Peek(); // Just looking, don't pop
                        }
                        return true; // We intercepted and supplied a message!
                    }
                }

                return result;
            }
            catch (Exception) { return false; }
        }

        private bool HookedTranslateMessage(ref MSG lpMsg)
        {
            _allHooksInstalled.Wait();
            if (lpMsg.message == WM_INPUT && (lpMsg.lParam == (IntPtr)MAGIC_RAW_HANDLE || lpMsg.lParam == (IntPtr)MAGIC_RAW_MOVE_HANDLE))
            {
                // Bypass OS translation for fake packets
                return true;
            }
            return _originalTranslateMessage(ref lpMsg);
        }

        private IntPtr HookedDispatchMessageW(ref MSG lpMsg)
        {
            _allHooksInstalled.Wait();
            if (lpMsg.message == WM_INPUT && (lpMsg.lParam == (IntPtr)MAGIC_RAW_HANDLE || lpMsg.lParam == (IntPtr)MAGIC_RAW_MOVE_HANDLE))
            {
                WndProcDelegate targetDelegate = null;
                IntPtr targetOrig = IntPtr.Zero;

                lock (_wndProcLock)
                {
                    _wndProcDelegates.TryGetValue(lpMsg.hwnd, out targetDelegate);
                    _originalWndProcs.TryGetValue(lpMsg.hwnd, out targetOrig);
                }

                // Route to OUR hook delegate first
                if (targetDelegate != null)
                {
                    return targetDelegate(lpMsg.hwnd, lpMsg.message, lpMsg.wParam, lpMsg.lParam);
                }
                else if (targetOrig != IntPtr.Zero)
                {
                    return CallWindowProc(targetOrig, lpMsg.hwnd, lpMsg.message, lpMsg.wParam, lpMsg.lParam);
                }
                return IntPtr.Zero;
            }
            return _originalDispatchMessageW(ref lpMsg);
        }

        private IntPtr HookedWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr originalProc = IntPtr.Zero;
            lock (_wndProcLock)
            {
                _originalWndProcs.TryGetValue(hWnd, out originalProc);
            }

            if (originalProc != IntPtr.Zero)
            {
                return CallWindowProc(originalProc, hWnd, msg, wParam, lParam);
            }
            
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private uint HookedGetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            _allHooksInstalled.Wait();
            try
            {
                if ((hRawInput == (IntPtr)MAGIC_RAW_HANDLE || hRawInput == (IntPtr)MAGIC_RAW_MOVE_HANDLE) && uiCommand == RID_INPUT)
                {
                    if (_rawInputHeaderSize == 0 && cbSizeHeader != 0) _rawInputHeaderSize = cbSizeHeader;
                    
                    // Wait until we have safely captured a genuine device handle
                    if (_capturedDevice == IntPtr.Zero) return unchecked((uint)-1);

                    // Force a standard lean packet size (Header + RAWMOUSE payload)
                    // This prevents Unity's strict stack buffer from rejecting the packet
                    uint standardPacketSize = _rawInputHeaderSize + 24;

                    if (pData == IntPtr.Zero) { pcbSize = standardPacketSize; return 0; }
                    if (pcbSize < standardPacketSize) { pcbSize = standardPacketSize; return unchecked((uint)-1); }

                    // Move handle = absolute position with no button flags, click handle = button down/up
                    uint buttonFlag = 0;
                    ushort mouseFlags = 0;
                    int lastX = 0, lastY = 0;

                    if (hRawInput == (IntPtr)MAGIC_RAW_MOVE_HANDLE)
                    {
                        mouseFlags = MOUSE_MOVE_ABSOLUTE | MOUSE_VIRTUAL_DESKTOP;
                        int cxScreen = GetSystemMetrics(SM_CXSCREEN);
                        int cyScreen = GetSystemMetrics(SM_CYSCREEN);
                        if (cxScreen > 0 && cyScreen > 0)
                        {
                            lastX = (_targetScreenX * 65535) / cxScreen;
                            lastY = (_targetScreenY * 65535) / cyScreen;
                        }
                    }
                    else
                    {
                        buttonFlag = (_rawState == ForgeState.ButtonDown) ? RI_MOUSE_LEFT_BUTTON_DOWN : RI_MOUSE_LEFT_BUTTON_UP;
                    }

                    if (EnsureNativePrepared(buttonFlag, mouseFlags, lastX, lastY))
                    {
                        lock (_nativeBufLock)
                        {
                            // CRITICAL: Copy standardPacketSize, NOT _capturedPacketSize
                            CopyMemory(pData, _nativeFakePacketPtr, (UIntPtr)standardPacketSize);
                            
                            if (hRawInput == (IntPtr)MAGIC_RAW_HANDLE)
                                _lastInjectedRawState = _rawState;
                                
                            return standardPacketSize; // Return the standard size to Unity
                        }
                    }
                    return unchecked((uint)-1);
                }

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
                            // We still log the bloated hardware size for legitimate pass-throughs
                            if (_capturedPacketSize == 0 && header.dwSize != 0) _capturedPacketSize = header.dwSize;
                        }
                    }
                }
                return result;
            }
            catch (Exception) { return _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader); }
        }

        private uint HookedGetRawInputBuffer(IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            _allHooksInstalled.Wait();
            try
            {
                uint result = _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);
                if (_rawInputHeaderSize == 0 && cbSizeHeader != 0) _rawInputHeaderSize = cbSizeHeader;

                // 1. HARDWARE RACE CONDITION FIX (Buffer has physical events)
                if (result > 0 && result != unchecked((uint)-1) && pData != IntPtr.Zero)
                {
                    if (_rawState == ForgeState.ButtonDown || _rawState == ForgeState.ButtonUp)
                    {
                        long currentPtr = pData.ToInt64();
                        uint buttonFlag = (_rawState == ForgeState.ButtonDown) ? RI_MOUSE_LEFT_BUTTON_DOWN : RI_MOUSE_LEFT_BUTTON_UP;

                        for (int i = 0; i < result; i++)
                        {
                            RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>((IntPtr)currentPtr);
                            if (header.dwType == RIM_TYPEMOUSE)
                            {
                                IntPtr pMouse = new IntPtr(currentPtr + _rawInputHeaderSize);
                                RAWMOUSE mouse = Marshal.PtrToStructure<RAWMOUSE>(pMouse);
                                mouse.usButtonFlags |= (ushort)buttonFlag;
                                Marshal.StructureToPtr(mouse, pMouse, false);
                            }
                            currentPtr += header.dwSize; 
                        }
                    }
                    return result;
                }

                // 2. EMPTY BUFFER INJECTION
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
                                    _lastInjectedRawState = _rawState;
                                    return 1; 
                                }
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception) { return _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader); }
        }

        // =========================================================
        // 6. NATIVE STRUCT PACKING AND STATE HOOKS
        // =========================================================
        private bool EnsureNativePrepared(uint buttonFlag, ushort mouseFlags = 0, int lastX = 0, int lastY = 0)
        {
            try
            {
                lock (_deviceLock) if (_capturedDevice == IntPtr.Zero || _capturedPacketSize == 0 || _rawInputHeaderSize == 0) return false;

                lock (_nativeBufLock)
                {
                    uint standardPacketSize = _rawInputHeaderSize + 24;
                    int alignedSize = (int)((standardPacketSize + 7) & ~7);

                    if (_nativeFakePacketPtr != IntPtr.Zero && _nativeFakePacketSize != standardPacketSize)
                    {
                        Marshal.FreeHGlobal(_nativeFakePacketPtr);
                        _nativeFakePacketPtr = IntPtr.Zero;
                    }

                    if (_nativeFakePacketPtr == IntPtr.Zero)
                    {
                        _nativeFakePacketPtr = Marshal.AllocHGlobal(alignedSize);
                        _nativeFakePacketSize = (int)standardPacketSize;

                        byte[] zeros = new byte[alignedSize];
                        Marshal.Copy(zeros, 0, _nativeFakePacketPtr, alignedSize);
                    }

                    // Pass standardPacketSize to dwSize
                    RAWINPUTHEADER header = new RAWINPUTHEADER { dwType = RIM_TYPEMOUSE, dwSize = standardPacketSize, hDevice = _capturedDevice, wParam = IntPtr.Zero };
                    RAWMOUSE mouse = new RAWMOUSE { usFlags = mouseFlags, ulButtons = buttonFlag, ulRawButtons = 0, lLastX = lastX, lLastY = lastY, ulExtraInformation = 0 };

                    Marshal.StructureToPtr(header, _nativeFakePacketPtr, false);
                    IntPtr pMouse = new IntPtr(_nativeFakePacketPtr.ToInt64() + _rawInputHeaderSize);
                    Marshal.StructureToPtr(mouse, pMouse, false);

                    return true;
                }
            }
            catch (Exception) { return false; }
        }

        private short HookedGetAsyncKeyState(int vKey)
        {
            _allHooksInstalled.Wait();
            short realState = _originalGetAsyncKeyState(vKey);
            if (vKey == VK_LBUTTON && _rawState == ForgeState.ButtonDown) 
            {
                return (short)(realState | unchecked((short)0x8000));
            }
            return realState;
        }

        private short HookedGetKeyState(int nVirtKey)
        {
            _allHooksInstalled.Wait();
            short realState = _originalGetKeyState(nVirtKey);
            if (nVirtKey == VK_LBUTTON && _rawState == ForgeState.ButtonDown) 
            {
                return (short)(realState | unchecked((short)0x8000));
            }
            return realState;
        }
        
        private IntPtr HookedSetCapture(IntPtr hWnd)
        {
            _allHooksInstalled.Wait();
            return _originalSetCapture(hWnd);
        }

        private bool HookedReleaseCapture()
        {
            _allHooksInstalled.Wait();
            // During synthetic button-down, block the game from releasing capture
            if (_rawState == ForgeState.ButtonDown)
                return true;
            return _originalReleaseCapture();
        }

        private IntPtr HookedGetCapture()
        {
            _allHooksInstalled.Wait();
            // During synthetic button-down, tell the game it has capture
            if (_rawState == ForgeState.ButtonDown && _mainHwnd != IntPtr.Zero)
                return _mainHwnd;
            return _originalGetCapture();
        }

        private void ProbeRawInputDevices()
        {
            try
            {
                _rawInputHeaderSize = (uint)Marshal.SizeOf<RAWINPUTHEADER>();

                uint numDevices = 0;
                uint cbSize = (uint)Marshal.SizeOf<RAWINPUTDEVICELIST>();
                GetRawInputDeviceList(IntPtr.Zero, ref numDevices, cbSize);

                if (numDevices == 0) return;

                var devices = new RAWINPUTDEVICELIST[numDevices];
                uint result = GetRawInputDeviceList(devices, ref numDevices, cbSize);
                if (result == unchecked((uint)-1)) return;

                for (int i = 0; i < result; i++)
                {
                    if (devices[i].dwType == RIM_TYPEMOUSE && devices[i].hDevice != IntPtr.Zero)
                    {
                        lock (_deviceLock)
                        {
                            _capturedDevice = devices[i].hDevice;
                            _capturedPacketSize = (uint)Marshal.SizeOf<RAWINPUT>();
                        }
                        _server.ReportMessage($"[LOG] Probed mouse device: 0x{_capturedDevice.ToInt64():X}, packetSize={_capturedPacketSize}, headerSize={_rawInputHeaderSize}");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in ProbeRawInputDevices]\n{ex}");
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputDeviceList(
            [Out] RAWINPUTDEVICELIST[] pRawInputDeviceList,
            ref uint puiNumDevices,
            uint cbSize);
            
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputDeviceList(
            IntPtr pRawInputDeviceList, // Accepts IntPtr.Zero
            ref uint puiNumDevices,
            uint cbSize);

        private void FreeNativeFakePacket()
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
        
        private void CleanupFakePacketResources()
        {
            _stopClickThread = true;
            FreeNativeFakePacket();
            
            // Optionally: Restore the subclassed windows via SetWindowLongPtr to their original delegates here
        }
    }
}