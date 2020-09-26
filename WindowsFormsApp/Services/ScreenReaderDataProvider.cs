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
using WindowsFormsApp.Contracts;

namespace WindowsFormsApp
{
    public class ScreenReaderDataProvider : DofusDataProvider
    {
        private DofusScreenScan scan;
        public Item.ItemStat[] lastScanResults;
        private IntPtr handle;
        private TextInfo text = CultureInfo.CurrentCulture.TextInfo;

        public ScreenReaderDataProvider(IntPtr handle) {
            this.handle = handle;
        }

        public void FetchData() {
            scan = new DofusScreenScan(handle);
        }

        public IEnumerable<MageHistoryRecord> History() {
            var scanResults =
                scan.History();

            var records = scanResults.Select(mageEntry => {
                var changes = Regex.Matches(mageEntry, @"(-?\d+) ?(%? ?[A-z ]+[A-z])");
                var sinkChange = Regex.Match(mageEntry, @"[+-]sink");


                var statChanges = changes.Cast<Match>().Select(change => {
                        var grouped = change.Groups;
                        var (value, name) = (grouped[1].Value, grouped[2].Value);
                        name = text.ToTitleCase(name);

                        var stat = Stat.Stats.First(statData => statData.DisplayName == name);
                        var valuee = int.Parse(value);
                        
                        return new StatChanged(stat, valuee);
                    }
                );
                
                return new MageHistoryRecord(statChanges, sinkChange.Success);
            });

            return records;
        }

        public Item.ItemStat[] Stats() {
            var scanResults =
                scan.Stats();
            

            var stats = new Item.ItemStat[scanResults.Length];

            int i = 0;
            foreach (var result in scanResults) {
                var stat = Stat.Stats.First(statData => statData.DisplayName == text.ToTitleCase(result.name));
                try {
                    var min = result.min != "-" ? int.Parse(result.min) : 0;
                    var max = result.max != "-" ? int.Parse(result.max) : 0;
                    var valuee = result.value.Length > 0 ? int.Parse(result.value) : 0;
                    stats[i++] = new Item.ItemStat(stat, valuee, min, max);
                }
                catch (Exception ec) {
                    Debug.WriteLine("EXCEPTION: " + ec.Message);
                    break;
                }
            }
            
            return lastScanResults = stats;
        }
    }
}