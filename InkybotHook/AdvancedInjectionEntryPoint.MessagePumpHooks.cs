using System;
using static InkybotHook.NativeMethods;
#pragma warning disable CS1690

namespace InkybotHook
{
    public partial class AdvancedInjectionEntryPoint
    {
        private static bool IsSyntheticRawInput(ref MSG lpMsg) =>
            lpMsg.message == WM_INPUT && lpMsg.lParam == (IntPtr)MAGIC_RAW_HANDLE;

        private static bool IsMouseOrPointerMessage(uint msg) =>
            msg == WM_INPUT ||
            (msg >= WM_MOUSEMOVE && msg <= WM_LBUTTONUP) ||
            (msg >= WM_POINTERUPDATE && msg <= WM_POINTERUP);

        private bool HookedPeekMessageW(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            if (_disposing) return _originalPeekMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
            try
            {
                if (!_allHooksInstalled.Wait(5000)) return _originalPeekMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
                LogFirstCall("PeekMessageW");

                bool result = _originalPeekMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);

                TryCapturePointerId(ref lpMsg);

                if (TryInjectSyntheticMessage(ref lpMsg, wRemoveMsg, result))
                    return true;

                return result;
            }
            catch (Exception ex)
            {
                if (!_disposing) QueueMessage($"[EXCEPTION in HookedPeekMessageW] {ex}");
                return _originalPeekMessageW(ref lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
            }
        }

        private bool HookedTranslateMessage(ref MSG lpMsg)
        {
            if (_disposing) return _originalTranslateMessage(ref lpMsg);
            try
            {
                if (!_allHooksInstalled.Wait(5000)) return _originalTranslateMessage(ref lpMsg);
                LogFirstCall("TranslateMessage");

                if (IsSyntheticRawInput(ref lpMsg))
                {
                    QueueMessage("[TranslateMessage] Bypassed OS translation for synthetic WM_INPUT (MAGIC_RAW_HANDLE)");
                    return true;
                }

                return _originalTranslateMessage(ref lpMsg);
            }
            catch (Exception ex)
            {
                if (!_disposing) QueueMessage($"[EXCEPTION in HookedTranslateMessage] {ex}");
                return _originalTranslateMessage(ref lpMsg);
            }
        }

        private IntPtr HookedDispatchMessageW(ref MSG lpMsg)
        {
            if (_disposing) return _originalDispatchMessageW(ref lpMsg);
            try
            {
                if (!_allHooksInstalled.Wait(5000)) return _originalDispatchMessageW(ref lpMsg);
                LogFirstCall("DispatchMessageW");

                // Synthetic WM_INPUT with fake HRAWINPUT must bypass DispatchMessageW
                // (Windows validates the handle internally and would drop it)
                if (IsSyntheticRawInput(ref lpMsg))
                {
                    if (lpMsg.hwnd == IntPtr.Zero) return IntPtr.Zero;
                    IntPtr wndProc = GetWindowLongPtrNative(lpMsg.hwnd, GWLP_WNDPROC);
                    if (wndProc == IntPtr.Zero) return IntPtr.Zero;
                    QueueMessage($"[DispatchMessageW] Dispatching synthetic WM_INPUT directly to WndProc for HWND 0x{lpMsg.hwnd.ToInt64():X}");
                    return CallWindowProc(wndProc, lpMsg.hwnd, lpMsg.message, lpMsg.wParam, lpMsg.lParam);
                }

                if (lpMsg.time != MAGIC_SYNTHETIC_TIME && IsCursorOverrideActive && IsMouseOrPointerMessage(lpMsg.message))
                    return IntPtr.Zero;

                if (lpMsg.time == MAGIC_SYNTHETIC_TIME)
                    lpMsg.time = (uint)Environment.TickCount;

                return _originalDispatchMessageW(ref lpMsg);
            }
            catch (Exception ex)
            {
                if (!_disposing) QueueMessage($"[EXCEPTION in HookedDispatchMessageW] {ex}");
                return _originalDispatchMessageW(ref lpMsg);
            }
        }

        private void TryCapturePointerId(ref MSG lpMsg)
        {
            if (lpMsg.message < WM_POINTERUPDATE || lpMsg.message > WM_POINTERUP) return;

            uint extractedPointerId = (uint)(lpMsg.wParam.ToInt64() & 0xFFFF);
            if (_capturedPointerId != extractedPointerId)
            {
                _capturedPointerId = extractedPointerId;
                QueueMessage($"[PeekMessageW] Captured real OS Pointer ID: {_capturedPointerId}");
            }
        }

        private bool TryInjectSyntheticMessage(ref MSG lpMsg, uint wRemoveMsg, bool peekResult)
        {
            lock (_queueLock)
            {
                if (_syntheticMessages.Count == 0 || _mainHwnd == IntPtr.Zero) return false;
                if (peekResult && lpMsg.message != WM_NULL) return false;

                if ((wRemoveMsg & 0x0001 /* PM_REMOVE */) != 0)
                {
                    lpMsg = _syntheticMessages.Dequeue();
                    QueueMessage($"[PeekMessageW] Injected synthetic MSG: 0x{lpMsg.message:X4}");
                }
                else
                {
                    lpMsg = _syntheticMessages.Peek();
                }
                return true;
            }
        }
    }
}
