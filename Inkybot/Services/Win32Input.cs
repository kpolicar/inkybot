using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Events;
using InkybotHook;

namespace Inkybot.Services.Win32Input
{
    public class Win32Input : Input, HasDependencies, IDisposable
    {
        private static ServerInterface _server;
        private static Int32 targetPID = 0;
        public static bool isInitialized = false;
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref ServerInterface.POINT lpPoint);
        
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }
        
        private IntPtr relativeToControl;
        private ServerInterface.POINT previousCursorPosition = new ServerInterface.POINT{X = -1, Y = -1};
        private ServerInterface.POINT preScreenshotCursorPosition = new ServerInterface.POINT{X = -1, Y = -1};
        private bool isMaging;


        public void BindDependencies(ServiceContainer serviceContainer) {
            var magingJob = serviceContainer.GetService<DofusMagingJob>();
            var screenCapture = serviceContainer.GetService<ScreenCapture>();
            magingJob.Starting += OnMagingJobStart;
            magingJob.Stopped += OnMagingJobStopped;
            screenCapture.BeginScreenshot += OnBeginScreenshot;
            screenCapture.EndScreenshot += OnEndScreenshot;
        }

        private void OnBeginScreenshot(object sender, EventArgs e) {
            if (isMaging) return;
            
            preScreenshotCursorPosition = previousCursorPosition;
            Init();
            SetCursorPosition(0, 0);
            Thread.Sleep(100); // wait for new frame to have been saved in screenreader
        }
        
        private void OnEndScreenshot(object sender, EventArgs e) {
            if (isMaging) return;
            
            Debug.WriteLine(preScreenshotCursorPosition.X + "," + preScreenshotCursorPosition.Y);
            _server.SetCursorFixedPosition(previousCursorPosition = preScreenshotCursorPosition);
        }


        private void SetCursorPosition(int x, int y) {
            if (!isInitialized) {
                Init();
            }
            if (x != -1 || y != -1) {
                // Convert client-relative coordinates to screen coordinates using
                // ClientToScreen so that window borders/title bar are accounted for.
                // All hooks (GetCursorPos, ScreenToClient, etc.) expect _server.point
                // to contain screen coordinates.
                var pt = new ServerInterface.POINT { X = x, Y = y };
                ClientToScreen(relativeToControl, ref pt);
                x = pt.X;
                y = pt.Y;
            }
            _server.SetCursorFixedPosition(previousCursorPosition = new ServerInterface.POINT{X = x, Y = y});
        }

        private void OnMagingJobStopped(object sender, EventArgs e) {
            isMaging = false;
            SetCursorPosition(-1, -1);
        }

        private void OnMagingJobStart(object sender, EventArgs e) {
            isMaging = true;
            Init();
            SetCursorPosition(0, 0);
        }
        
        public static void SetTargetProcessId(int processId) => targetPID = processId;

        /// <summary>
        /// Waits for the injected hook DLL to reach the Running state.
        /// Call after Init() to ensure hooks are fully installed before proceeding.
        /// </summary>
        public static async Task WaitForHookReady(int timeoutMs = 30000) {
            var log = FileEventLogger.SystemLogger;
            if (_server == null)
                throw new InvalidOperationException("[EasyHook:Host] WaitForHookReady called before Init()");

            var sw = Stopwatch.StartNew();
            while (_server.State != HookState.Running)
            {
                if (_server.State == HookState.Failed)
                    throw new Exception($"[EasyHook:Host] Hook injection failed (state: {_server.State})");
                if (sw.ElapsedMilliseconds > timeoutMs)
                    throw new TimeoutException($"[EasyHook:Host] Hook did not reach Running state within {timeoutMs}ms (current state: {_server.State})");
                await Task.Delay(100);
            }
            log.Info($"[EasyHook:Host] Hook reached Running state in {sw.ElapsedMilliseconds}ms");
        }

        public static void Init() {
            var log = FileEventLogger.SystemLogger;

            if (isInitialized) {
                log.Info("[EasyHook:Host] Init() skipped — already initialized");
                return;
            }

            log.Info("[EasyHook:Host] Init() starting...");

            // Wire up IPC logging callback to SystemLogger
            ServerInterface.Logger = message => log.Info(message);
            
            // Will contain the name of the IPC server channel
            string channelName = null;
            _server = new ServerInterface();

            if (targetPID <= 0)
            {
                log.Error($"[EasyHook:Host] Cannot initialize hook: target process ID is not set (was {targetPID})");
                throw new Exception("Could not initialize input handler");
            }

            // Create the IPC server
            try
            {
                EasyHook.RemoteHooking.IpcCreateServer<InkybotHook.ServerInterface>(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton, _server);
                _server.SetState(HookState.IpcCreated);
                log.Info($"[EasyHook:Host] IPC server created on channel: {channelName}");
            }
            catch (Exception e)
            {
                log.Error(e, "[EasyHook:Host] Failed to create IPC server");
                _server.SetState(HookState.Failed);
                return;
            }

            // Get the full path to the assembly we want to inject into the target process
            string assemblyPath = AppContext.BaseDirectory;
            string injectionLibrary = Path.Combine(assemblyPath, "InkybotHook.dll");

            if (!File.Exists(injectionLibrary))
            {
                log.Error($"[EasyHook:Host] Injection library not found at: {injectionLibrary}");
                _server.SetState(HookState.Failed);
                return;
            }

            try
            {
                if (targetPID > 0)
                {
                    log.Info($"[EasyHook:Host] Injecting hook DLL into process {targetPID} (library: {injectionLibrary})");
                    _server.SetState(HookState.Injecting);

                    EasyHook.RemoteHooking.Inject(
                        targetPID,          // ID of process to inject into
                        injectionLibrary,   // 32-bit library to inject (if target is 32-bit)
                        injectionLibrary,   // 64-bit library to inject (if target is 64-bit)
                        channelName         // the parameters to pass into injected library
                    );

                    isInitialized = true;
                    log.Info($"[EasyHook:Host] Injection call completed successfully for process {targetPID}");
                }
            }
            catch (System.IO.FileNotFoundException e)
            {
                log.Error(e, "[EasyHook:Host] Injection DLL or dependency not found");
                _server.SetState(HookState.Failed);
            }
            catch (UnauthorizedAccessException e)
            {
                log.Error(e, $"[EasyHook:Host] Insufficient privileges to inject into process {targetPID}. Try running as administrator");
                _server.SetState(HookState.Failed);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                log.Error(e, $"[EasyHook:Host] Win32 error during injection (code {e.NativeErrorCode}): target process may have exited or be protected");
                _server.SetState(HookState.Failed);
            }
            catch (ApplicationException e)
            {
                log.Error(e, "[EasyHook:Host] EasyHook injection failed (possible architecture mismatch or target process issue)");
                _server.SetState(HookState.Failed);
            }
            catch (Exception e)
            {
                log.Error(e, $"[EasyHook:Host] Unexpected error during injection into process {targetPID}");
                _server.SetState(HookState.Failed);
            }
        }

        public void Click(int x, int y) {
            SetCursorPosition(x, y);
            Thread.Sleep(10);

            // Convert client coords to screen coords for the hook
            var pt = new ServerInterface.POINT { X = x, Y = y };
            ClientToScreen(relativeToControl, ref pt);
            _server.RequestClick(pt.X, pt.Y);

            // Wait for the hook to complete the full click cycle (down + 50ms + up + 50ms)
            Thread.Sleep(150);
        }

        public void Drag(int x, int y, int tX, int tY) {
            SetCursorPosition(x, y);
            Thread.Sleep(10);
            Win32.PostMessage(relativeToControl, (uint)Win32.WM_LBUTTONDOWN, (IntPtr)1, (IntPtr)Win32.MakeLParam(x, y));

            SetCursorPosition(tX, tY);
            Thread.Sleep(10);
            Win32.PostMessage(relativeToControl, (uint)Win32.WM_LBUTTONUP, (IntPtr)0, (IntPtr)Win32.MakeLParam(tX, tY));
        }

        public void DoubleClick(int x, int y) {
            Click(x,y);
            Thread.Sleep(50);
            Click(x,y);
        }

        public void TypeMessage(string message, CancellationToken? cancel=null) {
            foreach (var character in message) {
                cancel?.ThrowIfCancellationRequested();
                Win32.PostMessage(relativeToControl, (uint)Win32.WM_KEYDOWN, (IntPtr)character, IntPtr.Zero);
                Win32.PostMessage(relativeToControl, (uint)Win32.WM_CHAR,    (IntPtr)character, IntPtr.Zero);
                Win32.PostMessage(relativeToControl, (uint)Win32.WM_KEYUP,   (IntPtr)character, IntPtr.Zero);
                Thread.Sleep(100);
            }
        }

        public void Move(int x, int y) {
            SetCursorPosition(x, y);
        }

        public void ReleaseCursor() {
            if (!isInitialized) return;
            _server.SetCursorFixedPosition(previousCursorPosition = new ServerInterface.POINT { X = -1, Y = -1 });
        }

        public void CtrlDoubleClick(int x, int y) {
            Move(x, y);
            Win32.PostMessage(relativeToControl, (uint)Win32.WM_KEYDOWN, (IntPtr)Keys.ControlKey,  IntPtr.Zero);
            Win32.PostMessage(relativeToControl, (uint)Win32.WM_KEYDOWN, (IntPtr)Keys.RControlKey, IntPtr.Zero);
            DoubleClick(x, y);
            Win32.PostMessage(relativeToControl, (uint)Win32.WM_KEYUP, (IntPtr)Keys.ControlKey,  IntPtr.Zero);
            Win32.PostMessage(relativeToControl, (uint)Win32.WM_KEYUP, (IntPtr)Keys.RControlKey, IntPtr.Zero);
        }

        public void SetRelativeToHandle(IntPtr handle) {
            relativeToControl = handle;
            if (_server != null)
                _server.targetHwnd = handle;
        }

        public void SelectAll() {
            Win32.PostMessage(relativeToControl, (uint)Win32.WM_KEYDOWN, (IntPtr)Keys.ControlKey,  IntPtr.Zero);
            Win32.PostMessage(relativeToControl, (uint)Win32.WM_KEYDOWN, (IntPtr)Keys.RControlKey, IntPtr.Zero);
            Thread.Sleep(500);
            Win32.PostMessage(relativeToControl, (uint)Win32.WM_KEYDOWN, (IntPtr)'A', IntPtr.Zero);
            Thread.Sleep(500);
            Win32.PostMessage(relativeToControl, (uint)Win32.WM_KEYUP, (IntPtr)Keys.ControlKey,  IntPtr.Zero);
            Win32.PostMessage(relativeToControl, (uint)Win32.WM_KEYUP, (IntPtr)Keys.RControlKey, IntPtr.Zero);
        }

        public void Dispose() {
            if (isInitialized) {
                FileEventLogger.SystemLogger.Info("[EasyHook:Host] Shutting down hook, setting ShutdownFlag");
                _server.ShutdownFlag = true;
                Thread.Sleep(5000); // Wait for dll to disinject (hopefully)
                FileEventLogger.SystemLogger.Info("[EasyHook:Host] Shutdown wait completed");
            }
        }
    }
}
