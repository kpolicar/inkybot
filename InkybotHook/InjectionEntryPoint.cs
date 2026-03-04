using System;
using System.Collections.Concurrent;
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

        private readonly ConcurrentDictionary<string, byte> _loggedFirstCalls = new ConcurrentDictionary<string, byte>();

        // TEMP: hardcoded test position (client coords relative to Dofus top-left) — remove when done
        private static readonly POINT _testPoint = new POINT { X = 1652, Y = 17 };
        private long _lastMarkerDrawTick = 0;
        private long _lastClickTick = 0;
        private long _testClickSequence = 0;

        // Magic handle used as lParam in posted WM_INPUT messages.
        // When GetRawInputData sees this handle, it fabricates a synthetic RAWMOUSE event
        // instead of calling the real API — this is how we trigger Unity's raw-input path
        // without SendInput (which would move the real cursor).
        private static readonly IntPtr MAGIC_RAWINPUT_HANDLE = new IntPtr(0x0DEADBEE);
        
        // Stolen hardware handle from the real physical mouse
        private static IntPtr _validMouseHandle = IntPtr.Zero;

        // Tracks which mouse buttons the bot currently holds down.
        // PostMessage doesn't update the OS key state, so we must fake it
        // in GetAsyncKeyState / GetKeyState / GetKeyboardState.
        private const int VK_LBUTTON = 0x01;
        private const int VK_RBUTTON = 0x02;
        private volatile bool _botLButtonDown = false;
        private volatile bool _botRButtonDown = false;

        // Separate queues for each raw-input API path.
        // Unity uses BOTH GetRawInputData (per-message, from WndProc) and GetRawInputBuffer
        // (batch polling). If we used one queue, GetRawInputData would consume the event
        // before GetRawInputBuffer ever saw it.
        private readonly ConcurrentQueue<ushort> _syntheticRawInputDataQueue = new ConcurrentQueue<ushort>();
        private readonly ConcurrentQueue<ushort> _syntheticRawInputBufferQueue = new ConcurrentQueue<ushort>();

        private int _allowUnsentinelLButtonMessages = 0;

        private void LogFirstCall(string hookName)
        {
            if (_loggedFirstCalls.TryAdd(hookName, 0))
            {
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

        /// <summary>
        /// Posts a synthetic bot click (or release) to the target window.
        /// 1. Updates button-down state for GetAsyncKeyState/GetKeyState hooks
        /// 2. Enqueues a RAWMOUSE button flag and posts WM_INPUT with MAGIC_RAWINPUT_HANDLE
        ///    so Unity's raw-input handler fires GetRawInputData (which we intercept and fabricate)
        /// 3. Posts the legacy WM_LBUTTONDOWN/UP message with sentinel marker
        /// </summary>
        private void PostSyntheticBotClick(IntPtr hwnd, uint buttonMsg, int clientX, int clientY)
        {
            ushort rawFlag;
            long mkFlag;
            switch (buttonMsg)
            {
                case WM_LBUTTONDOWN:
                    _botLButtonDown = true;
                    rawFlag = RI_MOUSE_LEFT_BUTTON_DOWN;
                    mkFlag = MK_LBUTTON;
                    break;
                case WM_LBUTTONUP:
                    _botLButtonDown = false;
                    rawFlag = RI_MOUSE_LEFT_BUTTON_UP;
                    mkFlag = 0;
                    break;
                case WM_RBUTTONDOWN:
                    _botRButtonDown = true;
                    rawFlag = RI_MOUSE_RIGHT_BUTTON_DOWN;
                    mkFlag = MK_RBUTTON;
                    break;
                case WM_RBUTTONUP:
                    _botRButtonDown = false;
                    rawFlag = RI_MOUSE_RIGHT_BUTTON_UP;
                    mkFlag = 0;
                    break;
                default:
                    return;
            }

            // 1. Enqueue synthetic raw-input event to BOTH queues + post WM_INPUT to trigger Unity's handler
            _syntheticRawInputDataQueue.Enqueue(rawFlag);
            _syntheticRawInputBufferQueue.Enqueue(rawFlag);
            bool wmInputPosted = PostMessage(hwnd, WM_INPUT, IntPtr.Zero /*RIM_INPUT*/, MAGIC_RAWINPUT_HANDLE);

            // 2. Post the legacy window message with sentinel + coordinates
            IntPtr lParam = MakeLParam(clientX, clientY);
            IntPtr wParam = (IntPtr)(mkFlag | BOT_INPUT_SENTINEL);
            bool wmButtonPosted = PostMessage(hwnd, buttonMsg, wParam, lParam);

            QueueMessage($"[CLICK-FLOW] PostSyntheticBotClick: msg=0x{buttonMsg:X4} rawFlag=0x{rawFlag:X4} at ({clientX},{clientY}) WM_INPUT_posted={wmInputPosted} WM_BUTTON_posted={wmButtonPosted} dataQ={_syntheticRawInputDataQueue.Count} bufferQ={_syntheticRawInputBufferQueue.Count}");
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
                () => TryInstallHook<GetKeyboardStateDelegate>("GetKeyboardState", new GetKeyboardStateDelegate(HookedGetKeyboardState), out _originalGetKeyboardState),
                () => TryInstallHook<GetMessagePosDelegate>("GetMessagePos", new GetMessagePosDelegate(HookedGetMessagePos), out _originalGetMessagePos)
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
                                    int clientX = unchecked((short)((long)outgoingLParam & 0xFFFF));
                                    int clientY = unchecked((short)(((long)outgoingLParam >> 16) & 0xFFFF));
                                    PostSyntheticBotClick(_server.targetHwnd, outgoingMsg, clientX, clientY);
                                }
                                else
                                {
                                    PostMessage(_server.targetHwnd, outgoingMsg, outgoingWParam, outgoingLParam);
                                }
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
                IntPtr lParam = MakeLParam(_testPoint.X, _testPoint.Y);

                QueueMessage($"[EasyHook:Target] Synthetic click #{clickId} at ({_testPoint.X},{_testPoint.Y})");

                // 1. Move the mouse to update raycasters
                PostMessage(_server.targetHwnd, WM_MOUSEMOVE, IntPtr.Zero, lParam);
                System.Threading.Thread.Sleep(20);

                // 2. Down — posts WM_INPUT(MAGIC) + WM_LBUTTONDOWN(sentinel)
                PostSyntheticBotClick(_server.targetHwnd, WM_LBUTTONDOWN, _testPoint.X, _testPoint.Y);
                QueueMessage($"[EasyHook:Target] Click #{clickId} DOWN posted");

                System.Threading.Thread.Sleep(100);

                // 3. Up — posts WM_INPUT(MAGIC) + WM_LBUTTONUP(sentinel)
                PostSyntheticBotClick(_server.targetHwnd, WM_LBUTTONUP, _testPoint.X, _testPoint.Y);
                QueueMessage($"[EasyHook:Target] Click #{clickId} UP posted");

                System.Threading.Thread.Sleep(50);
                QueueMessage($"[EasyHook:Target] Synthetic click #{clickId} completed");
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
                    if (msg == WM_INPUT)
                    {
                        bool isMagic = lParam == MAGIC_RAWINPUT_HANDLE;
                        QueueMessage($"[CLICK-FLOW] WndProc received WM_INPUT: lParam=0x{lParam.ToInt64():X} isMagic={isMagic} wParam=0x{wParam.ToInt64():X}");
                    }

                    if (msg == WM_LBUTTONDOWN || msg == WM_LBUTTONUP ||
                        msg == WM_RBUTTONDOWN || msg == WM_RBUTTONUP)
                    {
                        bool hasSentinel = ((long)wParam & BOT_INPUT_SENTINEL) != 0;
                        int lx = unchecked((short)((long)lParam & 0xFFFF));
                        int ly = unchecked((short)(((long)lParam >> 16) & 0xFFFF));

                        if (hasSentinel)
                        {
                            wParam = (IntPtr)((long)wParam & ~BOT_INPUT_SENTINEL); 
                            QueueMessage($"[CLICK-FLOW] WndProc ACCEPTED bot click: msg=0x{msg:X4} wParam=0x{wParam.ToInt64():X} pos=({lx},{ly}) -> forwarding to original WndProc");

                            if (msg == WM_LBUTTONDOWN) _botLButtonDown = true;
                            else if (msg == WM_LBUTTONUP) _botLButtonDown = false;
                            else if (msg == WM_RBUTTONDOWN) _botRButtonDown = true;
                            else if (msg == WM_RBUTTONUP) _botRButtonDown = false;
                        }
                        else if ((msg == WM_LBUTTONDOWN || msg == WM_LBUTTONUP) &&
                                 System.Threading.Interlocked.CompareExchange(ref _allowUnsentinelLButtonMessages, 0, 0) > 0)
                        {
                            System.Threading.Interlocked.Decrement(ref _allowUnsentinelLButtonMessages);
                            QueueMessage($"[CLICK-FLOW] WndProc ACCEPTED free-pass click: msg=0x{msg:X4} pos=({lx},{ly})");
                            if (msg == WM_LBUTTONDOWN) _botLButtonDown = true;
                            else if (msg == WM_LBUTTONUP) _botLButtonDown = false;
                        }
                        else
                        {
                            QueueMessage($"[CLICK-FLOW] WndProc BLOCKED real user click: msg=0x{msg:X4} wParam=0x{wParam.ToInt64():X} pos=({lx},{ly})");
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
                // Detect our magic handle — fabricate a synthetic RAWMOUSE event
                if (hRawInput == MAGIC_RAWINPUT_HANDLE)
                {
                    QueueMessage($"[CLICK-FLOW] GetRawInputData: MAGIC handle detected! uiCommand=0x{uiCommand:X} pData={(pData == IntPtr.Zero ? "NULL" : "0x" + pData.ToInt64().ToString("X"))} pcbSize={pcbSize} dataQ={_syntheticRawInputDataQueue.Count}");
                    uint syntheticResult = HandleSyntheticRawInput(uiCommand, pData, ref pcbSize, cbSizeHeader);
                    QueueMessage($"[CLICK-FLOW] GetRawInputData: HandleSyntheticRawInput returned {syntheticResult} (0x{syntheticResult:X})");
                    return syntheticResult;
                }

                uint result = _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
                
                if (pData != IntPtr.Zero && uiCommand == RID_INPUT && result > 0 && result != unchecked((uint)-1))
                {
                    uint dwType = (uint)Marshal.ReadInt32(pData, 0);
                    if (dwType == RIM_TYPEMOUSE)
                    {
                        // Steal the real device handle for use in synthetic events
                        if (_validMouseHandle == IntPtr.Zero) 
                        {
                            _validMouseHandle = Marshal.ReadIntPtr(pData, 8); 
                            QueueMessage($"[EasyHook:Target] Captured real valid hDevice: 0x{_validMouseHandle.ToInt64():X}");
                        }

                        int headerSize = Marshal.SizeOf(typeof(RAWINPUTHEADER));

                        // Zero out mouse movement deltas to keep cursor pinned
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
                QueueMessage($"[EasyHook:Target] GetRawInputData error: {e.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Fabricates a synthetic RAWINPUT (RAWMOUSE) response for our magic WM_INPUT handle.
        /// Dequeues the next button-flag entry from _syntheticRawInputQueue.
        /// </summary>
        private uint HandleSyntheticRawInput(uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            int headerSize = Marshal.SizeOf(typeof(RAWINPUTHEADER));
            const int rawMouseSize = 24; // sizeof(RAWMOUSE) — fixed on all platforms
            uint totalSize = (uint)(headerSize + rawMouseSize);

            // Size query
            if (pData == IntPtr.Zero)
            {
                pcbSize = totalSize;
                return 0;
            }

            // Buffer too small
            if (pcbSize < totalSize)
            {
                pcbSize = totalSize;
                return unchecked((uint)-1);
            }

            // Dequeue the button flags (if queue is empty, use 0 — harmless no-op event)
            ushort buttonFlags;
            _syntheticRawInputDataQueue.TryDequeue(out buttonFlags);

            QueueMessage($"[EasyHook:Target] Fabricating synthetic RAWMOUSE: buttonFlags=0x{buttonFlags:X4}, hDevice=0x{_validMouseHandle.ToInt64():X}");

            // Write RAWINPUTHEADER
            Marshal.WriteInt32(pData, 0, (int)RIM_TYPEMOUSE);           // dwType
            Marshal.WriteInt32(pData, 4, (int)totalSize);               // dwSize
            Marshal.WriteIntPtr(pData, 8, _validMouseHandle);           // hDevice
            Marshal.WriteIntPtr(pData, 8 + IntPtr.Size, IntPtr.Zero);   // wParam (RIM_INPUT = 0)

            // Write RAWMOUSE (24 bytes starting at headerSize)
            // Zero the entire RAWMOUSE region first
            for (int i = 0; i < rawMouseSize; i++)
                Marshal.WriteByte(pData, headerSize + i, 0);

            // usButtonFlags at offset headerSize + 4 (after usFlags[2] + padding[2])
            Marshal.WriteInt16(pData, headerSize + 4, (short)buttonFlags);

            // lLastX, lLastY already zeroed — no cursor movement

            pcbSize = totalSize;
            return totalSize;
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

                case WM_INPUT:
                {
                    bool isMagic = lpMsg.lParam == MAGIC_RAWINPUT_HANDLE;
                    QueueMessage($"[CLICK-FLOW] FilterMessage: WM_INPUT lParam=0x{lpMsg.lParam.ToInt64():X} isMagic={isMagic}");
                    break;
                }

                case WM_LBUTTONDOWN:
                case WM_LBUTTONUP:
                case WM_RBUTTONDOWN:
                case WM_RBUTTONUP:
                    if (IsCursorOverrideActive())
                    {
                        bool hasSentinel = ((long)lpMsg.wParam & BOT_INPUT_SENTINEL) != 0;
                        int fx = unchecked((short)((long)lpMsg.lParam & 0xFFFF));
                        int fy = unchecked((short)(((long)lpMsg.lParam >> 16) & 0xFFFF));

                        // Check if this click has a free pass
                        if ((lpMsg.message == WM_LBUTTONDOWN || lpMsg.message == WM_LBUTTONUP) &&
                            System.Threading.Interlocked.CompareExchange(ref _allowUnsentinelLButtonMessages, 0, 0) > 0)
                        {
                            QueueMessage($"[CLICK-FLOW] FilterMessage ACCEPTED free-pass: msg=0x{lpMsg.message:X4} pos=({fx},{fy})");
                            break; 
                        }
                        // Bot click — let it through WITH sentinel intact.
                        // WndProc will strip the sentinel when it processes this message.
                        else if (hasSentinel)
                        {
                            QueueMessage($"[CLICK-FLOW] FilterMessage PASSING bot click (sentinel kept): msg=0x{lpMsg.message:X4} wParam=0x{lpMsg.wParam.ToInt64():X} pos=({fx},{fy})");
                            break; 
                        }
                        else
                        {
                            QueueMessage($"[CLICK-FLOW] FilterMessage BLOCKED real click: msg=0x{lpMsg.message:X4} pos=({fx},{fy})");
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
                    // The game may get screen coords from sources we don't hook (e.g. cached values,
                    // internal state). Force the result to _testPoint so every ScreenToClient query
                    // for the target window returns our spoofed client position.
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

                int headerSize = (int)cbSizeHeader;
                const int rawMouseSize = 24;
                uint oneEventSize = (uint)(headerSize + rawMouseSize);

                // Process real events: steal device handle + zero out mouse deltas
                if (pData != IntPtr.Zero && result > 0 && result != unchecked((uint)-1))
                {
                    IntPtr current = pData;
                    for (uint i = 0; i < result; i++)
                    {
                        uint dwType = (uint)Marshal.ReadInt32(current, 0);
                        uint dwSize = (uint)Marshal.ReadInt32(current, 4);

                        if (dwType == RIM_TYPEMOUSE)
                        {
                            if (_validMouseHandle == IntPtr.Zero)
                                _validMouseHandle = Marshal.ReadIntPtr(current, 8);

                            Marshal.WriteInt32(current, headerSize + 12, 0); // lLastX
                            Marshal.WriteInt32(current, headerSize + 16, 0); // lLastY
                        }

                        long aligned = ((long)dwSize + 7) & ~7L;
                        current = new IntPtr(current.ToInt64() + aligned);
                    }
                }

                // If we have pending synthetic events, append them to the buffer
                // (Unity uses GetRawInputBuffer to batch-poll, so our posted WM_INPUT
                // may have triggered this call — inject synthetic events here too)
                if (!_syntheticRawInputBufferQueue.IsEmpty && pData != IntPtr.Zero)
                {
                    // Calculate remaining buffer space
                    long alignedOneEvent = ((long)oneEventSize + 7) & ~7L;
                    long usedBytes = 0;
                    if (result > 0 && result != unchecked((uint)-1))
                    {
                        // Walk to find end of existing data
                        IntPtr walk = pData;
                        for (uint i = 0; i < result; i++)
                        {
                            uint dwSize = (uint)Marshal.ReadInt32(walk, 4);
                            long aligned = ((long)dwSize + 7) & ~7L;
                            walk = new IntPtr(walk.ToInt64() + aligned);
                        }
                        usedBytes = walk.ToInt64() - pData.ToInt64();
                    }

                    long remainingBytes = (long)pcbSize - usedBytes;
                    uint appendedCount = (result > 0 && result != unchecked((uint)-1)) ? result : 0;
                    IntPtr writePtr = new IntPtr(pData.ToInt64() + usedBytes);

                    ushort buttonFlags;
                    while (_syntheticRawInputBufferQueue.TryDequeue(out buttonFlags))
                    {
                        if (remainingBytes < alignedOneEvent)
                        {
                            // Re-enqueue — we'll catch it next call
                            _syntheticRawInputBufferQueue.Enqueue(buttonFlags);
                            break;
                        }

                        // Write RAWINPUTHEADER
                        Marshal.WriteInt32(writePtr, 0, (int)RIM_TYPEMOUSE);           // dwType
                        Marshal.WriteInt32(writePtr, 4, (int)oneEventSize);             // dwSize
                        Marshal.WriteIntPtr(writePtr, 8, _validMouseHandle);            // hDevice
                        Marshal.WriteIntPtr(writePtr, 8 + IntPtr.Size, IntPtr.Zero);    // wParam

                        // Write RAWMOUSE — zero everything, then set button flags
                        for (int i = 0; i < rawMouseSize; i++)
                            Marshal.WriteByte(writePtr, headerSize + i, 0);
                        Marshal.WriteInt16(writePtr, headerSize + 4, (short)buttonFlags);

                        QueueMessage($"[EasyHook:Target] GetRawInputBuffer: appended synthetic RAWMOUSE flags=0x{buttonFlags:X4}");

                        writePtr = new IntPtr(writePtr.ToInt64() + alignedOneEvent);
                        remainingBytes -= alignedOneEvent;
                        appendedCount++;
                    }

                    if (appendedCount > 0)
                        return appendedCount;
                }

                // If buffer is empty and we have synthetics queued, handle size query
                if (!_syntheticRawInputBufferQueue.IsEmpty && pData == IntPtr.Zero)
                {
                    if (pcbSize < oneEventSize)
                        pcbSize = oneEventSize;
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

        #region GetMessagePos hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint GetMessagePosDelegate();
        private GetMessagePosDelegate _originalGetMessagePos;

        public uint HookedGetMessagePos()
        {
            LogFirstCall("GetMessagePos");
            try
            {
                if (IsCursorOverrideActive())
                {
                    // Return spoofed screen coords packed as DWORD (low=X, high=Y)
                    var sp = GetTestScreenPoint();
                    return (uint)((sp.Y << 16) | (sp.X & 0xFFFF));
                }
                return _originalGetMessagePos != null ? _originalGetMessagePos() : 0;
            }
            catch
            {
                return 0;
            }
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