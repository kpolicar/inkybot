namespace Inkybot.Exceptions
{
    public class SinkNegativeException : MagingException
    {
        public SinkNegativeException(decimal sink) : base($"Calculated sink is lower than 0: {sink}") {
        }
    }
}
