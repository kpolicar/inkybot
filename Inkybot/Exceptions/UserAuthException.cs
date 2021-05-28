using System;

namespace Inkybot.Exceptions
{
    public class UserAuthException : ApplicationException
    {
        public UserAuthException() : base("User auth exception!") {
        }
        public UserAuthException(string message) : base(message) {
        }
    }
}
