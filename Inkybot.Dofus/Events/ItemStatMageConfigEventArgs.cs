using System;
using Inkybot.Dofus;
using ItemStatMageConfig = Inkybot.Dofus.MageConfig.ItemStatMageConfig;

namespace Inkybot.Events
{
    public class MageConfigEventArgs : EventArgs
    {
        private readonly ItemStatMageConfig Previous;
        private readonly ItemStatMageConfig Current;

        public MageConfigEventArgs(ItemStatMageConfig previous, ItemStatMageConfig current) =>
            (Previous, Current) = (previous, current);
    }
}
