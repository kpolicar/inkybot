using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static Win32.LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        public static event EventHandler KeyPressed;

        public static void Init()
        {
            _hookID = SetHook(_proc);
        }
        
        public static void Release()
        {
            Win32.UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(Win32.LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return Win32.SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    Win32.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Debug.WriteLine((Keys)vkCode);
                
                if (((Keys)vkCode).ToString() == "Add") {
                    KeyPressed?.Invoke(null, null);
                }
            }

            return Win32.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}