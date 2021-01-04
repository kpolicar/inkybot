using System;
using Inkybot.Dofus;

namespace Inkybot.Events
{
    public class RuneQuantityChangedEventArgs : EventArgs
    {
        
        public readonly Rune Rune;
        public readonly float Quantity;
        public readonly float OldQuantity;


        public RuneQuantityChangedEventArgs(Rune rune, float oldQuantity, float quantity) {
            this.Rune = rune;
            this.Quantity = quantity;
            this.OldQuantity = oldQuantity;
        }

    }
}
