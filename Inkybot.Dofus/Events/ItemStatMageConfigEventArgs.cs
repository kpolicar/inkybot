using System;
using Inkybot.Dofus;
using ItemStatMageConfig = Inkybot.Dofus.MageConfig.ItemStatMageConfig;

namespace Inkybot.Dofus.Events
{
    /**
     * <summary>Mage config change event arguments</summary>
     */
    public class MageConfigEventArgs : EventArgs
    {
        /**
         * <summary>The previous item stat mage configuration</summary>
         */
        public readonly ItemStatMageConfig Previous;
        
        /**
         * <summary>The new item stat mage configuration</summary>
         */
        public readonly ItemStatMageConfig Current;

        public MageConfigEventArgs(ItemStatMageConfig previous, ItemStatMageConfig current) =>
            (Previous, Current) = (previous, current);
    }
}
