using Inkybot.Actions;

namespace Inkybot.Services
{
    internal class SingleItemMageExecutor : MageExecutor
    {
        public override bool Execute() {
            var result = MageSingleItem();
            session.IsMaging = false;
            return result.AutoShutdown || session.PreviousAction is Finish;
        }
    }
}
