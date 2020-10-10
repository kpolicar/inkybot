using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static readonly Win32.LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        public static event KeyEventHandler KeyPressed;

        public static void Init() {
            _hookID = SetHook(_proc);
        }

        public static void Release() {
            Win32.UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(Win32.LowLevelKeyboardProc proc) {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule) {
                return Win32.SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    Win32.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && wParam == (IntPtr) WM_KEYDOWN) {
                var vkCode = Marshal.ReadInt32(lParam);

                KeyPressed?.Invoke(null, new KeyEventArgs((Keys) vkCode));
            }

            return Win32.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
