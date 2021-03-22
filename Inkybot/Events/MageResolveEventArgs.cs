using System;
using Inkybot.Dofus.Domain;

namespace Inkybot.Events
{
    internal class MageResolveEventArgs : EventArgs
    {
        public readonly ItemMage Mage;
        
        public MageResolveEventArgs(ItemMage proposedMage) =>
            Mage = proposedMage;
    }
}
