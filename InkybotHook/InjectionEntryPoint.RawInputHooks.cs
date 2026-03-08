using System;
using System.Runtime.InteropServices;
using static InkybotHook.NativeMethods;

namespace InkybotHook
{
    public partial class InjectionEntryPoint
    {
        // =============================================================
        // STATE
        // =============================================================

        private volatile ForgeState _lastInjectedRawState = ForgeState.Idle;

        private IntPtr _capturedDevice = IntPtr.Zero;
        private uint _capturedPacketSize = 0;
        private uint _rawInputHeaderSize = 0;
        private readonly object _deviceLock = new object();

        private IntPtr _nativeFakePacketPtr = IntPtr.Zero;
        private int _nativeFakePacketSize = 0;
        private readonly object _nativeBufLock = new object();

        private const uint RAW_INPUT_FAILED = unchecked((uint)-1);

        // =============================================================
        // QUERIES
        // =============================================================

        private static bool IsFakeRawInputHandle(IntPtr handle) =>
            handle == (IntPtr)MAGIC_RAW_HANDLE || handle == (IntPtr)MAGIC_RAW_MOVE_HANDLE;

        private bool HasCapturedDeviceInfo
        {
            get
            {
                lock (_deviceLock)
                    return _capturedDevice != IntPtr.Zero && _capturedPacketSize != 0 && _rawInputHeaderSize != 0;
            }
        }

        // =============================================================
        // GetRawInputData
        // =============================================================

        private uint HookedGetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            _allHooksInstalled.Wait();
            try
            {
                CacheHeaderSize(cbSizeHeader);

                if (IsFakeRawInputHandle(hRawInput) && uiCommand == RID_INPUT)
                    return ServeFakeRawInputPacket(hRawInput, pData, ref pcbSize);

                if (IsInputOverriden)
                    return RAW_INPUT_FAILED;

                uint result = _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
                TryCaptureDeviceFromRealData(pData, result, uiCommand, cbSizeHeader);
                return result;
            }
            catch (Exception) { return _originalGetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader); }
        }

        private uint ServeFakeRawInputPacket(IntPtr hRawInput, IntPtr pData, ref uint pcbSize)
        {
            if (_capturedDevice == IntPtr.Zero) return RAW_INPUT_FAILED;

            uint packetSize = _rawInputHeaderSize + 24;

            if (pData == IntPtr.Zero) { pcbSize = packetSize; return 0; }
            if (pcbSize < packetSize) { pcbSize = packetSize; return RAW_INPUT_FAILED; }

            bool isMovePacket = hRawInput == (IntPtr)MAGIC_RAW_MOVE_HANDLE;

            uint buttonFlag = 0;
            ushort mouseFlags = 0;
            int lastX = 0, lastY = 0;

            if (isMovePacket)
                ComputeAbsoluteMousePosition(out mouseFlags, out lastX, out lastY);
            else
                buttonFlag = CurrentButtonFlag;

            if (!BuildFakeRawInputPacket(buttonFlag, mouseFlags, lastX, lastY))
                return RAW_INPUT_FAILED;

            lock (_nativeBufLock)
            {
                CopyMemory(pData, _nativeFakePacketPtr, (UIntPtr)packetSize);
                if (!isMovePacket) _lastInjectedRawState = _rawState;
                return packetSize;
            }
        }

        private void ComputeAbsoluteMousePosition(out ushort flags, out int lastX, out int lastY)
        {
            flags = MOUSE_MOVE_ABSOLUTE | MOUSE_VIRTUAL_DESKTOP;
            int cxScreen = GetSystemMetrics(SM_CXSCREEN);
            int cyScreen = GetSystemMetrics(SM_CYSCREEN);
            lastX = (cxScreen > 0) ? (_targetScreenX * 65535) / cxScreen : 0;
            lastY = (cyScreen > 0) ? (_targetScreenY * 65535) / cyScreen : 0;
        }

        private void TryCaptureDeviceFromRealData(IntPtr pData, uint result, uint uiCommand, uint cbSizeHeader)
        {
            bool hasValidData = pData != IntPtr.Zero && result > 0 && result != RAW_INPUT_FAILED && uiCommand == RID_INPUT;
            if (!hasValidData) return;

            CacheHeaderSize(cbSizeHeader);
            RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>(pData);

            bool isMouseDevice = header.dwType == RIM_TYPEMOUSE && header.hDevice != IntPtr.Zero;
            if (!isMouseDevice) return;

            lock (_deviceLock)
            {
                if (_capturedDevice == IntPtr.Zero) _capturedDevice = header.hDevice;
                if (_capturedPacketSize == 0 && header.dwSize != 0) _capturedPacketSize = header.dwSize;
            }
        }

        // =============================================================
        // GetRawInputBuffer
        // =============================================================

        private uint HookedGetRawInputBuffer(IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            _allHooksInstalled.Wait();
            try
            {
                uint result = _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader);
                CacheHeaderSize(cbSizeHeader);

                if (ShouldDropRawInputBuffer(result, pData))
                    return 0;

                if (HasPhysicalEvents(result, pData))
                    return ApplyButtonFlagsToBuffer(pData, result);

                if (CanInjectIntoEmptyBuffer(result, pData))
                    return TryInjectFakePacketIntoBuffer(pData, ref pcbSize, result);

                return result;
            }
            catch (Exception) { return _originalGetRawInputBuffer(pData, ref pcbSize, cbSizeHeader); }
        }

        private bool ShouldDropRawInputBuffer(uint result, IntPtr pData) =>
            IsInputOverriden && pData != IntPtr.Zero && result > 0 && result != RAW_INPUT_FAILED;

        private static bool HasPhysicalEvents(uint result, IntPtr pData) =>
            result > 0 && result != RAW_INPUT_FAILED && pData != IntPtr.Zero;

        private bool CanInjectIntoEmptyBuffer(uint result, IntPtr pData) =>
            result == 0 && _capturedDevice != IntPtr.Zero && pData != IntPtr.Zero && IsForging;

        private uint ApplyButtonFlagsToBuffer(IntPtr pData, uint count)
        {
            if (!IsForging) return count;

            long ptr = pData.ToInt64();
            uint buttonFlag = CurrentButtonFlag;

            for (int i = 0; i < count; i++)
            {
                RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>((IntPtr)ptr);
                if (header.dwType == RIM_TYPEMOUSE)
                {
                    IntPtr pMouse = new IntPtr(ptr + _rawInputHeaderSize);
                    RAWMOUSE mouse = Marshal.PtrToStructure<RAWMOUSE>(pMouse);
                    mouse.usButtonFlags |= (ushort)buttonFlag;
                    Marshal.StructureToPtr(mouse, pMouse, false);
                }
                ptr += header.dwSize;
            }
            return count;
        }

        private uint TryInjectFakePacketIntoBuffer(IntPtr pData, ref uint pcbSize, uint originalResult)
        {
            if (_lastInjectedRawState == _rawState) return originalResult;

            if (!BuildFakeRawInputPacket(CurrentButtonFlag))
                return originalResult;

            lock (_nativeBufLock)
            {
                if (pcbSize >= (uint)_nativeFakePacketSize)
                {
                    CopyMemory(pData, _nativeFakePacketPtr, (UIntPtr)_nativeFakePacketSize);
                    _lastInjectedRawState = _rawState;
                    return 1;
                }
            }
            return originalResult;
        }

        // =============================================================
        // FAKE PACKET CONSTRUCTION
        // =============================================================

        private bool BuildFakeRawInputPacket(uint buttonFlag, ushort mouseFlags = 0, int lastX = 0, int lastY = 0)
        {
            try
            {
                if (!HasCapturedDeviceInfo) return false;

                lock (_nativeBufLock)
                {
                    uint packetSize = _rawInputHeaderSize + 24;
                    EnsureNativeBufferAllocated(packetSize);
                    WriteRawInputToNativeBuffer(buttonFlag, mouseFlags, lastX, lastY, packetSize);
                    return true;
                }
            }
            catch (Exception) { return false; }
        }

        private void EnsureNativeBufferAllocated(uint packetSize)
        {
            int alignedSize = (int)((packetSize + 7) & ~7);

            if (_nativeFakePacketPtr != IntPtr.Zero && _nativeFakePacketSize != packetSize)
            {
                Marshal.FreeHGlobal(_nativeFakePacketPtr);
                _nativeFakePacketPtr = IntPtr.Zero;
            }

            if (_nativeFakePacketPtr == IntPtr.Zero)
            {
                _nativeFakePacketPtr = Marshal.AllocHGlobal(alignedSize);
                _nativeFakePacketSize = (int)packetSize;

                byte[] zeros = new byte[alignedSize];
                Marshal.Copy(zeros, 0, _nativeFakePacketPtr, alignedSize);
            }
        }

        private void WriteRawInputToNativeBuffer(uint buttonFlag, ushort mouseFlags, int lastX, int lastY, uint packetSize)
        {
            var header = new RAWINPUTHEADER
            {
                dwType = RIM_TYPEMOUSE,
                dwSize = packetSize,
                hDevice = _capturedDevice,
                wParam = IntPtr.Zero
            };
            var mouse = new RAWMOUSE
            {
                usFlags = mouseFlags,
                ulButtons = buttonFlag,
                ulRawButtons = 0,
                lLastX = lastX,
                lLastY = lastY,
                ulExtraInformation = 0
            };

            Marshal.StructureToPtr(header, _nativeFakePacketPtr, false);
            IntPtr pMouse = new IntPtr(_nativeFakePacketPtr.ToInt64() + _rawInputHeaderSize);
            Marshal.StructureToPtr(mouse, pMouse, false);
        }

        // =============================================================
        // DEVICE PROBING
        // =============================================================

        private void ProbeRawInputDevices()
        {
            try
            {
                _rawInputHeaderSize = (uint)Marshal.SizeOf<RAWINPUTHEADER>();

                uint numDevices = 0;
                uint cbSize = (uint)Marshal.SizeOf<RAWINPUTDEVICELIST>();
                GetRawInputDeviceList(IntPtr.Zero, ref numDevices, cbSize);
                if (numDevices == 0) return;

                var devices = new RAWINPUTDEVICELIST[numDevices];
                uint result = GetRawInputDeviceList(devices, ref numDevices, cbSize);
                if (result == RAW_INPUT_FAILED) return;

                for (int i = 0; i < result; i++)
                {
                    if (devices[i].dwType == RIM_TYPEMOUSE && devices[i].hDevice != IntPtr.Zero)
                    {
                        lock (_deviceLock)
                        {
                            _capturedDevice = devices[i].hDevice;
                            _capturedPacketSize = (uint)Marshal.SizeOf<RAWINPUT>();
                        }
                        _server.ReportMessage($"[LOG] Probed mouse device: 0x{_capturedDevice.ToInt64():X}, packetSize={_capturedPacketSize}, headerSize={_rawInputHeaderSize}");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _server.ReportMessage($"[EXCEPTION in ProbeRawInputDevices]\n{ex}");
            }
        }

        // =============================================================
        // HELPERS
        // =============================================================

        private void CacheHeaderSize(uint cbSizeHeader)
        {
            if (_rawInputHeaderSize == 0 && cbSizeHeader != 0)
                _rawInputHeaderSize = cbSizeHeader;
        }

        private void FreeFakePacketBuffer()
        {
            lock (_nativeBufLock)
            {
                if (_nativeFakePacketPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_nativeFakePacketPtr);
                    _nativeFakePacketPtr = IntPtr.Zero;
                    _nativeFakePacketSize = 0;
                }
            }
        }
    }
}
