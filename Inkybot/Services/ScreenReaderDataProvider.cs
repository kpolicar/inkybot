using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Inkybot.Adapters;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Domain;
using Inkybot.Domain.Repositories;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Helpers;
using Debug = System.Diagnostics.Debug;

namespace Inkybot.Services
{
    public partial class ScreenReaderDataProvider : DofusDataProvider, InjectableService
    {
        internal const int MaxSupportedStatsForMage = 12;
        public event EventHandler<ScannedRegionEventArgs> ScannedStats;
        public event EventHandler<ScannedRegionEventArgs> ScannedHistory;
        public event EventHandler<ScanBoundsChanged> LatestHistoryBoundsChanged;
        public event EventHandler<ItemEventArgs> FetchedItem;
        private IntPtr handle;
        public Item previousScannedItem;
        public DofusScreenScan Scan {
            get;
            private set;
        }
        public Responsive.Measurement LatestHistoryBounds = Measurements.HistoryBounds;
        private string[] previousMinMaxScan = {};
        private ServiceContainer serviceContainer;


        public void BindDependencies(ServiceContainer serviceContainer) {
            var magingJob = serviceContainer.GetService<DofusMagingJob>();
            this.serviceContainer = serviceContainer;
            magingJob.Stopped += (sender, args) => Reset();
        }

        public void Reset() {
            LatestHistoryBounds = Measurements.HistoryBounds;
            LatestHistoryBoundsChanged?.Invoke(this, new ScanBoundsChanged(LatestHistoryBounds));
            previousScannedItem = null;
            previousMinMaxScan = new string[] {};
        }
        
        public void BindTo(IntPtr handle) {
            this.handle = handle;
        }

        public void FetchData() {
            Scan?.Dispose();
            Scan = new DofusScreenScan(handle, serviceContainer, LatestHistoryBounds);
        }

        public void FetchData(Image image, bool saveToDisk=false) {
            Scan?.Dispose();
            Scan = new DofusScreenScan(image, serviceContainer, LatestHistoryBounds, saveToDisk);
        }

        public int? AverageItemBalance() {
            var scanResults = Scan.AverageItemBalance().Result;

            return scanResults;
        }

        public IEnumerable<MageHistoryRecord> LatestHistory() {
            var scanResults = Scan.LatestHistory()
                .Result
                .Select(line => line.Replace("\n", " "))
                .ToArray();
            ScannedHistory?.Invoke(this, new ScannedRegionEventArgs(scanResults));
            
            var historyResults = new DofusHistoryOcrResultAdapter(scanResults).ToMageHistoryRecords();

            return historyResults;
        }
        
        public void ApproveLatestHistoryContinueToNextScanBounds() {
            LatestHistoryBounds = Scan.CalculateNextHistoryBounds();
            LatestHistoryBoundsChanged?.Invoke(this, new ScanBoundsChanged(LatestHistoryBounds));
        }

        public IEnumerable<MageHistoryRecord> History() {
            var scanResults = Scan.History()
                .Result
                .Select(line => line.Replace("\n", " "))
                .ToArray();
            ScannedHistory?.Invoke(this, new ScannedRegionEventArgs(scanResults));
            
            var historyResults = new DofusHistoryOcrResultAdapter(scanResults).ToMageHistoryRecords();

            return historyResults;
        }

        public Item Item() {
            if (previousMinMaxScan.Length == 0) {
                previousMinMaxScan = Scan.MinMaxStats().Result;
            }
            var scanResults = Scan.Stats().Result;
            var statsResult = scanResults
                .ZipWithDefault(previousMinMaxScan, (value, minmax) => (minmax ?? "- -") + " " + value)
                .ToArray();
            
            ScannedStats?.Invoke(this, new ScannedRegionEventArgs(statsResult));
            
            var stats = new DofusStatsOcrResultAdapter(statsResult).ToItemStats();
            var item = new Item(stats);
            FetchedItem?.Invoke(this, new ItemEventArgs(item));

            return previousScannedItem = item;
        }

        public Dictionary<Stat, UserRune[]> Runes() {
            var item = Item();
            var scanResults = Scan.RunesQuantities().Result;

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
            var runeQuantityScan = Scan.RuneQuantity(column, row).Result;
            
            return new UserRune(rune, runeQuantityScan.Quantity);
        }

        public bool IsSupportedItem(Item item) {
            return item.Stats.Length <= MaxSupportedStatsForMage;
        }

        public bool IsSupportedConfig(Config config) {
            return config.StatsConfig.Count <= MaxSupportedStatsForMage;
        }
    }
}
