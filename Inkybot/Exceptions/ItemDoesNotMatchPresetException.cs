using Inkybot.Dofus;

namespace Inkybot.Exceptions
{
    public class ItemDoesNotMatchPresetException : MagingException
    {
        public readonly Item Item;

        public ItemDoesNotMatchPresetException(Item item) : base("The selected item does not match the configured preset!") {
            Item = item;
        }
    }
}
