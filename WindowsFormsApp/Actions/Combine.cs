namespace WindowsFormsApp.Actions
{
    public class Combine : MouseAction
    {
        public Item.ItemStat target;

        public Combine(Item.ItemStat target) {
            this.target = target;
        }
        
        public override void Execute() {
            command.Combine();
        }
    }
}