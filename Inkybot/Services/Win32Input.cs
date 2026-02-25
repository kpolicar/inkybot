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
            
            // Will contain the name of the IPC server channel
            string channelName = null;
            _server = new ServerInterface();

            if (targetPID <= 0)
                throw new Exception("Could not initialize input handler");


            // Create the IPC server using the FileMonitorIPC.ServiceInterface class as a singleton
            EasyHook.RemoteHooking.IpcCreateServer<InkybotHook.ServerInterface>(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton, _server);

            // Get the full path to the assembly we want to inject into the target process
            string assemblyPath = AppContext.BaseDirectory;
            string injectionLibrary = Path.Combine(assemblyPath, "InkybotHook.dll");

            try
            {
                // Injecting into existing process by Id
                if (targetPID > 0)
                {
                    Console.WriteLine("Attempting to inject into process {0}", targetPID);

                    // inject into existing process
                    EasyHook.RemoteHooking.Inject(
                        targetPID,          // ID of process to inject into
                        injectionLibrary,   // 32-bit library to inject (if target is 32-bit)
                        injectionLibrary,   // 64-bit library to inject (if target is 64-bit)
                        channelName         // the parameters to pass into injected library
                                            // ...
                    );
                    isInitialized = true;
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was an error while injecting into target:");
                Console.ResetColor();
                Console.WriteLine(e.ToString());
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
                _server.ShutdownFlag = true;
                Thread.Sleep(5000); // Wait for dll to disinject (hopefully)
            }
        }
    }
}
