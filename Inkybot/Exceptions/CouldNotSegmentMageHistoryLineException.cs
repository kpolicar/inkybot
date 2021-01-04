namespace Inkybot.Exceptions
{
    public class CouldNotSegmentMageHistoryLineException : OcrException
    {
        public CouldNotSegmentMageHistoryLineException(string message) : base(message) {
        }
    }
}
