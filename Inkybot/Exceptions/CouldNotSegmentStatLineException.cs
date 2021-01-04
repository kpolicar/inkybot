namespace Inkybot.Exceptions
{
    public class CouldNotSegmentStatLineException : OcrException
    {
        public CouldNotSegmentStatLineException(string message) : base(message) {
        }
    }
}
