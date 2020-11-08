using System;
using Inkybot.Domain.Repositories;

namespace Inkybot.Events
{
    public class ItemEventArgs : EventArgs
    {
        public readonly Item Item;


        public ItemEventArgs(Item item) {
            this.Item = item;
        }
    }
}
