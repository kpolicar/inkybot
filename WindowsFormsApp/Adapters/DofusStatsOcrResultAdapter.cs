using System;
using System.Diagnostics;
using System.Linq;

namespace WindowsFormsApp.Adapters
{
    public class DofusStatsOcrResultAdapter : DofusOcrResultAdapter
    {
        private readonly StatLineScanResult[] statLines;

        public DofusStatsOcrResultAdapter(StatLineScanResult[] statLines) {
            this.statLines = statLines;
        }

        public Item.ItemStat[] ToItemStats() {
            var stats = new Item.ItemStat[statLines.Length];

            var i = 0;
            foreach (var result in statLines) {
                var name = SpellCorrectStatName(result.name);
                var stat = Stat.Stats.First(statData => statData.DisplayName == name);
                try {
                    var min = result.min != "-" ? int.Parse(result.min) : 0;
                    var max = result.max != "-" ? int.Parse(result.max) : 0;
                    var valuee = result.value.Length > 0 ? int.Parse(result.value) : 0;
                    stats[i++] = new Item.ItemStat(stat, valuee, min, max);
                } catch (Exception ec) {
                    Debug.WriteLine("EXCEPTION: " + ec.Message);
                    break;
                }
            }

            return stats;
        }
    }
}
