using System;

namespace Inkybot.Exceptions
{
    public class UnexpectedMageResultException : MagingException
    {
        public UnexpectedMageResultException(string message) : base(message) {
        }
    }
}
