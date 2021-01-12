using System;
using Inkybot.Dofus;

namespace Inkybot.Events
{
    public class RuneQuantityChangedEventArgs : EventArgs
    {
        
        public readonly Rune Rune;
        public readonly int Quantity;
        public readonly int OldQuantity;


        public RuneQuantityChangedEventArgs(Rune rune, int oldQuantity, int quantity) {
            Rune = rune;
            Quantity = quantity;
            OldQuantity = oldQuantity;
        }

    }
}
