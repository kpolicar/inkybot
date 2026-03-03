using System;
using System.Threading;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Events;

namespace Inkybot.Services
{
    internal abstract class MageExecutor : HasDependencies
    {
        protected MageSession session;
        protected ScreenReaderDataProvider dataProvider;
        protected DofusMagingAI magus;
        protected ActionHandler actions;
        protected ActionFactory actionFactory;
        protected ConfigManager configManager;
        protected MageQueueManager mageQueue;

        public event EventHandler SuccessfulCombineTick;
        public event EventHandler<MagingJobStartedEventArgs> ItemPrepared;
        public event EventHandler<MagingJobStartedEventArgs> SensitiveMage;
        public event EventHandler<SinkChangedEventArgs> SinkChanged;
        public event EventHandler<MagingJobErrorEventArgs> Error;
        public event EventHandler<MagingJobErrorEventArgs> Warning;

        public virtual void BindDependencies(ServiceContainer serviceContainer) {
            actions = serviceContainer.GetService<ActionHandler>();
            actionFactory = serviceContainer.GetService<ActionFactory>();
            configManager = (ConfigManager) serviceContainer.GetService<MageConfigManager>();
            dataProvider = (ScreenReaderDataProvider) serviceContainer.GetService<DofusDataProvider>();
            mageQueue = serviceContainer.GetService<MageQueueManager>();
        }

        public void Init(MageSession session, DofusMagingAI magus) {
            this.session = session;
            this.magus = magus;
        }

        public abstract bool Execute();

        protected MageRetryHandler.Result MageSingleItem() {
            return MageRetryHandler.ExecuteWithRetry(
                RunMageLoop,
                session, dataProvider,
                onError: e => Error?.Invoke(this, e),
                onWarning: e => Warning?.Invoke(this, e));
        }

        protected virtual void BeforePrepare() { }
        protected virtual void AfterPrepare(Item item) { }
        protected virtual void OnItemFinished() { }

        private bool RunMageLoop() {
            BeforePrepare();

            var item = MagePreparation.Prepare(session, dataProvider, configManager);

            AfterPrepare(item);

            var sink = dataProvider.Sink() ?? 0;
            RaiseSinkChanged(sink);

            ItemPrepared?.Invoke(this, new MagingJobStartedEventArgs(
                session.IsRestarting, item, configManager.Config!, false));

            actions.Execute(actionFactory.InventorySelectResourcesAction());
            Thread.Sleep(30);
            actions.Execute(actionFactory.InventoryClearSelectionAction());

            var finished = RunTickLoop();

            if (finished)
                OnItemFinished();

            return finished;
        }

        private bool RunTickLoop() {
            while (session.IsMaging) {
                session.Ticks++;
                var tick = new Tick(session, dataProvider, magus, actions, configManager);
                BindTickEvents(tick);

                if (tick.Execute() == TickResult.Finished)
                    return true;
            }

            return false;
        }

        private void BindTickEvents(Tick tick) {
            tick.SinkRead += (s, e) => RaiseSinkChanged(e);
            tick.SuccessfulCombine += (s, e) => SuccessfulCombineTick?.Invoke(s, e);
            tick.SensitiveMage += (s, e) => SensitiveMage?.Invoke(s, e);
        }

        private void RaiseSinkChanged(decimal newSink) {
            SinkChanged?.Invoke(this, new SinkChangedEventArgs(
                session.PreviousItem!, configManager.Config!, session.Sink, newSink));
            session.Sink = newSink;
        }
    }
}
