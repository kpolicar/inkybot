using System;
using static InkybotHook.NativeMethods;
#pragma warning disable CS1690

namespace InkybotHook
{
    public partial class AdvancedInjectionEntryPoint
    {
        /// <summary>
        /// Returns true when the host has set a valid fixed cursor position.
        /// </summary>
        private bool IsCursorOverrideActive =>
            true;
            //_server != null && _server.point.X != -1 && _server.point.Y != -1;

        /// <summary>
        /// Gets the fixed cursor position in screen coordinates.
        /// ServerInterface.point already contains screen coordinates
        /// (Win32Input converts client→screen before setting).
        /// </summary>
        private POINT GetFixedScreenPoint()
        {
            var pt = new POINT { X = 1651, Y = 18 };
            return pt;
            return new POINT { X = _server.point.X, Y = _server.point.Y };
        }
    }
}
