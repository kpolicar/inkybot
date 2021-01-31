using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

namespace Inkybot.Dofus.Contracts
{
    public interface ActionFactory
    {
        IAction Finish(Item item);
        IAction CombineRune(Rune rune, bool exo);
        IAction InventorySelectResourcesAction();
        IAction InventoryClearSelectionAction();
    }
}
