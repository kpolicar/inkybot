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
        public event EventHandler<ItemEventArgs> FetchedItem;
        private IntPtr handle;
        public ItemStatRepository lastScanResults;
        private DofusScreenScan scan;

        public void BindTo(IntPtr handle) {
            this.handle = handle;
        }

        public void FetchData() {
            if (handle == IntPtr.Zero)
                throw new SystemException();
            scan = new DofusScreenScan(handle);
        }

        public IEnumerable<MageHistoryRecord> History() {
            var scanResults = scan.History()
                .Select(line => line.Replace("\n", " "))
                .ToArray();
            var historyResults = new DofusHistoryOcrResultAdapter(scanResults).ToMageHistoryRecords();

            return historyResults;
        }

        public Item Item() {
            var scanResults = scan.Stats();
            var stats = new DofusStatsOcrResultAdapter(scanResults).ToItemStats();
            var item = new Item(lastScanResults = stats);
            FetchedItem?.Invoke(this, new ItemEventArgs(item));

            return item;
        }
    }
}
