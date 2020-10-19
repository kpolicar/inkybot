using System;

namespace Inkybot.Exceptions
{
    public class OcrException : ApplicationException
    {
        public OcrException(string message)
            : base(message)
        {
        }
        public OcrException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
