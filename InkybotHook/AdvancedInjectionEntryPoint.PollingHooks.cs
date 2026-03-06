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
                bool result = _originalGetCursorPos(out lpPoint);
                if (IsCursorOverrideActive)
                {
                    var fixedPt = GetFixedScreenPoint();
                    lpPoint.X = fixedPt.X;
                    lpPoint.Y = fixedPt.Y;
                    LogFirstCall("GetCursorPos:Override");
                }
                return result;
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
                _allHooksInstalled.Wait();
                LogFirstCall("GetAsyncKeyState");
                short realState = _originalGetAsyncKeyState(vKey);
                if (vKey == VK_LBUTTON && _rawState == ForgeState.ButtonDown)
                {
                    LogFirstCall("GetAsyncKeyState:Spoofed");
                    return (short)(realState | unchecked((short)0x8000));
                }
                return realState;
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
                _allHooksInstalled.Wait();
                LogFirstCall("GetKeyState");
                short realState = _originalGetKeyState(nVirtKey);
                if (nVirtKey == VK_LBUTTON && _rawState == ForgeState.ButtonDown)
                {
                    LogFirstCall("GetKeyState:Spoofed");
                    return (short)(realState | unchecked((short)0x8000));
                }
                return realState;
            }
            catch (Exception ex)
            {
                if (!_disposing) QueueMessage($"[EXCEPTION in HookedGetKeyState] {ex}");
                return _originalGetKeyState(nVirtKey);
            }
        }
    }
}
