using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static InkybotHook.NativeMethods;
#pragma warning disable CS1690 // Accessing a member on a field of a marshal-by-reference class

namespace InkybotHook
{
    /// <summary>
    /// EasyHook entry point — connects IPC, installs hooks, runs the message loop, and cleans up.
    /// </summary>
    public partial class InjectionEntryPoint : EasyHook.IEntryPoint
    {
        private readonly ServerInterface _server;
        private readonly Queue<string> _messageQueue = new Queue<string>();
        private readonly ConcurrentDictionary<string, byte> _loggedFirstCalls = new ConcurrentDictionary<string, byte>();

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
                installedHooks.AddRange(InstallMousePositionHooks());
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
                    DrawDebugMarkerIfDue();

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
                EasyHook.LocalHook.Release();
                _server.ReportMessage("[EasyHook:Target] Hooks disposed and released");
                _server.SetState(HookState.Disposed);
            }
            catch { }
        }

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
                return hook;
            }
            catch (Exception)
            {
                original = null;
                return null;
            }
        }
    }
}