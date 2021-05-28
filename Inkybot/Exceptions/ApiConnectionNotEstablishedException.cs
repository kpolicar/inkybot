using System;

namespace Inkybot.Exceptions
{
    public class ApiConnectionNotEstablishedException : Exception
    {
        public ApiConnectionNotEstablishedException() : base("API Connection has not been established!") {
        }
    }
}
