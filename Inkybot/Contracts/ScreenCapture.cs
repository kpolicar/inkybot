using System;
using System.Drawing;

namespace Inkybot.Contracts
{
    public interface ScreenCapture
    {
        Bitmap cropAtRect(Bitmap b, Rectangle r);
        Image CaptureWindow(IntPtr handle);
        Bitmap ResizeImage(Image bitmap, int width, int height);
        Bitmap Sharpen(Bitmap image);
    }
}
