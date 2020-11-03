using System;

namespace Inkybot.Events
{
    public class MagingJobErrorEventArgs : ExceptionEventArgs
    {
        private string additionalInfo;

        public MagingJobErrorEventArgs(Exception exception, string additionalInfo) : base(exception, additionalInfo) {
        }
    }
}
