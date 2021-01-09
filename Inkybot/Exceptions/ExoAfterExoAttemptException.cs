using System;

namespace Inkybot.Exceptions
{
    public class ExoAfterExoAttemptException : MagingException
    {
        public ExoAfterExoAttemptException(string message) : base(message) {
        }

        public ExoAfterExoAttemptException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
