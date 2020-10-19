using System;

namespace Inkybot.Exceptions
{
    public class CouldNotResolveStatValueException : OcrException
    {
        public CouldNotResolveStatValueException(string message)
            : base(message)
        {
        }
        public CouldNotResolveStatValueException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
