using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public class WindowHelpers
    {
        public static IntPtr DockProcess(Process process, Panel destination, ref IntPtr hWndDocked) {
            if (process == null || hWndDocked != IntPtr.Zero)
                return IntPtr.Zero;
      
            while (hWndDocked == IntPtr.Zero)
            {
                process.WaitForInputIdle(1000);
                process.Refresh();
                if (process.HasExited)
                    return IntPtr.Zero;
                hWndDocked = process.MainWindowHandle;
            }
            var oldParentHandle = Win32.SetParent(hWndDocked, destination.Handle);
            
            var docked = hWndDocked;
            EventHandler moveEventHandler = (object sender, EventArgs e) =>
                Win32.MoveWindow(docked, 0, 0, destination.Width, destination.Height, true);
            destination.SizeChanged += moveEventHandler;
            moveEventHandler(new object(), new EventArgs());

            return oldParentHandle;
        }
        
        public static void RemoveWindowBorders(IntPtr window)
        {
            int style = Win32.GetWindowLong(window, Win32.GWL_STYLE);
            Win32.SetWindowLong(window, Win32.GWL_STYLE, (style & ~Win32.WS_CAPTION));
        }
    }
}