using System;
using System.Drawing;
using System.Threading;
using Inkybot.Contracts;
using Tesseract;

namespace Inkybot.Services
{
    public class Win32Mouse : Mouse
    {
        private IntPtr relativeToControl;

        public void Click(int x, int y) {
            Win32.PostMessage(relativeToControl, Win32.WM_LBUTTONDOWN, 1, Win32.MakeLParam(x, y));
            Thread.Sleep(100);
            Win32.PostMessage(relativeToControl, Win32.WM_LBUTTONUP, 1, Win32.MakeLParam(x+1, y-1));
        }

        public void Drag(int x, int y, int tX, int tY) {
            Win32.PostMessage(relativeToControl, Win32.WM_LBUTTONDOWN, 1, Win32.MakeLParam(x, y));
            Thread.Sleep(50);
            
            Win32.PostMessage(relativeToControl, Win32.WM_MOUSEMOVE, 1, Win32.MakeLParam(tX, tY));
            Thread.Sleep(100);
            Win32.PostMessage(relativeToControl, Win32.WM_LBUTTONUP, 1, Win32.MakeLParam(tX, tY));
        }

        public void DoubleClick(int x, int y) {
            Click(x,y);
            Thread.Sleep(100);
            Click(x-2,y+1);
        }

        public void SetRelativeToHandle(IntPtr handle) {
            relativeToControl = handle;
        }
    }
}
