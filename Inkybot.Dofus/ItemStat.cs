using Inkybot.Dofus;

namespace Inkybot.Dofus
{
    public struct ItemStat
    {
        public Stat stat;
        public int value;
        public int min;
        public int max;
        public bool Exo => max == 0;

        public ItemStat(string statIdentifier, int value, int min, int max)
            : this (Stat.FirstOrNew(statIdentifier), value, min, max) {
        }

        public ItemStat(Stat stat, int value, int min, int max) {
            this.stat = stat;
            this.value = value;
            this.min = min;
            this.max = max;
        }

        public static bool operator == (ItemStat operand1, ItemStat operand2) {
            return operand1.stat == operand2.stat &&
                   operand1.min == operand2.min &&
                   operand1.max == operand2.max;
        }
        
        public static bool operator != (ItemStat operand1, ItemStat operand2) {
            return !(operand1 == operand2);
        }
    }
}
