namespace Inkybot.Dofus
{
    /**
     * <summary>
     * The UserRune class represents the user's quantity of a certain rune.
     * The quantity value is modified during the maging process.
     * </summary>
     */
    public class UserRune
    {
        /**
         * <summary>The type of rune that is represented.</summary>
         */
        public readonly Rune Rune;
        
        /**
         * <summary>The number of runes the user has.</summary>
         */
        public int Quantity;

        /**
         * <param name="rune">The type of rune that is represented.</param>
         * <param name="quantity">The number of runes the user has.</param>
         */
        public UserRune(Rune rune, int quantity=0) {
            Rune = rune;
            Quantity = quantity;
        }

        public static bool operator == (UserRune operand1, UserRune operand2) {
            return operand1.Rune == operand2.Rune;
        }
        
        public static bool operator != (UserRune operand1, UserRune operand2) {
            return !(operand1 == operand2);
        }

        public override string ToString() {
            return $"{Rune}: {Quantity}";
        }
    }
}
