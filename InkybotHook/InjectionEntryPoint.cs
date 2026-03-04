using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static InkybotHook.NativeMethods;
#pragma warning disable CS1690 // Accessing a member on a field of a marshal-by-reference class

namespace InkybotHook
{
    public class InjectionEntryPoint : EasyHook.IEntryPoint
    {
        ServerInterface _server = null;

        Queue<string> _messageQueue = new Queue<string>();

        private readonly HashSet<string> _loggedFirstCalls = new HashSet<string>();

        // TEMP: hardcoded test position (client coords relative to Dofus top-left) — remove when done
        private static readonly POINT _testPoint = new POINT { X = 1652, Y = 17 };
        private long _lastMarkerDrawTick = 0;
        private long _lastClickTick = 0;
        private long _testClickSequence = 0;
        private long _lastKeyStateSpoofLogTick = 0;

        // Magic Handle for background raw input
        private const long MAGIC_RAWINPUT_HANDLE = 0xDEADBEEFL;
        
        // Stolen hardware handle from the real physical mouse
        private static IntPtr _validMouseHandle = IntPtr.Zero;

        // Tracks which mouse buttons the bot currently holds down.
        // PostMessage doesn't update the OS key state, so we must fake it
        // in GetAsyncKeyState / GetKeyState / GetKeyboardState.
        private const int VK_LBUTTON = 0x01;
        private const int VK_RBUTTON = 0x02;
        private volatile bool _botLButtonDown = false;
        private volatile bool _botRButtonDown = false;

        // Pending raw-input button transitions.
        // Set BEFORE PostMessage so the next GetRawInputData call injects them
        // into RAWMOUSE.usButtonFlags (Unity reads buttons from raw input, not WM_LBUTTONDOWN).
        private volatile bool _botLButtonPendingDown = false;
        private volatile bool _botLButtonPendingUp = false;
        private volatile bool _botRButtonPendingDown = false;
        private volatile bool _botRButtonPendingUp = false;
        private int _allowUnsentinelLButtonMessages = 0;

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
            string channelName)
        {
            try
            {
                _server = EasyHook.RemoteHooking.IpcConnectClient<ServerInterface>(channelName);
                _server.Ping();
            }
            catch (Exception e)
            {
                throw new Exception("[EasyHook:Target] Failed to connect IPC channel '" + channelName + "': " + e.Message, e);
            }
        }

        public void Run(
            EasyHook.RemoteHooking.IContext context,
            string channelName)
        {
            var installedHooks = new List<EasyHook.LocalHook>();

            var hookInstallers = new List<Func<EasyHook.LocalHook>>
            {
                () => TryInstallHook<GetCursorPosDelegate>("GetCursorPos", new GetCursorPosDelegate(HookedGetCursorPos), out _originalGetCursorPos),
                () => TryInstallHook<IsIconicDelegate>("IsIconic", new IsIconicDelegate(HookedIsIconic), out _originalIsIconic),
                () => TryInstallHook<GetMessageWDelegate>("GetMessageW", new GetMessageWDelegate(HookedGetMessageW), out _originalGetMessageW),
                () => TryInstallHook<PeekMessageWDelegate>("PeekMessageW", new PeekMessageWDelegate(HookedPeekMessageW), out _originalPeekMessageW),
                () => TryInstallHook<GetRawInputDataDelegate>("GetRawInputData", new GetRawInputDataDelegate(HookedGetRawInputData), out _originalGetRawInputData),
                () => TryInstallHook<GetCursorInfoDelegate>("GetCursorInfo", new GetCursorInfoDelegate(HookedGetCursorInfo), out _originalGetCursorInfo),
                () => TryInstallHook<GetRawInputBufferDelegate>("GetRawInputBuffer", new GetRawInputBufferDelegate(HookedGetRawInputBuffer), out _originalGetRawInputBuffer),
                () => TryInstallHook<GetCursorPosDelegate>("GetPhysicalCursorPos", new GetCursorPosDelegate(HookedGetPhysicalCursorPos), out _originalGetPhysicalCursorPos),
                () => TryInstallHook<ClipCursorDelegate>("ClipCursor", new ClipCursorDelegate(HookedClipCursor), out _originalClipCursor),
                () => TryInstallHook<ScreenToClientDelegate>("ScreenToClient", new ScreenToClientDelegate(HookedScreenToClient), out _originalScreenToClient),
                () => TryInstallHook<ClientToScreenDelegate>("ClientToScreen", new ClientToScreenDelegate(HookedClientToScreen), out _originalClientToScreen),
                () => TryInstallHook<GetMessageWDelegate>("GetMessageA", new GetMessageWDelegate(HookedGetMessageA), out _originalGetMessageA),
                () => TryInstallHook<PeekMessageWDelegate>("PeekMessageA", new PeekMessageWDelegate(HookedPeekMessageA), out _originalPeekMessageA),
                () => TryInstallHook<GetForegroundWindowDelegate>("GetForegroundWindow", new GetForegroundWindowDelegate(HookedGetForegroundWindow), out _originalGetForegroundWindow),
                () => TryInstallHook<GetActiveWindowDelegate>("GetActiveWindow", new GetActiveWindowDelegate(HookedGetActiveWindow), out _originalGetActiveWindow),
                () => TryInstallHook<GetFocusDelegate>("GetFocus", new GetFocusDelegate(HookedGetFocus), out _originalGetFocus),
                () => TryInstallHook<GetGUIThreadInfoDelegate>("GetGUIThreadInfo", new GetGUIThreadInfoDelegate(HookedGetGUIThreadInfo), out _originalGetGUIThreadInfo),
                () => TryInstallHook<GetCaptureDelegate>("GetCapture", new GetCaptureDelegate(HookedGetCapture), out _originalGetCapture),
                () => TryInstallHook<GetAsyncKeyStateDelegate>("GetAsyncKeyState", new GetAsyncKeyStateDelegate(HookedGetAsyncKeyState), out _originalGetAsyncKeyState),
                () => TryInstallHook<GetKeyStateDelegate>("GetKeyState", new GetKeyStateDelegate(HookedGetKeyState), out _originalGetKeyState),
                () => TryInstallHook<GetKeyboardStateDelegate>("GetKeyboardState", new GetKeyboardStateDelegate(HookedGetKeyboardState), out _originalGetKeyboardState)
            };

            try
            {
                _server.IsInstalled(EasyHook.RemoteHooking.GetCurrentProcessId());

                foreach (var installHook in hookInstallers)
                {
                    var hook = installHook();
                    if (hook != null)
                        installedHooks.Add(hook);
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
                while (!_server.ShutdownFlag)
                {
                    EnsureWndProcSubclassed();
                    DrawDebugMarkerIfDue();
                    ClickFixedPositionIfDue();

                    if (_server.targetHwnd != IntPtr.Zero)
                    {
                        ServerInterface.InputMessage inputMsg;
                        while (_server.TryDequeueInput(out inputMsg))
                        {
                            try
                            {
                                uint outgoingMsg = inputMsg.Msg;
                                IntPtr outgoingWParam = inputMsg.WParam;
                                IntPtr outgoingLParam = inputMsg.LParam;
                                NormalizeNonClientButtonMessage(_server.targetHwnd, ref outgoingMsg, ref outgoingWParam, ref outgoingLParam);

                                bool isButtonMsg =
                                    outgoingMsg == WM_LBUTTONDOWN || outgoingMsg == WM_LBUTTONUP ||
                                    outgoingMsg == WM_RBUTTONDOWN || outgoingMsg == WM_RBUTTONUP;
                                if (isButtonMsg)
                                {
                                    if (outgoingMsg == WM_LBUTTONDOWN) _botLButtonPendingDown = true;
                                    else if (outgoingMsg == WM_LBUTTONUP) _botLButtonPendingUp = true;
                                    else if (outgoingMsg == WM_RBUTTONDOWN) _botRButtonPendingDown = true;
                                    else if (outgoingMsg == WM_RBUTTONUP) _botRButtonPendingUp = true;
                                }
                                IntPtr wParam = isButtonMsg
                                    ? (IntPtr)((long)outgoingWParam | BOT_INPUT_SENTINEL)
                                    : outgoingWParam;
                                PostMessage(_server.targetHwnd, outgoingMsg, wParam, outgoingLParam);
                                System.Threading.Thread.Sleep(2); 
                            }
                            catch (Exception e)
                            {
                                QueueMessage("[EasyHook:Target] Error posting bot input msg=0x"
                                    + inputMsg.Msg.ToString("X") + ": " + e.Message);
                            }
                        }
                    }

                    System.Threading.Thread.Sleep(1);

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
            catch { }

            try
            {
                foreach (var hook in installedHooks)
                    hook.Dispose();
                RestoreWndProcSubclass();
                EasyHook.LocalHook.Release();
                _server.ReportMessage("[EasyHook:Target] Hooks disposed and released");
                _server.SetState(HookState.Disposed);
            }
            catch { }
        }

        private EasyHook.LocalHook TryInstallHook<TOriginal>(string functionName, Delegate hookedMethod, out TOriginal original)
            where TOriginal : class
        {
            try
            {
                var targetFunction = EasyHook.LocalHook.GetProcAddress("user32.dll", functionName);
                var hook = EasyHook.LocalHook.Create(targetFunction, hookedMethod, this);
                original = Marshal.GetDelegateForFunctionPointer<TOriginal>(targetFunction);
                hook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                return hook;
            }
            catch (Exception)
            {
                original = null;
                return null;
            }
        }

        private void NormalizeNonClientButtonMessage(IntPtr hwnd, ref uint msg, ref IntPtr wParam, ref IntPtr lParam)
        {
            uint mappedMsg;
            long mappedWParamBase;
            switch (msg)
            {
                case WM_NCLBUTTONDOWN:
                    mappedMsg = WM_LBUTTONDOWN;
                    mappedWParamBase = MK_LBUTTON;
                    break;
                case WM_NCLBUTTONUP:
                    mappedMsg = WM_LBUTTONUP;
                    mappedWParamBase = 0;
                    break;
                case WM_NCRBUTTONDOWN:
                    mappedMsg = WM_RBUTTONDOWN;
                    mappedWParamBase = MK_RBUTTON;
                    break;
                case WM_NCRBUTTONUP:
                    mappedMsg = WM_RBUTTONUP;
                    mappedWParamBase = 0;
                    break;
                default:
                    return;
            }

            bool hasSentinel = ((long)wParam & BOT_INPUT_SENTINEL) != 0;
            msg = mappedMsg;
            wParam = (IntPtr)(mappedWParamBase | (hasSentinel ? BOT_INPUT_SENTINEL : 0L));

            if (IsCursorOverrideActive())
            {
                lParam = MakeLParam(_testPoint.X, _testPoint.Y);
                return;
            }

            int screenX = unchecked((short)((long)lParam & 0xFFFF));
            int screenY = unchecked((short)(((long)lParam >> 16) & 0xFFFF));
            var clientPoint = new POINT { X = screenX, Y = screenY };
            if (hwnd != IntPtr.Zero)
            {
                if (_originalScreenToClient != null)
                    _originalScreenToClient(hwnd, ref clientPoint);
                else
                    ScreenToClient(hwnd, ref clientPoint);
            }
            lParam = MakeLParam(clientPoint.X, clientPoint.Y);
        }

        #region Debug marker

        private POINT GetTestScreenPoint()
        {
            var pt = new POINT { X = _testPoint.X, Y = _testPoint.Y };
            try
            {
                IntPtr hwnd = (_server != null) ? _server.targetHwnd : IntPtr.Zero;
                if (hwnd != IntPtr.Zero)
                {
                    if (_originalClientToScreen != null)
                        _originalClientToScreen(hwnd, ref pt);
                    else
                        ClientToScreen(hwnd, ref pt);
                }
            }
            catch { }
            return pt;
        }

        private void DrawDebugMarkerIfDue()
        {
            long now = System.Diagnostics.Stopwatch.GetTimestamp();
            long freq = System.Diagnostics.Stopwatch.Frequency;
            if ((now - _lastMarkerDrawTick) < freq / 2) return;
            _lastMarkerDrawTick = now;
            DrawDebugMarker();
        }

        private void DrawDebugMarker()
        {
            var sp = GetTestScreenPoint();
            int x = sp.X;
            int y = sp.Y;
            const int r = 12;
            const int arm = 20;
            IntPtr hdc = GetDC(IntPtr.Zero);
            if (hdc == IntPtr.Zero) return;
            try
            {
                SetROP2(hdc, R2_NOT);
                IntPtr pen = CreatePen(PS_SOLID, 2, 0x0000FF00);
                IntPtr oldPen = SelectObject(hdc, pen);
                IntPtr oldBrush = SelectObject(hdc, GetStockObject(5 /*NULL_BRUSH*/));

                MoveToEx(hdc, x - arm, y, IntPtr.Zero); LineTo(hdc, x + arm, y);
                MoveToEx(hdc, x, y - arm, IntPtr.Zero); LineTo(hdc, x, y + arm);

                Ellipse(hdc, x - r, y - r, x + r, y + r);

                SelectObject(hdc, oldPen);
                SelectObject(hdc, oldBrush);
                DeleteObject(pen);
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, hdc);
            }
        }

        private void ClickFixedPositionIfDue()
        {
            if (_server == null || _server.targetHwnd == IntPtr.Zero) return;
            long now = System.Diagnostics.Stopwatch.GetTimestamp();
            long freq = System.Diagnostics.Stopwatch.Frequency;
            if ((now - _lastClickTick) < freq) return; 
            _lastClickTick = now;
            
            try
            {
                long clickId = System.Threading.Interlocked.Increment(ref _testClickSequence);
                
                // Get screen coordinates for the test point
                var screenPt = GetTestScreenPoint();
                
                // Normalize to 0-65535 range for absolute coordinates
                int screenWidth = GetSystemMetrics(SM_CXSCREEN);
                int screenHeight = GetSystemMetrics(SM_CYSCREEN);
                int absX = (screenPt.X * 65536) / screenWidth;
                int absY = (screenPt.Y * 65536) / screenHeight;
                
                QueueMessage($"[EasyHook:Target] SendInput click #{clickId} at screen({screenPt.X},{screenPt.Y}) abs({absX},{absY})");

                // Set key state flags so GetAsyncKeyState/GetKeyState return correct values
                _botLButtonDown = true;
                _botLButtonPendingDown = true;

                // 1. Move mouse to position using SendInput
                var moveInput = new INPUT
                {
                    type = INPUT_MOUSE,
                    mi = new MOUSEINPUT
                    {
                        dx = absX,
                        dy = absY,
                        dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE,
                        mouseData = 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                };
                
                uint sent = SendInput(1, new[] { moveInput }, Marshal.SizeOf(typeof(INPUT)));
                QueueMessage($"[EasyHook:Target] Click #{clickId} MOVE sent={sent}, error={Marshal.GetLastWin32Error()}");
                
                System.Threading.Thread.Sleep(16); // One frame
                
                // 2. Mouse down
                var downInput = new INPUT
                {
                    type = INPUT_MOUSE,
                    mi = new MOUSEINPUT
                    {
                        dx = absX,
                        dy = absY,
                        dwFlags = MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_ABSOLUTE,
                        mouseData = 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                };
                
                sent = SendInput(1, new[] { downInput }, Marshal.SizeOf(typeof(INPUT)));
                QueueMessage($"[EasyHook:Target] Click #{clickId} DOWN sent={sent}, error={Marshal.GetLastWin32Error()}");
                
                System.Threading.Thread.Sleep(50); // Hold down briefly
                
                _botLButtonPendingDown = false;
                
                // 3. Mouse up
                _botLButtonDown = false;
                _botLButtonPendingUp = true;
                
                var upInput = new INPUT
                {
                    type = INPUT_MOUSE,
                    mi = new MOUSEINPUT
                    {
                        dx = absX,
                        dy = absY,
                        dwFlags = MOUSEEVENTF_LEFTUP | MOUSEEVENTF_ABSOLUTE,
                        mouseData = 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                };
                
                sent = SendInput(1, new[] { upInput }, Marshal.SizeOf(typeof(INPUT)));
                QueueMessage($"[EasyHook:Target] Click #{clickId} UP sent={sent}, error={Marshal.GetLastWin32Error()}");
                
                System.Threading.Thread.Sleep(16);
                _botLButtonPendingUp = false;

                QueueMessage($"[EasyHook:Target] SendInput click #{clickId} completed");
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] Test click error: {e.Message}");
            }
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
            return true; // TEMP: test override
        }

        private void EnsureWndProcSubclassed()
        {
            try
            {
                if (_server == null || _server.targetHwnd == IntPtr.Zero)
                    return;

                var targetHandle = _server.targetHwnd;

                if (_subclassedWindowHandle == targetHandle && _originalWndProc != IntPtr.Zero)
                    return;

                if (_subclassedWindowHandle != IntPtr.Zero && _originalWndProc != IntPtr.Zero)
                {
                    RestoreWndProcSubclass();
                }

                _subclassedWndProcDelegate = HookedWndProc;
                var newWndProcPointer = Marshal.GetFunctionPointerForDelegate(_subclassedWndProcDelegate);
                var originalWndProc = SetWindowLongPtrW(targetHandle, GWLP_WNDPROC, newWndProcPointer);

                if (originalWndProc == IntPtr.Zero)
                    return;

                _originalWndProc = originalWndProc;
                _subclassedWindowHandle = targetHandle;
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
            try
            {
                NormalizeNonClientButtonMessage(hWnd, ref msg, ref wParam, ref lParam);

                if (IsCursorOverrideActive())
                {
                    if (msg == WM_LBUTTONDOWN || msg == WM_LBUTTONUP ||
                        msg == WM_RBUTTONDOWN || msg == WM_RBUTTONUP)
                    {
                        if (((long)wParam & BOT_INPUT_SENTINEL) != 0)
                        {
                            wParam = (IntPtr)((long)wParam & ~BOT_INPUT_SENTINEL); 

                            if (msg == WM_LBUTTONDOWN) _botLButtonDown = true;
                            else if (msg == WM_LBUTTONUP) _botLButtonDown = false;
                            else if (msg == WM_RBUTTONDOWN) _botRButtonDown = true;
                            else if (msg == WM_RBUTTONUP) _botRButtonDown = false;
                        }
                        else if ((msg == WM_LBUTTONDOWN || msg == WM_LBUTTONUP) &&
                                 System.Threading.Interlocked.CompareExchange(ref _allowUnsentinelLButtonMessages, 0, 0) > 0)
                        {
                            // Consume token
                            System.Threading.Interlocked.Decrement(ref _allowUnsentinelLButtonMessages);
                            if (msg == WM_LBUTTONDOWN) _botLButtonDown = true;
                            else if (msg == WM_LBUTTONUP) _botLButtonDown = false;
                        }
                        else
                        {
                            return IntPtr.Zero; 
                        }
                    }

                    if (msg == WM_MOUSEMOVE)
                    {
                        lParam = MakeLParam(_testPoint.X, _testPoint.Y);
                    }
                }
            }
            catch { }

            try
            {
                if (_originalWndProc != IntPtr.Zero)
                    return CallWindowProcW(_originalWndProc, hWnd, msg, wParam, lParam);
            }
            catch { }

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
                if (!IsCursorOverrideActive())
                    return _originalGetCursorPos(out lpPoint);
                lpPoint = GetTestScreenPoint();
                return true;
            }
            catch
            {
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
            try
            {
                int result = _originalGetMessageW(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
                if (result != 0)
                {
                    FilterMessage(ref lpMsg);
                }
                return result;
            }
            catch
            {
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
            try
            {
                bool result = _originalPeekMessageW(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
                if (result)
                {
                    FilterMessage(ref lpMsg);
                }
                return result;
            }
            catch
            {
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
            try
            {
                uint result = _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
                
                if (pData != IntPtr.Zero && uiCommand == RID_INPUT && result > 0 && result != unchecked((uint)-1))
                {
                    uint dwType = (uint)Marshal.ReadInt32(pData, 0);
                    if (dwType == RIM_TYPEMOUSE)
                    {
                        // STEAL THE REAL DEVICE HANDLE
                        if (_validMouseHandle == IntPtr.Zero) 
                        {
                            _validMouseHandle = Marshal.ReadIntPtr(pData, 8); 
                            QueueMessage($"[EasyHook:Target] Captured real valid hDevice: 0x{_validMouseHandle.ToInt64():X}");
                        }

                        int headerSize = Marshal.SizeOf(typeof(RAWINPUTHEADER));

                        // Zero out mouse movement deltas (cursor is spoofed elsewhere)
                        int lLastXOffset = headerSize + 12;
                        int lLastYOffset = headerSize + 16;
                        Marshal.WriteInt32(pData, lLastXOffset, 0);
                        Marshal.WriteInt32(pData, lLastYOffset, 0);

                        // Inject any pending button flags from our state
                        InjectBotButtonFlags(pData, headerSize);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetRawInputData error: {e.Message}");
                return 0;
            }
        }

        private void InjectBotButtonFlags(IntPtr pRawInput, int headerSize)
        {
            if (!_botLButtonPendingDown && !_botLButtonPendingUp &&
                !_botRButtonPendingDown && !_botRButtonPendingUp)
                return;

            QueueMessage($"[EasyHook:Target] InjectBotButtonFlags: L_DOWN={_botLButtonPendingDown}, L_UP={_botLButtonPendingUp}, R_DOWN={_botRButtonPendingDown}, R_UP={_botRButtonPendingUp}");

            int usButtonFlagsOffset = headerSize + 4;
            ushort flags = (ushort)Marshal.ReadInt16(pRawInput, usButtonFlagsOffset);

            // DO NOT clear the flags here. Let both Unity APIs read the exact same state!
            if (_botLButtonPendingDown) { flags |= RI_MOUSE_LEFT_BUTTON_DOWN; }
            if (_botLButtonPendingUp)   { flags |= RI_MOUSE_LEFT_BUTTON_UP; }
            if (_botRButtonPendingDown) { flags |= RI_MOUSE_RIGHT_BUTTON_DOWN; }
            if (_botRButtonPendingUp)   { flags |= RI_MOUSE_RIGHT_BUTTON_UP; }

            Marshal.WriteInt16(pRawInput, usButtonFlagsOffset, (short)flags);
        }

        #endregion

        #region GetCursorInfo hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool GetCursorInfoDelegate(ref CURSORINFO pci);
        private GetCursorInfoDelegate _originalGetCursorInfo;

        public bool HookedGetCursorInfo(ref CURSORINFO pci)
        {
            LogFirstCall("GetCursorInfo");
            try
            {
                bool result = _originalGetCursorInfo(ref pci);
                if (result && IsCursorOverrideActive())
                {
                    var sp = GetTestScreenPoint();
                    pci.ptScreenPos.X = sp.X;
                    pci.ptScreenPos.Y = sp.Y;
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Shared message filter

        private void FilterMessage(ref MSG lpMsg)
        {
            NormalizeNonClientButtonMessage(lpMsg.hwnd, ref lpMsg.message, ref lpMsg.wParam, ref lpMsg.lParam);

            switch (lpMsg.message)
            {
                case WM_MOUSEMOVE:
                {
                    if (!IsCursorOverrideActive()) break;
                    lpMsg.lParam = MakeLParam(_testPoint.X, _testPoint.Y);
                    var sp = GetTestScreenPoint();
                    lpMsg.pt = new POINT { X = sp.X, Y = sp.Y };
                    break;
                }
                case WM_KILLFOCUS:
                    lpMsg.message = WM_SETFOCUS;
                    lpMsg.wParam = IntPtr.Zero;
                    break;

                case WM_ACTIVATE:
                {
                    uint loWord = (uint)((long)lpMsg.wParam & 0xFFFF);
                    if (loWord == WA_INACTIVE)
                    {
                        lpMsg.wParam = (IntPtr)WA_ACTIVE;
                    }
                    break;
                }

                case WM_ACTIVATEAPP:
                case WM_NCACTIVATE:
                    if (lpMsg.wParam == IntPtr.Zero)
                    {
                        lpMsg.wParam = (IntPtr)1;
                    }
                    break;

                case WM_LBUTTONDOWN:
                case WM_LBUTTONUP:
                case WM_RBUTTONDOWN:
                case WM_RBUTTONUP:
                    if (IsCursorOverrideActive())
                    {
                        // Check if this click has a free pass
                        if ((lpMsg.message == WM_LBUTTONDOWN || lpMsg.message == WM_LBUTTONUP) &&
                            System.Threading.Interlocked.CompareExchange(ref _allowUnsentinelLButtonMessages, 0, 0) > 0)
                        {
                            break; 
                        }
                        // Legacy support for IPC bot clicks
                        else if (((long)lpMsg.wParam & BOT_INPUT_SENTINEL) != 0)
                        {
                            break; 
                        }
                        else
                        {
                            // Real user click — suppress
                            lpMsg.message = WM_NULL;
                        }
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
            try
            {
                if (IsCursorOverrideActive() && _server.targetHwnd != IntPtr.Zero && hWnd == _server.targetHwnd)
                {
                    lpPoint = new POINT { X = _testPoint.X, Y = _testPoint.Y };
                    return true;
                }
                return _originalScreenToClient != null && _originalScreenToClient(hWnd, ref lpPoint);
            }
            catch
            {
                return false;
            }
        }

        public bool HookedClientToScreen(IntPtr hWnd, ref POINT lpPoint)
        {
            LogFirstCall("ClientToScreen");
            try
            {
                if (IsCursorOverrideActive() && _server.targetHwnd != IntPtr.Zero && hWnd == _server.targetHwnd)
                {
                    lpPoint = new POINT { X = _testPoint.X, Y = _testPoint.Y };
                    return _originalClientToScreen != null && _originalClientToScreen(hWnd, ref lpPoint);
                }
                return _originalClientToScreen != null && _originalClientToScreen(hWnd, ref lpPoint);
            }
            catch
            {
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
            try
            {
                if (pgui.cbSize == 0)
                    pgui.cbSize = (uint)Marshal.SizeOf(typeof(GUITHREADINFO));

                bool result = false;
                if (_originalGetGUIThreadInfo != null)
                    result = _originalGetGUIThreadInfo(idThread, ref pgui);

                if (IsCursorOverrideActive() && _server.targetHwnd != IntPtr.Zero)
                {
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
            catch
            {
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
            try
            {
                if (IsCursorOverrideActive() && _server.targetHwnd != IntPtr.Zero)
                    return _server.targetHwnd;
                return _originalGetCapture != null ? _originalGetCapture() : IntPtr.Zero;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        public short HookedGetAsyncKeyState(int vKey)
        {
            LogFirstCall("GetAsyncKeyState");
            try
            {
                if (vKey == VK_LBUTTON && _botLButtonDown) return unchecked((short)0x8000);
                if (vKey == VK_RBUTTON && _botRButtonDown) return unchecked((short)0x8000);

                if (_originalGetAsyncKeyState != null)
                    return _originalGetAsyncKeyState(vKey);
            }
            catch { }
            return 0;
        }

        public short HookedGetKeyState(int nVirtKey)
        {
            LogFirstCall("GetKeyState");
            try
            {
                if (nVirtKey == VK_LBUTTON && _botLButtonDown) return unchecked((short)0x8000);
                if (nVirtKey == VK_RBUTTON && _botRButtonDown) return unchecked((short)0x8000);

                if (_originalGetKeyState != null)
                    return _originalGetKeyState(nVirtKey);
            }
            catch { }
            return 0;
        }

        public bool HookedGetKeyboardState(IntPtr lpKeyState)
        {
            LogFirstCall("GetKeyboardState");
            try
            {
                bool result = false;
                if (_originalGetKeyboardState != null)
                    result = _originalGetKeyboardState(lpKeyState);

                if (IsCursorOverrideActive() && lpKeyState != IntPtr.Zero)
                {
                    if (!result)
                    {
                        for (int i = 0; i < 256; i++)
                            Marshal.WriteByte(lpKeyState, i, 0);
                    }

                    byte left = Marshal.ReadByte(lpKeyState, VK_LBUTTON);
                    byte right = Marshal.ReadByte(lpKeyState, VK_RBUTTON);

                    left = _botLButtonDown ? (byte)(left | 0x80) : (byte)(left & 0x7F);
                    right = _botRButtonDown ? (byte)(right | 0x80) : (byte)(right & 0x7F);

                    Marshal.WriteByte(lpKeyState, VK_LBUTTON, left);
                    Marshal.WriteByte(lpKeyState, VK_RBUTTON, right);
                    return true;
                }
                return result;
            }
            catch { }
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
            try
            {
                if (_server.targetHwnd != IntPtr.Zero) return _server.targetHwnd;
                return _originalGetForegroundWindow();
            }
            catch
            {
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
            try
            {
                if (_server.targetHwnd != IntPtr.Zero) return _server.targetHwnd;
                return _originalGetActiveWindow();
            }
            catch
            {
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
            try
            {
                if (_server.targetHwnd != IntPtr.Zero) return _server.targetHwnd;
                return _originalGetFocus();
            }
            catch
            {
                return IntPtr.Zero;
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
            try
            {
                uint result = _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);

                if (!_botLButtonPendingDown && !_botLButtonPendingUp &&
                    !_botRButtonPendingDown && !_botRButtonPendingUp)
                {
                    return result;
                }

                int headerSize = (int)cbSizeHeader; 
                uint requiredSizeForOneEvent = (uint)(headerSize + 24); 

                if (pData == IntPtr.Zero)
                {
                    if (result == 0 && pcbSize < requiredSizeForOneEvent)
                        pcbSize = requiredSizeForOneEvent;
                    return result; 
                }

                if (result == 0)
                {
                    if (pcbSize >= requiredSizeForOneEvent)
                    {
                        Marshal.WriteInt32(pData, 0, (int)RIM_TYPEMOUSE); 
                        Marshal.WriteInt32(pData, 4, (int)requiredSizeForOneEvent); 
                        // INJECT REAL STOLEN DEVICE HANDLE
                        Marshal.WriteIntPtr(pData, 8, _validMouseHandle); 
                        Marshal.WriteIntPtr(pData, 8 + IntPtr.Size, IntPtr.Zero); 

                        for (int i = headerSize; i < headerSize + 24; i++)
                            Marshal.WriteByte(pData, i, 0);

                        InjectBotButtonFlags(pData, headerSize);
                        return 1; 
                    }
                    else
                    {
                        pcbSize = requiredSizeForOneEvent;
                        return unchecked((uint)-1);
                    }
                }

                if (result > 0 && result != unchecked((uint)-1))
                {
                    IntPtr current = pData;
                    for (uint i = 0; i < result; i++)
                    {
                        uint dwType = (uint)Marshal.ReadInt32(current, 0);
                        uint dwSize = (uint)Marshal.ReadInt32(current, 4);

                        if (dwType == RIM_TYPEMOUSE)
                        {
                            // STEAL THE REAL DEVICE HANDLE
                            if (_validMouseHandle == IntPtr.Zero) 
                            {
                                _validMouseHandle = Marshal.ReadIntPtr(current, 8); 
                            }

                            int lLastXOffset = headerSize + 12;
                            int lLastYOffset = headerSize + 16;
                            Marshal.WriteInt32(current, lLastXOffset, 0);
                            Marshal.WriteInt32(current, lLastYOffset, 0);

                            InjectBotButtonFlags(current, headerSize);
                        }

                        long aligned = ((long)dwSize + 7) & ~7L;
                        current = new IntPtr(current.ToInt64() + aligned);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetRawInputBuffer error: {e.Message}");
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
                if (!IsCursorOverrideActive())
                    return _originalGetPhysicalCursorPos(out lpPoint);
                lpPoint = GetTestScreenPoint();
                return true;
            }
            catch
            {
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
            try { return true; } catch { return false; }
        }

        #endregion

        #region GetMessageA / PeekMessageA hooks (ANSI variants)

        private GetMessageWDelegate _originalGetMessageA;
        private PeekMessageWDelegate _originalPeekMessageA;

        public int HookedGetMessageA(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
        {
            LogFirstCall("GetMessageA");
            try
            {
                int result = _originalGetMessageA(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
                if (result != 0) FilterMessage(ref lpMsg);
                return result;
            }
            catch { lpMsg = new MSG(); return 0; }
        }

        public bool HookedPeekMessageA(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            LogFirstCall("PeekMessageA");
            try
            {
                bool result = _originalPeekMessageA(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
                if (result) FilterMessage(ref lpMsg);
                return result;
            }
            catch { lpMsg = new MSG(); return false; }
        }

        #endregion
    }
}