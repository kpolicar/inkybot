using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static InkybotHook.NativeMethods;

namespace InkybotHook
{
    public partial class InjectionEntryPoint
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GetCursorPosDelegate(out POINT lpPoint);
        private GetCursorPosDelegate _originalGetCursorPos;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool IsIconicDelegate(IntPtr hWnd);
        private IsIconicDelegate _originalIsIconic;

        /// <summary>
        /// Installs the GetCursorPos and IsIconic hooks into the target process.
        /// Returns all successfully installed hooks.
        /// </summary>
        private List<EasyHook.LocalHook> InstallMousePositionHooks()
        {
            var hooks = new List<EasyHook.LocalHook>();

            var getCursorPosHook = TryInstallHook<GetCursorPosDelegate>(
                "GetCursorPos", new GetCursorPosDelegate(HookedGetCursorPos), out _originalGetCursorPos);
            if (getCursorPosHook != null) hooks.Add(getCursorPosHook);

            var isIconicHook = TryInstallHook<IsIconicDelegate>(
                "IsIconic", new IsIconicDelegate(HookedIsIconic), out _originalIsIconic);
            if (isIconicHook != null) hooks.Add(isIconicHook);

            hooks.AddRange(InstallMessageHooks());

            return hooks;
        }

        private bool HookedGetCursorPos(out POINT lpPoint)
        {
            LogFirstCall("GetCursorPos");
            try
            {
                if (!IsCursorOverrideActive)
                    return _originalGetCursorPos(out lpPoint);
                lpPoint = GetFixedScreenPoint();
                return true;
            }
            catch
            {
                lpPoint = new POINT();
                return false;
            }
        }

        private bool HookedIsIconic(IntPtr hWnd)
        {
            LogFirstCall("IsIconic");
            return false;
        }
    }
}
