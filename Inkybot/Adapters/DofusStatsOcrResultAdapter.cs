using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inkybot.Exceptions;

namespace Inkybot.Adapters
{
    public class DofusStatsOcrResultAdapter : DofusOcrResultAdapter
    {
        private readonly StatLineScanResult[] statLines;

        public DofusStatsOcrResultAdapter(StatLineScanResult[] statLines) {
            this.statLines = statLines;
        }

        public IEnumerable<Item.ItemStat> ToItemStats() {
            return statLines.Select(result => {

                var stat = GetStatFromName(result.name);
                var (min, max, value) = GetMinMaxValueFromScanResult(result);

                return new Item.ItemStat(stat, value, min, max);
            });
        }

        private (int min, int max, int value) GetMinMaxValueFromScanResult(StatLineScanResult result) {
            try {
                var min = result.min != "-" ? int.Parse(result.min) : 0;
                var max = result.max != "-" ? int.Parse(result.max) : 0;
                var value = result.value.Length > 0 ? int.Parse(result.value) : 0;

                return (min, max, value);
            } catch (Exception exception) {
                throw new CouldNotResolveStatValueException("", exception);
            }
        }
    }
}
