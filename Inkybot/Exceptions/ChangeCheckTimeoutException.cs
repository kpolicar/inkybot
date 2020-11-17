using System;

namespace Inkybot.Exceptions
{
    public class ChangeCheckTimeoutException : MagingException
    {
        public ChangeCheckTimeoutException(string message) : base(message) {
        }
    }
}
