using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Inkybot.Domain;
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
                var sinkHasChanged = Regex.IsMatch(mageEntry, Properties.Regex.SinkHasChangedPattern);

                var statChanges = changes
                    .Cast<Match>()
                    .Select(change => HistoryEntrySegmentToStatChange(change.Groups));

                return new MageHistoryRecord(statChanges, sinkHasChanged);
            });
        }

        private StatChanged HistoryEntrySegmentToStatChange(GroupCollection historyEntrySegments) {
            if (historyEntrySegments.Count != 3)
                throw new CouldNotSegmentMageHistoryLineException("Error occured trying to segment history line");
                
            // in case there is a space after the minus sign, remove it
            var (value, name) = (historyEntrySegments[1].Value.Replace(" ", ""), historyEntrySegments[2].Value);

            var stat = GetStatFromName(name);

            int parsedValue;
            if (!int.TryParse(value, out parsedValue)) {
                throw new CouldNotResolveStatValueException($"Error occured trying to resolve stat value for \"{name}\"");
            }

            return new StatChanged(stat, parsedValue);
        }

        private MatchCollection SegmentMageHistoryEntry(string historyLine) {
            var segments = Regex.Matches(historyLine, @"(-? ?\d+) ?(%? ?[A-z( ]+[A-z)])");
            return segments;
        }

        private bool IsHistoryEntryMageFailure(string historyEntry) {
            var spellCorrected = spellCorrect.Lookup(historyEntry, SymSpell.Verbosity.Closest).FirstOrDefault();

            return spellCorrected != null && spellCorrected.term == "Failure";
        }
    }
}
