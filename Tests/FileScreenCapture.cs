#nullable enable
using System;
using System.Drawing;
using Inkybot.Contracts;

namespace Tests
{
    public class FileScreenCapture : ScreenCapture
    {
        private readonly string path;
        public event EventHandler? BeginScreenshot;
        public event EventHandler? EndScreenshot;

        public FileScreenCapture(string path) {
            this.path = path;
        }
        
        public Image CaptureWindow(IntPtr handle) {
            return Image.FromFile(path);
        }
    }
}
