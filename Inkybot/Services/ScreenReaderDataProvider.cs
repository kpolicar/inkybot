using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inkybot.Adapters;
using Inkybot.Contracts;
using Inkybot.Domain.Repositories;
using Inkybot.Events;
using Inkybot.Exceptions;

namespace Inkybot
{
    public class ScreenReaderDataProvider : DofusDataProvider
    {
        private readonly IntPtr handle;
        public ItemStatRepository lastScanResults;
        private DofusScreenScan scan;
        
        public ScreenReaderDataProvider(IntPtr handle) {
            this.handle = handle;
        }

        public event EventHandler<StatsEventArgs> FetchedStats;

        public void FetchData() {
            scan = new DofusScreenScan(handle);
        }

        public IEnumerable<MageHistoryRecord> History() {
            var scanResults = scan.History()
                .Select(line => line.Replace("\n", " "))
                .ToArray();
            var historyResults = new DofusHistoryOcrResultAdapter(scanResults).ToMageHistoryRecords();

            return historyResults;
        }

        public ItemStatRepository Stats() {
            var scanResults = scan.Stats();
            var stats = new DofusStatsOcrResultAdapter(scanResults).ToItemStats();
            FetchedStats?.Invoke(this, new StatsEventArgs(stats));

            return lastScanResults = stats;
        }
    }
}
