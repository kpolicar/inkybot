using System;

namespace Inkybot.Exceptions
{
    public class CouldNotResolveSinkException : OcrException
    {
        public CouldNotResolveSinkException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
