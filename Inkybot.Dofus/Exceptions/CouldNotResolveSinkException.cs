using System;

namespace Inkybot.Dofus.Exceptions
{
    /**
     * <summary>An application exception that occurs when the bot cannot correctly resolve sink changes</summary>
     */
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
