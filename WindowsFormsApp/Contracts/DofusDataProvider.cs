using System;
using System.Collections.Generic;
using WindowsFormsApp.Events;

namespace WindowsFormsApp.Contracts
{
    public interface DofusDataProvider
    {
        public event EventHandler<StatsEventArgs> FetchedStats;

        Item.ItemStat[] Stats();
        void FetchData();
        IEnumerable<MageHistoryRecord> History();
    }
}
