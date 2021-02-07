using System;
using System.Globalization;
using System.Resources;

namespace Inkybot.Dofus
{
    public class Rune
    {
        public static ResourceSet Dictionary = null!;

        public enum RuneType
        {
            Sm,
            Pa,
            Ra
        }

        public readonly Stat Stat;
        public readonly RuneType Type;

        public Rune? Weaker =>
            Type != RuneType.Sm
                ? new Rune(Stat, Type - 1)
                : null;

        public Rune(Stat stat, RuneType type) =>
            (Stat, Type) = (stat, type);

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

        public float Sink => Stat.SinkValue * IncreaseInValue;
        
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
