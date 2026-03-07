using System;
using static InkybotHook.NativeMethods;
#pragma warning disable CS1690

namespace InkybotHook
{
    public partial class AdvancedInjectionEntryPoint
    {
        private bool HookedGetCursorPos(out POINT lpPoint)
        {
            if (_disposing) return _originalGetCursorPos(out lpPoint);
            try
            {
                if (IsCursorOverrideActive)
                {
                    lpPoint = GetFixedScreenPoint();
                    LogFirstCall("GetCursorPos:Override");
                    return true;
                }
                return _originalGetCursorPos(out lpPoint);
            }
            catch (Exception ex)
            {
                if (!_disposing) QueueMessage($"[EXCEPTION in HookedGetCursorPos] {ex}");
                lpPoint = default;
                return true;
            }
        }

        private bool HookedIsIconic(IntPtr hWnd)
        {
            if (_disposing) return _originalIsIconic(hWnd);
            try
            {
                LogFirstCall("IsIconic");
                return false;
            }
            catch (Exception ex)
            {
                if (!_disposing) QueueMessage($"[EXCEPTION in HookedIsIconic] {ex}");
                return false;
            }
        }

        private short HookedGetAsyncKeyState(int vKey)
        {
            if (_disposing) return _originalGetAsyncKeyState(vKey);
            try
            {
                if (!_allHooksInstalled.Wait(5000)) return _originalGetAsyncKeyState(vKey);
                LogFirstCall("GetAsyncKeyState");

                if (vKey == VK_LBUTTON && _rawState == ForgeState.ButtonDown)
                {
                    LogFirstCall("GetAsyncKeyState:Spoofed");
                    return unchecked((short)0x8000);
                }
                return _originalGetAsyncKeyState(vKey);
            }
            catch (Exception ex)
            {
                if (!_disposing) QueueMessage($"[EXCEPTION in HookedGetAsyncKeyState] {ex}");
                return _originalGetAsyncKeyState(vKey);
            }
        }

        private short HookedGetKeyState(int nVirtKey)
        {
            if (_disposing) return _originalGetKeyState(nVirtKey);
            try
            {
                if (!_allHooksInstalled.Wait(5000)) return _originalGetKeyState(nVirtKey);
                LogFirstCall("GetKeyState");

                if (nVirtKey == VK_LBUTTON && _rawState == ForgeState.ButtonDown)
                {
                    LogFirstCall("GetKeyState:Spoofed");
                    return unchecked((short)0x8000);
                }
                return _originalGetKeyState(nVirtKey);
            }
            catch (Exception ex)
            {
                if (!_disposing) QueueMessage($"[EXCEPTION in HookedGetKeyState] {ex}");
                return _originalGetKeyState(nVirtKey);
            }
        }

        private IntPtr HookedSetCapture(IntPtr hWnd)
        {
            if (_disposing) return _originalSetCapture(hWnd);
            try
            {
                if (!_allHooksInstalled.Wait(5000)) return _originalSetCapture(hWnd);
                LogFirstCall("SetCapture");
                if (IsCursorOverrideActive) return IntPtr.Zero;
                return _originalSetCapture(hWnd);
            }
            catch (Exception ex)
            {
                if (!_disposing) QueueMessage($"[EXCEPTION in HookedSetCapture] {ex}");
                return _originalSetCapture(hWnd);
            }
        }

        private bool HookedReleaseCapture()
        {
            if (_disposing) return _originalReleaseCapture();
            try
            {
                if (!_allHooksInstalled.Wait(5000)) return _originalReleaseCapture();
                LogFirstCall("ReleaseCapture");
                if (IsCursorOverrideActive) return true;
                return _originalReleaseCapture();
            }
            catch (Exception ex)
            {
                if (!_disposing) QueueMessage($"[EXCEPTION in HookedReleaseCapture] {ex}");
                return _originalReleaseCapture();
            }
        }

        private IntPtr HookedGetCapture()
        {
            if (_disposing) return _originalGetCapture();
            try
            {
                if (!_allHooksInstalled.Wait(5000)) return _originalGetCapture();
                LogFirstCall("GetCapture");
                if (IsCursorOverrideActive && _mainHwnd != IntPtr.Zero) return _mainHwnd;
                return _originalGetCapture();
            }
            catch (Exception ex)
            {
                if (!_disposing) QueueMessage($"[EXCEPTION in HookedGetCapture] {ex}");
                return _originalGetCapture();
            }
        }
    }
}
