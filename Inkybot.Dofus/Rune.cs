using System;
using System.Globalization;
using System.Resources;

namespace Inkybot.Dofus
{
    /**
     * <summary>
     * The Rune class represents a single rune that can be used on items.
     * </summary>
     */
    public class Rune
    {
        /**
         * <summary>The active dictionary used to represent runes</summary>
         */
        public static ResourceSet Dictionary = null!;

        /**
         * <summary>The type (strength) of the rune.</summary>
         */
        public enum RuneType
        {
            Sm,
            Pa,
            Ra
        }

        /**
         * <summary>The stat that the rune represents.</summary>
         */
        public readonly Stat Stat;
        
        /**
         * <summary>The type (strength) of the rune.</summary>
         */
        public readonly RuneType Type;

        /**
         * <summary>
         *  A rune of the same type, but one strength lower.
         *  If there is no weaker rune, the property returns null.
         * </summary>
         */
        public Rune? Weaker =>
            Type != RuneType.Sm
                ? new Rune(Stat, Type - 1)
                : null;

        /**
         * <param name="stat">The stat that the rune represents.</param>
         * <param name="type">The type (strength) of the rune.</param>
         */
        public Rune(Stat stat, RuneType type) =>
            (Stat, Type) = (stat, type);

        /**
         * <summary>The amount the rune will increase.</summary>
         */
        public int IncreaseInValue {
            get {
                var typeValue = Type switch {
                    RuneType.Sm => 1,
                    RuneType.Pa => 3,
                    RuneType.Ra => 10
                };
                if (Stat.SinkValue < 1) {
                    var increase = typeValue / Stat.SinkValue;
                    return (int) Math.Ceiling(increase);
                }

                return typeValue;
            }
        }

        /**
         * <summary>The amount of sink the rune will consume.</summary>
         */
        public decimal Sink => Stat.SinkValue * IncreaseInValue;
        
        /**
         * <summary>The representable display name of the rune.</summary>
         */
        public string DisplayName {
            get {
                var runeName = Stat.RuneName;
                var format = Dictionary.GetString("format")!;
        
                return Type switch {
                    RuneType.Ra =>
                        format.Replace(":name", runeName)
                            .Replace(":strength", "RA"),
                    RuneType.Pa =>
                        format.Replace(":name", runeName)
                            .Replace(":strength", "PA"),
                    RuneType.Sm =>
                        format.Replace(":name", runeName)
                            .Replace(":strength ", ""),
                };
            }
        }
        
        
        public static bool operator == (Rune? operand1, Rune? operand2) =>
            ReferenceEquals(operand1, null) == ReferenceEquals(operand2, null) &&
            Equals(operand1, operand2);

        public static bool operator !=(Rune? operand1, Rune? operand2) =>
            !(operand1 == operand2);
        public override string ToString() => DisplayName;
        
        public override bool Equals(object? obj) =>
            obj is Rune other && Equals(other);

        public bool Equals(Rune other) =>
            (Stat, Type).Equals(
                (other.Stat, other.Type));

        public override int GetHashCode() {
            unchecked {
                return (Stat.GetHashCode() * 397) ^ (int) Type;
            }
        }
    }
}
