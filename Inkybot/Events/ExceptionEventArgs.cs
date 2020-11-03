using System;

namespace Inkybot.Events
{
    public class ExceptionEventArgs : EventArgs
    {
        public readonly Exception exception;
        public readonly string additionalInfo;

        public string Message => exception.Message + (additionalInfo.Length > 0 ? "\r\n"+additionalInfo : "");

        public ExceptionEventArgs(Exception exception, string additionalInfo="") {
            this.exception = exception;
            this.additionalInfo = additionalInfo;
        }
    }
}
