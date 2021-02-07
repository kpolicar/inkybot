using System;

namespace Inkybot.Dofus
{
    public class ItemStat
    {
        public readonly Stat Stat;
        public readonly int Value;
        public readonly int Min;
        public readonly int Max;
        public bool Exo => Max == 0;
        public float Oversink => Math.Max(Value-Max, 0) * Stat.SinkValue;

        public ItemStat(string statIdentifier, int value, int min, int max)
            : this (Stat.FirstOrNew(statIdentifier), value, min, max) {
        }

        public ItemStat(Stat stat, int value, int min, int max) =>
            (Stat, Value, Min, Max) =
            (stat, value, min, max);
        
        public override bool Equals(object obj) =>
            obj is ItemStat other && Equals(other);

        public bool Equals(ItemStat other) => 
            (Stat, Value, Min, Max) ==
            (other.Stat, other.Value, other.Min, other.Max);
        
        public override int GetHashCode() {
            unchecked {
                var hashCode = Stat.GetHashCode();
                hashCode = (hashCode * 397) ^ Value;
                hashCode = (hashCode * 397) ^ Min;
                hashCode = (hashCode * 397) ^ Max;
                return hashCode;
            }
        }

        public static bool operator == (ItemStat operand1, ItemStat operand2) {
            return operand1.Stat == operand2.Stat &&
                   operand1.Min == operand2.Min &&
                   operand1.Max == operand2.Max;
        }
        
        public static bool operator != (ItemStat operand1, ItemStat operand2) {
            return !(operand1 == operand2);
        }
    }
}
