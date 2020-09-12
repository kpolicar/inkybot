using System.Collections.Generic;

namespace WindowsFormsApp
{
    public class Rune
    {
        private Item.ItemStat itemStat;
        private Type type;

        public Rune(Item.ItemStat itemStat, Type type) {
            this.itemStat = itemStat;
            this.type = type;
        }

        public enum Type
        {
            Sm, Pa, Ra
        }
    }
}