using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static InkybotHook.NativeMethods;
#pragma warning disable CS1690 // Accessing a member on a field of a marshal-by-reference class

namespace InkybotHook
{
    
    public class InjectionEntryPoint: EasyHook.IEntryPoint
    {
        ServerInterface _server = null;

        Queue<string> _messageQueue = new Queue<string>();

        private readonly HashSet<string> _loggedFirstCalls = new HashSet<string>();

        // TEMP: hardcoded test position (client coords relative to Dofus top-left) — remove when done
        private static readonly POINT _testPoint = new POINT { X = 1652, Y = 17 };
        private long _lastMarkerDrawTick = 0;
        private long _lastClickTick = 0;

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
            EasyHook.LocalHook getRawInputBufferHook = null;
            EasyHook.LocalHook getForegroundWindowHook = null;
            EasyHook.LocalHook getActiveWindowHook = null;
            EasyHook.LocalHook getFocusHook = null;
            EasyHook.LocalHook getPhysicalCursorPosHook = null;
            EasyHook.LocalHook clipCursorHook = null;
            EasyHook.LocalHook getMessageAHook = null;
            EasyHook.LocalHook peekMessageAHook = null;
            EasyHook.LocalHook screenToClientHook = null;
            EasyHook.LocalHook clientToScreenHook = null;
            EasyHook.LocalHook getGuiThreadInfoHook = null;
            EasyHook.LocalHook getCaptureHook = null;
            EasyHook.LocalHook getAsyncKeyStateHook = null;
            EasyHook.LocalHook getKeyStateHook = null;
            EasyHook.LocalHook getKeyboardStateHook = null;

            try
            {
                // Injection is now complete and the server interface is connected
                _server.IsInstalled(EasyHook.RemoteHooking.GetCurrentProcessId());

                getCursorPosHook    = TryInstallHook<GetCursorPosDelegate>("GetCursorPos",    new GetCursorPosDelegate(HookedGetCursorPos),       out _originalGetCursorPos);

                isIconicHook           = TryInstallHook<IsIconicDelegate>("IsIconic",             new IsIconicDelegate(HookedIsIconic),               out _originalIsIconic);
                getMessageWHook        = TryInstallHook<GetMessageWDelegate>("GetMessageW",         new GetMessageWDelegate(HookedGetMessageW),          out _originalGetMessageW);
                peekMessageWHook       = TryInstallHook<PeekMessageWDelegate>("PeekMessageW",       new PeekMessageWDelegate(HookedPeekMessageW),        out _originalPeekMessageW);
                getRawInputDataHook    = TryInstallHook<GetRawInputDataDelegate>("GetRawInputData", new GetRawInputDataDelegate(HookedGetRawInputData),  out _originalGetRawInputData);
                getCursorInfoHook      = TryInstallHook<GetCursorInfoDelegate>("GetCursorInfo",     new GetCursorInfoDelegate(HookedGetCursorInfo),      out _originalGetCursorInfo);
                getRawInputBufferHook  = TryInstallHook<GetRawInputBufferDelegate>("GetRawInputBuffer", new GetRawInputBufferDelegate(HookedGetRawInputBuffer), out _originalGetRawInputBuffer);
                getPhysicalCursorPosHook = TryInstallHook<GetCursorPosDelegate>("GetPhysicalCursorPos", new GetCursorPosDelegate(HookedGetPhysicalCursorPos), out _originalGetPhysicalCursorPos);
                clipCursorHook         = TryInstallHook<ClipCursorDelegate>("ClipCursor",           new ClipCursorDelegate(HookedClipCursor),            out _originalClipCursor);
                screenToClientHook     = TryInstallHook<ScreenToClientDelegate>("ScreenToClient",   new ScreenToClientDelegate(HookedScreenToClient),    out _originalScreenToClient);
                clientToScreenHook     = TryInstallHook<ClientToScreenDelegate>("ClientToScreen",   new ClientToScreenDelegate(HookedClientToScreen),    out _originalClientToScreen);
                getMessageAHook        = TryInstallHook<GetMessageWDelegate>("GetMessageA",         new GetMessageWDelegate(HookedGetMessageA),          out _originalGetMessageA);
                peekMessageAHook       = TryInstallHook<PeekMessageWDelegate>("PeekMessageA",       new PeekMessageWDelegate(HookedPeekMessageA),        out _originalPeekMessageA);
                getForegroundWindowHook = TryInstallHook<GetForegroundWindowDelegate>("GetForegroundWindow", new GetForegroundWindowDelegate(HookedGetForegroundWindow), out _originalGetForegroundWindow);
                getActiveWindowHook    = TryInstallHook<GetActiveWindowDelegate>("GetActiveWindow", new GetActiveWindowDelegate(HookedGetActiveWindow),  out _originalGetActiveWindow);
                getFocusHook           = TryInstallHook<GetFocusDelegate>("GetFocus",               new GetFocusDelegate(HookedGetFocus),                out _originalGetFocus);
                getGuiThreadInfoHook   = TryInstallHook<GetGUIThreadInfoDelegate>("GetGUIThreadInfo", new GetGUIThreadInfoDelegate(HookedGetGUIThreadInfo), out _originalGetGUIThreadInfo);
                getCaptureHook         = TryInstallHook<GetCaptureDelegate>("GetCapture",           new GetCaptureDelegate(HookedGetCapture),            out _originalGetCapture);
                getAsyncKeyStateHook   = TryInstallHook<GetAsyncKeyStateDelegate>("GetAsyncKeyState", new GetAsyncKeyStateDelegate(HookedGetAsyncKeyState), out _originalGetAsyncKeyState);
                getKeyStateHook        = TryInstallHook<GetKeyStateDelegate>("GetKeyState",         new GetKeyStateDelegate(HookedGetKeyState),          out _originalGetKeyState);
                getKeyboardStateHook   = TryInstallHook<GetKeyboardStateDelegate>("GetKeyboardState", new GetKeyboardStateDelegate(HookedGetKeyboardState), out _originalGetKeyboardState);

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
                    EnsureWndProcSubclassed();
                    DrawDebugMarkerIfDue();
                    ClickFixedPositionIfDue();

                    // Dispatch bot input actions via PostMessage so they land on the game's
                    // UI thread message queue rather than being called directly from here.
                    // Button messages are stamped with BOT_INPUT_SENTINEL so FilterMessage
                    // and HookedWndProc can distinguish bot clicks from real user input.
                    if (_server.targetHwnd != IntPtr.Zero)
                    {
                        ServerInterface.InputMessage inputMsg;
                        while (_server.TryDequeueInput(out inputMsg))
                        {
                            try
                            {
                                bool isButtonMsg =
                                    inputMsg.Msg == WM_LBUTTONDOWN || inputMsg.Msg == WM_LBUTTONUP ||
                                    inputMsg.Msg == WM_RBUTTONDOWN || inputMsg.Msg == WM_RBUTTONUP;
                                IntPtr wParam = isButtonMsg
                                    ? (IntPtr)((long)inputMsg.WParam | BOT_INPUT_SENTINEL)
                                    : inputMsg.WParam;
                                PostMessage(_server.targetHwnd, inputMsg.Msg, wParam, inputMsg.LParam);
                                System.Threading.Thread.Sleep(2); // let the posted message get processed
                                if (isButtonMsg)
                                {
                                    string msgName =
                                        inputMsg.Msg == WM_LBUTTONDOWN ? "WM_LBUTTONDOWN" :
                                        inputMsg.Msg == WM_LBUTTONUP   ? "WM_LBUTTONUP"   :
                                        inputMsg.Msg == WM_RBUTTONDOWN ? "WM_RBUTTONDOWN" :
                                                                          "WM_RBUTTONUP";
                                    int lp = inputMsg.LParam.ToInt32();
                                    int cx = lp & 0xFFFF;
                                    int cy = (lp >> 16) & 0xFFFF;
                                    QueueMessage($"[EasyHook:Target] Bot input: {msgName} at client ({cx}, {cy})");
                                }
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
                getRawInputBufferHook?.Dispose();
                getForegroundWindowHook?.Dispose();
                getActiveWindowHook?.Dispose();
                getFocusHook?.Dispose();
                getPhysicalCursorPosHook?.Dispose();
                clipCursorHook?.Dispose();
                getMessageAHook?.Dispose();
                peekMessageAHook?.Dispose();
                screenToClientHook?.Dispose();
                clientToScreenHook?.Dispose();
                getGuiThreadInfoHook?.Dispose();
                getCaptureHook?.Dispose();
                getAsyncKeyStateHook?.Dispose();
                getKeyStateHook?.Dispose();
                getKeyboardStateHook?.Dispose();
                RestoreWndProcSubclass();
                EasyHook.LocalHook.Release();
                _server.ReportMessage("[EasyHook:Target] Hooks disposed and released");
                _server.SetState(HookState.Disposed);
            }
            catch
            {
                // Host may already be gone, swallow
            }
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
                _server.ReportMessage("[EasyHook:Target] " + functionName + " hook installed successfully");
                return hook;
            }
            catch (Exception e)
            {
                _server.ReportMessage("[EasyHook:Target] " + functionName + " hook FAILED: " + e.Message);
                original = null;
                return null;
            }
        }

        #region Debug marker

        /// <summary>Converts _testPoint (client) to screen coords.</summary>
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
            // Redraw every 500 ms — the R2_NOT mode makes each call toggle visibility
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
                // R2_NOT inverts whatever is under the pen — drawing twice restores original
                SetROP2(hdc, R2_NOT);
                IntPtr pen = CreatePen(PS_SOLID, 2, 0x0000FF00); // green — colour doesn't matter with R2_NOT
                IntPtr oldPen = SelectObject(hdc, pen);
                IntPtr oldBrush = SelectObject(hdc, GetStockObject(5 /*NULL_BRUSH*/));

                // Crosshair
                MoveToEx(hdc, x - arm, y, IntPtr.Zero); LineTo(hdc, x + arm, y);
                MoveToEx(hdc, x, y - arm, IntPtr.Zero); LineTo(hdc, x, y + arm);

                // Circle
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
            if ((now - _lastClickTick) < freq) return; // 1 second interval
            _lastClickTick = now;
            try
            {
                IntPtr lParam = MakeLParam(_testPoint.X, _testPoint.Y);
                PostMessage(_server.targetHwnd, WM_LBUTTONDOWN, (IntPtr)(BOT_INPUT_SENTINEL | 0x0001L), lParam);
                System.Threading.Thread.Sleep(50);
                PostMessage(_server.targetHwnd, WM_LBUTTONUP,   (IntPtr)BOT_INPUT_SENTINEL,             lParam);
                QueueMessage($"[EasyHook:Target] Test click at client ({_testPoint.X}, {_testPoint.Y})");
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
            if (_server == null) return false;
            return _server.point.X != -1 || _server.point.Y != -1;
        }

        private void EnsureWndProcSubclassed()
        {
            try
            {
                if (_server == null)
                {
                    QueueMessage("[EasyHook:Target] EnsureWndProcSubclassed: server is null, skipping");
                    return;
                }

                if (_server.targetHwnd == IntPtr.Zero)
                {
                    //QueueMessage("[EasyHook:Target] EnsureWndProcSubclassed: targetHwnd is Zero, skipping");
                    return;
                }

                var targetHandle = _server.targetHwnd;

                if (_subclassedWindowHandle == targetHandle && _originalWndProc != IntPtr.Zero)
                {
                    // Already subclassed for this handle — no action needed
                    return;
                }

                if (_subclassedWindowHandle != IntPtr.Zero && _originalWndProc != IntPtr.Zero)
                {
                    QueueMessage("[EasyHook:Target] EnsureWndProcSubclassed: target handle changed (old=0x" + _subclassedWindowHandle.ToString("X") + ", new=0x" + targetHandle.ToString("X") + "), restoring previous subclass");
                    RestoreWndProcSubclass();
                }

                QueueMessage("[EasyHook:Target] EnsureWndProcSubclassed: installing WndProc subclass on hwnd=0x" + targetHandle.ToString("X"));
                _subclassedWndProcDelegate = HookedWndProc;
                var newWndProcPointer = Marshal.GetFunctionPointerForDelegate(_subclassedWndProcDelegate);
                var originalWndProc = SetWindowLongPtrW(targetHandle, GWLP_WNDPROC, newWndProcPointer);

                if (originalWndProc == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != 0)
                        QueueMessage("[EasyHook:Target] Failed to subclass WndProc on hwnd=0x" + targetHandle.ToString("X") + ", SetWindowLongPtrW error: " + error);
                    else
                        QueueMessage("[EasyHook:Target] SetWindowLongPtrW returned Zero with no error on hwnd=0x" + targetHandle.ToString("X") + " (may already be subclassed or handle invalid)");
                    return;
                }

                _originalWndProc = originalWndProc;
                _subclassedWindowHandle = targetHandle;
                QueueMessage("[EasyHook:Target] WndProc subclass installed on hwnd=0x" + targetHandle.ToString("X") + ", original WndProc=0x" + originalWndProc.ToString("X"));
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
            string step = "checking override active";
            try
            {
                if (IsCursorOverrideActive())
                {
                    // Suppress real user button clicks — only bot-injected clicks (stamped
                    // with BOT_INPUT_SENTINEL) should reach the game.
                    if (msg == WM_LBUTTONDOWN || msg == WM_LBUTTONUP ||
                        msg == WM_RBUTTONDOWN || msg == WM_RBUTTONUP)
                    {
                        if (((long)wParam & BOT_INPUT_SENTINEL) != 0)
                            wParam = (IntPtr)((long)wParam & ~BOT_INPUT_SENTINEL); // strip sentinel, allow through
                        else
                            return IntPtr.Zero; // real user click — suppress
                    }

                    if (msg == WM_MOUSEMOVE)
                    {
                        step = "rewriting WM_MOUSEMOVE lParam";
                        // _testPoint is already client coords — use directly
                        lParam = MakeLParam(_testPoint.X, _testPoint.Y);
                    }
                    else if (msg == WM_INPUT)
                    {
                        step = "converting WM_INPUT to WM_MOUSEMOVE";
                        lParam = MakeLParam(_testPoint.X, _testPoint.Y);
                        msg = WM_MOUSEMOVE;
                        wParam = IntPtr.Zero;
                    }
                }
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] WndProc error at '{step}': {e.Message}");
            }

            step = "calling original WndProc";
            try
            {
                if (_originalWndProc != IntPtr.Zero)
                    return CallWindowProcW(_originalWndProc, hWnd, msg, wParam, lParam);
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] WndProc error at '{step}': {e.Message}");
            }

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
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetCursorPos error: {e.Message}");
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
            string step = "calling original";
            try
            {
                int result = _originalGetMessageW(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
                if (result != 0)
                {
                    step = "filtering message";
                    FilterMessage(ref lpMsg);
                }
                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetMessageW error at '{step}': {e.Message}");
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
            string step = "calling original";
            try
            {
                bool result = _originalPeekMessageW(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
                if (result)
                {
                    step = "filtering message";
                    FilterMessage(ref lpMsg);
                }
                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] PeekMessageW error at '{step}': {e.Message}");
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
            string step = "calling original";
            try
            {
                uint result = _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
                step = "checking if mouse RID_INPUT";
                if (pData != IntPtr.Zero && uiCommand == RID_INPUT)
                {
                    step = "reading RAWINPUTHEADER dwType";
                    uint dwType = (uint)Marshal.ReadInt32(pData, 0);
                    if (dwType == RIM_TYPEMOUSE)
                    {
                        step = "zeroing lLastX/lLastY in RAWMOUSE";
                        int headerSize = Marshal.SizeOf(typeof(RAWINPUTHEADER));
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
                QueueMessage($"[EasyHook:Target] GetRawInputData error at '{step}': {e.Message}");
                return 0;
            }
        }

        #endregion

        #region GetCursorInfo hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool GetCursorInfoDelegate(ref CURSORINFO pci);
        private GetCursorInfoDelegate _originalGetCursorInfo;

        public bool HookedGetCursorInfo(ref CURSORINFO pci)
        {
            LogFirstCall("GetCursorInfo");
            string step = "calling original";
            try
            {
                bool result = _originalGetCursorInfo(ref pci);
                step = "overriding cursor position";
                if (result && IsCursorOverrideActive())
                {
                    var sp = GetTestScreenPoint();
                    pci.ptScreenPos.X = sp.X;
                    pci.ptScreenPos.Y = sp.Y;
                }
                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetCursorInfo error at '{step}': {e.Message}");
                return false;
            }
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
                    if (!IsCursorOverrideActive()) break;
                    // lParam = client coords
                    lpMsg.lParam = MakeLParam(_testPoint.X, _testPoint.Y);
                    // pt = screen coords
                    var sp = GetTestScreenPoint();
                    lpMsg.pt = new POINT { X = sp.X, Y = sp.Y };
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

                case WM_LBUTTONDOWN:
                case WM_LBUTTONUP:
                case WM_RBUTTONDOWN:
                case WM_RBUTTONUP:
                    // Bot/real click distinction is handled exclusively in HookedWndProc.
                    // Do NOT strip the sentinel here — WndProc needs it to tell bot
                    // clicks apart from real user clicks.
                    if (IsCursorOverrideActive() && ((long)lpMsg.wParam & BOT_INPUT_SENTINEL) == 0)
                    {
                        // Real user click — suppress
                        lpMsg.message = WM_NULL;
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
            string step = "checking override active";
            try
            {
                if (IsCursorOverrideActive() && _server.targetHwnd != IntPtr.Zero && hWnd == _server.targetHwnd)
                {
                    // _testPoint is already client coords — return as-is
                    step = "returning fixed client coords";
                    lpPoint = new POINT { X = _testPoint.X, Y = _testPoint.Y };
                    return true;
                }
                step = "calling original ScreenToClient (passthrough)";
                return _originalScreenToClient != null && _originalScreenToClient(hWnd, ref lpPoint);
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] ScreenToClient error at '{step}': {e.Message}");
                return false;
            }
        }

        public bool HookedClientToScreen(IntPtr hWnd, ref POINT lpPoint)
        {
            LogFirstCall("ClientToScreen");
            string step = "checking override active";
            try
            {
                if (IsCursorOverrideActive() && _server.targetHwnd != IntPtr.Zero && hWnd == _server.targetHwnd)
                {
                    // Convert client coords to screen coords via the real ClientToScreen
                    step = "converting fixed client coords to screen coords";
                    lpPoint = new POINT { X = _testPoint.X, Y = _testPoint.Y };
                    return _originalClientToScreen != null && _originalClientToScreen(hWnd, ref lpPoint);
                }
                step = "calling original ClientToScreen (passthrough)";
                return _originalClientToScreen != null && _originalClientToScreen(hWnd, ref lpPoint);
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] ClientToScreen error at '{step}': {e.Message}");
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
            string step = "init";
            try
            {
                step = "setting cbSize";
                if (pgui.cbSize == 0)
                    pgui.cbSize = (uint)Marshal.SizeOf(typeof(GUITHREADINFO));

                step = "calling original GetGUIThreadInfo";
                bool result = false;
                if (_originalGetGUIThreadInfo != null)
                    result = _originalGetGUIThreadInfo(idThread, ref pgui);

                step = "checking override active";
                if (IsCursorOverrideActive() && _server.targetHwnd != IntPtr.Zero)
                {
                    step = "spoofing GUI thread info handles";
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
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetGUIThreadInfo error at '{step}': {e.Message}");
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
            string step = "checking override active";
            try
            {
                if (IsCursorOverrideActive() && _server.targetHwnd != IntPtr.Zero)
                    return _server.targetHwnd;
                step = "calling original GetCapture";
                return _originalGetCapture != null ? _originalGetCapture() : IntPtr.Zero;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetCapture error at '{step}': {e.Message}");
                return IntPtr.Zero;
            }
        }

        public short HookedGetAsyncKeyState(int vKey)
        {
            LogFirstCall("GetAsyncKeyState");
            try
            {
                if (_originalGetAsyncKeyState != null)
                    return _originalGetAsyncKeyState(vKey);
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetAsyncKeyState error: {e.Message}");
            }
            return 0;
        }

        public short HookedGetKeyState(int nVirtKey)
        {
            LogFirstCall("GetKeyState");
            try
            {
                if (_originalGetKeyState != null)
                    return _originalGetKeyState(nVirtKey);
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetKeyState error: {e.Message}");
            }
            return 0;
        }

        public bool HookedGetKeyboardState(IntPtr lpKeyState)
        {
            LogFirstCall("GetKeyboardState");
            string step = "calling original";
            try
            {
                if (_originalGetKeyboardState != null)
                {
                    bool result = _originalGetKeyboardState(lpKeyState);
                    if (result)
                        return true;
                }
                step = "providing zeroed fallback state";
                if (IsCursorOverrideActive() && lpKeyState != IntPtr.Zero)
                {
                    for (int i = 0; i < 256; i++)
                        Marshal.WriteByte(lpKeyState, i, 0);
                    return true;
                }
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetKeyboardState error at '{step}': {e.Message}");
            }
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
            string step = "checking targetHwnd";
            try
            {
                if (_server.targetHwnd != IntPtr.Zero)
                    return _server.targetHwnd;
                step = "calling original GetForegroundWindow";
                return _originalGetForegroundWindow();
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetForegroundWindow error at '{step}': {e.Message}");
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
            string step = "checking targetHwnd";
            try
            {
                if (_server.targetHwnd != IntPtr.Zero)
                    return _server.targetHwnd;
                step = "calling original GetActiveWindow";
                return _originalGetActiveWindow();
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetActiveWindow error at '{step}': {e.Message}");
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
            string step = "checking targetHwnd";
            try
            {
                if (_server.targetHwnd != IntPtr.Zero)
                    return _server.targetHwnd;
                step = "calling original GetFocus";
                return _originalGetFocus();
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetFocus error at '{step}': {e.Message}");
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
            string step = "calling original";
            try
            {
                uint result = _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);
                step = "checking if buffer has mouse data";
                if (pData != IntPtr.Zero && result > 0)
                {
                    int headerSize = Marshal.SizeOf(typeof(RAWINPUTHEADER));
                    IntPtr current = pData;

                    for (uint i = 0; i < result; i++)
                    {
                        step = $"reading RAWINPUTHEADER[{i}]";
                        uint dwType = (uint)Marshal.ReadInt32(current, 0);
                        uint dwSize = (uint)Marshal.ReadInt32(current, 4);

                        if (dwType == RIM_TYPEMOUSE)
                        {
                            step = $"zeroing mouse delta[{i}]";
                            int lLastXOffset = headerSize + 12;
                            int lLastYOffset = headerSize + 16;
                            Marshal.WriteInt32(current, lLastXOffset, 0);
                            Marshal.WriteInt32(current, lLastYOffset, 0);
                        }

                        long aligned = ((long)dwSize + 7) & ~7L;
                        current = new IntPtr(current.ToInt64() + aligned);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetRawInputBuffer error at '{step}': {e.Message}");
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
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetPhysicalCursorPos error: {e.Message}");
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
            try
            {
                return true;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] ClipCursor error: {e.Message}");
                return false;
            }
        }

        #endregion

        #region GetMessageA / PeekMessageA hooks (ANSI variants)

        private GetMessageWDelegate _originalGetMessageA;
        private PeekMessageWDelegate _originalPeekMessageA;

        public int HookedGetMessageA(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
        {
            LogFirstCall("GetMessageA");
            string step = "calling original";
            try
            {
                int result = _originalGetMessageA(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
                if (result != 0)
                {
                    step = "filtering message";
                    FilterMessage(ref lpMsg);
                }
                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] GetMessageA error at '{step}': {e.Message}");
                lpMsg = new MSG();
                return 0;
            }
        }

        public bool HookedPeekMessageA(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            LogFirstCall("PeekMessageA");
            string step = "calling original";
            try
            {
                bool result = _originalPeekMessageA(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
                if (result)
                {
                    step = "filtering message";
                    FilterMessage(ref lpMsg);
                }
                return result;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] PeekMessageA error at '{step}': {e.Message}");
                lpMsg = new MSG();
                return false;
            }
        }

        #endregion
    }
}
