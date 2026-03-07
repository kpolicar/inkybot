using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using static InkybotHook.NativeMethods;

namespace InkybotHook
{
    public partial class AdvancedInjectionEntryPoint
    {
        // =========================================================
        // STATE
        // =========================================================
        private IntPtr _originalWndProc = IntPtr.Zero;
        private WndProcDelegate _wndProcDelegate;
        private volatile bool _needsSubclass;

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
        // CONSTANTS
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

        // =========================================================
        // COORDINATE HELPERS
        // =========================================================
        private POINT ResolveScreenPoint()
        {
            if (IsCursorOverrideActive)
                return GetFixedScreenPoint();
            GetCursorPosNative(out POINT pt);
            return pt;
        }

        private (POINT screen, POINT client, IntPtr screenLParam, IntPtr clientLParam) ResolveMsgCoordinates()
        {
            POINT screenPt = ResolveScreenPoint();
            POINT clientPt = screenPt;
            ScreenToClientNative(_mainHwnd, ref clientPt);

            IntPtr clientLParam = (IntPtr)((uint)((clientPt.Y << 16) | (clientPt.X & 0xFFFF)));
            IntPtr screenLParam = (IntPtr)((uint)((screenPt.Y << 16) | (screenPt.X & 0xFFFF)));

            return (screenPt, clientPt, screenLParam, clientLParam);
        }

        private (IntPtr pointerWParam, uint now) ResolvePointerParams()
        {
            uint activePointerId = _capturedPointerId == 0 ? 1 : _capturedPointerId;
            IntPtr pointerWParam = (IntPtr)((0x0016 << 16) | activePointerId);
            return (pointerWParam, (uint)Environment.TickCount);
        }

        // =========================================================
        // AUTOMATION LOOP
        // =========================================================
        private void AutomationThreadLoop()
        {
            while (!_stopAutomationThread)
            {
                try
                {
                    int now = Environment.TickCount;
                    int elapsed = now - _lastStateChangeTime;

                    if (_rawState == ForgeState.Idle && _server.ClickRequested)
                    {
                        _server.ClickRequested = false;
                        _server.ClickCompleted = false;
                        _server.IsClickActive = true;
                        _rawState = ForgeState.ButtonDown;
                        _lastStateChangeTime = now;
                        QueueMessage("[AutomationLoop] Click requested, State: Idle -> ButtonDown");
                        if (IsCursorOverrideActive) DrawDebugMarker();
                        if (_mainHwnd != IntPtr.Zero)
                        {
                            EnqueueMouseMoveMessages();
                            EnqueueClickPhaseMessages(WM_POINTERDOWN, WM_LBUTTONDOWN, (IntPtr)MK_LBUTTON);
                        }
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
                    if (!_disposing) _server.ReportMessage($"[EXCEPTION in AutomationThreadLoop]\n{ex}");
                }

                Thread.Sleep(1);
            }
        }

        private void EnqueueMouseMoveMessages()
        {
            var (screenPt, clientPt, screenLParam, clientLParam) = ResolveMsgCoordinates();
            var (pointerWParam, now) = ResolvePointerParams();

            lock (_queueLock)
            {
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_POINTERUPDATE, wParam = pointerWParam, lParam = screenLParam, time = now, pt = screenPt });
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_MOUSEMOVE, wParam = IntPtr.Zero, lParam = clientLParam, time = now, pt = screenPt });
            }
            PostMessage(_mainHwnd, WM_NULL, IntPtr.Zero, IntPtr.Zero);
            QueueMessage($"[AutomationLoop] Enqueued mouse move at screen=({screenPt.X},{screenPt.Y}) client=({clientPt.X},{clientPt.Y})");
        }

        private void EnqueueClickPhaseMessages(uint pointerMsg, uint lbuttonMsg, IntPtr lbuttonWParam)
        {
            var (screenPt, clientPt, screenLParam, clientLParam) = ResolveMsgCoordinates();
            var (pointerWParam, now) = ResolvePointerParams();

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
        // NATIVE STRUCT PACKING
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
    }
}
