using System;
using Inkybot.Dofus;
using Inkybot.Helpers;
using Inkybot.Services;

namespace Inkybot.Events
{
    public class MageQueueEventArgs : EventArgs
    {
        public readonly MageQueueManager.MageQueueItem QueueItem;

        public MageQueueEventArgs(MageQueueManager.MageQueueItem queueItem) =>
            (QueueItem) = (queueItem);
    }
}
