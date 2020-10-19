using System;

namespace Inkybot.Exceptions
{
    public class CouldNotResolveStatNameException : OcrException
    {
        public CouldNotResolveStatNameException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
