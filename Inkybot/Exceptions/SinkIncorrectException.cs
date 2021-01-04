namespace Inkybot.Exceptions
{
    public class SinkIncorrectException : MagingException
    {
        public SinkIncorrectException(float sinkChange, float sink, float attemptedSinkChange) :
            base($"Calculated sink was incorrect: Current sink: {sink}, sink change: {sinkChange}, attempted sink change: {attemptedSinkChange}") {
        }
    }
}
