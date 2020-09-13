namespace WindowsFormsApp.Actions
{
    public class Finish : MouseAction
    {
        public override void Execute() {
            command.RemoveItem();
            var magus = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            magus.StopMage();
        }
    }
}