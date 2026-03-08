using System;
using System.Collections.Generic;
using System.Threading;
using static InkybotHook.NativeMethods;
#pragma warning disable CS1690

namespace InkybotHook
{
    public partial class InjectionEntryPoint
    {
        // =============================================================
        // CONSTANTS
        // =============================================================

        private const int MAGIC_RAW_HANDLE      = 0x1337;
        private const int MAGIC_RAW_MOVE_HANDLE = 0x1338;
        private const int MAGIC_KEY_HANDLE      = 0x1339;

        // =============================================================
        // CLICK STATE MACHINE
        // =============================================================

        private enum ForgeState { Idle, ButtonDown, ButtonUp }

        private volatile ForgeState _rawState = ForgeState.Idle;
        private int _lastStateChangeTime = 0;

        private IntPtr _mainHwnd = IntPtr.Zero;
        private uint _capturedPointerId = 0;

        private volatile int _targetScreenX;
        private volatile int _targetScreenY;

        private Queue<MSG> _syntheticMessages = new Queue<MSG>();
        private readonly object _queueLock = new object();

        // =============================================================
        // STATE QUERIES
        // =============================================================

        private bool IsIdleWithPendingClick =>
            _rawState == ForgeState.Idle && _server.clickRequested;

        private bool IsIdleWithPendingKey =>
            _rawState == ForgeState.Idle && !_server.clickRequested && _server.keyRequested;

        private bool IsClickHeldLongEnough(int now) =>
            _rawState == ForgeState.ButtonDown && (now - _lastStateChangeTime >= 50);

        private bool IsReleaseComplete(int now) =>
            _rawState == ForgeState.ButtonUp && (now - _lastStateChangeTime >= 100);

        private bool IsForging =>
            _rawState == ForgeState.ButtonDown || _rawState == ForgeState.ButtonUp;

        private uint CurrentButtonFlag =>
            (_rawState == ForgeState.ButtonDown) ? RI_MOUSE_LEFT_BUTTON_DOWN : RI_MOUSE_LEFT_BUTTON_UP;

        // =============================================================
        // INPUT PROCESSOR LOOP
        // =============================================================

        private void InputProcessorLoop()
        {
            while (!_stopInputThread)
            {
                try
                {
                    int now = Environment.TickCount;

                    if (IsIdleWithPendingClick)
                        BeginClick(now);
                    else if (IsClickHeldLongEnough(now))
                        ReleaseClick(now);
                    else if (IsReleaseComplete(now))
                        ReturnToIdle(now);
                    else if (IsIdleWithPendingKey)
                        EnqueueKeyPress(now);
                }
                catch (Exception ex)
                {
                    _server.ReportMessage($"[EXCEPTION in InputProcessorLoop]\n{ex}");
                }

                Thread.Sleep(1);
            }
        }

        // =============================================================
        // CLICK PROCESSING
        // =============================================================

        private void BeginClick(int now)
        {
            int screenX = _server.clickScreenX;
            int screenY = _server.clickScreenY;
            _server.clickRequested = false;

            _targetScreenX = screenX;
            _targetScreenY = screenY;
            _rawState = ForgeState.ButtonDown;
            _lastStateChangeTime = now;

            if (_mainHwnd == IntPtr.Zero) return;

            SetCapture(_mainHwnd);
            EnqueueMouseDown(screenX, screenY, (uint)now);
            WakeGameMessageLoop();
        }

        private void ReleaseClick(int now)
        {
            _rawState = ForgeState.ButtonUp;
            _lastStateChangeTime = now;

            if (_mainHwnd == IntPtr.Zero) return;

            _originalReleaseCapture?.Invoke();
            EnqueueMouseUp(_targetScreenX, _targetScreenY, (uint)now);
            WakeGameMessageLoop();
        }

        private void ReturnToIdle(int now)
        {
            _rawState = ForgeState.Idle;
            _lastStateChangeTime = now;
        }

        // =============================================================
        // KEY PROCESSING
        // =============================================================

        private void EnqueueKeyPress(int now)
        {
            char c = _server.keyChar;
            _server.keyRequested = false;

            if (_mainHwnd == IntPtr.Zero) return;

            lock (_queueLock)
            {
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_KEYDOWN, wParam = (IntPtr)c, lParam = (IntPtr)MAGIC_KEY_HANDLE, time = (uint)now });
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_CHAR,    wParam = (IntPtr)c, lParam = IntPtr.Zero,                time = (uint)now });
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_KEYUP,   wParam = (IntPtr)c, lParam = (IntPtr)MAGIC_KEY_HANDLE, time = (uint)now });
            }
            WakeGameMessageLoop();
        }

        // =============================================================
        // SYNTHETIC MESSAGE HELPERS
        // =============================================================

        private void EnqueueMouseDown(int screenX, int screenY, uint time)
        {
            var screenPt = new POINT { X = screenX, Y = screenY };
            var clientPt = ScreenToClientPoint(_mainHwnd, screenPt);
            IntPtr clientLParam = PackPointToLParam(clientPt);
            IntPtr screenLParam = PackPointToLParam(screenPt);
            uint pointerId = _capturedPointerId == 0 ? 1u : _capturedPointerId;
            IntPtr pointerWParam = (IntPtr)((0x0016 << 16) | pointerId);

            lock (_queueLock)
            {
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_INPUT,         wParam = IntPtr.Zero,           lParam = (IntPtr)MAGIC_RAW_MOVE_HANDLE, time = time, pt = screenPt });
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_POINTERUPDATE,  wParam = pointerWParam,         lParam = screenLParam,                  time = time, pt = screenPt });
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_MOUSEMOVE,      wParam = IntPtr.Zero,           lParam = clientLParam,                  time = time, pt = screenPt });
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_INPUT,         wParam = IntPtr.Zero,           lParam = (IntPtr)MAGIC_RAW_HANDLE,      time = time, pt = screenPt });
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_POINTERDOWN,    wParam = pointerWParam,         lParam = screenLParam,                  time = time, pt = screenPt });
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_LBUTTONDOWN,    wParam = (IntPtr)MK_LBUTTON,    lParam = clientLParam,                  time = time, pt = clientPt });
            }
        }

        private void EnqueueMouseUp(int screenX, int screenY, uint time)
        {
            var screenPt = new POINT { X = screenX, Y = screenY };
            var clientPt = ScreenToClientPoint(_mainHwnd, screenPt);
            IntPtr clientLParam = PackPointToLParam(clientPt);
            IntPtr screenLParam = PackPointToLParam(screenPt);
            uint pointerId = _capturedPointerId == 0 ? 1u : _capturedPointerId;
            IntPtr pointerWParam = (IntPtr)((0x0002 << 16) | pointerId);

            lock (_queueLock)
            {
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_INPUT,         wParam = IntPtr.Zero,       lParam = (IntPtr)MAGIC_RAW_MOVE_HANDLE, time = time, pt = screenPt });
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_POINTERUPDATE,  wParam = pointerWParam,     lParam = screenLParam,                  time = time, pt = screenPt });
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_MOUSEMOVE,      wParam = IntPtr.Zero,       lParam = clientLParam,                  time = time, pt = screenPt });
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_INPUT,         wParam = IntPtr.Zero,       lParam = (IntPtr)MAGIC_RAW_HANDLE,      time = time, pt = screenPt });
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_POINTERUP,      wParam = pointerWParam,     lParam = screenLParam,                  time = time, pt = screenPt });
                _syntheticMessages.Enqueue(new MSG { hwnd = _mainHwnd, message = WM_LBUTTONUP,      wParam = IntPtr.Zero,       lParam = clientLParam,                  time = time, pt = clientPt });
            }
        }

        private static POINT ScreenToClientPoint(IntPtr hwnd, POINT screenPt)
        {
            POINT clientPt = screenPt;
            ScreenToClient(hwnd, ref clientPt);
            return clientPt;
        }

        private static IntPtr PackPointToLParam(POINT pt) =>
            (IntPtr)((uint)((pt.Y << 16) | (pt.X & 0xFFFF)));

        private void WakeGameMessageLoop() =>
            PostMessage(_mainHwnd, WM_NULL, IntPtr.Zero, IntPtr.Zero);

        // =============================================================
        // CAPTURE HOOKS
        // =============================================================

        private bool HookedReleaseCapture()
        {
            _allHooksInstalled.Wait();
            if (_rawState == ForgeState.ButtonDown)
                return true;
            return _originalReleaseCapture();
        }

        private IntPtr HookedGetCapture()
        {
            _allHooksInstalled.Wait();
            if (_rawState == ForgeState.ButtonDown && _mainHwnd != IntPtr.Zero)
                return _mainHwnd;
            return _originalGetCapture();
        }
    }
}
