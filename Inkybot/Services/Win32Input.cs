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
                Thread.Sleep(4000); // wait for dll to have been injected
            }
            if (x != -1 || y != -1) {
                var r = new RECT();
                GetWindowRect(relativeToControl, out r);
                x += r.Left;
                y += r.Top;
                //x += 114;
                //y += 23;
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
        public static void Init() {
            if (isInitialized) return;

            var log = FileEventLogger.SystemLogger;

            // Wire up IPC logging callback to SystemLogger
            ServerInterface.Logger = message => log.Info(message);
            
            // Will contain the name of the IPC server channel
            string channelName = null;
            _server = new ServerInterface();

            if (targetPID <= 0)
            {
                log.Error("[EasyHook] Cannot initialize hook: target process ID is not set (was {0})", targetPID);
                throw new Exception("Could not initialize input handler");
            }

            // Create the IPC server
            try
            {
                EasyHook.RemoteHooking.IpcCreateServer<InkybotHook.ServerInterface>(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton, _server);
                _server.SetState(HookState.IpcCreated);
                log.Info("[EasyHook] IPC server created on channel: {0}", channelName);
            }
            catch (Exception e)
            {
                log.Error(e, "[EasyHook] Failed to create IPC server");
                _server.SetState(HookState.Failed);
                return;
            }

            // Get the full path to the assembly we want to inject into the target process
            string assemblyPath = AppContext.BaseDirectory;
            string injectionLibrary = Path.Combine(assemblyPath, "InkybotHook.dll");

            if (!File.Exists(injectionLibrary))
            {
                log.Error("[EasyHook] Injection library not found at: {0}", injectionLibrary);
                _server.SetState(HookState.Failed);
                return;
            }

            try
            {
                if (targetPID > 0)
                {
                    log.Info("[EasyHook] Injecting hook DLL into process {0} (library: {1})", targetPID, injectionLibrary);
                    _server.SetState(HookState.Injecting);

                    EasyHook.RemoteHooking.Inject(
                        targetPID,          // ID of process to inject into
                        injectionLibrary,   // 32-bit library to inject (if target is 32-bit)
                        injectionLibrary,   // 64-bit library to inject (if target is 64-bit)
                        channelName         // the parameters to pass into injected library
                    );

                    isInitialized = true;
                    log.Info("[EasyHook] Injection call completed successfully for process {0}", targetPID);
                }
            }
            catch (System.IO.FileNotFoundException e)
            {
                log.Error(e, "[EasyHook] Injection DLL or dependency not found");
                _server.SetState(HookState.Failed);
            }
            catch (UnauthorizedAccessException e)
            {
                log.Error(e, "[EasyHook] Insufficient privileges to inject into process {0}. Try running as administrator", targetPID);
                _server.SetState(HookState.Failed);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                log.Error(e, "[EasyHook] Win32 error during injection (code {0}): target process may have exited or be protected", e.NativeErrorCode);
                _server.SetState(HookState.Failed);
            }
            catch (ApplicationException e)
            {
                log.Error(e, "[EasyHook] EasyHook injection failed (possible architecture mismatch or target process issue)");
                _server.SetState(HookState.Failed);
            }
            catch (Exception e)
            {
                log.Error(e, "[EasyHook] Unexpected error during injection into process {0}", targetPID);
                _server.SetState(HookState.Failed);
            }
        }

        public void Click(int x, int y) {
            SetCursorPosition(x, y);
            Thread.Sleep(10);
            
            Win32.SendMessage(relativeToControl, Win32.WM_LBUTTONDOWN, 1, Win32.MakeLParam(x, y));
            Win32.SendMessage(relativeToControl, Win32.WM_LBUTTONUP, 1, Win32.MakeLParam(x, y));
        }

        public void Drag(int x, int y, int tX, int tY) {
            SetCursorPosition(x, y);
            Thread.Sleep(10);
            Win32.SendMessage(relativeToControl, Win32.WM_LBUTTONDOWN, 1, Win32.MakeLParam(x, y));
            
            SetCursorPosition(tX, tY);
            Thread.Sleep(10);
            Win32.SendMessage(relativeToControl, Win32.WM_LBUTTONUP, 1, Win32.MakeLParam(tX, tY));
        }

        public void DoubleClick(int x, int y) {
            Click(x,y);
            Thread.Sleep(50);
            Click(x,y);
        }

        public void TypeMessage(string message, CancellationToken? cancel=null) {
            foreach (var character in message) {
                cancel?.ThrowIfCancellationRequested();
                Win32.SendMessage(relativeToControl, 
                    Win32.WM_KEYDOWN, 
                    (IntPtr) character, 
                    IntPtr.Zero);
                Win32.SendMessage(relativeToControl, 
                    Win32.WM_CHAR, 
                    (IntPtr) character, 
                    IntPtr.Zero);
                Win32.SendMessage(relativeToControl, 
                    Win32.WM_KEYUP,
                    (IntPtr) character, 
                    IntPtr.Zero);
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
            Win32.SendMessage(relativeToControl, Win32.WM_KEYDOWN, (IntPtr) Keys.ControlKey, IntPtr.Zero);
            Win32.SendMessage(relativeToControl, Win32.WM_KEYDOWN, (IntPtr) Keys.RControlKey, IntPtr.Zero);
            DoubleClick(x, y);
            Win32.SendMessage(relativeToControl, Win32.WM_KEYUP, (IntPtr) Keys.ControlKey, IntPtr.Zero);
            Win32.SendMessage(relativeToControl, Win32.WM_KEYUP, (IntPtr) Keys.RControlKey, IntPtr.Zero);
        }

        public void SetRelativeToHandle(IntPtr handle) {
            relativeToControl = handle;
        }

        public void SelectAll() {
            Win32.SendMessage(relativeToControl, Win32.WM_KEYDOWN, (IntPtr) Keys.ControlKey, IntPtr.Zero);
            Win32.SendMessage(relativeToControl, Win32.WM_KEYDOWN, (IntPtr) Keys.RControlKey, IntPtr.Zero);
            Thread.Sleep(500);
            Win32.SendMessage(relativeToControl, 
                Win32.WM_KEYDOWN, 
                (IntPtr) 'A', 
                IntPtr.Zero);
            Thread.Sleep(500);
            Win32.SendMessage(relativeToControl, Win32.WM_KEYUP, (IntPtr) Keys.ControlKey, IntPtr.Zero);
            Win32.SendMessage(relativeToControl, Win32.WM_KEYUP, (IntPtr) Keys.RControlKey, IntPtr.Zero);
            
        }

        public void Dispose() {
            if (isInitialized) {
                FileEventLogger.SystemLogger.Info("[EasyHook] Shutting down hook, setting ShutdownFlag");
                _server.ShutdownFlag = true;
                Thread.Sleep(5000); // Wait for dll to disinject (hopefully)
                FileEventLogger.SystemLogger.Info("[EasyHook] Shutdown wait completed");
            }
        }
    }
}
