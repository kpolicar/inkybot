// RemoteFileMonitor (File: FileMonitorHook\ServerInterface.cs)
//
// Copyright (c) 2017 Justin Stenning
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// Please visit https://easyhook.github.io for more information
// about the project, latest updates and other tutorials.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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

        [Serializable]
        public struct InputMessage
        {
            public uint Msg;
            public IntPtr WParam;
            public IntPtr LParam;
        }

        public bool ShutdownFlag = false;

        public POINT point = new POINT { X = -1, Y = -1 };

        private readonly ConcurrentQueue<InputMessage> _inputQueue = new ConcurrentQueue<InputMessage>();

        /// <summary>
        /// Enqueue a window message to be dispatched from within the target process
        /// directly to the original WndProc, bypassing all hooks.
        /// </summary>
        public void EnqueueInput(uint msg, IntPtr wParam, IntPtr lParam)
        {
            _inputQueue.Enqueue(new InputMessage { Msg = msg, WParam = wParam, LParam = lParam });
        }

        public bool TryDequeueInput(out InputMessage msg) => _inputQueue.TryDequeue(out msg);

        /// <summary>
        /// The target window handle. Set this from the host so that SetCursorFixedPosition
        /// can post WM_MOUSEMOVE messages to the target window.
        /// </summary>
        public IntPtr targetHwnd = IntPtr.Zero;

        public HookState State { get; private set; } = HookState.NotInitialized;

        private bool _hasLoggedFirstCursorChange = false;

        #region Win32 imports for WM_MOUSEMOVE posting

        const uint WM_MOUSEMOVE = 0x0200;

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        static IntPtr MakeLParam(int x, int y)
        {
            return (IntPtr)((y << 16) | (x & 0xFFFF));
        }

        #endregion

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

        /// <summary>
        /// Output the message to the console.
        /// </summary>
        /// <param name="messages"></param>
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

            if (!_hasLoggedFirstCursorChange && point.X != -1 && point.Y != -1)
            {
                _hasLoggedFirstCursorChange = true;
            }

            // Post WM_MOUSEMOVE to the target window with the fixed position
            if (targetHwnd != IntPtr.Zero && point.X != -1 && point.Y != -1)
            {
                try
                {
                    var clientPt = new POINT { X = point.X, Y = point.Y };
                    ScreenToClient(targetHwnd, ref clientPt);
                    PostMessage(targetHwnd, WM_MOUSEMOVE, IntPtr.Zero, MakeLParam(clientPt.X, clientPt.Y));
                }
                catch (Exception e)
                {
                    Logger?.Invoke("[EasyHook:Host] Failed to post WM_MOUSEMOVE: " + e.Message);
                }
            }
        }

        public void ReportMessage(string message) {
            Logger?.Invoke(message);
        }

        /// <summary>
        /// Report exception
        /// </summary>
        /// <param name="e"></param>
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