namespace Inkybot.Services
{
    public partial class ScreenReaderDofusMagingJob
    {
        internal enum State
        {
            STANDARD,
            EXECUTING_COMBINE,
            CALCULATING_SINK_CHANGE
        }
    }
}
