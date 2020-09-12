using System.Collections.Generic;

namespace WindowsFormsApp
{
    public class Rune
    {
        public Stat.Data stat;
        public Type type;

        public Rune(Stat.Data stat, Type type) {
            this.stat = stat;
            this.type = type;
        }

        public enum Type
        {
            Sm, Pa, Ra
        }
    }
}