using Inkybot.Dofus;

namespace Inkybot.Events
{
    public class MagingJobFinishedEventArgs : MagingJobEventArgs
    {
        public bool AutoShutdown;
        public MagingJobFinishedEventArgs(Item item, MageConfig config, bool autoShutdown) : base(item, config) {
            AutoShutdown = autoShutdown;
        }
    }
}
