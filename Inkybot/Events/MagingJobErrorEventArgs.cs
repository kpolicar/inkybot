using System;

namespace Inkybot.Events
{
    public class MagingJobErrorEventArgs : EventArgs
    {
        public readonly Exception exception;

        public MagingJobErrorEventArgs(Exception exception) {
            this.exception = exception;
        }
    }
}
