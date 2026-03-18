using System.Threading;
using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Exceptions;

namespace Inkybot.Services
{
    internal class QueueMageExecutor : MageExecutor
    {
        public override bool Execute() {
            var autoShutdown = false;
            session.IsMaging = true;

            while (!mageQueue.Empty && session.IsMaging) {
                PrepareInventoryForNextItem();
                if (!session.IsMaging) break;

                var result = MageSingleItem();
                autoShutdown = result.AutoShutdown || session.PreviousAction is Finish;

                var shouldContinue = !result.StopMage
                    && session.PreviousAction is Finish
                    && !mageQueue.Empty;

                session.IsMaging = shouldContinue;

                if (!mageQueue.Empty && shouldContinue) Thread.Sleep(500);
            }

            return autoShutdown;
        }

        protected override void BeforePrepare() {
            if (!session.IsRestarting) {
                actions.Execute(actionFactory.SelectItemFromQueue());
                Thread.Sleep(1000);
            }
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
