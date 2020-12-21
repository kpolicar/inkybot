using System;
using Inkybot.Domain;

namespace Inkybot.Events
{
    public class MagingJobEventArgs : EventArgs
    {
        public readonly Item Item;

        public MagingJobEventArgs(Item item) {
            Item = item;
        }
    }
}
