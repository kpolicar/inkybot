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
        private SymSpell symSpell;

        public ScreenReaderDataProvider(IntPtr handle) {
            this.handle = handle;
            this.symSpell = new SymSpell();
            symSpell.LoadDictionary(@"A:\Projects\RiderProjects\bot\WindowsFormsApp\frequency_dictionary_en_82_765.txt", 0, 1);
        }

        public void FetchData() {
            scan = new DofusScreenScan(handle);
        }

        public IEnumerable<MageHistoryRecord> History() {
            var scanResults =
                scan.History();
            
            var records = scanResults.Select(mageEntry => {
                var changes = Regex.Matches(mageEntry, @"(-?\d+) ?(%? ?[A-z ]+[A-z])");
                var sinkChange = Regex.Match(mageEntry, @"[+-] ?sink");


                var statChanges = changes.Cast<Match>().Select(change =>
                {
                    var grouped = change.Groups;
                    var (value, name) = (grouped[1].Value, grouped[2].Value);
                    name = SpellCorrectStatName(name);

                    var stat = Stat.Stats.First(statData => statData.DisplayName == name);
                    var valuee = int.Parse(value);
                    
                    return new StatChanged(stat, valuee);
                }
            );
                
            return new MageHistoryRecord(statChanges, sinkChange.Success);
        });

        return records;
    }

        protected string SpellCorrectStatName(string name)
        {
            var terms = Regex.Split(name, " ")
                .Select(term => Regex.IsMatch(term, @"[\-\+\%]") ?
                    term :
                    symSpell.Lookup(term, SymSpell.Verbosity.Closest).First().term)
                .ToArray();

            if (string.Join(" ", terms) != name)
            {
                Debug.WriteLine("OCR error, original:"+name+", fixed:"+string.Join(" ", terms));
            }
            return string.Join(" ", terms);
        }

        public Item.ItemStat[] Stats() {
            var scanResults =
                scan.Stats();
            

            var stats = new Item.ItemStat[scanResults.Length];

            int i = 0;
            foreach (var result in scanResults)
            {
                var name = SpellCorrectStatName(result.name);
                var stat = Stat.Stats.First(statData => statData.DisplayName == name);
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