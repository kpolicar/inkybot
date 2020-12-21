using System.Collections.Generic;
using System.Linq;
using Inkybot.Domain;
using Inkybot.Exceptions;

namespace Inkybot.Domain
{
    public struct StatChanged
    {
        public Stat stat;
        public int value;

        public float SinkModifier => stat.SinkValue * -value;

        public StatChanged(Stat stat, int value) {
            this.stat = stat;
            this.value = value;
        }

        public override string ToString() {
            return $"{(value >= 0 ? $"+{value}" : $"{value}")} {stat}";
        }
    }

    public class MageHistoryRecord
    {
        public static MageHistoryRecord Failure = new MageHistoryRecord(new StatChanged[] {}, false);
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
                if (Landed == null)
                    throw new CouldNotResolveSinkException("Could not resolve sink solely from history record");
                
                return ChangeInSinkFromFallen + Landed.Value.SinkModifier;
            }
        }

        public float ChangeInSinkFromFallen =>
            Fell.Sum(statChange => statChange.SinkModifier);

        public StatChanged? Landed =>
            changed.Cast<StatChanged?>()
                .DefaultIfEmpty(null)
                .FirstOrDefault(change => {
                    if (change == null)
                        return false;
                    return change.Value.value >= 0;
                });

        public StatChanged[] Fell =>
            changed.Where(change => change.value < 0).ToArray();

        public static bool operator ==(MageHistoryRecord operand1, MageHistoryRecord operand2) {
            if (ReferenceEquals(null, operand1) && !ReferenceEquals(null, operand2)) return false;
            if (!ReferenceEquals(null, operand1) && ReferenceEquals(null, operand2)) return false;
            if (ReferenceEquals(null, operand1) && ReferenceEquals(null, operand2)) return true;
            if (operand1.changed.Count() != operand2.changed.Count())
                return false;
            
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
            if (operand1.changed.Count() != operand2.changed.Count())
                return true;
            
            var comparison = operand1.changed.Zip(operand2.changed,
                (record1, record2) => new {Record1 = record1, Record2 = record2});

            return operand1.sinkChanged != operand2.sinkChanged ||
                   comparison.Any(comparison =>
                       comparison.Record1.stat.DisplayName != comparison.Record2.stat.DisplayName ||
                       comparison.Record1.value != comparison.Record2.value);
        }

        public override string ToString() {
            if (this == Failure)
                return "Failure";
            return string.Join(", ", changed.Select(change => change.ToString()).Append(sinkChanged ? "sink" : ""));
        }
    }
}
