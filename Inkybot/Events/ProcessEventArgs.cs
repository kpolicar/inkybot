using System;
using System.Diagnostics;

namespace Inkybot.Events
{
    public class ProcessEventArgs : EventArgs
    {
        public readonly Process Process;


        public ProcessEventArgs(Process process) {
            this.Process = process;
        }
    }
}
