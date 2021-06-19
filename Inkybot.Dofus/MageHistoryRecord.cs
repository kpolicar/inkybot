using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus.Exceptions;

namespace Inkybot.Dofus
{
    /**
     * <summary>
     * A single stat change record instance
     * </summary>
     */
    public struct StatChanged
    {
        /**
         * <summary>The stat that was changed</summary>
         */
        public Stat stat;
        /**
         * <summary>The amount the stat was changed</summary>
         */
        public int value;

        /**
         * <summary>How much sink had changed as a result of the stat change</summary>
         */
        public decimal SinkModifier => stat.SinkValue * -value;

        /**
         * <param name="stat">The stat that was changed</param>
         * <param name="value">The amount the stat was changed</param>
         */
        public StatChanged(Stat stat, int value) {
            this.stat = stat;
            this.value = value;
        }

        public override string ToString() {
            return $"{(value >= 0 ? $"+{value}" : $"{value}")} {stat}";
        }
    }

    /**
     * <summary>
     * The MageHistoryRecord class represents a single record within an item's mage history.
     * These are created during the maging process.
     * </summary>
     */
    public class MageHistoryRecord
    {
        /**
         * <summary>A mage history record representing a failed attempt.</summary>
         */
        public static MageHistoryRecord Failure = new MageHistoryRecord(new StatChanged[] {}, false);
        
        /**
         * <summary>An enumerable of all the stat changes that have occured in history record.</summary>
         */
        public readonly IEnumerable<StatChanged> Changed;
        
        /**
         * <summary>Whether or not the item's sink has been modified by the history record.</summary>
         */
        public readonly bool SinkChanged;

        /**
         * <param name="changed">An enumerable of all the stat changes that have occured in history record.</param>
         * <param name="sinkChanged">Whether or not the item's sink has been modified by the history record.</param>
         */
        public MageHistoryRecord(IEnumerable<StatChanged> changed, bool sinkChanged) {
            this.Changed = changed;
            this.SinkChanged = sinkChanged;
        }

        /**
         * <summary>The amount of sink that has been changed by the history record.</summary>
         */
        public decimal ChangeInSink {
            get {
                if (!SinkChanged)
                    return 0m;
                if (Landed == null)
                    throw new CouldNotResolveSinkException("Could not resolve sink solely from history record");
                
                return ChangeInSinkFromFallen + Landed.Value.SinkModifier;
            }
        }

        /**
         * <summary>The amount of sink that has been decreased by the fallen stats in the history record.</summary>
         */
        public decimal ChangeInSinkFromFallen =>
            Fell.Sum(statChange => statChange.SinkModifier);

        /**
         * <summary>The stat that has landed in the history record. If no stat had landed, the value is null.</summary>
         */
        public StatChanged? Landed =>
            Changed.Cast<StatChanged?>()
                .DefaultIfEmpty(null)
                .FirstOrDefault(change => {
                    if (change == null)
                        return false;
                    return change.Value.value >= 0;
                });

        /**
         * <summary>
         * The stats that have been modified as a result of the history record.
         * If the history record resulted in a failure, this array is empty.
         * </summary>
         */
        public StatChanged[] Fell =>
            Changed.Where(change => change.value < 0).ToArray();

        public static bool operator ==(MageHistoryRecord? operand1, MageHistoryRecord? operand2) {
            if (ReferenceEquals(null, operand1) && !ReferenceEquals(null, operand2)) return false;
            if (!ReferenceEquals(null, operand1) && ReferenceEquals(null, operand2)) return false;
            if (ReferenceEquals(null, operand1) && ReferenceEquals(null, operand2)) return true;
            if (operand1!.Changed.Count() != operand2!.Changed.Count())
                return false;
            
            var comparison = operand1.Changed.Zip(operand2.Changed,
                (record1, record2) => new {Record1 = record1, Record2 = record2});

            return comparison.All(comparison =>
                comparison.Record1.stat == comparison.Record2.stat &&
                comparison.Record1.value == comparison.Record2.value) && operand1.SinkChanged == operand2.SinkChanged;
        }

        public static bool operator !=(MageHistoryRecord? operand1, MageHistoryRecord? operand2) {
            if (ReferenceEquals(null, operand1) && ReferenceEquals(null, operand2)) return false;
            if (!ReferenceEquals(null, operand1) && ReferenceEquals(null, operand2)) return true;
            if (ReferenceEquals(null, operand1) && !ReferenceEquals(null, operand2)) return true;
            if (operand1!.Changed.Count() != operand2!.Changed.Count())
                return true;
            
            var comparison = operand1.Changed.Zip(operand2.Changed,
                (record1, record2) => new {Record1 = record1, Record2 = record2});

            return operand1.SinkChanged != operand2.SinkChanged ||
                   comparison.Any(comparison =>
                       comparison.Record1.stat != comparison.Record2.stat ||
                       comparison.Record1.value != comparison.Record2.value);
        }

        public override string ToString() {
            if (this == Failure)
                return "Failure";
            return string.Join(", ", Changed.Select(change => change.ToString()).Append(SinkChanged ? "sink" : ""));
        }
    }
}
