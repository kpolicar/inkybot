using System;

namespace Inkybot.Exceptions
{
    public class MagingException : ApplicationException
    {
        
        public MagingException(string message)
            : base(message)
        {
        }
        public MagingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
