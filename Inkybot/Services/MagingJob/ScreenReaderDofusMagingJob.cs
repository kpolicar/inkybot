using System;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus.Contracts;
using Inkybot.Events;
using Inkybot.Exceptions;
using DofusMagingJobContract = Inkybot.Contracts.DofusMagingJob;
using DofusMagingAIContract = Inkybot.Dofus.Contracts.DofusMagingAI;

namespace Inkybot.Services
{
    public partial class ScreenReaderDofusMagingJob : DofusMagingJobContract, IDisposable, HasDependencies
    {
        public event EventHandler<MagingJobStartedEventArgs>? Started;
        public event EventHandler<MagingJobStartedEventArgs>? SensitiveMage;
        public event EventHandler? Starting;
        public event EventHandler? SuccessfulCombineTick;
        public event EventHandler? Stopped;
        public event EventHandler? Preparing;
        public event EventHandler<MagingJobFinishedEventArgs>? Finished;
        public event EventHandler<SinkChangedEventArgs>? SinkChanged;
        public event EventHandler<BalanceChangedEventArgs>? BalanceChanged;
        public event EventHandler<BalanceChangedEventArgs>? BalanceSpent;
        public event EventHandler<RuneQuantityChangedEventArgs>? RuneQuantityChanged;
        public event EventHandler<MagingJobErrorEventArgs>? Error;
        public event EventHandler<MagingJobErrorEventArgs>? Warning;

        private IMagingDataProvider dataProvider = null!;
        private IMagingConfigManager configManager = null!;
        private DofusMagingAIContract magus = null!;
        private ServiceContainer serviceContainer = null!;
        private MageQueueManager mageQueue = null!;

        private readonly ManualResetEventSlim mageRequested = new ManualResetEventSlim(false);
        private Thread? workerThread;
        private MageSession session = new MageSession();
        private BalanceTracker balanceTracker;

        public void BindDependencies(ServiceContainer serviceContainer) {
            mageQueue = serviceContainer.GetService<MageQueueManager>();
            configManager = (IMagingConfigManager) serviceContainer.GetService<MageConfigManager>();
            var magingAiManager = serviceContainer.GetService<MagingAIServiceManager>();
            dataProvider = (IMagingDataProvider) serviceContainer.GetService<DofusDataProvider>();
            this.serviceContainer = serviceContainer;

            configManager.ConfigModified += OnConfigModified;
            if (magingAiManager != null)
                magingAiManager.MagingAIChanged += OnMagingAiChanged;
            mageQueue.Enqueued += OnMageQueueChanged;
            mageQueue.Removed += OnMageQueueChanged;
            mageQueue.Moved += OnMageQueueMoved;

            workerThread = new Thread(WorkerLoop) { IsBackground = true };
            workerThread.Start();
        }

        public int Sink => (int) session.Sink;

        public bool IsMaging => session.IsMaging;

        #region Public API

        public void BeginMage(bool begin) {
            if (begin) BeginMage();
            else StopMage();
        }

        public void BeginMage() {
            if (session.IsMaging) return;

            try {
                magus = serviceContainer.GetService<DofusMagingAIContract>();
                Starting?.Invoke(this, EventArgs.Empty);

                session = new MageSession();
                balanceTracker = new BalanceTracker(session);
                balanceTracker.BalanceChanged += (s, e) => BalanceChanged?.Invoke(this, e);
                balanceTracker.BalanceSpent += (s, e) => BalanceSpent?.Invoke(this, e);
                mageRequested.Set();
                Preparing?.Invoke(this, EventArgs.Empty);
            } catch (Exception exception) {
                Error?.Invoke(this, new MagingJobErrorEventArgs(exception));
            }
        }

        public void ResetBalance() {
            balanceTracker?.Reset();
        }

        public void StopMage() {
            if (!session.IsMaging) return;

            session.ManuallyStopped = true;
            session.IsMaging = false;
            session.ChangeTimeout.Reset();
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Worker Thread

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        private void WorkerLoop() {
            while (true) {
                mageRequested.Wait();
                mageRequested.Reset();

                DoMage();
            }
        }

        private void DoMage() {
            var executor = mageQueue.Empty
                ? (MageExecutor) new SingleItemMageExecutor()
                : new QueueMageExecutor();

            executor.BindDependencies(serviceContainer);
            executor.Init(session, magus);
            BindExecutorEvents(executor);

            var autoShutdown = executor.Execute();
            StopMage();

            Finished?.Invoke(this,
                new MagingJobFinishedEventArgs(session.PreviousItem!, configManager.Config!, autoShutdown));
        }

        private void BindExecutorEvents(MageExecutor executor) {
            executor.SinkChanged += (s, e) => SinkChanged?.Invoke(this, e);
            executor.SuccessfulCombineTick += (s, e) => SuccessfulCombineTick?.Invoke(this, e);
            executor.SensitiveMage += (s, e) => SensitiveMage?.Invoke(this, e);
            executor.Error += (s, e) => Error?.Invoke(this, e);
            executor.Warning += (s, e) => Warning?.Invoke(this, e);
            executor.ItemPrepared += OnExecutorItemPrepared;
        }

        private void OnExecutorItemPrepared(object sender, MagingJobStartedEventArgs e) {
            if (session.HasStartedFired && e.Restarting) return;
            session.HasStartedFired = true;
            Started?.Invoke(this, e);
        }

        #endregion

        #region Stop Triggers

        private void OnConfigModified(object sender, ConfigModifiedEventArgs e) {
            if (!ShouldRespondToConfigChange()) return;

            var configChangedMidRun = e.Changed && (!session.IsPreparing || session.IsRestarting);
            if (!configChangedMidRun) return;

            if (session.IsRestarting && session.PreviousItem != null)
                throw new ItemHasChangedException(session.PreviousItem);

            StopMage();
        }

        private bool ShouldRespondToConfigChange() {
            if (session.IsRestarting) return true;
            var magingAI = serviceContainer.GetService<DofusMagingAIContract>();
            return magingAI is DofusMagingAI || magingAI is DofusStandardStatsMagingAI;
        }

        private void OnMagingAiChanged(object sender, MagingAIChangedEventArgs e) {
            if (!session.IsPreparing)
                StopMage();
            magus = e.AI;
        }

        private void OnMageQueueMoved(object sender, MageQueueMovedEventArgs e) {
            var currentItemAffected = mageQueue.Peek() == e.QueueItem || e.Index == 0;
            if (currentItemAffected) StopMage();
        }

        private void OnMageQueueChanged(object sender, MageQueueMovedEventArgs e) {
            var frontOfQueueChanged = e.Index == 0;
            if (frontOfQueueChanged) StopMage();
        }

        #endregion

        public void Dispose() => StopMage();
    }
}
