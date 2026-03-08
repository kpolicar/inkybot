using System;
using static InkybotHook.NativeMethods;

namespace InkybotHook
{
    public partial class InjectionEntryPoint
    {
        private bool HookedGetCursorPos(out POINT lpPoint)
        {
            _allHooksInstalled.Wait();
            if (IsInputOverriden)
            {
                lpPoint = GetFixedScreenPoint();
                return true;
            }
            return _originalGetCursorPos(out lpPoint);
        }

        private short HookedGetAsyncKeyState(int vKey)
        {
            _allHooksInstalled.Wait();
            short realState = _originalGetAsyncKeyState(vKey);
            if (vKey == VK_LBUTTON && _rawState == ForgeState.ButtonDown)
                return (short)(realState | unchecked((short)0x8000));
            return realState;
        }

        private short HookedGetKeyState(int nVirtKey)
        {
            _allHooksInstalled.Wait();
            short realState = _originalGetKeyState(nVirtKey);
            if (nVirtKey == VK_LBUTTON && _rawState == ForgeState.ButtonDown)
                return (short)(realState | unchecked((short)0x8000));
            return realState;
        }
    }
}
