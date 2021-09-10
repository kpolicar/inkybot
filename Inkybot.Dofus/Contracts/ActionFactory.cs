using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

namespace Inkybot.Dofus.Contracts
{
    /**
     * <summary>The ActionFactory interface specifies methods the bot can execute.</summary>
     */
    public interface ActionFactory
    {
        /**
         * <summary>An action that will finish the bot maging session.</summary>
         * <param name="item">The item that should be finished</param>
         */
        IAction Finish(Item item);

        /**
         * <summary>
         * An action that will combine the specified rune with information regarding whether or
         * not the combination is exotic to the active item.
         * </summary>
         * <param name="rune">The rune that should be combined</param>
         * <param name="exo">Whether or not the combination is exotic to the item</param>
         */
        IAction CombineRune(Rune rune, bool exo);
        
        /**
         * <summary>An action that will select the in-game resources tab in the user's inventory.</summary>
         */
        IAction InventorySelectResourcesAction();
        
        IAction InventorySelectAllAction();
        
        IAction InventorySelectEquipmentAction();
        
        /**
         * <summary>An action that will clear the in-game selection query in the user's inventory.</summary>
         */
        IAction InventoryClearSelectionAction();

        IAction SelectItemFromQueue();
    }
}
