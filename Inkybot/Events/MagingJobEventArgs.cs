using System;
using Inkybot.Dofus;

namespace Inkybot.Events
{
    public class MagingJobEventArgs : EventArgs
    {
        public readonly Item Item;
        public readonly MageConfig Config;

        public MagingJobEventArgs(Item item, MageConfig config) {
            Item = item;
            Config = config;
        }
    }
}
