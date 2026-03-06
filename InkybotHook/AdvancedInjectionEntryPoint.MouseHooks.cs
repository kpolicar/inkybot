using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using static InkybotHook.NativeMethods;

namespace InkybotHook
{
    public partial class AdvancedInjectionEntryPoint
    {
        private readonly System.Threading.ManualResetEventSlim _allHooksInstalled = new System.Threading.ManualResetEventSlim(false);

        // =========================================================
        // 1. DELEGATES 
        // =========================================================
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GetCursorPosDelegate(out POINT lpPoint);
        private GetCursorPosDelegate _originalGetCursorPos;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool IsIconicDelegate(IntPtr hWnd);
        private IsIconicDelegate _originalIsIconic;

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

        private Thread _automationThread;
        private volatile bool _stopAutomationThread = false;

        // =========================================================
        // 3. CONSTANTS & NATIVE IMPORTS
        // =========================================================
        private const int MAGIC_RAW_HANDLE = 0x1337;
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
        
        // =========================================================
        // 4. INSTALLATION & AUTOMATION LOOP
        // =========================================================
        private List<EasyHook.LocalHook> InstallHooks()
        {
            var hooks = new List<EasyHook.LocalHook>();


            hooks.Add(TryInstallHook<GetCursorPosDelegate>("GetCursorPos",
                new GetCursorPosDelegate(HookedGetCursorPos), out _originalGetCursorPos));
            hooks.Add(TryInstallHook<IsIconicDelegate>("IsIconic",
                new IsIconicDelegate(HookedIsIconic), out _originalIsIconic));
            hooks.Add(TryInstallHook<PeekMessageWDelegate>("PeekMessageW", new PeekMessageWDelegate(HookedPeekMessageW), out _originalPeekMessageW));
            hooks.Add(TryInstallHook<TranslateMessageDelegate>("TranslateMessage", new TranslateMessageDelegate(HookedTranslateMessage), out _originalTranslateMessage));
            hooks.Add(TryInstallHook<DispatchMessageWDelegate>("DispatchMessageW", new DispatchMessageWDelegate(HookedDispatchMessageW), out _originalDispatchMessageW));
            hooks.Add(TryInstallHook<GetRawInputDataDelegate>("GetRawInputData", new GetRawInputDataDelegate(HookedGetRawInputData), out _originalGetRawInputData));
            hooks.Add(TryInstallHook<GetRawInputBufferDelegate>("GetRawInputBuffer", new GetRawInputBufferDelegate(HookedGetRawInputBuffer), out _originalGetRawInputBuffer));
            hooks.Add(TryInstallHook<GetAsyncKeyStateDelegate>("GetAsyncKeyState", new GetAsyncKeyStateDelegate(HookedGetAsyncKeyState), out _originalGetAsyncKeyState));
            hooks.Add(TryInstallHook<GetKeyStateDelegate>("GetKeyState", new GetKeyStateDelegate(HookedGetKeyState), out _originalGetKeyState));

            hooks.RemoveAll(item => item == null);

            _stopAutomationThread = false;
            _automationThread = new Thread(AutomationThreadLoop) { IsBackground = true, Name = "Inkybot_AutomationThread" };
            _automationThread.Start();

            _allHooksInstalled.Set();
            return hooks;
        }

        private void AutomationThreadLoop()
        {
            while (!_stopAutomationThread)
            {
                try
                {
                    int now = Environment.TickCount;
                    int elapsed = now - _lastStateChangeTime;

                    // Wait for a click request from Win32Input
                    if (_rawState == ForgeState.Idle && _server.ClickRequested)
                    {
                        _server.ClickRequested = false;
                        _server.ClickCompleted = false;
                        _server.IsClickActive = true;
                        _rawState = ForgeState.ButtonDown;
                        _lastStateChangeTime = now;
                        QueueMessage("[AutomationLoop] Click requested, State: Idle -> ButtonDown");
                        if (_mainHwnd != IntPtr.Zero)
                            EnqueueClickPhaseMessages(WM_POINTERDOWN, WM_LBUTTONDOWN, (IntPtr)MK_LBUTTON);
                    }
                    else if (_rawState == ForgeState.ButtonDown && elapsed >= 50)
                    {
                        _rawState = ForgeState.ButtonUp;
                        _lastStateChangeTime = now;
                        QueueMessage("[AutomationLoop] State: ButtonDown -> ButtonUp");
                        if (_mainHwnd != IntPtr.Zero)
                            EnqueueClickPhaseMessages(WM_POINTERUP, WM_LBUTTONUP, IntPtr.Zero);
                    }
                    else if (_rawState == ForgeState.ButtonUp && elapsed >= 50)
                    {
                        _rawState = ForgeState.Idle;
                        _server.IsClickActive = false;
                        _server.ClickCompleted = true;
                        _lastStateChangeTime = now;
                        QueueMessage("[AutomationLoop] State: ButtonUp -> Idle (click completed)");
                    }
                }
                catch (Exception ex)
                {
                    _server.ReportMessage($"[EXCEPTION in AutomationThreadLoop]\n{ex}");
                }

                Thread.Sleep(1);
            }
        }
        
        private bool HookedGetCursorPos(out POINT lpPoint)
        {
            bool result = _originalGetCursorPos(out lpPoint);
            return result;  
            if (_server.point.X != -1 && _server.point.Y != -1)
            {
                // server.point contains screen coordinates (Win32Input converts client->screen before setting)
                lpPoint.X = _server.point.X;
                lpPoint.Y = _server.point.Y;
                LogFirstCall("GetCursorPos:Override");
            }
            return result;
        }

        private bool HookedIsIconic(IntPtr hWnd)
        {
            LogFirstCall("IsIconic");
            // Always report as not minimized so Dofus keeps rendering
            return false;
        }

        private void EnqueueClickPhaseMessages(uint pointerMsg, uint lbuttonMsg, IntPtr lbuttonWParam)
        {
            GetCursorPos(out POINT screenPt);
            POINT clientPt = screenPt;
            ScreenToClient(_mainHwnd, ref clientPt);

            IntPtr clientLParam = (IntPtr)((uint)((clientPt.Y << 16) | (clientPt.X & 0xFFFF)));
            IntPtr screenLParam = (IntPtr)((uint)((screenPt.Y << 16) | (screenPt.X & 0xFFFF)));

            uint activePointerId = _capturedPointerId == 0 ? 1 : _capturedPointerId;
            IntPtr pointerWParam = (IntPtr)((0x0016 << 16) | activePointerId);
            uint now = (uint)Environment.TickCount;

            lock (_queueLock)
            {
                // Stealth Hardware
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_INPUT, wParam = IntPtr.Zero, lParam = (IntPtr)MAGIC_RAW_HANDLE, time = now, pt = screenPt });
                // Modern UI: screen coordinates
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = pointerMsg, wParam = pointerWParam, lParam = screenLParam, time = now, pt = screenPt });
                // Legacy UI: client coordinates
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = lbuttonMsg, wParam = lbuttonWParam, lParam = clientLParam, time = now, pt = clientPt });
            }
            PostMessage(_mainHwnd, WM_NULL, IntPtr.Zero, IntPtr.Zero);
            QueueMessage($"[AutomationLoop] Enqueued 3 messages at screen=({screenPt.X},{screenPt.Y}) client=({clientPt.X},{clientPt.Y})");
        }

        // =========================================================
        // 5. IMPLEMENTATIONS
        // =========================================================
        private bool HookedPeekMessageW(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            _allHooksInstalled.Wait();
            try
            {
                LogFirstCall("PeekMessageW");
                bool result = _originalPeekMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);

                if (lpMsg.hwnd != IntPtr.Zero)
                {
                    lock (_wndProcLock)
                    {
                        TrySubclassWindow(lpMsg.hwnd);
                        TryLockOntoInputWindow(ref lpMsg);
                    }
                }

                if (TryInjectSyntheticMessage(ref lpMsg, wRemoveMsg, result))
                    return true;

                return result;
            }
            catch (Exception) { return false; }
        }

        private void TrySubclassWindow(IntPtr hwnd)
        {
            if (_originalWndProcs.ContainsKey(hwnd)) return;

            WndProcDelegate newDelegate = new WndProcDelegate(HookedWndProc);
            _wndProcDelegates[hwnd] = newDelegate;
            IntPtr orig = SetWindowLongPtr(hwnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(newDelegate));
            _originalWndProcs[hwnd] = orig;

            System.Text.StringBuilder windowText = new System.Text.StringBuilder(256);
            System.Text.StringBuilder className = new System.Text.StringBuilder(256);
            GetWindowText(hwnd, windowText, windowText.Capacity);
            GetClassName(hwnd, className, className.Capacity);

            string wName = string.IsNullOrEmpty(windowText.ToString()) ? "[No Name]" : windowText.ToString();
            QueueMessage($"[PeekMessageW] Subclassed HWND: 0x{hwnd.ToInt64():X} | Name: '{wName}' | Class: '{className}'");
        }

        private void TryLockOntoInputWindow(ref MSG lpMsg)
        {
            bool isInputMsg = lpMsg.message == WM_INPUT || lpMsg.message == WM_MOUSEMOVE
                || (lpMsg.message >= WM_POINTERUPDATE && lpMsg.message <= WM_POINTERUP);
            if (!isInputMsg) return;

            if (_mainHwnd != lpMsg.hwnd)
            {
                _mainHwnd = lpMsg.hwnd;
                QueueMessage($"[PeekMessageW] Locked onto active Input HWND: 0x{_mainHwnd.ToInt64():X}");
            }

            if (lpMsg.message >= WM_POINTERUPDATE && lpMsg.message <= WM_POINTERUP)
            {
                uint extractedPointerId = (uint)(lpMsg.wParam.ToInt64() & 0xFFFF);
                if (_capturedPointerId != extractedPointerId)
                {
                    _capturedPointerId = extractedPointerId;
                    QueueMessage($"[PeekMessageW] Captured real OS Pointer ID: {_capturedPointerId}");
                }
            }
        }

        private bool TryInjectSyntheticMessage(ref MSG lpMsg, uint wRemoveMsg, bool peekResult)
        {
            lock (_queueLock)
            {
                if (_syntheticMessages.Count == 0 || _mainHwnd == IntPtr.Zero) return false;
                if (peekResult && lpMsg.message != WM_NULL) return false;

                if ((wRemoveMsg & 0x0001 /* PM_REMOVE */) != 0)
                {
                    lpMsg = _syntheticMessages.Dequeue();
                    QueueMessage($"[PeekMessageW] Injected synthetic MSG: 0x{lpMsg.message:X4}");
                }
                else
                {
                    lpMsg = _syntheticMessages.Peek();
                }
                return true;
            }
        }

        private bool HookedTranslateMessage(ref MSG lpMsg)
        {
            _allHooksInstalled.Wait();
            LogFirstCall("TranslateMessage");
            if (lpMsg.message == WM_INPUT && lpMsg.lParam == (IntPtr)MAGIC_RAW_HANDLE)
            {
                QueueMessage("[TranslateMessage] Bypassed OS translation for synthetic WM_INPUT (MAGIC_RAW_HANDLE)");
                return true;
            }
            return _originalTranslateMessage(ref lpMsg);
        }

        private IntPtr HookedDispatchMessageW(ref MSG lpMsg)
        {
            _allHooksInstalled.Wait();
            LogFirstCall("DispatchMessageW");
            if (lpMsg.message == WM_INPUT && lpMsg.lParam == (IntPtr)MAGIC_RAW_HANDLE)
            {
                WndProcDelegate targetDelegate = null;
                IntPtr targetOrig = IntPtr.Zero;

                lock (_wndProcLock)
                {
                    _wndProcDelegates.TryGetValue(lpMsg.hwnd, out targetDelegate);
                    _originalWndProcs.TryGetValue(lpMsg.hwnd, out targetOrig);
                }

                if (targetDelegate != null)
                {
                    QueueMessage($"[DispatchMessageW] Dispatched synthetic WM_INPUT to HWND 0x{lpMsg.hwnd.ToInt64():X} via hook delegate");
                    return targetDelegate(lpMsg.hwnd, lpMsg.message, lpMsg.wParam, lpMsg.lParam);
                }
                else if (targetOrig != IntPtr.Zero)
                {
                    QueueMessage($"[DispatchMessageW] Dispatched synthetic WM_INPUT to HWND 0x{lpMsg.hwnd.ToInt64():X} via original WndProc");
                    return CallWindowProc(targetOrig, lpMsg.hwnd, lpMsg.message, lpMsg.wParam, lpMsg.lParam);
                }
                QueueMessage("[DispatchMessageW] Synthetic WM_INPUT had no target WndProc, dropped");
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
                LogFirstCall("GetRawInputData");
                if (hRawInput == (IntPtr)MAGIC_RAW_HANDLE && uiCommand == RID_INPUT)
                    return HandleMagicRawInput(pData, ref pcbSize);

                uint result = _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
                TryCaptureDeviceInfo(pData, result, uiCommand, cbSizeHeader);
                return result;
            }
            catch (Exception) { return _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader); }
        }

        private uint HandleMagicRawInput(IntPtr pData, ref uint pcbSize)
        {
            if (_capturedDevice == IntPtr.Zero || _capturedPacketSize == 0) return unchecked((uint)-1);
            if (pData == IntPtr.Zero) { pcbSize = _capturedPacketSize; return 0; }
            if (pcbSize < _capturedPacketSize) { pcbSize = _capturedPacketSize; return unchecked((uint)-1); }

            uint buttonFlag = (_rawState == ForgeState.ButtonDown) ? RI_MOUSE_LEFT_BUTTON_DOWN : RI_MOUSE_LEFT_BUTTON_UP;
            if (EnsureNativePrepared(buttonFlag))
            {
                lock (_nativeBufLock)
                {
                    CopyMemory(pData, _nativeFakePacketPtr, (UIntPtr)_capturedPacketSize);
                    _lastInjectedRawState = _rawState;
                    QueueMessage($"[GetRawInputData] Fabricated RAWINPUT packet: buttonFlag=0x{buttonFlag:X}, device=0x{_capturedDevice.ToInt64():X}");
                    return _capturedPacketSize;
                }
            }
            return unchecked((uint)-1);
        }

        private void TryCaptureDeviceInfo(IntPtr pData, uint result, uint uiCommand, uint cbSizeHeader)
        {
            if (pData == IntPtr.Zero || result == 0 || result == unchecked((uint)-1) || uiCommand != RID_INPUT) return;
            if (_rawInputHeaderSize == 0 && cbSizeHeader != 0) _rawInputHeaderSize = cbSizeHeader;

            RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>(pData);
            if (header.dwType != RIM_TYPEMOUSE || header.hDevice == IntPtr.Zero) return;

            lock (_deviceLock)
            {
                bool captured = false;
                if (_capturedDevice == IntPtr.Zero) { _capturedDevice = header.hDevice; captured = true; }
                if (_capturedPacketSize == 0 && header.dwSize != 0) { _capturedPacketSize = header.dwSize; captured = true; }
                if (captured)
                    QueueMessage($"[GetRawInputData] Captured device=0x{_capturedDevice.ToInt64():X}, packetSize={_capturedPacketSize}");
            }
        }

        private uint HookedGetRawInputBuffer(IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            _allHooksInstalled.Wait();
            try
            {
                LogFirstCall("GetRawInputBuffer");
                uint result = _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);
                if (_rawInputHeaderSize == 0 && cbSizeHeader != 0) _rawInputHeaderSize = cbSizeHeader;

                if (result > 0 && result != unchecked((uint)-1) && pData != IntPtr.Zero)
                    return PatchBufferWithButtonFlags(pData, result);

                if (result == 0 && _capturedDevice != IntPtr.Zero && pData != IntPtr.Zero)
                    return TryInjectIntoEmptyBuffer(pData, ref pcbSize, result);

                return result;
            }
            catch (Exception) { return _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader); }
        }

        private uint PatchBufferWithButtonFlags(IntPtr pData, uint packetCount)
        {
            if (_rawState != ForgeState.ButtonDown && _rawState != ForgeState.ButtonUp) return packetCount;

            long currentPtr = pData.ToInt64();
            uint buttonFlag = (_rawState == ForgeState.ButtonDown) ? RI_MOUSE_LEFT_BUTTON_DOWN : RI_MOUSE_LEFT_BUTTON_UP;

            for (int i = 0; i < packetCount; i++)
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
            QueueMessage($"[GetRawInputBuffer] Patched {packetCount} real packets with buttonFlag=0x{buttonFlag:X}");
            return packetCount;
        }

        private uint TryInjectIntoEmptyBuffer(IntPtr pData, ref uint pcbSize, uint originalResult)
        {
            if (_rawState != ForgeState.ButtonDown && _rawState != ForgeState.ButtonUp) return originalResult;
            if (_lastInjectedRawState == _rawState) return originalResult;

            uint buttonFlag = (_rawState == ForgeState.ButtonDown) ? RI_MOUSE_LEFT_BUTTON_DOWN : RI_MOUSE_LEFT_BUTTON_UP;
            if (EnsureNativePrepared(buttonFlag))
            {
                lock (_nativeBufLock)
                {
                    if (pcbSize >= (uint)_nativeFakePacketSize)
                    {
                        CopyMemory(pData, _nativeFakePacketPtr, (UIntPtr)_nativeFakePacketSize);
                        _lastInjectedRawState = _rawState;
                        QueueMessage($"[GetRawInputBuffer] Injected fake packet into empty buffer, buttonFlag=0x{buttonFlag:X}");
                        return 1;
                    }
                }
            }
            return originalResult;
        }

        // =========================================================
        // 6. NATIVE STRUCT PACKING AND STATE HOOKS
        // =========================================================
        private bool EnsureNativePrepared(uint buttonFlag)
        {
            try
            {
                lock (_deviceLock) if (_capturedDevice == IntPtr.Zero || _capturedPacketSize == 0 || _rawInputHeaderSize == 0) return false;

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

                    RAWINPUTHEADER header = new RAWINPUTHEADER { dwType = RIM_TYPEMOUSE, dwSize = _capturedPacketSize, hDevice = _capturedDevice, wParam = IntPtr.Zero };
                    RAWMOUSE mouse = new RAWMOUSE { usFlags = 0, ulButtons = buttonFlag, ulRawButtons = 0, lLastX = 0, lLastY = 0, ulExtraInformation = 0 };

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
            LogFirstCall("GetAsyncKeyState");
            short realState = _originalGetAsyncKeyState(vKey);
            if (vKey == VK_LBUTTON && _rawState == ForgeState.ButtonDown)
            {
                LogFirstCall("GetAsyncKeyState:Spoofed");
                return (short)(realState | unchecked((short)0x8000));
            }
            return realState;
        }

        private short HookedGetKeyState(int nVirtKey)
        {
            _allHooksInstalled.Wait();
            LogFirstCall("GetKeyState");
            short realState = _originalGetKeyState(nVirtKey);
            if (nVirtKey == VK_LBUTTON && _rawState == ForgeState.ButtonDown)
            {
                LogFirstCall("GetKeyState:Spoofed");
                return (short)(realState | unchecked((short)0x8000));
            }
            return realState;
        }
        
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
            _stopAutomationThread = true;
            FreeNativeFakePacket();
            
            // Optionally: Restore the subclassed windows via SetWindowLongPtr to their original delegates here
        }
    }
}