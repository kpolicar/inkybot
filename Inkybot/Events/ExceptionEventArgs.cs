using System;

namespace Inkybot.Events
{
    public class ExceptionEventArgs : EventArgs
    {
        public readonly Exception exception;

        public ExceptionEventArgs(Exception exception) {
            this.exception = exception;
        }
    }
}
