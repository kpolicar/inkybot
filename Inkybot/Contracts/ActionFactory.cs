using Inkybot.Domain;

namespace Inkybot.Contracts
{
    public interface ActionFactory
    {
        IAction Finish(Item item);
        IAction CombineRune(Rune rune, bool exo);
        IAction InventorySelectResourcesAction();
    }
}
