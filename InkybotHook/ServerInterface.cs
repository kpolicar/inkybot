using System;
using System.Runtime.InteropServices;

namespace InkybotHook
{
    public enum HookState
    {
        NotInitialized,
        IpcCreated,
        Injecting,
        Injected,
        HooksInstalled,
        Running,
        IpcDisconnected,
        Disposed,
        Failed
    }

    /// <summary>
    /// Provides an interface for communicating from the client (target) to the server (injector)
    /// </summary>
    public class ServerInterface : MarshalByRefObject
    {
        [Serializable]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// Set this from the host process to forward log messages to your logging system.
        /// </summary>
        public static Action<string> Logger;

        public bool ShutdownFlag = false;

        /// <summary>
        /// The fixed cursor position in screen coordinates.
        /// A value of (-1, -1) means no override is active.
        /// </summary>
        public POINT point = new POINT { X = -1, Y = -1 };

        /// <summary>
        /// Set by the injected hook: true if the Dofus process has enabled mouse-in-pointer mode.
        /// Read by Win32Input to decide whether to send WM_POINTER or WM_LBUTTON messages.
        /// </summary>
        public bool IsPointerInputEnabled = false;

        /// <summary>
        /// The real OS pointer ID captured from WM_POINTER messages inside the Dofus process.
        /// Used by Win32Input for sending WM_POINTER messages with a valid pointer ID.
        /// </summary>
        public uint CapturedPointerId = 0;

        /// <summary>
        /// Set by Win32Input to true while a simulated click is active (between down and up).
        /// The hook uses this to spoof GetKeyState/GetAsyncKeyState for VK_LBUTTON.
        /// </summary>
        public volatile bool IsClickActive = false;

        /// <summary>
        /// Set by Win32Input to request the advanced hook to perform a click at the current cursor position.
        /// The hook clears this after initiating the click sequence.
        /// </summary>
        public volatile bool ClickRequested = false;

        /// <summary>
        /// Set by the advanced hook to true once the click sequence (down + up) has completed.
        /// Win32Input polls this to know when the click is done.
        /// </summary>
        public volatile bool ClickCompleted = false;

        public HookState State { get; private set; } = HookState.NotInitialized;

        public void SetState(HookState newState)
        {
            var oldState = State;
            State = newState;
            Logger?.Invoke($"[EasyHook:Target] State changed: {oldState} -> {newState}");
        }

        public void IsInstalled(int clientPID) {
            ReportMessage($"[EasyHook:Target] Hook DLL injected into process {clientPID}");
            SetState(HookState.Injected);
        }

        public void ReportMessages(string[] messages) {
            for (int i = 0; i < messages.Length; i++) {
                ReportMessage(messages[i]);
            }
        }

        public void SetCursorFixedPosition(POINT point) {
            if (point.X != this.point.X || point.Y != this.point.Y)
            {
                Logger?.Invoke($"[EasyHook:Host] Cursor position -> ({point.X}, {point.Y})");
            }
            this.point = point;
        }

        public void ReportMessage(string message) {
            Logger?.Invoke(message);
        }

        public void ReportException(Exception e) {
            Logger?.Invoke("[EasyHook:Target] Target process error: " + e.ToString());
        }

        /// <summary>
        /// Called to confirm that the IPC channel is still open / host application has not closed
        /// </summary>
        public void Ping() {
            // No-op keep-alive; intentionally not logged to avoid noise
        }
    }
}