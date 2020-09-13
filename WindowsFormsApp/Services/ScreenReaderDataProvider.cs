using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
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

        public async Task<Item.ItemStat[]> Stats() {
            var currentStatsResult =
                await scan.Stats();
            var minStatsResult =
                await scan.Min();
            var maxStatsResult = 
                await scan.Max();

            var stats = new Item.ItemStat[currentStatsResult.Length];

            for (int i = 0; i < currentStatsResult.Length; i++) {
                var (name, value) = currentStatsResult[i];
                var min = minStatsResult[i];
                var max = maxStatsResult[i];
                
                var stat = Stat.Stats.First(statData => statData.DisplayName == name);
                stats[i++] = new Item.ItemStat(stat, int.Parse(value), int.Parse(min), int.Parse(max));
            }

            return lastScanResults = stats;
        }
    }
}