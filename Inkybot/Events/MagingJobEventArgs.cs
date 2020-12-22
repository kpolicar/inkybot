using System;
using Inkybot.Domain;

namespace Inkybot.Events
{
    public class MagingJobEventArgs : EventArgs
    {
        public readonly Item Item;
        public readonly Config Config;

        public MagingJobEventArgs(Item item, Config config) {
            Item = item;
            Config = config;
        }
    }
}
