using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Inkybot.Contracts;
using Inkybot.Exceptions;
using ScreenRecorderLib;
using ImageFormat = ScreenRecorderLib.ImageFormat;

namespace Inkybot
{
    /// <summary>
    ///     Provides functions to capture the entire screen, or a particular window, and save it to a file.
    /// </summary>
    public class Win32ScreenCapture : ScreenCapture
    {
        private Recorder recorder;
        private IntPtr handle;
        private Semaphore waitUntilFrameRecorded;
        public event EventHandler? BeginScreenshot;
        public event EventHandler? EndScreenshot;
        
        
        public void BindTo(IntPtr handle) {
            var source = new WindowRecordingSource(handle);
            
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
                    RecorderMode = RecorderMode.Screenshot
                },
                MouseOptions = new MouseOptions() {
                    IsMousePointerEnabled = false,
                }
            };  
            this.recorder = Recorder.CreateRecorder(opts);
            this.handle = handle;
            this.waitUntilFrameRecorded = new Semaphore(1, 1);
            waitUntilFrameRecorded.WaitOne();
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
            
            recorder.OnRecordingFailed += (sender, args) => {
                Console.WriteLine(args.Error);
            };
            recorder.OnRecordingComplete += (sender, args) => {
                Console.WriteLine("finished");
            };
            recorder.OnFrameRecorded += (sender, eventArgs) => {
                Console.WriteLine("frame recorded");
                waitUntilFrameRecorded.Release();
            };
            
            recorder.Record(mstream);
            waitUntilFrameRecorded.WaitOne();
            recorder.Stop();
            
            EndScreenshot?.Invoke(this, EventArgs.Empty);

            mstream.Position = 0;
            return Image.FromStream(mstream);
        }
    }
}
