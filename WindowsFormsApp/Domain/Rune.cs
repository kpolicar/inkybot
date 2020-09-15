using System.Collections.Generic;

namespace WindowsFormsApp
{
    public class Rune
    {
        public Stat stat;
        public Type type;

        public Rune(Stat stat, Type type) {
            this.stat = stat;
            this.type = type;
        }

        public enum Type
        {
            Sm, Pa, Ra
        }
    }
}