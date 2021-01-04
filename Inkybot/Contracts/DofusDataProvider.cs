using System;
using System.Collections.Generic;
using Inkybot.Dofus;
using Inkybot.Domain;
using Inkybot.Events;

namespace Inkybot.Contracts
{
    public interface DofusDataProvider
    {
        public event EventHandler<ItemEventArgs>? FetchedItem;

        Item Item();
        void FetchData();
        IEnumerable<MageHistoryRecord> History();
        public Dictionary<Stat, UserRune[]> Runes();
        public UserRune RuneQuantity(Rune rune);
    }
}
