using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static InkybotHook.NativeMethods;
#pragma warning disable CS1690 // Accessing a member on a field of a marshal-by-reference class

namespace InkybotHook
{
    /// <summary>
    /// Simplified EasyHook entry point — hooks GetCursorPos, IsIconic, DispatchMessageW,
    /// GetKeyState, and GetAsyncKeyState. Passive: waits for Win32Input to send messages
    /// via PostMessage. No automation thread or synthetic message injection.
    /// </summary>
    public class InjectionEntryPoint
    {
        private readonly ServerInterface _server;
        private readonly Queue<string> _messageQueue = new Queue<string>();
        private readonly ConcurrentDictionary<string, byte> _loggedFirstCalls = new ConcurrentDictionary<string, byte>();

        // =========================================================
        // DELEGATES
        // =========================================================
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GetCursorPosDelegate(out POINT lpPoint);
        private GetCursorPosDelegate _originalGetCursorPos;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool IsIconicDelegate(IntPtr hWnd);
        private IsIconicDelegate _originalIsIconic;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate IntPtr DispatchMessageWDelegate(ref MSG lpMsg);
        private DispatchMessageWDelegate _originalDispatchMessageW;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate short GetKeyStateDelegate(int nVirtKey);
        private GetKeyStateDelegate _originalGetKeyState;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate short GetAsyncKeyStateDelegate(int vKey);
        private GetAsyncKeyStateDelegate _originalGetAsyncKeyState;

        // =========================================================
        // CONSTANTS
        // =========================================================
        private const int VK_LBUTTON = 0x01;
        private const uint WM_POINTERUPDATE = 0x0245;
        private const uint WM_POINTERDOWN = 0x0246;
        private const uint WM_POINTERUP = 0x0247;
        private const uint WM_MOUSEMOVE = 0x0200;
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;

        // =========================================================
        // CONSTRUCTOR & RUN
        // =========================================================
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

            try
            {
                _server.IsInstalled(EasyHook.RemoteHooking.GetCurrentProcessId());
                installedHooks.AddRange(InstallHooks());
                _server.SetState(HookState.HooksInstalled);
            }
            catch (Exception e)
            {
                _server.ReportMessage("[EasyHook:Target] Unexpected error during hook setup: " + e.ToString());
                _server.SetState(HookState.Failed);
                return;
            }

            // Detect pointer input mode from inside the target process
            DetectPointerInputMode();

            _server.SetState(HookState.Running);

            try
            {
                while (!_server.ShutdownFlag)
                {
                    System.Threading.Thread.Sleep(1);

                    string[] queued = null;
                    lock (_messageQueue)
                    {
                        queued = _messageQueue.ToArray();
                        _messageQueue.Clear();
                    }

                    if (queued != null && queued.Length > 0)
                        _server.ReportMessages(queued);
                    else
                        _server.Ping();
                }
                _server.ReportMessage("[EasyHook:Target] Shutdown flag received, cleaning up hooks");
            }
            catch { }

            try
            {
                foreach (var hook in installedHooks)
                    hook.Dispose();
                EasyHook.LocalHook.Release();
                _server.ReportMessage("[EasyHook:Target] Hooks disposed and released");
                _server.SetState(HookState.Disposed);
            }
            catch { }
        }

        // =========================================================
        // HOOK INSTALLATION
        // =========================================================
        private List<EasyHook.LocalHook> InstallHooks()
        {
            var hooks = new List<EasyHook.LocalHook>();

            hooks.Add(TryInstallHook<GetCursorPosDelegate>("GetCursorPos",
                new GetCursorPosDelegate(HookedGetCursorPos), out _originalGetCursorPos));
            hooks.Add(TryInstallHook<IsIconicDelegate>("IsIconic",
                new IsIconicDelegate(HookedIsIconic), out _originalIsIconic));
            hooks.Add(TryInstallHook<DispatchMessageWDelegate>("DispatchMessageW",
                new DispatchMessageWDelegate(HookedDispatchMessageW), out _originalDispatchMessageW));
            hooks.Add(TryInstallHook<GetKeyStateDelegate>("GetKeyState",
                new GetKeyStateDelegate(HookedGetKeyState), out _originalGetKeyState));
            hooks.Add(TryInstallHook<GetAsyncKeyStateDelegate>("GetAsyncKeyState",
                new GetAsyncKeyStateDelegate(HookedGetAsyncKeyState), out _originalGetAsyncKeyState));

            hooks.RemoveAll(item => item == null);

            QueueMessage($"[EasyHook:Target] Installed {hooks.Count} hooks (simplified entry point)");
            return hooks;
        }

        private void DetectPointerInputMode()
        {
            try
            {
                bool enabled = NativeMethods.IsMouseInPointerEnabled();
                _server.IsPointerInputEnabled = enabled;
                QueueMessage($"[EasyHook:Target] IsMouseInPointerEnabled = {enabled}");
            }
            catch (Exception e)
            {
                _server.IsPointerInputEnabled = false;
                QueueMessage($"[EasyHook:Target] IsMouseInPointerEnabled check failed: {e.Message}");
            }
        }

        // =========================================================
        // HOOK IMPLEMENTATIONS
        // =========================================================
        private bool HookedGetCursorPos(out POINT lpPoint)
        {
            bool result = _originalGetCursorPos(out lpPoint);
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

        private IntPtr HookedDispatchMessageW(ref MSG lpMsg)
        {
            LogFirstCall("DispatchMessageW");

            // Capture real pointer ID from WM_POINTER messages
            if (lpMsg.message >= WM_POINTERUPDATE && lpMsg.message <= WM_POINTERUP)
            {
                uint extractedPointerId = (uint)(lpMsg.wParam.ToInt64() & 0xFFFF);
                if (_server.CapturedPointerId != extractedPointerId)
                {
                    _server.CapturedPointerId = extractedPointerId;
                    QueueMessage($"[DispatchMessageW] Captured real OS Pointer ID: {extractedPointerId}");
                }
            }

            // Override coordinates on pointer/mouse move messages when cursor override is active
            if (_server.point.X != -1 && _server.point.Y != -1)
            {
                if (lpMsg.message == WM_MOUSEMOVE)
                {
                    // WM_MOUSEMOVE uses client coordinates — convert screen->client
                    POINT clientPt = new POINT { X = _server.point.X, Y = _server.point.Y };
                    if (_server.targetHwnd != IntPtr.Zero)
                        ScreenToClient(_server.targetHwnd, ref clientPt);
                    lpMsg.lParam = (IntPtr)((uint)((clientPt.Y << 16) | (clientPt.X & 0xFFFF)));
                    LogFirstCall("DispatchMessageW:OverrideMouseMove");
                }
                else if (lpMsg.message == WM_POINTERUPDATE)
                {
                    // WM_POINTERUPDATE uses screen coordinates
                    lpMsg.lParam = (IntPtr)((uint)((_server.point.Y << 16) | (_server.point.X & 0xFFFF)));
                    LogFirstCall("DispatchMessageW:OverridePointerUpdate");
                }
            }

            // Log synthetic click messages (posted by Win32Input via PostMessage)
            if (lpMsg.message == WM_LBUTTONDOWN || lpMsg.message == WM_LBUTTONUP
                || lpMsg.message == WM_POINTERDOWN || lpMsg.message == WM_POINTERUP)
            {
                LogFirstCall($"DispatchMessageW:Click:0x{lpMsg.message:X4}");
            }

            return _originalDispatchMessageW(ref lpMsg);
        }

        private short HookedGetKeyState(int nVirtKey)
        {
            LogFirstCall("GetKeyState");
            short realState = _originalGetKeyState(nVirtKey);
            if (nVirtKey == VK_LBUTTON && _server.IsClickActive)
            {
                LogFirstCall("GetKeyState:Spoofed");
                return (short)(realState | unchecked((short)0x8000));
            }
            return realState;
        }

        private short HookedGetAsyncKeyState(int vKey)
        {
            LogFirstCall("GetAsyncKeyState");
            short realState = _originalGetAsyncKeyState(vKey);
            if (vKey == VK_LBUTTON && _server.IsClickActive)
            {
                LogFirstCall("GetAsyncKeyState:Spoofed");
                return (short)(realState | unchecked((short)0x8000));
            }
            return realState;
        }

        // =========================================================
        // UTILITIES
        // =========================================================
        private void LogFirstCall(string hookName)
        {
            if (_loggedFirstCalls.TryAdd(hookName, 0))
                QueueMessage("[EasyHook:Target] First call intercepted: " + hookName);
        }

        private void QueueMessage(string message)
        {
            lock (_messageQueue)
            {
                _messageQueue.Enqueue(message);
            }
        }

        private EasyHook.LocalHook TryInstallHook<TOriginal>(
            string functionName, Delegate hookedMethod, out TOriginal original)
            where TOriginal : class
        {
            try
            {
                var targetFunction = EasyHook.LocalHook.GetProcAddress("user32.dll", functionName);
                var hook = EasyHook.LocalHook.Create(targetFunction, hookedMethod, this);
                original = Marshal.GetDelegateForFunctionPointer<TOriginal>(targetFunction);
                hook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                QueueMessage($"[EasyHook:Target] Hook installed: {functionName}");
                return hook;
            }
            catch (Exception e)
            {
                QueueMessage($"[EasyHook:Target] Failed to install hook {functionName}: {e.Message}");
                original = null;
                return null;
            }
        }
    }
}
