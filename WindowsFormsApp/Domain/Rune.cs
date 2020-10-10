namespace WindowsFormsApp
{
    public class Rune
    {
        public enum Type
        {
            Sm,
            Pa,
            Ra
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
                if (stat.sinkValue < 1) {
                    var increase = typeValue / stat.sinkValue;
                    return (int) increase;
                }

                return typeValue;
            }
        }

        public float Sink => stat.sinkValue * IncreaseInValue;
    }
}
