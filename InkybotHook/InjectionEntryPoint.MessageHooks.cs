using System;
using static InkybotHook.NativeMethods;
#pragma warning disable CS1690

namespace InkybotHook
{
    public partial class InjectionEntryPoint
    {
        // =============================================================
        // MESSAGE CLASSIFICATION
        // =============================================================

        private static bool IsRealMouseMessage(uint msg) =>
            msg == WM_INPUT ||
            (msg >= WM_MOUSEMOVE && msg <= WM_LBUTTONUP) ||
            (msg >= WM_POINTERUPDATE && msg <= WM_POINTERUP);

        private static bool IsRealKeyMessage(uint msg) =>
            msg == WM_KEYDOWN || msg == WM_KEYUP || msg == WM_CHAR;

        private static bool IsHardwareInputMessage(uint msg) =>
            IsRealMouseMessage(msg) || IsRealKeyMessage(msg);

        private static bool IsPointerMessage(uint msg) =>
            msg >= WM_POINTERUPDATE && msg <= WM_POINTERUP;

        private static bool IsSyntheticRawInputMessage(MSG msg) =>
            msg.message == WM_INPUT &&
            (msg.lParam == (IntPtr)MAGIC_RAW_HANDLE || msg.lParam == (IntPtr)MAGIC_RAW_MOVE_HANDLE);

        private static bool IsSyntheticKeyMessage(MSG msg) =>
            (msg.message == WM_KEYDOWN || msg.message == WM_KEYUP) &&
            msg.lParam == (IntPtr)MAGIC_KEY_HANDLE;

        // =============================================================
        // PeekMessageW
        // =============================================================

        private bool HookedPeekMessageW(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            _allHooksInstalled.Wait();
            try
            {
                bool hasMessage = _originalPeekMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);

                if (lpMsg.hwnd != IntPtr.Zero)
                    TryTrackInputWindow(ref lpMsg);

                if (hasMessage && IsInputOverriden && IsHardwareInputMessage(lpMsg.message))
                    hasMessage = ConsumeAndDrop(hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);

                if (TryFeedSyntheticMessage(ref lpMsg, wRemoveMsg, hasMessage))
                    return true;

                return hasMessage;
            }
            catch (Exception) { return false; }
        }

        private void TryTrackInputWindow(ref MSG lpMsg)
        {
            bool isInputMessage = lpMsg.message == WM_INPUT
                               || lpMsg.message == WM_MOUSEMOVE
                               || IsPointerMessage(lpMsg.message);

            if (isInputMessage && _mainHwnd != lpMsg.hwnd)
            {
                _mainHwnd = lpMsg.hwnd;
                _server.ReportMessage($"[LOG] Locked onto active Input HWND: 0x{_mainHwnd.ToInt64():X}");
            }

            if (IsPointerMessage(lpMsg.message))
                TryHijackPointerId(lpMsg.wParam);
        }

        private void TryHijackPointerId(IntPtr wParam)
        {
            uint extractedId = (uint)(wParam.ToInt64() & 0xFFFF);
            if (_capturedPointerId != extractedId)
            {
                _capturedPointerId = extractedId;
                _server.ReportMessage($"[LOG] Hijacked real OS Pointer ID: {_capturedPointerId}");
            }
        }

        private bool ConsumeAndDrop(IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            if ((wRemoveMsg & PM_REMOVE) == 0)
            {
                MSG dummy = default;
                _originalPeekMessageW(ref dummy, hWnd, wMsgFilterMin, wMsgFilterMax, PM_REMOVE);
            }
            return false;
        }

        private bool TryFeedSyntheticMessage(ref MSG lpMsg, uint wRemoveMsg, bool hasMessage)
        {
            lock (_queueLock)
            {
                bool canFeed = _syntheticMessages.Count > 0
                            && (!hasMessage || lpMsg.message == WM_NULL)
                            && _mainHwnd != IntPtr.Zero;
                if (!canFeed) return false;

                lpMsg = (wRemoveMsg & PM_REMOVE) != 0
                    ? _syntheticMessages.Dequeue()
                    : _syntheticMessages.Peek();
                return true;
            }
        }

        // =============================================================
        // TranslateMessage
        // =============================================================

        private bool HookedTranslateMessage(ref MSG lpMsg)
        {
            _allHooksInstalled.Wait();

            if (IsSyntheticRawInputMessage(lpMsg))
                return true;

            if (IsSyntheticKeyMessage(lpMsg))
                return true;

            return _originalTranslateMessage(ref lpMsg);
        }

        // =============================================================
        // DispatchMessageW
        // =============================================================

        private IntPtr HookedDispatchMessageW(ref MSG lpMsg)
        {
            _allHooksInstalled.Wait();
            return _originalDispatchMessageW(ref lpMsg);
        }
    }
}
