using System;
using System.Collections.Generic;
using Inkybot.Events;

namespace Inkybot.Contracts
{
    public interface DofusDataProvider
    {
        public event EventHandler<StatsEventArgs> FetchedStats;

        Item.ItemStat[] Stats();
        void FetchData();
        IEnumerable<MageHistoryRecord> History();
    }
}
