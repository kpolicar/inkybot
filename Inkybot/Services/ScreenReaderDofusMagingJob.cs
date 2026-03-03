using System;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using Inkybot.Actions;
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

        private ActionHandler actions = null!;
        private ActionFactory actionFactory = null!;
        private ScreenReaderDataProvider dataProvider = null!;
        private ConfigManager configManager = null!;
        private DofusMagingAIContract magus = null!;
        private ServiceContainer serviceContainer = null!;
        private MageQueueManager mageQueue = null!;

        private Thread? job;
        private MageSession session = new MageSession();
        private const int MaxReasonableBalanceDifference = 300000;

        public void BindDependencies(ServiceContainer serviceContainer) {
            mageQueue = serviceContainer.GetService<MageQueueManager>();
            actions = serviceContainer.GetService<ActionHandler>();
            actionFactory = serviceContainer.GetService<ActionFactory>();
            configManager = (ConfigManager) serviceContainer.GetService<MageConfigManager>();
            var magingAiManager = serviceContainer.GetService<MagingAIServiceManager>();
            dataProvider = (ScreenReaderDataProvider) serviceContainer.GetService<DofusDataProvider>();
            configManager.ConfigModified += OnConfigModified;
            this.serviceContainer = serviceContainer;
            if (magingAiManager != null)
                magingAiManager.MagingAIChanged += OnMagingAiChanged;
            actions.ActionExecuted += OnActionExecuted;
            mageQueue.Enqueued += OnMagingEnqueued;
            mageQueue.Removed += OnMagingRemoved;
            mageQueue.Moved += OnMagingMoved;
        }

        #region Properties

        private int BalanceSpending;

        private int Balance {
            get => session.Balance;
            set {
                var newBalance = Math.Max(value, 0);
                var previousBalance = session.Balance;
                var newBalanceSpent = Math.Max(0, previousBalance - newBalance);

                session.Balance = newBalance;
                BalanceChanged?.Invoke(this, new BalanceChangedEventArgs(previousBalance, newBalance));

                if (newBalanceSpent <= MaxReasonableBalanceDifference) {
                    var previousBalanceSpent = BalanceSpending;
                    BalanceSpending += newBalanceSpent;
                    BalanceSpent?.Invoke(this, new BalanceChangedEventArgs(previousBalanceSpent, BalanceSpending));
                }
            }
        }

        public int Sink => (int) dSink;
        public decimal dSink {
            get => session.Sink;
            private set {
                SinkChanged?.Invoke(this,
                    new SinkChangedEventArgs(session.PreviousItem!, configManager.Config!, session.Sink, value));
                session.Sink = value;
            }
        }

        public bool IsMaging => session.IsMaging;

        #endregion

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

                session.Ticks = 0;
                session.UnsuccessfulCombineTicks = 0;
                session.PreviousCheckHadRunOutOfRunes = null;
                job = new Thread(() => DoMage());
                job.Start();
                Preparing?.Invoke(this, EventArgs.Empty);
            } catch (Exception exception) {
                Error?.Invoke(this, new MagingJobErrorEventArgs(exception));
            }
        }

        public void ResetBalance() {
            session.Balance = 0;
        }

        public void StopMage() {
            if (!session.IsMaging) return;

            session.IsMaging = false;
            Stopped?.Invoke(this, EventArgs.Empty);
            session.ChangeTimeout.Reset();
        }

        #endregion

        #region Mage Execution

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        private void DoMage(bool restarting = false) {
            var executor = mageQueue.Empty
                ? (MageExecutor) new SingleItemMageExecutor(session, dataProvider, magus, actions, actionFactory, configManager, mageQueue)
                : new QueueMageExecutor(session, dataProvider, magus, actions, actionFactory, configManager, mageQueue);

            executor.SetSink = v => dSink = v;
            executor.Started += (s, e) => Started?.Invoke(this, e);
            executor.SuccessfulCombineTick += (s, e) => SuccessfulCombineTick?.Invoke(this, e);
            executor.SensitiveMage += (s, e) => SensitiveMage?.Invoke(this, e);
            executor.Error += (s, e) => Error?.Invoke(this, e);
            executor.Warning += (s, e) => Warning?.Invoke(this, e);

            var autoShutdown = executor.Execute(restarting);
            StopMage();

            Finished?.Invoke(this,
                new MagingJobFinishedEventArgs(session.PreviousItem!, configManager.Config!, autoShutdown));
        }

        #endregion

        #region Event Handlers

        private void OnActionExecuted(object sender, ActionExecutedEventArgs e) {
            if (e.action is Finish)
                session.IsMaging = false;
        }

        public void OnConfigModified(object sender, ConfigModifiedEventArgs e) {
            var magingAI = serviceContainer.GetService<DofusMagingAIContract>();
            if (!(magingAI is DofusMagingAI) && !(magingAI is DofusStandardStatsMagingAI) && !session.IsRestarting)
                return;

            if (e.Changed && (!session.IsPreparing || session.IsRestarting)) {
                if (session.IsRestarting && session.PreviousItem != null)
                    throw new ItemHasChangedException(session.PreviousItem);
                StopMage();
            }
        }

        private void OnMagingAiChanged(object sender, MagingAIChangedEventArgs e) {
            if (!session.IsPreparing)
                StopMage();
            magus = e.AI;
        }

        private void OnMagingMoved(object sender, MageQueueMovedEventArgs e) {
            if (mageQueue.Peek() == e.QueueItem || e.Index == 0)
                StopMage();
        }

        private void OnMagingRemoved(object sender, MageQueueMovedEventArgs e) {
            if (e.Index == 0) StopMage();
        }

        private void OnMagingEnqueued(object sender, MageQueueMovedEventArgs e) {
            if (e.Index == 0) StopMage();
        }

        #endregion

        public void Dispose() => StopMage();
    }
}
