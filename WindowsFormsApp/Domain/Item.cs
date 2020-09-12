namespace WindowsFormsApp
{
    public class Item
    {
        public struct ItemStat
        {
            public Stat.Data stat;
            public int value;

            public ItemStat(Stat.Data itemStat, int value) {
                this.stat = itemStat;
                this.value = value;
            }
        }
    }
}