using Inkybot.Dofus;

namespace Inkybot.Exceptions
{
    public class ItemHasNotChangedException : UnexpectedMageResultException
    {
        public readonly Item Item;

        public ItemHasNotChangedException(Item item) : base("Item has not changed!") {
        }
    }
}
