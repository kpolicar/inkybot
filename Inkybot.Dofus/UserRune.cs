namespace Inkybot.Dofus
{
    public class UserRune
    {
        public readonly Rune Rune;
        public int Quantity;

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
