using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Exceptions;

namespace Inkybot
{
    /// <summary>
    ///     Provides functions to capture the entire screen, or a particular window, and save it to a file.
    /// </summary>
    public class Win32ScreenCapture : ScreenCapture, IDisposable
    {
        private IntPtr handle;
        private Form mainForm;
        private Panel dofusClientPanel;
        public event EventHandler? BeginScreenshot;
        public event EventHandler? EndScreenshot;
        private int xOffsetLeft;
        private int xOffsetRight;

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

        public void BindTo(IntPtr handle, Panel dofusClientPanel, int xOffsetLeft, int xOffsetRight, Form mainForm) {
            (this.xOffsetLeft, this.xOffsetRight) = (xOffsetLeft, xOffsetRight);
            this.dofusClientPanel = dofusClientPanel;
            this.mainForm = mainForm;
            this.handle = handle;
        }

        /// <summary>
        ///     Creates an Image object containing a screen shot of a specific window
        /// </summary>
        /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>
        /// <returns></returns>
        public Image CaptureWindow() {
            var handle = this.handle;

            if (handle == IntPtr.Zero)
                throw new DofusProcessDetachedException("Handle of window to capture is invalid.");

            BeginScreenshot?.Invoke(this, EventArgs.Empty);

            var totalSw = Stopwatch.StartNew();

            // GetWindowRect includes the invisible DWM shadow/resize border.
            // DWMWA_EXTENDED_FRAME_BOUNDS returns only the visible frame, so the
            // difference gives the exact pixel insets to strip from each edge.
            GetWindowRect(handle, out RECT rawRect);
            DwmGetWindowAttribute(handle, DWMWA_EXTENDED_FRAME_BOUNDS, out RECT visibleRect,
                Marshal.SizeOf(typeof(RECT)));

            int rawWidth  = rawRect.Right  - rawRect.Left;
            int rawHeight = rawRect.Bottom - rawRect.Top;

            int borderLeft   = visibleRect.Left   - rawRect.Left;
            int borderTop    = visibleRect.Top    - rawRect.Top;
            int borderRight  = rawRect.Right  - visibleRect.Right;

            var bmp = new Bitmap(rawWidth, rawHeight, PixelFormat.Format32bppArgb);

            var captureSw = Stopwatch.StartNew();
            using (var gfx = Graphics.FromImage(bmp)) {
                var hdc = gfx.GetHdc();
                try {
                    PrintWindow(handle, hdc, PW_RENDERFULLCONTENT);
                } finally {
                    gfx.ReleaseHdc(hdc);
                }
            }
            Profiler.Record("Capture", "PrintWindow", captureSw.ElapsedMilliseconds);

            EndScreenshot?.Invoke(this, EventArgs.Empty);

            var cropSw = Stopwatch.StartNew();
            int cropLeft = borderLeft + xOffsetLeft;
            int cropTop  = borderTop  + yOffset();
            int cropW    = rawWidth  - borderLeft - borderRight - xOffsetLeft - xOffsetRight;
            int cropH    = rawHeight - cropTop;
            var cropRect = new Rectangle(cropLeft, cropTop, cropW, cropH);
            var result = bmp.Clone(cropRect, bmp.PixelFormat);
            bmp.Dispose();
            Profiler.Record("Capture", "crop", cropSw.ElapsedMilliseconds);
            Profiler.Record("Capture", "total", totalSw.ElapsedMilliseconds);

            return result;
        }

        public void Dispose() { }

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
