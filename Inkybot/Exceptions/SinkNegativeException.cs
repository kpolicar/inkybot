using System;

namespace Inkybot.Exceptions
{
    public class SinkNegativeException : MagingException
    {
        public SinkNegativeException(float sink) : base($"Calculated sink is lower than 0: {sink}") {
        }
    }
}
