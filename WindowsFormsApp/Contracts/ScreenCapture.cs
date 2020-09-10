using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace WindowsFormsApp
{
    public interface ScreenCapture
    {
        Bitmap cropAtRect(Bitmap b, Rectangle r);
        Image CaptureWindow(IntPtr handle);
    }
}