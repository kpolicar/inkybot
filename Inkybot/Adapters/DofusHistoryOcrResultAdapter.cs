using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Inkybot.Exceptions;

namespace Inkybot.Adapters
{
    public class DofusHistoryOcrResultAdapter : DofusOcrResultAdapter
    {
        private readonly string[] historyLines;

        public DofusHistoryOcrResultAdapter(string[] historyLines) {
            this.historyLines = historyLines;
        }

        public IEnumerable<MageHistoryRecord> ToMageHistoryRecords() {
            return historyLines.Select(mageEntry => {
                var changes = SegmentMageHistoryEntry(mageEntry);
                var sinkHasChanged = Regex.IsMatch(mageEntry, @"[+-] ?sink");

                var statChanges = changes
                    .Cast<Match>()
                    .Select(change => HistoryEntrySegmentToStatChange(change.Groups));

                return new MageHistoryRecord(statChanges, sinkHasChanged);
            });
        }

        private StatChanged HistoryEntrySegmentToStatChange(GroupCollection historyEntrySegments) {
            var (value, name) = (historyEntrySegments[1].Value, historyEntrySegments[2].Value);

            var stat = GetStatFromName(name);

            int parsedValue;
            if (!int.TryParse(value, out parsedValue)) {
                throw new CouldNotResolveStatValueException("");
            }

            return new StatChanged(stat, parsedValue);
        }

        private MatchCollection SegmentMageHistoryEntry(string historyLine) {
            var segments = Regex.Matches(historyLine, @"(-?\d+) ?(%? ?[A-z ]+[A-z])");
            if (segments.Count != 2) 
                throw new CouldNotSegmentMageHistoryLineException("");
            return segments;
        }
    }
}
