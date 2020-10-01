using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WindowsFormsApp.Actions;
using WindowsFormsApp.Adapters;
using WindowsFormsApp.Contracts;

namespace WindowsFormsApp
{
    public class ScreenReaderDataProvider : DofusDataProvider
    {
        private DofusScreenScan scan;
        public Item.ItemStat[] lastScanResults;
        private IntPtr handle;

        public ScreenReaderDataProvider(IntPtr handle) {
            this.handle = handle;
        }

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
            var stats = new DofusStatsOcrResultAdapter(scanResults).ToItemStats();
            
            return lastScanResults = stats;
        }
    }
}