using Inkybot.Domain;

namespace Inkybot.Contracts
{
    public interface ActionFactory
    {
        IAction Finish();
        IAction CombineRune(Rune rune, bool exo);
        IAction InventorySelectResourcesAction();
    }
}
