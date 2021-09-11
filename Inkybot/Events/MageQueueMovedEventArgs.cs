using System;
using Inkybot.Dofus;
using Inkybot.Helpers;
using Inkybot.Services;

namespace Inkybot.Events
{
    public class MageQueueMovedEventArgs : MageQueueEventArgs
    {
        public readonly int Index;

        public MageQueueMovedEventArgs(MageQueueManager.MageQueueItem queueItem, int index) : base(queueItem) =>
            Index = index;
    }
}
