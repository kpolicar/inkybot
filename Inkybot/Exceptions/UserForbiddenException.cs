using System;

namespace Inkybot.Exceptions
{
    public class UserForbiddenException : ApplicationException
    {
        public UserForbiddenException(string message)
            : base(message) {
        }
    }
}
