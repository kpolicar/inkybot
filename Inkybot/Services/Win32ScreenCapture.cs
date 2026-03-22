using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Exceptions;
using Inkybot.Services;
using NLog;

namespace Inkybot
{
    /// <summary>
    ///     Provides functions to capture the entire screen, or a particular window, and save it to a file.
    /// </summary>
    public class Win32ScreenCapture : ScreenCapture, IDisposable, HasDependencies
    {
        private static readonly Logger Log = LogManager.GetLogger("system");

        private IntPtr handle;
        private Form mainForm;
        private Panel dofusClientPanel;
        public event EventHandler? BeginScreenshot;
        public event EventHandler? EndScreenshot;
        private int xOffsetLeft;
        private int xOffsetRight;

        private Thread? captureThread;
        private volatile bool running;
        private readonly object frameLock = new();
        private Bitmap? latestFrame;
        private RECT latestRawRect;
        private RECT latestVisibleRect;
        private bool startedByCaptureWindow;
        private bool _firstValidFrameLogged = false;
        private readonly Stopwatch _invalidFrameLogThrottle = Stopwatch.StartNew();

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, uint dwAttribute, out RECT pvAttribute, int cbAttribute);

        private const uint PW_RENDERFULLCONTENT = 2;
        private const uint DWMWA_EXTENDED_FRAME_BOUNDS = 9;

        public void BindDependencies(ServiceContainer serviceContainer) {
            var magingJob = serviceContainer.GetService<DofusMagingJob>();
            magingJob.Starting += (_, _) => StartCapturing();
            magingJob.Stopped += (_, _) => StopCapturing();
        }

        public void BindTo(IntPtr handle, Panel dofusClientPanel, int xOffsetLeft, int xOffsetRight, Form mainForm) {
            (this.xOffsetLeft, this.xOffsetRight) = (xOffsetLeft, xOffsetRight);
            this.dofusClientPanel = dofusClientPanel;
            this.mainForm = mainForm;
            this.handle = handle;
        }

        public void StartCapturing() {
            if (running) return;
            Log.Info($"[Win32Capture] Starting background capture thread for handle 0x{handle:X}.");
            startedByCaptureWindow = false;
            running = true;
            captureThread = new Thread(CaptureLoop) { IsBackground = true, Name = "ScreenCapture" };
            captureThread.Start();
        }

        public void StopCapturing() {
            if (!running) return;
            Log.Info("[Win32Capture] Stopping background capture thread.");
            running = false;
            captureThread?.Join(3000);
            captureThread = null;
            lock (frameLock) {
                latestFrame?.Dispose();
                latestFrame = null;
            }
            _firstValidFrameLogged = false;
        }

        private void CaptureLoop() {
            while (running) {
                try {
                    GetWindowRect(handle, out RECT rawRect);
                    DwmGetWindowAttribute(handle, DWMWA_EXTENDED_FRAME_BOUNDS, out RECT visibleRect,
                        Marshal.SizeOf(typeof(RECT)));

                    int rawWidth = rawRect.Right - rawRect.Left;
                    int rawHeight = rawRect.Bottom - rawRect.Top;
                    if (rawWidth <= 0 || rawHeight <= 0)
                        continue;

                    var bmp = new Bitmap(rawWidth, rawHeight, PixelFormat.Format32bppArgb);
                    using (var gfx = Graphics.FromImage(bmp)) {
                        var hdc = gfx.GetHdc();
                        try {
                            PrintWindow(handle, hdc, PW_RENDERFULLCONTENT);
                        } finally {
                            gfx.ReleaseHdc(hdc);
                        }
                    }

                    lock (frameLock) {
                        latestFrame?.Dispose();
                        latestFrame = bmp;
                        latestRawRect = rawRect;
                        latestVisibleRect = visibleRect;
                    }
                } catch (Exception ex) {
                    FileEventLogger.SystemLogger.Warn($"[ScreenCapture] background capture error: {ex.Message}");
                }
            }
        }

        private (Bitmap frame, RECT rawRect, RECT visibleRect) TakeFrameInline() {
            GetWindowRect(handle, out RECT rawRect);
            DwmGetWindowAttribute(handle, DWMWA_EXTENDED_FRAME_BOUNDS, out RECT visibleRect,
                Marshal.SizeOf(typeof(RECT)));

            int rawWidth = rawRect.Right - rawRect.Left;
            int rawHeight = rawRect.Bottom - rawRect.Top;

            var bmp = new Bitmap(rawWidth, rawHeight, PixelFormat.Format32bppArgb);
            var sw = Stopwatch.StartNew();
            using (var gfx = Graphics.FromImage(bmp)) {
                var hdc = gfx.GetHdc();
                try {
                    PrintWindow(handle, hdc, PW_RENDERFULLCONTENT);
                } finally {
                    gfx.ReleaseHdc(hdc);
                }
            }
            Profiler.Record("Capture", "PrintWindow (inline fallback)", sw.ElapsedMilliseconds);

            return (bmp, rawRect, visibleRect);
        }

        /// <summary>
        ///     Returns the most recently captured frame, cropped to the visible content area.
        ///     If the background capture thread is not running, starts it, takes one frame
        ///     inline, and stops the thread after returning.
        /// </summary>
        public Image CaptureWindow() {
            if (handle == IntPtr.Zero)
                throw new DofusProcessDetachedException("Handle of window to capture is invalid.");

            bool startedHere = false;
            if (!running) {
                StartCapturing();
                startedByCaptureWindow = true;
                startedHere = true;
            }

            BeginScreenshot?.Invoke(this, EventArgs.Empty);

            var totalSw = Stopwatch.StartNew();

            Bitmap frame;
            RECT rawRect, visibleRect;

            lock (frameLock) {
                if (latestFrame != null) {
                    frame = latestFrame;
                    rawRect = latestRawRect;
                    visibleRect = latestVisibleRect;
                    latestFrame = null;
                } else {
                    frame = null!;
                    rawRect = default;
                    visibleRect = default;
                }
            }

            if (frame == null) {
                Log.Info("[Win32Capture] No background frame available — taking frame inline.");
                (frame, rawRect, visibleRect) = TakeFrameInline();
            }

            if (!_firstValidFrameLogged)
                LogPixelStats(frame);

            int rawWidth  = rawRect.Right  - rawRect.Left;
            int rawHeight = rawRect.Bottom - rawRect.Top;
            int borderLeft  = visibleRect.Left  - rawRect.Left;
            int borderTop   = visibleRect.Top   - rawRect.Top;
            int borderRight = rawRect.Right - visibleRect.Right;

            var cropSw = Stopwatch.StartNew();
            int cropLeft = borderLeft + xOffsetLeft;
            int cropTop  = borderTop  + yOffset();
            int cropW    = rawWidth  - borderLeft - borderRight - xOffsetLeft - xOffsetRight;
            int cropH    = rawHeight - cropTop;
            var cropRect = new Rectangle(cropLeft, cropTop, cropW, cropH);
            var result = frame.Clone(cropRect, frame.PixelFormat);
            frame.Dispose();
            Profiler.Record("Capture", "crop", cropSw.ElapsedMilliseconds);
            Profiler.Record("Capture", "total", totalSw.ElapsedMilliseconds);

            EndScreenshot?.Invoke(this, EventArgs.Empty);

            if (startedHere && startedByCaptureWindow)
                StopCapturing();

            return result;
        }

        private void LogPixelStats(Bitmap bmp) {
            const int gridSize = 5; // 5x5 = 25 sample points
            var lumas = new int[gridSize * gridSize];
            long sum = 0;

            for (int row = 0; row < gridSize; row++) {
                for (int col = 0; col < gridSize; col++) {
                    int px = (int)((col + 0.5f) / gridSize * bmp.Width);
                    int py = (int)((row + 0.5f) / gridSize * bmp.Height);
                    var c = bmp.GetPixel(px, py);
                    int luma = (c.R + c.G + c.B) / 3;
                    lumas[row * gridSize + col] = luma;
                    sum += luma;
                }
            }

            int total = lumas.Length;
            double mean = (double)sum / total;
            double variance = 0;
            foreach (var l in lumas) variance += (l - mean) * (l - mean);
            double stddev = Math.Sqrt(variance / total);

            const double validThreshold = 10.0;
            bool isValid = stddev >= validThreshold;

            if (isValid) {
                _firstValidFrameLogged = true;
                Log.Info($"[Win32Capture] First valid frame ({bmp.Width}x{bmp.Height}): " +
                         $"sum={sum}, mean={mean:F1}, stddev={stddev:F1} — looks like real content.");
            } else if (_invalidFrameLogThrottle.ElapsedMilliseconds >= 1000) {
                _invalidFrameLogThrottle.Restart();
                Log.Warn($"[Win32Capture] Frame content looks invalid ({bmp.Width}x{bmp.Height}): " +
                         $"sum={sum}, mean={mean:F1}, stddev={stddev:F1} (threshold={validThreshold}).");
            }
        }

        public void Dispose() {
            StopCapturing();
        }

        public int yOffset() {
            int borderHeight = 0;

            mainForm.Invoke(() => {
                var formScreenLocation = mainForm.WindowState == FormWindowState.Maximized ? Point.Empty : mainForm.Location;

                // Get the panel's screen position
                var panelScreenLocation = dofusClientPanel.PointToScreen(dofusClientPanel.Location);

                // Calculate the window border size (top and left borders)
                borderHeight = panelScreenLocation.Y - formScreenLocation.Y;
            });

            return borderHeight;
        }
    }
}
