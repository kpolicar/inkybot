using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static InkybotHook.NativeMethods;

namespace InkybotHook
{
    public partial class InjectionEntryPoint
    {
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

        // --- UPDATED AUTOMATION STATE VARIABLES ---
        private enum ForgeState { Idle, Hover, ButtonDown, ButtonUp } 
        private volatile ForgeState _rawState = ForgeState.Idle;
        private volatile ForgeState _msgState = ForgeState.Idle;
        private volatile ForgeState _lastInjectedRawState = ForgeState.Idle;
        private int _lastClickTime = 0;
        private IntPtr _mainHwnd = IntPtr.Zero; // Tracks the game's window handle

        // =========================================================
        // 3. INSTALLATION
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

            /*
            var screenToClientHook = TryInstallHook<ScreenToClientDelegate>("ScreenToClient", new ScreenToClientDelegate(HookedScreenToClient), out _originalScreenToClient);
            if (screenToClientHook != null) hooks.Add(screenToClientHook);

            var clientToScreenHook = TryInstallHook<ClientToScreenDelegate>("ClientToScreen", new ClientToScreenDelegate(HookedClientToScreen), out _originalClientToScreen);
            if (clientToScreenHook != null) hooks.Add(clientToScreenHook);*/

            return hooks;
        }

        // =========================================================
        // 4. TIMER AUTOMATION
        // =========================================================
        private void CheckTimer()
        {
            if (!IsCursorOverrideActive) return;

            int now = Environment.TickCount;
            if (now - _lastClickTime >= 1000 && _rawState == ForgeState.Idle && _msgState == ForgeState.Idle)
            {
                _rawState = ForgeState.ButtonDown; // Raw input doesn't need hover
                _msgState = ForgeState.Hover;      // UI input MUST hover first
                _lastClickTime = now;
            }
        }

        // =========================================================
        // 5. IMPLEMENTATIONS
        // =========================================================

        private bool HookedGetCursorPos(out POINT lpPoint)
        {
            if (IsCursorOverrideActive)
            {
                lpPoint = GetFixedScreenPoint();
                return true;
            }
            return _originalGetCursorPos(out lpPoint);
        }

        private bool HookedGetCursorInfo(ref CURSORINFO pci)
        {
            bool result = _originalGetCursorInfo(ref pci);
            if (IsCursorOverrideActive && result)
            {
                pci.ptScreenPos = GetFixedScreenPoint();
            }
            return result;
        }

        private bool HookedGetPointerInfo(uint pointerId, ref POINTER_INFO pointerInfo)
        {
            bool result = _originalGetPointerInfo(pointerId, ref pointerInfo);
            if (IsCursorOverrideActive && result)
            {
                pointerInfo.ptPixelLocation = GetFixedScreenPoint();
                pointerInfo.ptPixelLocationRaw = GetFixedScreenPoint();

                // CRITICAL: We must force a 'true' return so Unity doesn't crash 
                // trying to parse the uninitialized struct from our fake injected ID!
                return true;
            }
            return result;
        }

        private bool HookedIsIconic(IntPtr hWnd) { return false; }



        // --- THE MESSAGE PUMP HOOKS (BACKGROUND UI DISPATCHER) ---
        private bool HookedPeekMessageW(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            CheckTimer();

            bool result = _originalPeekMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);

            if (IsCursorOverrideActive)
            {
                // 1. Capture the window handle so we can do coordinate math
                if (_mainHwnd == IntPtr.Zero && lpMsg.hwnd != IntPtr.Zero) 
                {
                    _mainHwnd = lpMsg.hwnd;
                }

                // 2. Filter physical mouse movements, BUT keep the position locked!
                if (result) FilterMouseMessage(ref lpMsg);

                // 3. Inject our sequence (Hover -> Down -> Up) into empty queues
                if (_msgState != ForgeState.Idle && (!result || lpMsg.message == WM_NULL))
                {
                    // Fallback to active window if PeekMessage was called with a null hWnd
                    IntPtr targetHwnd = (hWnd != IntPtr.Zero) ? hWnd : _mainHwnd;
                    if (targetHwnd == IntPtr.Zero) return result; 

                    lpMsg.hwnd = targetHwnd; 
                    
                    // --- TRANSLATE SCREEN TO CLIENT COORDS ---
                    POINT target = GetFixedScreenPoint();
                    lpMsg.pt = target; 
                    lpMsg.time = (uint)Environment.TickCount;

                    // Pointer messages use Screen Coordinates in lParam
                    lpMsg.lParam = (IntPtr)((target.Y << 16) | (target.X & 0xFFFF));

                    // The wParam for the primary mouse pointer is usually ID 1 in the LOWORD. 
                    // We use 1 to match the ID Unity expects for the mouse (as seen in your log).
                    lpMsg.wParam = (IntPtr)1; 

                    // --- THE 4-STEP STATE MACHINE (NOW USING POINTERS) ---
                    if (_msgState == ForgeState.Hover)
                    {
                        lpMsg.message = 0x0245; // WM_POINTERUPDATE
                        //lpMsg.message = WM_MOUSEMOVE; 
                        if ((wRemoveMsg & PM_REMOVE) != 0) _msgState = ForgeState.ButtonDown;
                    }
                    else if (_msgState == ForgeState.ButtonDown)
                    {
                        lpMsg.message = 0x0246; // WM_POINTERDOWN
                        //lpMsg.message = WM_LBUTTONDOWN; 
                        if ((wRemoveMsg & PM_REMOVE) != 0) _msgState = ForgeState.ButtonUp;
                    }
                    else if (_msgState == ForgeState.ButtonUp)
                    {
                        lpMsg.message = 0x0247; // WM_POINTERUP
                        //lpMsg.message = WM_LBUTTONUP; 
                        if ((wRemoveMsg & PM_REMOVE) != 0) _msgState = ForgeState.Idle;
                    }

                    return true;
                }
            }
            return result;
        }

        private int HookedGetMessageW(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
        {
            int result = _originalGetMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
            if (IsCursorOverrideActive && result > 0)
            {
                FilterMouseMessage(ref lpMsg);
            }
            return result;
        }

        private void FilterMouseMessage(ref MSG msg)
        {
            bool isStandardMouse = (msg.message >= WM_MOUSEFIRST && msg.message <= WM_MOUSELAST);
            bool isPointerOrTouch = (msg.message >= WM_POINTERFIRST && msg.message <= WM_POINTERLAST);
            
            // --- 1. HIJACK LEGACY MOUSE MOVEMENT ---
            if (msg.message == 0x0200 /* WM_MOUSEMOVE */)
            {
                POINT target = GetFixedScreenPoint();
                msg.pt = target; // Lock absolute screen coordinates

                if (_mainHwnd != IntPtr.Zero)
                {
                    _originalScreenToClient(_mainHwnd, ref target); // Convert to client
                    msg.lParam = (IntPtr)((target.Y << 16) | (target.X & 0xFFFF)); // Lock relative coordinates
                }
                return; 
            }

            // --- 2. HIJACK MODERN POINTER MOVEMENT ---
            if (msg.message == 0x0245 /* WM_POINTERUPDATE */)
            {
                POINT target = GetFixedScreenPoint();
                msg.pt = target; // Lock absolute screen coordinates
                
                // DO NOT CONVERT TO CLIENT! WM_POINTERUPDATE expects Screen Coordinates in the lParam.
                msg.lParam = (IntPtr)((target.Y << 16) | (target.X & 0xFFFF)); 
                return;
            }

            // Shred everything else (physical clicks, raw input notifications, touch events)
            if (isStandardMouse || isPointerOrTouch/* || msg.message == WM_INPUT*/) // todo by removing this, real clicks are coming through now 
            {
                msg.message = WM_NULL; 
            }
        }

        // --- THE RAW INPUT HOOKS (3D WORLD DISPATCHER) ---
        private uint HookedGetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            uint result = _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);

            if (IsCursorOverrideActive && pData != IntPtr.Zero && result > 0 && result != unchecked((uint)-1))
            {
                if (uiCommand == RID_INPUT) 
                {
                    uint dwType = (uint)Marshal.ReadInt32(pData, 0);
                    if (dwType == 0) // RIM_TYPEMOUSE
                    {
                        // Surgically scrub clicks without re-allocating memory
                        Marshal.WriteInt32(pData, 36, 0);
                        Marshal.WriteInt32(pData, 40, 0);
                        Marshal.WriteInt16(pData, 28, 0);
                    }
                }
            }
            return result;
        }

        private uint HookedGetRawInputBuffer(IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            uint result = _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);

            if (!IsCursorOverrideActive) return result;
            if (pData == IntPtr.Zero) return result;

            if (result > 0 && result != unchecked((uint)-1))
            {
                ScrubRawInputBuffer(pData, (int)result); 
                return result; 
            }

            // INJECT OUR SYNTHETIC CLICK SAFELY
            if (result == 0)
            {
                if (_rawState == ForgeState.ButtonDown)
                {
                    if (_lastInjectedRawState != ForgeState.ButtonDown)
                    {
                        if (pcbSize >= 48) // 48 bytes is the size of RAWINPUT
                        {
                            InjectFakeMousePacket(pData, RI_MOUSE_LEFT_BUTTON_DOWN);
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
                    if (_lastInjectedRawState != ForgeState.ButtonUp)
                    {
                        if (pcbSize >= 48)
                        {
                            InjectFakeMousePacket(pData, RI_MOUSE_LEFT_BUTTON_UP);
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

        // Replaces your CreateFakeRawInputPacket method entirely!
        private void InjectFakeMousePacket(IntPtr pData, int buttonFlag)
        {
            // 1. Zero out the memory first
            for (int i = 0; i < 48; i += 4) Marshal.WriteInt32(pData, i, 0);

            // 2. Write Header
            Marshal.WriteInt32(pData, 0, 0); // dwType = RIM_TYPEMOUSE
            Marshal.WriteInt32(pData, 4, 48); // dwSize = 48

            // 3. Write Mouse Payload
            Marshal.WriteInt16(pData, 24, 0); // usFlags
            Marshal.WriteInt32(pData, 28, buttonFlag); // ulButtons
        }

        private void ScrubRawInputBuffer(IntPtr pData, int packetCount)
        {
            try
            {
                IntPtr currentPtr = pData;
                int align = (IntPtr.Size == 8) ? 7 : 3; // 64-bit vs 32-bit padding

                for (int i = 0; i < packetCount; i++)
                {
                    uint dwType = (uint)Marshal.ReadInt32(currentPtr, 0);
                    uint dwSize = (uint)Marshal.ReadInt32(currentPtr, 4);

                    if (dwType == 0) // RIM_TYPEMOUSE
                    {
                        // Direct surgical writes. No dangerous Struct casts!
                        Marshal.WriteInt32(currentPtr, 36, 0); // Scrub X Delta
                        Marshal.WriteInt32(currentPtr, 40, 0); // Scrub Y Delta
                        Marshal.WriteInt16(currentPtr, 28, 0); // Scrub Clicks
                    }

                    if (dwSize == 0) break; // Safety net

                    // Perfectly align the pointer to prevent Access Violations
                    long nextPtr = currentPtr.ToInt64() + dwSize;
                    nextPtr = (nextPtr + align) & ~((long)align); 
                    currentPtr = new IntPtr(nextPtr);
                }
            }
            catch { }
        }

        // --- THE HARDWARE STATE HOOKS (MODIFIER BYPASS) ---
        private short HookedGetAsyncKeyState(int vKey)
        {
            if (IsCursorOverrideActive)
            {
                if (vKey == 16 || vKey == 17 || vKey == 18 || vKey == 91 || vKey == 92 || vKey == 20) return 0;

                if (vKey == VK_LBUTTON || vKey == VK_RBUTTON)
                {
                    // Sync strictly with the raw state machine!
                    if (vKey == VK_LBUTTON && (_rawState == ForgeState.ButtonDown || _rawState == ForgeState.ButtonUp))
                        return unchecked((short)0x8000); 
                    return 0;
                }
            }
            return _originalGetAsyncKeyState(vKey);
        }

        private short HookedGetKeyState(int nVirtKey)
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

        private bool HookedScreenToClient(IntPtr hWnd, ref POINT lpPoint)
        {
            if (IsCursorOverrideActive)
            {
                // 1. Overwrite whatever physical point the game provided with our fake screen point
                lpPoint = GetFixedScreenPoint();
                
                // 2. Let Windows do the math to convert OUR fake screen point into a client point
                return _originalScreenToClient(hWnd, ref lpPoint);
            }
            
            return _originalScreenToClient(hWnd, ref lpPoint);
        }

        private bool HookedClientToScreen(IntPtr hWnd, ref POINT lpPoint)
        {
            bool result = _originalClientToScreen(hWnd, ref lpPoint);
            
            if (IsCursorOverrideActive && result)
            {
                // No matter what client point they asked about, tell them the screen point is our fixed target
                lpPoint = GetFixedScreenPoint();
            }
            
            return result;
        }
    }
}