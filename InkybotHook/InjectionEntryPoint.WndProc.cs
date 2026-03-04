using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static InkybotHook.NativeMethods;

namespace InkybotHook
{
    public partial class InjectionEntryPoint
    {

        // ── Delegates ──────────────────────────────────────────────────────
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private WndProcDelegate _originalWndProc;

        // ── Install ────────────────────────────────────────────────────────
        private List<EasyHook.LocalHook> InstallMessageHooks()
        {
            var hooks = new List<EasyHook.LocalHook>();
            var wndProcHook = TryInstallHook<WndProcDelegate>(
                "DefWindowProcW", new WndProcDelegate(HookedWndProc), out _originalWndProc);
            if (wndProcHook != null) hooks.Add(wndProcHook);
            return hooks;
        }

        // ── WndProc hook ─────────────────────────────────────────────────--
        private IntPtr HookedWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (msg == 0x00FF) // WM_INPUT
                {
                    if (IsRawInputClick(lParam))
                    {
                        LogFirstCall("WndProc:blocked WM_INPUT click");
                        return IntPtr.Zero;
                    }
                }
                else if (IsBlockedMouseMessage(msg))
                {
                    LogFirstCall("WndProc:blocked");
                    return IntPtr.Zero;
                }
            }
            catch (Exception ex)
            {
                LogFirstCall($"WndProc:exception {ex.GetType().Name}: {ex.Message}");
            }
            return _originalWndProc(hWnd, msg, wParam, lParam);
        }

        private static bool IsRawInputClick(IntPtr lParam)
        {
            uint dwSize = 0;
            uint headerSize = (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER));
            // First, get the size
            if (GetRawInputData(lParam, RID_INPUT, IntPtr.Zero, ref dwSize, headerSize) == 0 && dwSize > 0)
            {
                IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);
                try
                {
                    if (GetRawInputData(lParam, RID_INPUT, buffer, ref dwSize, headerSize) == dwSize)
                    {
                        RAWINPUT raw = Marshal.PtrToStructure<RAWINPUT>(buffer);
                        if (raw.header.dwType == RIM_TYPEMOUSE)
                        {
                            uint btns = raw.mouse.ulButtons;
                            if ((btns & (RI_MOUSE_LEFT_BUTTON_DOWN | RI_MOUSE_LEFT_BUTTON_UP |
                                         RI_MOUSE_RIGHT_BUTTON_DOWN | RI_MOUSE_RIGHT_BUTTON_UP |
                                         RI_MOUSE_MIDDLE_BUTTON_DOWN | RI_MOUSE_MIDDLE_BUTTON_UP |
                                         RI_MOUSE_BUTTON_1_DOWN | RI_MOUSE_BUTTON_1_UP |
                                         RI_MOUSE_BUTTON_2_DOWN | RI_MOUSE_BUTTON_2_UP |
                                         RI_MOUSE_BUTTON_3_DOWN | RI_MOUSE_BUTTON_3_UP |
                                         RI_MOUSE_WHEEL | RI_MOUSE_HWHEEL)) != 0)
                                return true;
                        }
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true for mouse/pointer button, wheel, and click messages.
        /// Move messages (WM_MOUSEMOVE, WM_NCMOUSEMOVE, WM_POINTERUPDATE) are allowed through.
        /// </summary>
        private static bool IsBlockedMouseMessage(uint msg)
        {
            // Client-area: WM_LBUTTONDOWN(0x0201)..WM_MOUSEHWHEEL(0x020E)
            // Allows WM_MOUSEMOVE (0x0200)
            if (msg >= 0x0201 && msg <= 0x020E) return true;

            // Non-client area: WM_NCLBUTTONDOWN(0x00A1)..WM_NCXBUTTONDBLCLK(0x00AD)
            // Allows WM_NCMOUSEMOVE (0x00A0)
            if (msg >= 0x00A1 && msg <= 0x00AD) return true;

            // Pointer: WM_POINTERDOWN(0x0246)..WM_POINTERWHEEL(0x0249)
            // Allows WM_POINTERUPDATE (0x0245)
            if (msg >= 0x0246 && msg <= 0x0249) return true;

            return false;
        }
    }
}
