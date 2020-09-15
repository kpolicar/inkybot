using System;

namespace WindowsFormsApp
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
                this.stat = itemStat;
                this.value = value;
                this.min = min;
                this.max = max;
            }
        }
    }
}