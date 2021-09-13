using System;
using System.Windows.Forms;
using Inkybot.Dofus;

namespace Inkybot.Actions
{
    public class Finish : RemoveItemFromMagingTable
    {
        public readonly Item Item;
        public readonly MageHistoryRecord? LastHistoryRecord;
        
        public Finish(Control targetControl, Item item, MageHistoryRecord? lastHistoryRecord) : base(targetControl) =>
            (Item, LastHistoryRecord) = (item, lastHistoryRecord);
    }
}
