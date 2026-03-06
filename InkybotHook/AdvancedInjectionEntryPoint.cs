using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using static InkybotHook.NativeMethods;
#pragma warning disable CS1690

namespace InkybotHook
{
    /// <summary>
    /// EasyHook entry point — connects IPC, installs hooks, runs the message loop, and cleans up.
    /// </summary>
    public partial class AdvancedInjectionEntryPoint : EasyHook.IEntryPoint
    {
        private readonly ServerInterface _server;
        private readonly Queue<string> _messageQueue = new Queue<string>();
        private readonly ConcurrentDictionary<string, byte> _loggedFirstCalls = new ConcurrentDictionary<string, byte>();
        private readonly ManualResetEventSlim _allHooksInstalled = new ManualResetEventSlim(false);
        private readonly List<EasyHook.LocalHook> _installedHooks = new List<EasyHook.LocalHook>();
        private volatile bool _disposing;

        public AdvancedInjectionEntryPoint(
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
            try
            {
                _server.IsInstalled(EasyHook.RemoteHooking.GetCurrentProcessId());
                InstallAllHooks();
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
                    Thread.Sleep(1);

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

            DisposeAllHooks();
        }

        // =========================================================
        // HOOK DEFINITIONS (single source of truth for all hooks)
        // =========================================================
        private (string Name, Action Install)[] GetHookDefinitions() => new (string, Action)[]
        {
            ("GetCursorPos",     () => _originalGetCursorPos     = InstallHook<GetCursorPosDelegate>("GetCursorPos", new GetCursorPosDelegate(HookedGetCursorPos))),
            ("IsIconic",         () => _originalIsIconic         = InstallHook<IsIconicDelegate>("IsIconic", new IsIconicDelegate(HookedIsIconic))),
            ("PeekMessageW",     () => _originalPeekMessageW     = InstallHook<PeekMessageWDelegate>("PeekMessageW", new PeekMessageWDelegate(HookedPeekMessageW))),
            ("TranslateMessage", () => _originalTranslateMessage = InstallHook<TranslateMessageDelegate>("TranslateMessage", new TranslateMessageDelegate(HookedTranslateMessage))),
            ("DispatchMessageW", () => _originalDispatchMessageW = InstallHook<DispatchMessageWDelegate>("DispatchMessageW", new DispatchMessageWDelegate(HookedDispatchMessageW))),
            ("GetRawInputData",  () => _originalGetRawInputData  = InstallHook<GetRawInputDataDelegate>("GetRawInputData", new GetRawInputDataDelegate(HookedGetRawInputData))),
            ("GetRawInputBuffer",() => _originalGetRawInputBuffer= InstallHook<GetRawInputBufferDelegate>("GetRawInputBuffer", new GetRawInputBufferDelegate(HookedGetRawInputBuffer))),
            ("GetAsyncKeyState", () => _originalGetAsyncKeyState  = InstallHook<GetAsyncKeyStateDelegate>("GetAsyncKeyState", new GetAsyncKeyStateDelegate(HookedGetAsyncKeyState))),
            ("GetKeyState",      () => _originalGetKeyState       = InstallHook<GetKeyStateDelegate>("GetKeyState", new GetKeyStateDelegate(HookedGetKeyState))),
        };

        // =========================================================
        // INSTALLATION & DISPOSAL
        // =========================================================
        private void InstallAllHooks()
        {
            foreach (var def in GetHookDefinitions())
            {
                try
                {
                    def.Install();
                }
                catch (Exception e)
                {
                    QueueMessage($"[EasyHook:Target] Failed to install hook {def.Name}: {e.Message}");
                }
            }

            _stopAutomationThread = false;
            _automationThread = new Thread(AutomationThreadLoop) { IsBackground = true, Name = "Inkybot_AutomationThread" };
            _automationThread.Start();

            _allHooksInstalled.Set();
            QueueMessage($"[EasyHook:Target] Installed {_installedHooks.Count} hooks");
        }

        private void DisposeAllHooks()
        {
            _disposing = true;

            // 1. Stop automation thread
            _stopAutomationThread = true;

            // 2. Restore subclassed windows (must happen before DLL unload)
            try { RestoreSubclassedWindows(); } catch { }

            // 3. Dispose EasyHook hooks (restores original function pointers)
            foreach (var hook in _installedHooks)
            {
                try { hook.Dispose(); } catch { }
            }
            _installedHooks.Clear();

            // 4. Free native memory
            try { FreeNativeFakePacket(); } catch { }

            // 5. Release EasyHook
            try { EasyHook.LocalHook.Release(); } catch { }

            try
            {
                _server.ReportMessage("[EasyHook:Target] Hooks disposed and released");
                _server.SetState(HookState.Disposed);
            }
            catch { }
        }

        private void RestoreSubclassedWindows()
        {
            lock (_wndProcLock)
            {
                foreach (var kvp in _originalWndProcs)
                {
                    try { SetWindowLongPtr(kvp.Key, GWLP_WNDPROC, kvp.Value); } catch { }
                }
                _originalWndProcs.Clear();
                _wndProcDelegates.Clear();
            }
        }

        // =========================================================
        // HOOK INSTALLATION HELPER
        // =========================================================
        private TOriginal InstallHook<TOriginal>(string functionName, Delegate hookedMethod)
            where TOriginal : class
        {
            var targetFunction = EasyHook.LocalHook.GetProcAddress("user32.dll", functionName);
            var hook = EasyHook.LocalHook.Create(targetFunction, hookedMethod, this);
            var original = Marshal.GetDelegateForFunctionPointer<TOriginal>(targetFunction);
            hook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            _installedHooks.Add(hook);
            QueueMessage($"[EasyHook:Target] Hook installed: {functionName}");
            return original;
        }

        // =========================================================
        // LOGGING
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
    }
}
