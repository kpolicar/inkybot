namespace WindowsFormsApp.Actions
{
    public class Combine : MouseAction
    {
        public Rune target;

        public Combine(Rune target) {
            this.target = target;
        }
        
        public override void Execute() {
            mouse.Click(1015, 225);
        }
    }
}