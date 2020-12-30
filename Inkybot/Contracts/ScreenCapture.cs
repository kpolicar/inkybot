using System;
using System.Drawing;

namespace Inkybot.Contracts
{
    public interface ScreenCapture
    {
        public event EventHandler? BeginScreenshot;
        public event EventHandler? EndScreenshot;
        
        Image CaptureWindow(IntPtr handle);
    }
}
