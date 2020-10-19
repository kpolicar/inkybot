using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Adapters;
using Inkybot.Contracts;
using Inkybot.Events;

namespace Inkybot
{
    public class ScreenReaderDataProvider : DofusDataProvider
    {
        private readonly IntPtr handle;
        public Item.ItemStat[] lastScanResults;
        private DofusScreenScan scan;
        
        public ScreenReaderDataProvider(IntPtr handle) {
            this.handle = handle;
        }

        public event EventHandler<StatsEventArgs> FetchedStats;

        public void FetchData() {
            scan = new DofusScreenScan(handle);
        }

        public IEnumerable<MageHistoryRecord> History() {
            var scanResults = scan.History();
            var historyResults = new DofusHistoryOcrResultAdapter(scanResults).ToMageHistoryRecords();

            return historyResults;
        }

        public Item.ItemStat[] Stats() {
            var scanResults = scan.Stats();
            var stats = new DofusStatsOcrResultAdapter(scanResults).ToItemStats().ToArray();
            FetchedStats?.Invoke(this, new StatsEventArgs(stats));

            return lastScanResults = stats;
        }
    }
}
