using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Inkybot.Adapters;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Domain.Repositories;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Helpers;

namespace Inkybot.Services
{
    public partial class ScreenReaderDataProvider : DofusDataProvider
    {
        public event EventHandler<ScannedRegionEventArgs> ScannedStats;
        public event EventHandler<ScannedRegionEventArgs> ScannedHistory;
        public event EventHandler<ItemEventArgs> FetchedItem;
        private IntPtr handle;
        public Item previousScannedItem;
        private DofusScreenScan scan;

        public void BindTo(IntPtr handle) {
            this.handle = handle;
        }

        public void FetchData() {
            if (handle == IntPtr.Zero)
                throw new SystemException();
            scan = new DofusScreenScan(handle);
        }

        public void FetchData(Image image, bool saveToDisk=false) {
            scan = new DofusScreenScan(image, saveToDisk);
        }

        public IEnumerable<MageHistoryRecord> LatestHistory() {
            var scanResults = scan.LatestHistory()
                .Result
                .Select(line => line.Replace("\n", " "))
                .ToArray();
            ScannedHistory?.Invoke(this, new ScannedRegionEventArgs(scanResults));
            
            var historyResults = new DofusHistoryOcrResultAdapter(scanResults).ToMageHistoryRecords();

            return historyResults;
        }

        public IEnumerable<MageHistoryRecord> History() {
            var scanResults = scan.History()
                .Result
                .Select(line => line.Replace("\n", " "))
                .ToArray();
            ScannedHistory?.Invoke(this, new ScannedRegionEventArgs(scanResults));
            
            var historyResults = new DofusHistoryOcrResultAdapter(scanResults).ToMageHistoryRecords();

            return historyResults;
        }

        public Item Item() {
            var scanResults = scan.Stats().Result;
            ScannedStats?.Invoke(this, new ScannedRegionEventArgs(scanResults));
            
            var stats = new DofusStatsOcrResultAdapter(scanResults).ToItemStats();
            var item = new Item(stats);
            FetchedItem?.Invoke(this, new ItemEventArgs(item));

            return previousScannedItem = item;
        }

        public Dictionary<Stat, UserRune[]> Runes() {
            var item = Item();
            var scanResults = scan.RunesQuantities().Result;

            var userRunes = new DofusStatUserRunesOcrResultAdapter(item, scanResults).ToUserRunes();

            return userRunes
                .ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);
        }

        public UserRune RuneQuantity(Rune rune) {
            var row = previousScannedItem.Stats
                .Select((Value, Index) => new { Value, Index })
                .Single(p => p.Value.stat == rune.stat)
                .Index;
            
            var column = (int) rune.type;
            var runeQuantityScan = scan.RuneQuantity(column, row).Result;
            
            return new UserRune(rune, runeQuantityScan.Quantity);
        }

        public void SetupForActiveItem() {
        }
    }
}
