namespace Inkybot.Exceptions
{
    public class SinkIncorrectException : MagingException
    {
        public SinkIncorrectException(decimal sinkChange, decimal sink, decimal attemptedSinkChange) :
            base($"Calculated sink was incorrect: Current sink: {sink}, sink change: {sinkChange}, attempted sink change: {attemptedSinkChange}") {
        }
    }
}
