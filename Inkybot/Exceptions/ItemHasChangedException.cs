using System;
using Inkybot.Domain;

namespace Inkybot.Exceptions
{
    public class ItemHasChangedException : MagingException
    {
        public readonly Item Item;

        public ItemHasChangedException(Item item) : base("Stats are not configured for this item.") {
            Item = item;
        }
    }
}
