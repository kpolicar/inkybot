using System;

namespace Inkybot.Events
{
    public class MagingJobErrorEventArgs : ExceptionEventArgs
    {
        public MagingJobErrorEventArgs(Exception exception, string additionalInfo="") : base(exception, additionalInfo) {
        }
    }
}
