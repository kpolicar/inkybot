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

        public InjectionEntryPoint(
            EasyHook.RemoteHooking.IContext context,
            string channelName) {
            _server = EasyHook.RemoteHooking.IpcConnectClient<ServerInterface>(channelName);

            // If Ping fails then the Run method will be not be called
            _server.Ping();
        }

        public void Run(
            EasyHook.RemoteHooking.IContext context,
            string channelName)
        {
            // Injection is now complete and the server interface is connected
            _server.IsInstalled(EasyHook.RemoteHooking.GetCurrentProcessId());

            // Install hooks
            IntPtr targetFunction = EasyHook.LocalHook.GetProcAddress("user32.dll", "GetCursorPos");
            var getCursorPosHook = EasyHook.LocalHook.Create(
                targetFunction,
                new GetCursorPosDelegate(HookedGetCursorPos),
                this);
            
            _originalGetCursorPos = Marshal.GetDelegateForFunctionPointer<GetCursorPosDelegate>(targetFunction);

            // Activate hooks on all threads except the current thread
            getCursorPosHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });

            _server.ReportMessage("Hooks installed");

            try
            {
                // Loop until IPC fails
                while (true)
                {
                    System.Threading.Thread.Sleep(500);

                    string[] queued = null;

                    lock (_messageQueue)
                    {
                        queued = _messageQueue.ToArray();
                        _messageQueue.Clear();
                    }

                    // Send newly monitored file accesses to FileMonitor
                    if (queued != null && queued.Length > 0)
                    {
                        _server.ReportMessages(queued);
                    }
                    else
                    {
                        _server.Ping();
                    }
                }
            }
            catch
            {
                // Ping() or ReportMessages() will raise an exception if host is unreachable
            }

            // Remove hooks
            getCursorPosHook.Dispose();

            // Finalise cleanup of hooks
            EasyHook.LocalHook.Release();
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
            // Call the original function
            if (_server.point.X == -1 &&  _server.point.Y == -1)
                return _originalGetCursorPos(out lpPoint);

            lpPoint.X = _server.point.X;
            lpPoint.Y = _server.point.Y;

            lock (_messageQueue) {
                _messageQueue.Enqueue($"x: {lpPoint.X}, y: {lpPoint.Y}");
            }
            
            return true;
        }

    }
}
