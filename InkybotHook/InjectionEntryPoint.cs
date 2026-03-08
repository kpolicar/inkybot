using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using static InkybotHook.NativeMethods;
#pragma warning disable CS1690

namespace InkybotHook
{
    public partial class InjectionEntryPoint : EasyHook.IEntryPoint
    {
        private readonly ServerInterface _server;

        private readonly ManualResetEventSlim _allHooksInstalled = new ManualResetEventSlim(false);
        private Thread _inputThread;
        private volatile bool _stopInputThread = false;

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

            _server.SetState(HookState.Running);

            try
            {
                while (!_server.ShutdownFlag)
                {
                    Thread.Sleep(1);
                    _server.Ping();
                }
                _server.ReportMessage("[EasyHook:Target] Shutdown flag received, cleaning up hooks");
            }
            catch { }

            try
            {
                CleanupResources();
                foreach (var hook in installedHooks)
                    hook.Dispose();
                EasyHook.LocalHook.Release();
                _server.ReportMessage("[EasyHook:Target] Hooks disposed and released");
                _server.SetState(HookState.Disposed);
            }
            catch { }
        }

        // =============================================================
        // HOOK INSTALLATION
        // =============================================================

        private List<EasyHook.LocalHook> InstallHooks()
        {
            var hooks = new List<EasyHook.LocalHook>();

            hooks.Add(TryInstallHook<PeekMessageWDelegate>("PeekMessageW", new PeekMessageWDelegate(HookedPeekMessageW), out _originalPeekMessageW));
            hooks.Add(TryInstallHook<TranslateMessageDelegate>("TranslateMessage", new TranslateMessageDelegate(HookedTranslateMessage), out _originalTranslateMessage));
            hooks.Add(TryInstallHook<DispatchMessageWDelegate>("DispatchMessageW", new DispatchMessageWDelegate(HookedDispatchMessageW), out _originalDispatchMessageW));
            hooks.Add(TryInstallHook<GetRawInputDataDelegate>("GetRawInputData", new GetRawInputDataDelegate(HookedGetRawInputData), out _originalGetRawInputData));
            hooks.Add(TryInstallHook<GetRawInputBufferDelegate>("GetRawInputBuffer", new GetRawInputBufferDelegate(HookedGetRawInputBuffer), out _originalGetRawInputBuffer));
            hooks.Add(TryInstallHook<GetAsyncKeyStateDelegate>("GetAsyncKeyState", new GetAsyncKeyStateDelegate(HookedGetAsyncKeyState), out _originalGetAsyncKeyState));
            hooks.Add(TryInstallHook<GetKeyStateDelegate>("GetKeyState", new GetKeyStateDelegate(HookedGetKeyState), out _originalGetKeyState));
            hooks.Add(TryInstallHook<ReleaseCaptureDelegate>("ReleaseCapture", new ReleaseCaptureDelegate(HookedReleaseCapture), out _originalReleaseCapture));
            hooks.Add(TryInstallHook<GetCaptureDelegate>("GetCapture", new GetCaptureDelegate(HookedGetCapture), out _originalGetCapture));
            hooks.Add(TryInstallHook<GetCursorPosHookDelegate>("GetCursorPos", new GetCursorPosHookDelegate(HookedGetCursorPos), out _originalGetCursorPos));

            hooks.RemoveAll(item => item == null);

            ProbeRawInputDevices();

            _stopInputThread = false;
            _inputThread = new Thread(InputProcessorLoop) { IsBackground = true, Name = "Inkybot_InputThread" };
            _inputThread.Start();

            _allHooksInstalled.Set();
            return hooks;
        }

        private void CleanupResources()
        {
            _stopInputThread = true;

            int deadline = Environment.TickCount + 1000;
            while (_rawState != ForgeState.Idle && (Environment.TickCount - deadline) < 0)
                Thread.Sleep(5);

            FreeFakePacketBuffer();
        }

        // =============================================================
        // HELPERS
        // =============================================================

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
