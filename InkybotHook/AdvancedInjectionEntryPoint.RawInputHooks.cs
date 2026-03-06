using System;
using System.Runtime.InteropServices;
using static InkybotHook.NativeMethods;

namespace InkybotHook
{
    public partial class AdvancedInjectionEntryPoint
    {
        private bool IsForgeActive => _rawState == ForgeState.ButtonDown || _rawState == ForgeState.ButtonUp;
        private uint GetCurrentButtonFlag() => (_rawState == ForgeState.ButtonDown) ? RI_MOUSE_LEFT_BUTTON_DOWN : RI_MOUSE_LEFT_BUTTON_UP;
        private static bool IsValidRawResult(uint result) => result > 0 && result != unchecked((uint)-1);

        private uint HookedGetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            if (_disposing) return _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
            try
            {
                if (!_allHooksInstalled.Wait(5000)) return _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
                LogFirstCall("GetRawInputData");

                if (hRawInput == (IntPtr)MAGIC_RAW_HANDLE && uiCommand == RID_INPUT)
                    return HandleMagicRawInput(pData, ref pcbSize);

                uint result = _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
                TryCaptureDeviceInfo(pData, result, uiCommand, cbSizeHeader);

                // Only reached for real (non-synthetic) packets — magic handle exits above
                if (IsCursorOverrideActive && pData != IntPtr.Zero && IsValidRawResult(result) && uiCommand == RID_INPUT)
                    WipeMouseRawInput(pData);

                return result;
            }
            catch (Exception ex)
            {
                if (!_disposing) QueueMessage($"[EXCEPTION in HookedGetRawInputData] {ex}");
                return _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
            }
        }

        private uint HookedGetRawInputBuffer(IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            if (_disposing) return _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);
            try
            {
                if (!_allHooksInstalled.Wait(5000)) return _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);
                LogFirstCall("GetRawInputBuffer");

                uint result = _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);
                if (_rawInputHeaderSize == 0 && cbSizeHeader != 0) _rawInputHeaderSize = cbSizeHeader;

                if (!IsValidRawResult(result) || pData == IntPtr.Zero)
                {
                    // No real packets — try injecting a synthetic one if we're mid-click
                    if (result == 0 && _capturedDevice != IntPtr.Zero && pData != IntPtr.Zero)
                        return TryInjectIntoEmptyBuffer(pData, ref pcbSize, result);
                    return result;
                }

                // Real packets arrived — wipe mouse data if cursor is locked, then patch button flags
                if (IsCursorOverrideActive)
                    WipeMouseRawInputBuffer(pData, result);

                return PatchBufferWithButtonFlags(pData, result);
            }
            catch (Exception ex)
            {
                if (!_disposing) QueueMessage($"[EXCEPTION in HookedGetRawInputBuffer] {ex}");
                return _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);
            }
        }

        private void WipeMouseRawInput(IntPtr pData)
        {
            RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>(pData);
            if (header.dwType != RIM_TYPEMOUSE || _rawInputHeaderSize == 0) return;

            IntPtr pMouse = new IntPtr(pData.ToInt64() + _rawInputHeaderSize);
            RAWMOUSE mouse = Marshal.PtrToStructure<RAWMOUSE>(pMouse);
            mouse.lLastX = 0;
            mouse.lLastY = 0;
            mouse.usButtonFlags = 0;
            mouse.usButtonData = 0;
            Marshal.StructureToPtr(mouse, pMouse, false);
        }

        private void WipeMouseRawInputBuffer(IntPtr pData, uint packetCount)
        {
            if (_rawInputHeaderSize == 0) return;
            long currentPtr = pData.ToInt64();

            for (int i = 0; i < packetCount; i++)
            {
                RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>((IntPtr)currentPtr);
                if (header.dwType == RIM_TYPEMOUSE)
                {
                    IntPtr pMouse = new IntPtr(currentPtr + _rawInputHeaderSize);
                    RAWMOUSE mouse = Marshal.PtrToStructure<RAWMOUSE>(pMouse);
                    mouse.lLastX = 0;
                    mouse.lLastY = 0;
                    mouse.usButtonFlags = 0;
                    mouse.usButtonData = 0;
                    Marshal.StructureToPtr(mouse, pMouse, false);
                }
                currentPtr += header.dwSize;
            }
        }

        private uint HandleMagicRawInput(IntPtr pData, ref uint pcbSize)
        {
            if (_capturedDevice == IntPtr.Zero || _capturedPacketSize == 0) return unchecked((uint)-1);
            if (pData == IntPtr.Zero) { pcbSize = _capturedPacketSize; return 0; }
            if (pcbSize < _capturedPacketSize) { pcbSize = _capturedPacketSize; return unchecked((uint)-1); }

            uint buttonFlag = GetCurrentButtonFlag();
            if (!EnsureNativePrepared(buttonFlag)) return unchecked((uint)-1);

            lock (_nativeBufLock)
            {
                CopyMemory(pData, _nativeFakePacketPtr, (UIntPtr)_capturedPacketSize);
                _lastInjectedRawState = _rawState;
                QueueMessage($"[GetRawInputData] Fabricated RAWINPUT packet: buttonFlag=0x{buttonFlag:X}, device=0x{_capturedDevice.ToInt64():X}");
                return _capturedPacketSize;
            }
        }

        private void TryCaptureDeviceInfo(IntPtr pData, uint result, uint uiCommand, uint cbSizeHeader)
        {
            if (pData == IntPtr.Zero || !IsValidRawResult(result) || result == 0 || uiCommand != RID_INPUT) return;
            if (_rawInputHeaderSize == 0 && cbSizeHeader != 0) _rawInputHeaderSize = cbSizeHeader;

            RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>(pData);
            if (header.dwType != RIM_TYPEMOUSE || header.hDevice == IntPtr.Zero) return;

            lock (_deviceLock)
            {
                bool captured = false;
                if (_capturedDevice == IntPtr.Zero) { _capturedDevice = header.hDevice; captured = true; }
                if (_capturedPacketSize == 0 && header.dwSize != 0) { _capturedPacketSize = header.dwSize; captured = true; }
                if (captured)
                    QueueMessage($"[GetRawInputData] Captured device=0x{_capturedDevice.ToInt64():X}, packetSize={_capturedPacketSize}");
            }
        }

        private uint PatchBufferWithButtonFlags(IntPtr pData, uint packetCount)
        {
            if (!IsForgeActive) return packetCount;

            long currentPtr = pData.ToInt64();
            uint buttonFlag = GetCurrentButtonFlag();

            for (int i = 0; i < packetCount; i++)
            {
                RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>((IntPtr)currentPtr);
                if (header.dwType == RIM_TYPEMOUSE)
                {
                    IntPtr pMouse = new IntPtr(currentPtr + _rawInputHeaderSize);
                    RAWMOUSE mouse = Marshal.PtrToStructure<RAWMOUSE>(pMouse);
                    mouse.usButtonFlags |= (ushort)buttonFlag;
                    Marshal.StructureToPtr(mouse, pMouse, false);
                }
                currentPtr += header.dwSize;
            }
            QueueMessage($"[GetRawInputBuffer] Patched {packetCount} real packets with buttonFlag=0x{buttonFlag:X}");
            return packetCount;
        }

        private uint TryInjectIntoEmptyBuffer(IntPtr pData, ref uint pcbSize, uint originalResult)
        {
            if (!IsForgeActive) return originalResult;
            if (_lastInjectedRawState == _rawState) return originalResult;

            uint buttonFlag = GetCurrentButtonFlag();
            if (!EnsureNativePrepared(buttonFlag)) return originalResult;

            lock (_nativeBufLock)
            {
                if (pcbSize < (uint)_nativeFakePacketSize) return originalResult;

                CopyMemory(pData, _nativeFakePacketPtr, (UIntPtr)_nativeFakePacketSize);
                _lastInjectedRawState = _rawState;
                QueueMessage($"[GetRawInputBuffer] Injected fake packet into empty buffer, buttonFlag=0x{buttonFlag:X}");
                return 1;
            }
        }
    }
}
