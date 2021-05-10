using System;
using Inkybot.Dofus;

namespace Inkybot.Events
{
    public class MagingJobStartedEventArgs : MagingJobEventArgs
    {
        public readonly bool Restarting;

        public MagingJobStartedEventArgs(bool restarting, Item item, MageConfig config) : base(item, config) =>
            Restarting = restarting;
    }
}
