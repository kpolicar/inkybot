using System;
using System.Collections.Generic;
using Inkybot.Domain.Repositories;
using Inkybot.Events;

namespace Inkybot.Contracts
{
    public interface DofusDataProvider
    {
        public event EventHandler<StatsEventArgs> FetchedStats;

        ItemStatRepository Stats();
        void FetchData();
        IEnumerable<MageHistoryRecord> History();
    }
}
