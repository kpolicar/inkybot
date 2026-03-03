using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace InkybotHook
{
    
    public class InjectionEntryPoint: EasyHook.IEntryPoint
    {
        ServerInterface _server = null;

        Queue<string> _messageQueue = new Queue<string>();

        private bool _hasLoggedFirstCursorIntercept = false;

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
                throw new Exception("[EasyHook] Failed to connect IPC channel '" + channelName + "': " + e.Message, e);
            }
        }

        public void Run(
            EasyHook.RemoteHooking.IContext context,
            string channelName)
        {
            EasyHook.LocalHook getCursorPosHook = null;
            EasyHook.LocalHook isIconicHook = null;

            try
            {
                // Injection is now complete and the server interface is connected
                _server.IsInstalled(EasyHook.RemoteHooking.GetCurrentProcessId());

                // Install hooks
                IntPtr targetFunction;
                try
                {
                    targetFunction = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetCursorPos");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook] Failed to find GetCursorPos in user32.dll: " + e.Message);
                    _server.SetState(HookState.Failed);
                    return;
                }

                try
                {
                    getCursorPosHook = EasyHook.LocalHook.Create(
                        targetFunction,
                        new GetCursorPosDelegate(HookedGetCursorPos),
                        this);
                    _originalGetCursorPos = Marshal.GetDelegateForFunctionPointer<GetCursorPosDelegate>(targetFunction);
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook] Failed to create GetCursorPos hook: " + e.Message);
                    _server.SetState(HookState.Failed);
                    return;
                }

                IntPtr isIconicTarget;
                try
                {
                    isIconicTarget = EasyHook.LocalHook.GetProcAddress("user32.dll", "IsIconic");
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook] Failed to find IsIconic in user32.dll: " + e.Message);
                    _server.SetState(HookState.Failed);
                    return;
                }

                try
                {
                    isIconicHook = EasyHook.LocalHook.Create(
                        isIconicTarget,
                        new IsIconicDelegate(HookedIsIconic),
                        this);
                    _originalIsIconic = Marshal.GetDelegateForFunctionPointer<IsIconicDelegate>(isIconicTarget);
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook] Failed to create IsIconic hook: " + e.Message);
                    _server.SetState(HookState.Failed);
                    return;
                }

                // Activate hooks on all threads except the current thread
                try
                {
                    getCursorPosHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                    isIconicHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                }
                catch (Exception e)
                {
                    _server.ReportMessage("[EasyHook] Failed to set thread ACL for hooks: " + e.Message);
                    _server.SetState(HookState.Failed);
                    return;
                }

                _server.ReportMessage("[EasyHook] GetCursorPos and IsIconic hooks installed successfully");
                _server.SetState(HookState.HooksInstalled);
            }
            catch (Exception e)
            {
                _server.ReportMessage("[EasyHook] Unexpected error during hook setup: " + e.ToString());
                _server.SetState(HookState.Failed);
                return;
            }

            _server.SetState(HookState.Running);

            try
            {
                // Loop until IPC fails
                while (!_server.ShutdownFlag)
                {
                    System.Threading.Thread.Sleep(50);

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

                _server.ReportMessage("[EasyHook] Shutdown flag received, cleaning up hooks");
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
                EasyHook.LocalHook.Release();
                _server.ReportMessage("[EasyHook] Hooks disposed and released");
                _server.SetState(HookState.Disposed);
            }
            catch
            {
                // Host may already be gone, swallow
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool GetCursorPosDelegate(out POINT lpPoint);
        private GetCursorPosDelegate _originalGetCursorPos;
        
        public bool HookedGetCursorPos(out POINT lpPoint)
        {
            try
            {
                // Call the original function
                if (_server.point.X == -1 && _server.point.Y == -1)
                    return _originalGetCursorPos(out lpPoint);

                lpPoint.X = _server.point.X;
                lpPoint.Y = _server.point.Y;

                if (!_hasLoggedFirstCursorIntercept)
                {
                    _hasLoggedFirstCursorIntercept = true;
                    try
                    {
                        _server.ReportMessage($"[EasyHook] First cursor position intercept: ({lpPoint.X}, {lpPoint.Y})");
                    }
                    catch { /* IPC may fail, don't crash target */ }
                }

                return true;
            }
            catch
            {
                // Fallback to original to avoid crashing the target process
                return _originalGetCursorPos(out lpPoint);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool IsIconicDelegate(IntPtr hWnd);
        private IsIconicDelegate _originalIsIconic;

        public bool HookedIsIconic(IntPtr hWnd)
        {
            return false;
        }

    }
}
