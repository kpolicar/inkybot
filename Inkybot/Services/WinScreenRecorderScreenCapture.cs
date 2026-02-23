using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using ImageMagick;
using ImageMagick.Factories;
using Inkybot.Contracts;
using Inkybot.Exceptions;
using ScreenRecorderLib;
using ImageFormat = ScreenRecorderLib.ImageFormat;

namespace Inkybot
{
    /// <summary>
    ///     Provides functions to capture the entire screen, or a particular window, and save it to a file.
    /// </summary>
    public class WinScreenRecorderScreenCapture : ScreenCapture, IDisposable
    {
        private Recorder recorder;
        private IntPtr handle;
        private Form mainForm;
        private Panel dofusClientPanel;
        private Semaphore waitUntilFrameRecorded = new Semaphore(0, 1);
        private Semaphore capturingWindow = new Semaphore(1, 1);
        public event EventHandler? BeginScreenshot;
        public event EventHandler? EndScreenshot;
        private int xOffsetLeft;
        private int xOffsetRight;
        
        public void BindTo(IntPtr handle, Panel dofusClientPanel, int xOffsetLeft, int xOffsetRight, Form mainForm) {
            (this.xOffsetLeft, this.xOffsetRight) = (xOffsetLeft, xOffsetRight);
            this.dofusClientPanel = dofusClientPanel;
            this.mainForm = mainForm;
            var source = new WindowRecordingSource {
                Handle = handle,
                IsBorderRequired = false,
                IsCursorCaptureEnabled = false
            };
            
            var opts = new RecorderOptions
            {
                SourceOptions = new SourceOptions
                {
                    RecordingSources = { { source } },
                    // RecordingSources = { { new DisplayRecordingSource(DisplayRecordingSource.MainMonitor) } },
                },
                AudioOptions = new AudioOptions { IsInputDeviceEnabled = false, IsOutputDeviceEnabled = false, IsAudioEnabled=false },
                SnapshotOptions = new SnapshotOptions() {SnapshotFormat = ImageFormat.PNG},
                OutputOptions = new OutputOptions() {
                    RecorderMode = RecorderMode.Screenshot,
                },
                MouseOptions = new MouseOptions() {
                    IsMousePointerEnabled = false,
                    IsMouseClicksDetected = false,
                },
            };  
            this.recorder = Recorder.CreateRecorder(opts);
            
            recorder.OnRecordingFailed += (sender, args) => {
                Console.WriteLine(args.Error);
                waitUntilFrameRecorded.Release();
            };
            recorder.OnRecordingComplete += (sender, args) => {
                waitUntilFrameRecorded.Release();
            };
            
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

            using var mstream = new MemoryStream();

            var totalSw = Stopwatch.StartNew();

            capturingWindow.WaitOne();
            var yOffset = this.yOffset();

            var recordSw = Stopwatch.StartNew();
            recorder.Record(mstream);
            waitUntilFrameRecorded.WaitOne();
            recorder.Stop();
            Profiler.Record("Capture", "record", recordSw.ElapsedMilliseconds);
            capturingWindow.Release();

            EndScreenshot?.Invoke(this, EventArgs.Empty);

            mstream.Seek(0, SeekOrigin.Begin);

            var decodeSw = Stopwatch.StartNew();
            using var newImage = new MagickImage(mstream);
            Profiler.Record("Capture", "decode", decodeSw.ElapsedMilliseconds);

            var cropSw = Stopwatch.StartNew();
            newImage.Crop(new MagickGeometry(xOffsetLeft, yOffset, (uint)(newImage.Width-xOffsetRight-xOffsetLeft), (uint)(newImage.Height-yOffset)));
            var result = newImage.ToBitmap();
            Profiler.Record("Capture", "crop+convert", cropSw.ElapsedMilliseconds);
            Profiler.Record("Capture", "total", totalSw.ElapsedMilliseconds);

            return result;
        }

        public void Dispose() {
            if (recorder!=null)
                recorder.Dispose();
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
