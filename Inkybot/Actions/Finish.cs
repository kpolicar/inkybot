using System;
using System.Windows.Forms;
using Inkybot.Dofus;

namespace Inkybot.Actions
{
    public class Finish : RemoveItemFromMagingTable
    {
        public readonly Item Item;
        
        public Finish(Control targetControl, Item item) : base(targetControl) =>
            (Item) = (item);
    }
}
