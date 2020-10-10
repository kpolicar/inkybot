using System;
using System.Threading;
using WindowsFormsApp.Contracts;

namespace WindowsFormsApp.Services
{
    public class Win32Mouse : Mouse
    {
        private IntPtr relativeToControl;

        public void Click(int x, int y) {
            MoveCursor(x, y);
            Thread.Sleep(50);

            Win32.MouseOperations.MouseEvent(Win32.MouseOperations.MouseEventFlags.LeftDown);
            Thread.Sleep(10);
            Win32.MouseOperations.MouseEvent(Win32.MouseOperations.MouseEventFlags.LeftUp);
        }

        public void DoubleClick(int x, int y) {
            MoveCursor(x, y);
            Thread.Sleep(50);

            Win32.MouseOperations.MouseEvent(Win32.MouseOperations.MouseEventFlags.LeftDown);
            Thread.Sleep(10);
            Win32.MouseOperations.MouseEvent(Win32.MouseOperations.MouseEventFlags.LeftUp);

            Thread.Sleep(50);
            Win32.MouseOperations.MouseEvent(Win32.MouseOperations.MouseEventFlags.LeftDown);
            Thread.Sleep(10);
            Win32.MouseOperations.MouseEvent(Win32.MouseOperations.MouseEventFlags.LeftUp);
        }

        public void MoveCursor(int x, int y) {
            var windowPostion = new Win32.Rect();
            Win32.GetWindowRect(relativeToControl, ref windowPostion);

            Win32.MouseOperations.SetCursorPos(windowPostion.Left + x, windowPostion.Top + y);
        }

        public void SetRelativeToHandle(IntPtr handle) {
            relativeToControl = handle;
        }
    }
}
