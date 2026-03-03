using System;
using System.Threading;
using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Events;

namespace Inkybot.Services
{
    internal abstract class MageExecutor
    {
        protected readonly MageSession session;
        protected readonly ScreenReaderDataProvider dataProvider;
        protected readonly DofusMagingAI magus;
        protected readonly ActionHandler actions;
        protected readonly ActionFactory actionFactory;
        protected readonly ConfigManager configManager;
        protected readonly MageQueueManager mageQueue;

        public event EventHandler SuccessfulCombineTick;
        public event EventHandler<MagingJobStartedEventArgs> Started;
        public event EventHandler<MagingJobStartedEventArgs> SensitiveMage;
        public event EventHandler<MagingJobErrorEventArgs> Error;
        public event EventHandler<MagingJobErrorEventArgs> Warning;
        public Action<decimal> SetSink;

        protected MageExecutor(
            MageSession session,
            ScreenReaderDataProvider dataProvider,
            DofusMagingAI magus,
            ActionHandler actions,
            ActionFactory actionFactory,
            ConfigManager configManager,
            MageQueueManager mageQueue) {
            this.session = session;
            this.dataProvider = dataProvider;
            this.magus = magus;
            this.actions = actions;
            this.actionFactory = actionFactory;
            this.configManager = configManager;
            this.mageQueue = mageQueue;
        }

        public abstract bool Execute(bool restarting);

        protected bool MageSingleItem(bool runStartedEvent, bool restarting) {
            var result = MageRetryHandler.ExecuteWithRetry(
                attempt => RunMageLoop(runStartedEvent || attempt > 0, attempt > 0 || restarting),
                session, dataProvider,
                onError: e => Error?.Invoke(this, e),
                onWarning: e => Warning?.Invoke(this, e),
                restarting);

            session.IsMaging = true;
            if (mageQueue.Empty || result.StopMage || result.AutoShutdown || !(session.PreviousAction is Finish))
                session.IsMaging = false;

            result.AutoShutdown |= session.PreviousAction is Finish;
            return result.AutoShutdown;
        }

        protected virtual void BeforePrepare() { }
        protected virtual void AfterPrepare(Item item) { }
        protected virtual void OnItemFinished() { }

        private bool RunMageLoop(bool runStartedEvent, bool restarting) {
            BeforePrepare();

            var item = MagePreparation.Prepare(
                session, restarting,
                dataProvider, configManager,
                setSink: v => SetSink?.Invoke(v));

            AfterPrepare(item);

            if (session.IsMaging && runStartedEvent)
                Started?.Invoke(this, new MagingJobStartedEventArgs(restarting, item, configManager.Config!, false));

            actions.Execute(actionFactory.InventorySelectResourcesAction());
            Thread.Sleep(30);
            actions.Execute(actionFactory.InventoryClearSelectionAction());

            while (session.IsMaging) {
                session.Ticks++;
                var tick = new Tick(session, dataProvider, magus, actions, configManager);
                tick.SetSink = v => SetSink?.Invoke(v);
                tick.SuccessfulCombine += (s, e) => SuccessfulCombineTick?.Invoke(s, e);
                tick.SensitiveMage += (s, e) => SensitiveMage?.Invoke(s, e);
                tick.Execute();
            }

            var finishedNormally = session.PreviousAction is Finish;
            session.IsMaging = true;
            if (finishedNormally)
                OnItemFinished();

            return finishedNormally;
        }
    }
}
