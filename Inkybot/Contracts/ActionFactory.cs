using Inkybot.Dofus;
using IAction = Inkybot.Domain.IAction;

namespace Inkybot.Contracts
{
    public interface ActionFactory
    {
        IAction Finish(Item item);
        IAction CombineRune(Rune rune, bool exo);
        IAction InventorySelectResourcesAction();
        IAction InventoryClearSelectionAction();
    }
}
