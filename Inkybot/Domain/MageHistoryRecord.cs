using System.Collections.Generic;
using System.Linq;
using Inkybot.Domain;
using Inkybot.Exceptions;

namespace Inkybot.Domain
{
    public struct StatChanged
    {
        public static StatChanged Failure => new StatChanged(default, 0);
        
        public Stat stat;
        public int value;

        public float SinkModifier => stat.SinkValue * -value;

        public StatChanged(Stat stat, int value) {
            this.stat = stat;
            this.value = value;
        }

        public override string ToString() {
            return $"{stat} {value}";
        }
    }

    public class MageHistoryRecord
    {
        private readonly IEnumerable<StatChanged> changed;
        private readonly bool sinkChanged;

        public MageHistoryRecord(IEnumerable<StatChanged> changed, bool sinkChanged) {
            this.changed = changed;
            this.sinkChanged = sinkChanged;
        }

        public float ChangeInSink {
            get {
                if (!sinkChanged)
                    return 0f;
                if (attempted.Equals(default(StatChanged)))
                    throw new CouldNotResolveSinkException("Could not resolve sink solely from history record");
                
                return ChangeInSinkFromFallen + attempted.SinkModifier;
            }
        }

        public float ChangeInSinkFromFallen =>
            fell.Sum(statChange => statChange.SinkModifier);

        public StatChanged attempted =>
            changed.FirstOrDefault(change => change.value >= 0);

        public StatChanged[] fell =>
            changed.Where(change => change.value < 0).ToArray();

        public static bool operator ==(MageHistoryRecord operand1, MageHistoryRecord operand2) {
            if (ReferenceEquals(null, operand1) && !ReferenceEquals(null, operand2)) return false;
            if (!ReferenceEquals(null, operand1) && ReferenceEquals(null, operand2)) return false;
            if (ReferenceEquals(null, operand1) && ReferenceEquals(null, operand2)) return true;
            
            var comparison = operand1.changed.Zip(operand2.changed,
                (record1, record2) => new {Record1 = record1, Record2 = record2});

            return comparison.All(comparison =>
                comparison.Record1.stat.DisplayName == comparison.Record2.stat.DisplayName &&
                comparison.Record1.value == comparison.Record2.value) && operand1.sinkChanged == operand2.sinkChanged;
        }

        public static bool operator !=(MageHistoryRecord operand1, MageHistoryRecord operand2) {
            if (ReferenceEquals(null, operand1) && ReferenceEquals(null, operand2)) return false;
            if (!ReferenceEquals(null, operand1) && ReferenceEquals(null, operand2)) return true;
            if (ReferenceEquals(null, operand1) && !ReferenceEquals(null, operand2)) return true;
            
            var comparison = operand1.changed.Zip(operand2.changed,
                (record1, record2) => new {Record1 = record1, Record2 = record2});

            return operand1.sinkChanged != operand2.sinkChanged ||
                   comparison.Any(comparison =>
                       comparison.Record1.stat.DisplayName != comparison.Record2.stat.DisplayName ||
                       comparison.Record1.value != comparison.Record2.value);
        }
    }
}
