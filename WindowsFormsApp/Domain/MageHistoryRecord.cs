using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WindowsFormsApp
{
    public struct StatChanged
    {
        public Stat stat;
        public int value;

        public float SinkModifier => stat.sinkValue * value;

        public StatChanged(Stat stat, int value) {
            this.stat = stat;
            this.value = value;
        }
    }
    
    public class MageHistoryRecord
    {
        private IEnumerable<StatChanged> changed;
        private bool sinkChanged;

        public float ChangeInSink =>
            !sinkChanged ? 0f :
            fell.Sum(statChange => -statChange.SinkModifier)
            - attempted.SinkModifier;

        public StatChanged attempted =>
            changed.First(change => change.value >= 0);
        
        public StatChanged[] fell =>
            changed.Where(change => change.value < 0).ToArray();
        
        public MageHistoryRecord(IEnumerable<StatChanged> changed, bool sinkChanged) {
            this.changed = changed;
            this.sinkChanged = sinkChanged;
        }

        public static bool operator ==  (MageHistoryRecord operand1, MageHistoryRecord operand2) {
            var comparison = operand1.changed.Zip(operand2.changed, (record1, record2) => new { Record1 = record1, Record2 = record2});
            
            return comparison.All(comparison =>
                comparison.Record1.stat.DisplayName == comparison.Record2.stat.DisplayName &&
                comparison.Record1.value == comparison.Record2.value) && operand1.sinkChanged == operand2.sinkChanged;
        }
        
        public static bool operator !=  (MageHistoryRecord operand1, MageHistoryRecord operand2) {
            var comparison = operand1.changed.Zip(operand2.changed, (record1, record2) => new { Record1 = record1, Record2 = record2});
            
            return operand1.sinkChanged != operand2.sinkChanged ||
                   comparison.Any(comparison =>
                       comparison.Record1.stat.DisplayName != comparison.Record2.stat.DisplayName ||
                       comparison.Record1.value != comparison.Record2.value);
        }
    }
}