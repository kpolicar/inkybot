using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Exceptions;
using ScreenRecorderLib;
using ImageFormat = ScreenRecorderLib.ImageFormat;

namespace Inkybot
{
    /// <summary>
    ///     Provides functions to capture the entire screen, or a particular window, and save it to a file.
    /// </summary>
    public class Win32ScreenCapture : ScreenCapture, IDisposable
    {
        private Recorder recorder;
        private IntPtr handle;
        private Form mainForm;
        private Panel dofusClientPanel;
        private Semaphore capturingWindow = new Semaphore(1, 1);
        private int cachedYOffset;

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
                IsCursorCaptureEnabled = false,
            };

            var opts = new RecorderOptions {
                SourceOptions = new SourceOptions {
                    RecordingSources = { { source } },
                },
                AudioOptions = new AudioOptions { IsInputDeviceEnabled = false, IsOutputDeviceEnabled = false, IsAudioEnabled = false },
                SnapshotOptions = new SnapshotOptions() { SnapshotFormat = ImageFormat.BMP },
                OutputOptions = new OutputOptions() {
                    RecorderMode = RecorderMode.Video,
                },
                MouseOptions = new MouseOptions() {
                    IsMousePointerEnabled = false,
                    IsMouseClicksDetected = false,
                },
            };

            this.recorder = Recorder.CreateRecorder(opts);

            recorder.OnRecordingFailed += (sender, args) => {
                Console.WriteLine(args.Error);
            };

            this.handle = handle;
            RefreshYOffset();
            mainForm.SizeChanged += (_, _) => RefreshYOffset();
            mainForm.Move += (_, _) => RefreshYOffset();

            // Start continuous recording so the DXGI pipeline stays warm.
            // TakeSnapshot() can then grab individual BMP frames without restarting DXGI each time.
            recorder.Record(new NullStream());
        }

        /// <summary>
        ///     Creates an Image object containing a screen shot of a specific window
        /// </summary>
        public Image CaptureWindow() {
            var handle = this.handle;

            if (handle == IntPtr.Zero)
                throw new DofusProcessDetachedException("Handle of window to capture is invalid.");

            BeginScreenshot?.Invoke(this, EventArgs.Empty);
            capturingWindow.WaitOne();

            using var mstream = new MemoryStream();
            if (!recorder.TakeSnapshot(mstream) || mstream.Length == 0)
                throw new DofusProcessDetachedException("Failed to capture frame from Dofus window.");

            capturingWindow.Release();
            EndScreenshot?.Invoke(this, EventArgs.Empty);

            mstream.Seek(0, SeekOrigin.Begin);
            using var bmp = new Bitmap(mstream);
            var cropRect = new Rectangle(xOffsetLeft, cachedYOffset,
                bmp.Width - xOffsetRight - xOffsetLeft, bmp.Height - cachedYOffset);
            return bmp.Clone(cropRect, bmp.PixelFormat);
        }

        private void RefreshYOffset() {
            mainForm.Invoke(() => {
                var formScreenLocation = mainForm.WindowState == FormWindowState.Maximized
                    ? Point.Empty
                    : mainForm.Location;
                var panelScreenLocation = dofusClientPanel.PointToScreen(dofusClientPanel.Location);
                cachedYOffset = panelScreenLocation.Y - formScreenLocation.Y;
            });
        }

        public int yOffset() => cachedYOffset;

        public void Dispose() {
            recorder?.Stop();
            if (recorder != null)
                recorder.Dispose();
        }

        /// <summary>
        /// Discards all video bytes written by ScreenRecorderLib while keeping the MP4 writer happy
        /// (it expects a seekable stream to patch container headers).
        /// </summary>
        private sealed class NullStream : Stream
        {
            private long _position;
            public override bool CanRead => false;
            public override bool CanSeek => true;
            public override bool CanWrite => true;
            public override long Length => _position;
            public override long Position { get => _position; set => _position = value; }
            public override void Flush() { }
            public override int Read(byte[] buffer, int offset, int count) => 0;
            public override long Seek(long offset, SeekOrigin origin) =>
                _position = origin switch {
                    SeekOrigin.Begin => offset,
                    SeekOrigin.Current => _position + offset,
                    SeekOrigin.End => _position + offset,
                    _ => throw new ArgumentOutOfRangeException(nameof(origin))
                };
            public override void SetLength(long value) => _position = value;
            public override void Write(byte[] buffer, int offset, int count) => _position += count;
        }
    }
}
