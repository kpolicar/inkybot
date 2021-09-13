using System;
using Inkybot.Dofus;

namespace Inkybot.Events
{
    public class MagingJobStartedEventArgs : MagingJobEventArgs
    {
        public readonly bool Restarting;
        public readonly bool Interrupted;

        public MagingJobStartedEventArgs(bool restarting, Item item, MageConfig config, bool interrupted) : base(item, config) =>
            (Restarting, Interrupted) = (restarting, interrupted);
    }
}
