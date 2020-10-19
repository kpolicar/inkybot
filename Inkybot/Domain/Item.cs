namespace Inkybot
{
    public class Item
    {
        public struct ItemStat
        {
            public Stat stat;
            public int value;
            public int min;
            public int max;

            public ItemStat(Stat itemStat, int value, int min, int max) {
                stat = itemStat;
                this.value = value;
                this.min = min;
                this.max = max;
            }

            public static bool operator == (ItemStat operand1, ItemStat operand2) {
                return operand1.stat == operand2.stat &&
                       operand1.value == operand2.value &&
                       operand1.min == operand2.min &&
                       operand1.max == operand2.max;
            }
            
            public static bool operator != (ItemStat operand1, ItemStat operand2) {
                return !(operand1 == operand2);
            }
        }
    }
}
