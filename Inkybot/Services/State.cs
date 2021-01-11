namespace Inkybot.Services
{
    public partial class ScreenReaderDofusMagingJob
    {
        private enum State
        {
            STANDARD,
            EXECUTING_COMBINE,
            CALCULATING_SINK_CHANGE,
            CALCULATING_PRICE_CHANGE
        }
    }
}
