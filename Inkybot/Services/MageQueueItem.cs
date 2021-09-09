namespace Inkybot.Services
{
    public partial class ScreenReaderDofusMagingJob
    {
        private class MageQueueItem
        {
            public readonly QueuedConfigProvider Config;

            public MageQueueItem(QueuedConfigProvider configManager) {
                Config = configManager;
            }
        }
    }
}
