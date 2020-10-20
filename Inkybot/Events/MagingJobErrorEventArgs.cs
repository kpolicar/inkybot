using System;

namespace Inkybot.Events
{
    public class MagingJobErrorEventArgs : ExceptionEventArgs
    {
        public MagingJobErrorEventArgs(Exception exception) : base(exception)
        {
        }
    }
}
