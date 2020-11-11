using System;

namespace Inkybot
{
    public class Rune
    {
        public static bool operator == (Rune operand1, Rune operand2) {
            return operand1?.type == operand2?.type && operand1?.stat == operand2?.stat;
        }
            
        public static bool operator != (Rune operand1, Rune operand2) {
            return !(operand1 == operand2);
        }
        
        public enum Type
        {
            Sm,
            Pa,
            Ra
        }

        public string DisplayName {
            get {
                var prefix = type switch {
                    Type.Sm => "",
                    Type.Pa => "Pa ",
                    Type.Ra => "Ra ",
                };
                return prefix + stat.RuneName+ " rune";
            }
        }
        public Stat stat;
        public Type type;

        public Rune(Stat stat, Type type) {
            this.stat = stat;
            this.type = type;
        }

        public int IncreaseInValue {
            get {
                var typeValue = type switch {
                    Type.Sm => 1,
                    Type.Pa => 3,
                    Type.Ra => 10
                };
                if (stat.SinkValue < 1) {
                    var increase = typeValue / stat.SinkValue;
                    return (int) Math.Ceiling(increase);
                }

                return typeValue;
            }
        }

        public float Sink => stat.SinkValue * IncreaseInValue;
    }
}
