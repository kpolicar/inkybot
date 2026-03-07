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
        /// The fixed cursor position in client coordinates relative to the target window.
        /// A value of (-1, -1) means no override is active.
        /// </summary>
        public POINT point = new POINT { X = -1, Y = -1 };

        /// <summary>
        /// The target window handle. Set this from the host so that the hook
        /// can convert client coordinates to screen coordinates.
        /// </summary>
        public IntPtr targetHwnd = IntPtr.Zero;

        /// <summary>
        /// Click request fields. Win32Input sets these via RequestClick(),
        /// the hook thread reads and clears clickRequested.
        /// Coordinates are in screen space.
        /// </summary>
        public volatile bool clickRequested = false;
        public int clickScreenX;
        public int clickScreenY;

        public void RequestClick(int screenX, int screenY)
        {
            clickScreenX = screenX;
            clickScreenY = screenY;
            clickRequested = true;
        }

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