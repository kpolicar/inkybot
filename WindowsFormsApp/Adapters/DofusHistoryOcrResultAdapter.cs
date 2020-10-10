using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WindowsFormsApp.Adapters
{
    public class DofusHistoryOcrResultAdapter : DofusOcrResultAdapter
    {
        private readonly string[] historyLines;

        public DofusHistoryOcrResultAdapter(string[] historyLines) {
            this.historyLines = historyLines;
        }

        public IEnumerable<MageHistoryRecord> ToMageHistoryRecords() {
            return historyLines.Select(mageEntry => {
                var changes = Regex.Matches(mageEntry, @"(-?\d+) ?(%? ?[A-z ]+[A-z])");
                var sinkChange = Regex.Match(mageEntry, @"[+-] ?sink");


                var statChanges = changes.Cast<Match>().Select(change => {
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
        }
    }
}
