using System;
using System.Drawing;

namespace Inkybot.Contracts
{
    public interface ScreenCapture
    {
        Image CaptureWindow(IntPtr handle);
    }
}
