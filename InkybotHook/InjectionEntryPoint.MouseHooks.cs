using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static InkybotHook.NativeMethods;

namespace InkybotHook
{
    public partial class InjectionEntryPoint
    {
        // =========================================================
        // 1. DELEGATES
        // =========================================================
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GetCursorPosDelegate(out POINT lpPoint);
        private GetCursorPosDelegate _originalGetCursorPos;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GetCursorInfoDelegate(ref CURSORINFO pci);
        private GetCursorInfoDelegate _originalGetCursorInfo;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool IsIconicDelegate(IntPtr hWnd);
        private IsIconicDelegate _originalIsIconic;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate bool PeekMessageWDelegate(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        private PeekMessageWDelegate _originalPeekMessageW;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate int GetMessageWDelegate(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        private GetMessageWDelegate _originalGetMessageW;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GetRawInputDataDelegate(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);
        private GetRawInputDataDelegate _originalGetRawInputData;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GetRawInputBufferDelegate(IntPtr pData, ref uint pcbSize, uint cbSizeHeader);
        private GetRawInputBufferDelegate _originalGetRawInputBuffer;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate short GetAsyncKeyStateDelegate(int vKey);
        private GetAsyncKeyStateDelegate _originalGetAsyncKeyState;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate short GetKeyStateDelegate(int nVirtKey);
        private GetKeyStateDelegate _originalGetKeyState;

        // =========================================================
        // 2. CONSTANTS & STRUCTS
        // =========================================================
        private const uint WM_NULL = 0x0000;
        private const uint WM_INPUT = 0x00FF;
        
        // Standard Client Mouse Messages (0x0200 - 0x020E)
        private const uint WM_MOUSEFIRST = 0x0200;
        private const uint WM_MOUSELAST = 0x020E;

        // Non-Client Mouse Messages (Off-Client/Borders/Titlebar: 0x00A0 - 0x00AD)
        private const uint WM_NCMOUSEFIRST = 0x00A0;
        private const uint WM_NCMOUSELAST = 0x00AD;

        // Pointer and Touch Messages (0x0240 - 0x0257)
        // Covers WM_TOUCH, WM_NCPOINTERUPDATE, WM_POINTERDOWN, WM_POINTERUPDATE, etc.
        private const uint WM_POINTERFIRST = 0x0240;
        private const uint WM_POINTERLAST = 0x0257;

        private const uint RID_INPUT = 0x10000003;
        private const uint RIM_TYPEMOUSE = 0;
        private const int VK_LBUTTON = 0x01;
        private const int VK_RBUTTON = 0x02;

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public uint cbSize;
            public uint flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }

        // =========================================================
        // 3. INSTALLATION
        // =========================================================
        private List<EasyHook.LocalHook> InstallMousePositionHooks()
        {
            var hooks = new List<EasyHook.LocalHook>();

            // Absolute Position Locators
            var getCursorPosHook = TryInstallHook<GetCursorPosDelegate>("GetCursorPos", new GetCursorPosDelegate(HookedGetCursorPos), out _originalGetCursorPos);
            if (getCursorPosHook != null) hooks.Add(getCursorPosHook);

            var getCursorInfoHook = TryInstallHook<GetCursorInfoDelegate>("GetCursorInfo", new GetCursorInfoDelegate(HookedGetCursorInfo), out _originalGetCursorInfo);
            if (getCursorInfoHook != null) hooks.Add(getCursorInfoHook);

            // Message Pump (The Mouse & Pointer Killers)
            var peekMessageWHook = TryInstallHook<PeekMessageWDelegate>("PeekMessageW", new PeekMessageWDelegate(HookedPeekMessageW), out _originalPeekMessageW);
            if (peekMessageWHook != null) hooks.Add(peekMessageWHook);

            var getMessageWHook = TryInstallHook<GetMessageWDelegate>("GetMessageW", new GetMessageWDelegate(HookedGetMessageW), out _originalGetMessageW);
            if (getMessageWHook != null) hooks.Add(getMessageWHook);

            // Raw Input 
            var getRawInputDataHook = TryInstallHook<GetRawInputDataDelegate>("GetRawInputData", new GetRawInputDataDelegate(HookedGetRawInputData), out _originalGetRawInputData);
            if (getRawInputDataHook != null) hooks.Add(getRawInputDataHook);

            var getRawInputBufferHook = TryInstallHook<GetRawInputBufferDelegate>("GetRawInputBuffer", new GetRawInputBufferDelegate(HookedGetRawInputBuffer), out _originalGetRawInputBuffer);
            if (getRawInputBufferHook != null) hooks.Add(getRawInputBufferHook);

            // Hardware Key States
            var getAsyncKeyStateHook = TryInstallHook<GetAsyncKeyStateDelegate>("GetAsyncKeyState", new GetAsyncKeyStateDelegate(HookedGetAsyncKeyState), out _originalGetAsyncKeyState);
            if (getAsyncKeyStateHook != null) hooks.Add(getAsyncKeyStateHook);

            var getKeyStateHook = TryInstallHook<GetKeyStateDelegate>("GetKeyState", new GetKeyStateDelegate(HookedGetKeyState), out _originalGetKeyState);
            if (getKeyStateHook != null) hooks.Add(getKeyStateHook);

            // Misc
            var isIconicHook = TryInstallHook<IsIconicDelegate>("IsIconic", new IsIconicDelegate(HookedIsIconic), out _originalIsIconic);
            if (isIconicHook != null) hooks.Add(isIconicHook);

            return hooks;
        }

        // =========================================================
        // 4. IMPLEMENTATIONS
        // =========================================================

        // --- POSITION SPOOFERS ---
        private bool HookedGetCursorPos(out POINT lpPoint)
        {
            if (IsCursorOverrideActive)
            {
                lpPoint = GetFixedScreenPoint();
                return true;
            }
            return _originalGetCursorPos(out lpPoint);
        }

        private bool HookedGetCursorInfo(ref CURSORINFO pci)
        {
            bool result = _originalGetCursorInfo(ref pci);
            if (IsCursorOverrideActive && result)
            {
                // Spoof the location within the struct just like GetCursorPos
                pci.ptScreenPos = GetFixedScreenPoint();
            }
            return result;
        }

        private bool HookedIsIconic(IntPtr hWnd)
        {
            return false;
        }

        // --- THE MESSAGE PUMP HOOKS ---
        private bool HookedPeekMessageW(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            bool result = _originalPeekMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);

            if (IsCursorOverrideActive && result)
            {
                FilterMouseMessage(ref lpMsg);
            }
            return result;
        }

        private int HookedGetMessageW(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
        {
            int result = _originalGetMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);

            if (IsCursorOverrideActive && result > 0)
            {
                FilterMouseMessage(ref lpMsg);
            }
            return result;
        }

        private void FilterMouseMessage(ref MSG msg)
        {
            // Block Standard Mouse, Non-Client Mouse, Pointer/Touch, and Raw Input notifications
            bool isStandardMouse = (msg.message >= WM_MOUSEFIRST && msg.message <= WM_MOUSELAST);
            bool isNonClientMouse = (msg.message >= WM_NCMOUSEFIRST && msg.message <= WM_NCMOUSELAST);
            bool isPointerOrTouch = (msg.message >= WM_POINTERFIRST && msg.message <= WM_POINTERLAST);
            bool isRawInput = (msg.message == WM_INPUT);

            if (isStandardMouse || isNonClientMouse || isPointerOrTouch || isRawInput)
            {
                // Erase the message entirely.
                msg.message = WM_NULL;
            }
        }

        // --- THE RAW INPUT HOOKS ---
        private uint HookedGetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            uint result = _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
            if (IsCursorOverrideActive && result > 0 && result != unchecked((uint)-1) && pData != IntPtr.Zero && uiCommand == RID_INPUT)
            {
                ScrubRawInputBuffer(pData, 1);
            }
            return result;
        }

        private uint HookedGetRawInputBuffer(IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            uint result = _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);
            if (IsCursorOverrideActive && result > 0 && result != unchecked((uint)-1) && pData != IntPtr.Zero)
            {
                ScrubRawInputBuffer(pData, (int)result);
            }
            return result;
        }

        private void ScrubRawInputBuffer(IntPtr pData, int packetCount)
        {
            try
            {
                IntPtr currentPtr = pData;
                for (int i = 0; i < packetCount; i++)
                {
                    uint dwType = (uint)Marshal.ReadInt32(currentPtr, 0);
                    uint dwSize = (uint)Marshal.ReadInt32(currentPtr, 4);

                    if (dwType == RIM_TYPEMOUSE)
                    {
                        // Zero out X and Y hardware deltas
                        Marshal.WriteInt32(currentPtr, 36, 0);
                        Marshal.WriteInt32(currentPtr, 40, 0);
                        // Zero out Button Flags (kills physical hardware clicks)
                        Marshal.WriteInt16(currentPtr, 20, 0); 
                    }

                    if (dwSize > 0) currentPtr = IntPtr.Add(currentPtr, (int)dwSize);
                    else break;
                }
            }
            catch { /* Ignore memory read/write errors to prevent game crash */ }
        }

        // --- THE HARDWARE STATE HOOKS ---
        private short HookedGetAsyncKeyState(int vKey)
        {
            if (IsCursorOverrideActive && (vKey == VK_LBUTTON || vKey == VK_RBUTTON))
            {
                return 0;
            }
            return _originalGetAsyncKeyState(vKey);
        }

        private short HookedGetKeyState(int nVirtKey)
        {
            if (IsCursorOverrideActive && (nVirtKey == VK_LBUTTON || nVirtKey == VK_RBUTTON))
            {
                return 0;
            }
            return _originalGetKeyState(nVirtKey);
        }
    }
}