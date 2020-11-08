using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Contracts;

namespace Inkybot.Services
{
    public class Win32Input : Input
    {
        private IntPtr relativeToControl;

        public void Click(int x, int y) {
            Win32.PostMessage(relativeToControl, Win32.WM_MOUSEMOVE, 1, Win32.MakeLParam(x, y));
            Win32.PostMessage(relativeToControl, Win32.WM_LBUTTONDOWN, 1, Win32.MakeLParam(x, y));
            Thread.Sleep(100);
            Win32.PostMessage(relativeToControl, Win32.WM_MOUSEMOVE, 1, Win32.MakeLParam(x, y));
            Win32.PostMessage(relativeToControl, Win32.WM_LBUTTONUP, 1, Win32.MakeLParam(x, y));
        }

        public void Drag(int x, int y, int tX, int tY) {
            Win32.PostMessage(relativeToControl, Win32.WM_LBUTTONDOWN, 1, Win32.MakeLParam(x, y));
            Thread.Sleep(50);
            
            Win32.PostMessage(relativeToControl, Win32.WM_MOUSEMOVE, 1, Win32.MakeLParam(tX, tY));
            Thread.Sleep(100);
            Win32.PostMessage(relativeToControl, Win32.WM_MOUSEMOVE, 1, Win32.MakeLParam(tX, tY));
            Win32.PostMessage(relativeToControl, Win32.WM_LBUTTONUP, 1, Win32.MakeLParam(tX, tY));
        }

        public void DoubleClick(int x, int y) {
            Click(x,y);
            Thread.Sleep(100);
            Click(x,y);
        }

        public void TypeMessage(string message) {
            foreach (var character in message) {
                Win32.PostMessage(relativeToControl, 
                    Win32.WM_CHAR, 
                    (IntPtr) character, 
                    IntPtr.Zero);
                Thread.Sleep(100);
            }
            
            Win32.PostMessage(relativeToControl, 
                Win32.WM_KEYUP, 
                (IntPtr) Keys.Right, 
                IntPtr.Zero);
        }
        
        public void CtrlDoubleClick(int x, int y) {
            Win32.PostMessage(relativeToControl, Win32.WM_KEYDOWN, (IntPtr) Keys.ControlKey, IntPtr.Zero);
            Win32.PostMessage(relativeToControl, Win32.WM_KEYDOWN, (IntPtr) Keys.RControlKey, IntPtr.Zero);
            DoubleClick(x, y);
            Thread.Sleep(100);
            Win32.PostMessage(relativeToControl, Win32.WM_KEYUP, (IntPtr) Keys.ControlKey, IntPtr.Zero);
            Win32.PostMessage(relativeToControl, Win32.WM_KEYUP, (IntPtr) Keys.RControlKey, IntPtr.Zero);
        }

        public void SetRelativeToHandle(IntPtr handle) {
            relativeToControl = handle;
        }

        public void SelectAll() {
            Win32.PostMessage(relativeToControl, Win32.WM_KEYDOWN, (IntPtr) Keys.ControlKey, IntPtr.Zero);
            Win32.PostMessage(relativeToControl, Win32.WM_KEYDOWN, (IntPtr) Keys.RControlKey, IntPtr.Zero);
            Thread.Sleep(50);
            Win32.PostMessage(relativeToControl, 
                Win32.WM_KEYDOWN, 
                (IntPtr) 'A', 
                IntPtr.Zero);
            Thread.Sleep(50);
            Win32.PostMessage(relativeToControl, Win32.WM_KEYUP, (IntPtr) Keys.ControlKey, IntPtr.Zero);
            Win32.PostMessage(relativeToControl, Win32.WM_KEYUP, (IntPtr) Keys.RControlKey, IntPtr.Zero);
            
        }
    }
}
