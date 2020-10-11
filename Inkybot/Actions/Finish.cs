namespace Inkybot.Actions
{
    public class Finish : MouseAction
    {
        public override void Execute() {
            mouse.DoubleClick(850, 165);
            var magus = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            magus.StopMage();
        }
    }
}
