using Inkybot.Dofus.Contracts;

namespace Inkybot.Services
{
    internal class SingleItemMageExecutor : MageExecutor
    {
        public SingleItemMageExecutor(
            MageSession session,
            ScreenReaderDataProvider dataProvider,
            DofusMagingAI magus,
            ActionHandler actions,
            ActionFactory actionFactory,
            ConfigManager configManager,
            MageQueueManager mageQueue)
            : base(session, dataProvider, magus, actions, actionFactory, configManager, mageQueue) { }

        public override bool Execute(bool restarting) {
            return MageSingleItem(runStartedEvent: true, restarting: restarting);
        }
    }
}
