using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Inkybot.Domain.Repositories;
using Inkybot.Exceptions;

namespace Inkybot.Adapters
{
    public class DofusStatsOcrResultAdapter : DofusOcrResultAdapter
    {
        private readonly string[] statLines;

        public DofusStatsOcrResultAdapter(string[] statLines) {
            this.statLines = statLines;
        }

        public ItemStatRepository ToItemStats() {
            return new ItemStatRepository(statLines.Select(mageEntry => {
                Debug.WriteLine(mageEntry);
                var changes = SegmentItemStatLine(mageEntry);

                try {
                    return StatLineToItemStat(changes.Groups);
                } catch (CouldNotSegmentStatLineException) {
                    throw new CouldNotSegmentStatLineException($"Error occured trying to segment stat line: {mageEntry}");
                }
            }).ToArray());
        }

        private (int min, int max, int value) GetMinMaxValueFromScanResult(GroupCollection historyEntrySegments) {
            try {
                var (min, max, value) =
                    (historyEntrySegments[1].Value, historyEntrySegments[2].Value, historyEntrySegments[3].Value);
                
                var parsedMin = min != "-" ? int.Parse(min) : 0;
                var parsedMax = max != "-" ? int.Parse(max) : 0;
                var parsedValue = value.Length > 0 ? int.Parse(value) : 0;

                return (parsedMin, parsedMax, parsedValue);
            } catch (Exception exception) {
                throw new CouldNotResolveStatValueException($"Error occured resolving value for stat {historyEntrySegments[4].Value}", exception);
            }
        }
        
        private ItemStat StatLineToItemStat(GroupCollection historyEntrySegments) {
            if (historyEntrySegments.Count != 5)
                throw new CouldNotSegmentStatLineException($"Error occured trying to segment stat line");

            var (min, max, value) = GetMinMaxValueFromScanResult(historyEntrySegments);

            var name = historyEntrySegments[4].Value;
            var stat = GetStatFromName(name);

            return new ItemStat(stat, value, min, max);
        }

        private Match SegmentItemStatLine(string historyLine) {
            var segments = Regex.Match(historyLine, Properties.Regex.ItemStatLinePattern);
            //todo fr: var segments = Regex.Match(historyLine, @"^(-?\d+|-) (-?\d+|-) (-?\d*) ?(%? ?[A-z()À-ÿ' ]+)$");
            return segments;
        }
    }
}
