namespace Inkybot.Exceptions
{
    public class SinkTooHighException : MagingException
    {
        public SinkTooHighException(decimal sink) : base($"Calculated sink is higher than 101: {sink}") {
        }
    }
}
