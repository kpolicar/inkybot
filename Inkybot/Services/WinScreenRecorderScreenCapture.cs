using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Inkybot.Contracts;

namespace Inkybot
{
    /// <summary>
    ///     Orchestrator that tries <see cref="WinGraphicsCaptureScreenCapture"/> first and permanently
    ///     switches to <see cref="Win32ScreenCapture"/> (PrintWindow) if the GPU pipeline
    ///     produces blank frames on every one of the first 10 captures.
    /// </summary>
    public class WinScreenRecorderScreenCapture : ScreenCapture, IDisposable
    {
        public event EventHandler? BeginScreenshot;
        public event EventHandler? EndScreenshot;

        private WinGraphicsCaptureScreenCapture gpuCapture = null!;
        private Win32ScreenCapture printWindowCapture = null!;

        private bool useFallback = false;
        private int blankChecksRemaining = 10;

        public void BindTo(IntPtr handle, Panel dofusClientPanel, int xOffsetLeft, int xOffsetRight, Form mainForm)
        {
            gpuCapture = new WinGraphicsCaptureScreenCapture();
            gpuCapture.BindTo(handle, dofusClientPanel, xOffsetLeft, xOffsetRight, mainForm);

            printWindowCapture = new Win32ScreenCapture();
            printWindowCapture.BindTo(handle, dofusClientPanel, xOffsetLeft, xOffsetRight, mainForm);
        }

        public Image CaptureWindow()
        {
            BeginScreenshot?.Invoke(this, EventArgs.Empty);

            Image result;

            if (useFallback)
            {
                result = printWindowCapture.CaptureWindow();
            }
            else
            {
                result = gpuCapture.CaptureWindow();

                if (blankChecksRemaining > 0)
                {
                    blankChecksRemaining--;
                    if (IsBlankImage((Bitmap)result))
                    {
                        Debug.WriteLine("[WinScreenRecorderScreenCapture] Blank frame detected — switching permanently to PrintWindow fallback.");
                        result.Dispose();
                        useFallback = true;
                        gpuCapture.Dispose();
                        result = printWindowCapture.CaptureWindow();
                    }
                }
            }

            EndScreenshot?.Invoke(this, EventArgs.Empty);
            return result;
        }

        /// <summary>
        ///     Returns true when all sampled pixels share nearly the same color (range &lt; 15 per channel),
        ///     indicating the capture produced a blank or solid-color frame.
        /// </summary>
        private static bool IsBlankImage(Bitmap bmp)
        {
            const int steps = 6; // 6×6 = 36 samples
            int stepX = Math.Max(1, bmp.Width  / steps);
            int stepY = Math.Max(1, bmp.Height / steps);

            int minR = 255, maxR = 0;
            int minG = 255, maxG = 0;
            int minB = 255, maxB = 0;

            for (int x = stepX / 2; x < bmp.Width; x += stepX)
            for (int y = stepY / 2; y < bmp.Height; y += stepY)
            {
                var c = bmp.GetPixel(Math.Min(x, bmp.Width - 1), Math.Min(y, bmp.Height - 1));
                if (c.R < minR) minR = c.R; if (c.R > maxR) maxR = c.R;
                if (c.G < minG) minG = c.G; if (c.G > maxG) maxG = c.G;
                if (c.B < minB) minB = c.B; if (c.B > maxB) maxB = c.B;
            }

            return (maxR - minR) < 15 && (maxG - minG) < 15 && (maxB - minB) < 15;
        }

        public void Dispose()
        {
            gpuCapture?.Dispose();
            printWindowCapture?.Dispose();
        }
    }
}
