using System;

namespace Inkybot.Dofus.Exceptions
{
    public class CouldNotResolveSinkException : ApplicationException
    {
        public CouldNotResolveSinkException(string message)
            : base(message)
        {
        }
        
        public CouldNotResolveSinkException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
