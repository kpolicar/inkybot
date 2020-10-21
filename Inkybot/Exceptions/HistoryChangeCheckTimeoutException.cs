using System;

namespace Inkybot.Exceptions
{
    public class HistoryChangeCheckTimeoutException : MagingException
    {
        public HistoryChangeCheckTimeoutException(string message) : base(message) {
        }
    }
}
