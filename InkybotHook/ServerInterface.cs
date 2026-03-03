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
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public bool ShutdownFlag = false;

        public POINT point = new POINT { X = -1, Y = -1 };

        public HookState State { get; private set; } = HookState.NotInitialized;

        private bool _hasLoggedFirstCursorChange = false;

        public void SetState(HookState newState)
        {
            var oldState = State;
            State = newState;
            Logger?.Invoke($"[EasyHook] State changed: {oldState} -> {newState}");
        }

        public void IsInstalled(int clientPID) {
            ReportMessage($"[EasyHook] Hook DLL injected into process {clientPID}");
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
            this.point = point;

            if (!_hasLoggedFirstCursorChange && point.X != -1 && point.Y != -1)
            {
                _hasLoggedFirstCursorChange = true;
                Logger?.Invoke($"[EasyHook] First cursor position override applied: ({point.X}, {point.Y})");
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
            Logger?.Invoke("[EasyHook] Target process error: " + e.ToString());
        }

        /// <summary>
        /// Called to confirm that the IPC channel is still open / host application has not closed
        /// </summary>
        public void Ping() {
            // No-op keep-alive; intentionally not logged to avoid noise
        }
    }
}