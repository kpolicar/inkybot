using Inkybot.Dofus;

namespace Inkybot.Events
{
    public class MagingJobFinishedEventArgs : MagingJobEventArgs
    {
        public MagingJobFinishedEventArgs(Item item, MageConfig config) : base(item, config) {
        }
    }
}
