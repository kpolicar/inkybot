using System.Threading;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Exceptions;

namespace Inkybot.Services
{
    internal class QueueMageExecutor : MageExecutor
    {
        public QueueMageExecutor(
            MageSession session,
            ScreenReaderDataProvider dataProvider,
            DofusMagingAI magus,
            ActionHandler actions,
            ActionFactory actionFactory,
            ConfigManager configManager,
            MageQueueManager mageQueue)
            : base(session, dataProvider, magus, actions, actionFactory, configManager, mageQueue) { }

        public override bool Execute(bool restarting) {
            var autoShutdown = false;
            var first = true;

            while (!mageQueue.Empty && session.IsMaging) {
                session.IsMaging = true;
                PrepareInventoryForNextItem();
                if (!session.IsMaging) break;

                autoShutdown = MageSingleItem(runStartedEvent: first, restarting: restarting);
                first = false;
                restarting = false;

                if (!mageQueue.Empty) Thread.Sleep(500);
            }

            return autoShutdown;
        }

        protected override void BeforePrepare() {
            actions.Execute(actionFactory.SelectItemFromQueue());
            Thread.Sleep(1000);
        }

        protected override void AfterPrepare(Item item) {
            if (mageQueue.Peek().Config.PresetIndex != null
                && !configManager.ConfigIsSetForItem(item)) {
                throw new ItemDoesNotMatchPresetException(item);
            }
        }

        protected override void OnItemFinished() {
            mageQueue.Dequeue();
        }

        private void PrepareInventoryForNextItem() {
            actions.Execute(actionFactory.RemoveItemFromMagingTable());
            if (session.IsMaging) Thread.Sleep(1000);
            actions.Execute(actionFactory.InventorySelectAllAction());
            if (session.IsMaging) Thread.Sleep(500);
            actions.Execute(actionFactory.InventorySelectEquipmentAction());
            if (session.IsMaging) Thread.Sleep(500);
        }
    }
}
