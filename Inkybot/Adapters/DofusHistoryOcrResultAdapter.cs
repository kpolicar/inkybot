using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Inkybot.Dofus;
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
            return historyLines.Reverse().Select(mageEntry => {
                try {
                    var changes = SegmentMageHistoryEntry(mageEntry);
                    var sinkHasChanged =
                        Regex.IsMatch(mageEntry, Regex.Unescape(GameLanguageDetector.Current.SinkHasChangedPattern));

                    var statChanges = changes
                        .Cast<Match>()
                        .Select(change => HistoryEntrySegmentToStatChange(change.Groups))
                        .ToArray();

                    if (statChanges.Length == 0 && !sinkHasChanged) {
                        var isFailureResult =
                            Regex.IsMatch(mageEntry, Regex.Unescape(GameLanguageDetector.Current.FailurePattern));
                        if (!isFailureResult)
                            throw new CouldNotSegmentMageHistoryLineException("Unrecognizable mage history record");
                        return MageHistoryRecord.Failure;
                    }

                    return new MageHistoryRecord(statChanges, sinkHasChanged);
                } catch (OcrException) {
                    Debug.WriteLine($"Failed to segment history line {mageEntry}");
                    return null;
                }
                
            }).Where(mageHistoryRecord => mageHistoryRecord != null).Cast<MageHistoryRecord>();
        }

        private StatChanged HistoryEntrySegmentToStatChange(GroupCollection historyEntrySegments) {
            if (historyEntrySegments.Count != 3)
                throw new CouldNotSegmentMageHistoryLineException("Error occured trying to segment history line");
                
            // in case there is a space after the minus sign, remove it
            var (value, name) = (historyEntrySegments[1].Value.Replace(" ", ""), historyEntrySegments[2].Value);

            var stat = GetStatFromName(name);

            int parsedValue;
            if (!int.TryParse(value.Replace(" ", ""), out parsedValue)) {
                throw new CouldNotResolveStatValueException($"Error occured trying to resolve stat value for \"{name}\"");
            }

            return new StatChanged(stat, parsedValue);
        }

        private MatchCollection SegmentMageHistoryEntry(string historyLine) {
            var segments = Regex.Matches(historyLine, GameLanguageDetector.Current.HistoryEntryPattern);
            return segments;
        }

        private bool IsHistoryEntryMageFailure(string historyEntry) {
            var spellCorrected = spellCorrect.Lookup(historyEntry, SymSpell.Verbosity.Closest).FirstOrDefault();

            return spellCorrected != null && spellCorrected.term == "Failure";
        }
    }
}
