using System;
using static InkybotHook.NativeMethods;
#pragma warning disable CS1690

namespace InkybotHook
{
    public partial class InjectionEntryPoint
    {
        /// <summary>
        /// Returns true when the host has set a valid fixed cursor position.
        /// </summary>
        private bool IsCursorOverrideActive =>
           _server != null && _server.point.X != -1 && _server.point.Y != -1;

        /// <summary>
        /// Gets the fixed cursor position in screen coordinates by converting
        /// the client-relative point from ServerInterface using ClientToScreen.
        /// </summary>
        private POINT GetFixedScreenPoint()
        {
            var pt = new POINT { X = _server.point.X, Y = _server.point.Y };
            try
            {
                IntPtr hwnd = _server.targetHwnd;
                if (hwnd != IntPtr.Zero)
                {
                    ClientToScreen(hwnd, ref pt);
                }
            }
            catch { }
            return pt;
        }
    }
}
