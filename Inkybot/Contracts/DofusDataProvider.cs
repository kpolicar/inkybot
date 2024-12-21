using System;
using System.Collections.Generic;
using Inkybot.Dofus;
using Inkybot.Domain;
using Inkybot.Events;
using Inkybot.Services;

namespace Inkybot.Contracts
{
    public interface DofusDataProvider
    {
        public event EventHandler<ItemEventArgs>? FetchedItem;

        void FetchData();
        Item Item();
        string[] History();
        //IEnumerable<MageHistoryRecord> History();
        public UserRunes Runes();
        public UserRune RuneQuantity(Rune rune);
    }
}
