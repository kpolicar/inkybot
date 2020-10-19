using System;

namespace Inkybot.Exceptions
{
    public class OcrEngineNotReadyYetException : InvalidOperationException
    {
        public OcrEngineNotReadyYetException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
