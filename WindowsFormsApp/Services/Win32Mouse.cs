using System.Threading;

namespace WindowsFormsApp.Services
{
    public class Win32Mouse : Mouse
    {
        public void DoubleClick(int x, int y) {
            MoveCursor(x, y);
            Thread.Sleep(50);
            
            Win32.MouseOperations.MouseEvent(Win32.MouseOperations.MouseEventFlags.LeftDown);
            Win32.MouseOperations.MouseEvent(Win32.MouseOperations.MouseEventFlags.LeftUp);

            Thread.Sleep(50);
            Win32.MouseOperations.MouseEvent(Win32.MouseOperations.MouseEventFlags.LeftDown);
            Win32.MouseOperations.MouseEvent(Win32.MouseOperations.MouseEventFlags.LeftUp);
        }

        public void MoveCursor(int x, int y) {
            Win32.MouseOperations.SetCursorPos(x, y);
        }
    }
}