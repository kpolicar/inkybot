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
            var scanResults =
                await scan.Stats();
            
            Debug.WriteLine(scanResults.Length);


            var stats = new Item.ItemStat[scanResults.Length];

            int i = 0;
            foreach (var result in scanResults) {
                var value = Regex.Match(result.stat, @"-?\d+").Value;
                var name = Regex.Replace(result.stat, @"-?\d+ ?", "");
                
                
                var stat = Stat.Stats.First(statData => statData.DisplayName == name);
                Debug.WriteLine("stat: "+name);
                Debug.WriteLine("value: "+value);
                Debug.WriteLine("min: "+result.min);
                Debug.WriteLine("max: "+result.max);
                try {
                    var min = result.min != "-" ? int.Parse(result.min) : 0;
                    var max = result.max != "-" ? int.Parse(result.max) : 0;
                    var valuee = value.Length > 0 ? int.Parse(value) : 0;
                    stats[i++] = new Item.ItemStat(stat, valuee, min, max);
                }
                catch (Exception ec) {
                    Debug.WriteLine("EXCEPTION: " + ec.Message);
                    return new Item.ItemStat[]{};
                }
            }
            Debug.WriteLine("-----");
            
            return lastScanResults = stats;
        }
    }
}