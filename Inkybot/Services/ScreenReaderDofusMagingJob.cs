using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Domain;
using Inkybot.Events;
using Inkybot.Exceptions;
using Debug = System.Diagnostics.Debug;
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
        private ItemHistoryAnalyzer history = new ItemHistoryAnalyzer();
        private ConfigManager configManager = null!;
        private DofusMagingAIContract magus = null!;
        private ServiceContainer serviceContainer = null!;
        private MageQueueManager mageQueue = null!;

        private Supervisor? supervisor;
        private Thread? job;
        private State state;
        private ItemInfo itemInfo;
        private Stopwatch changeTimeout = new Stopwatch();
        private int unsuccessfulCombineTicks;
        private int ticks;
        public MageHistoryRecord? LastHistoryRecord => state.PreviousHistory?.history.FirstOrDefault();
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

        private int BalanceSpending;

        private int Balance {
            get => state.Balance;
            set {
                var newBalance = Math.Max(value, 0);
                var previousBalance = state.Balance;
                var newBalanceSpent = Math.Max(0, previousBalance - newBalance);
                    
                state.Balance = newBalance;
                BalanceChanged?.Invoke(this, new BalanceChangedEventArgs(previousBalance, newBalance));

                if (newBalanceSpent <= MaxReasonableBalanceDifference) {
                    var previousBalanceSpent = BalanceSpending;
                    BalanceSpending += newBalanceSpent;
                    BalanceSpent?.Invoke(this, new BalanceChangedEventArgs(previousBalanceSpent, BalanceSpending));
                }
            }
        }
        public decimal Sink {
            get => state.Sink;
            private set {
                SinkChanged?.Invoke(this, 
                    new SinkChangedEventArgs(state.PreviousItem!, configManager.Config!, state.Sink, value));
                state.Sink = value;
            }
        }

        public bool IsMaging => state.IsMaging;

        public void BeginMage(bool begin) {
            if (begin)
                BeginMage();
            else
                StopMage();
        }

        public void BeginMage() {
            if (state.IsMaging) return;

            try {
                
                magus = serviceContainer.GetService<DofusMagingAIContract>();
                Starting?.Invoke(this, EventArgs.Empty);

                ticks = 0;
                unsuccessfulCombineTicks = 0;
                state.PreviousCheckHadRunOutOfRunes = null;
                job = new Thread(() => DoMage());
                job.Start();
                Preparing?.Invoke(this, EventArgs.Empty);
                
            } catch (Exception exception) {
                Error?.Invoke(this, new MagingJobErrorEventArgs(exception));
            }
        }

        public void StopMage() {
            if (!state.IsMaging) return;
            
            state.IsMaging = false;
            Stopped?.Invoke(this, EventArgs.Empty);
            changeTimeout.Reset();
        }

        private Item PrepareMage(bool selectNewFromQueue, bool restarting) {
            var (previousItem, previousSink, previousCheckHadRunOutOfRunes) =
                (state.PreviousItem, state.Sink, state.PreviousCheckHadRunOutOfRunes);
            state.Reset();
            state.IsPreparing = true;
            state.IsRestarting = restarting;
            state.PreviousCheckHadRunOutOfRunes = previousCheckHadRunOutOfRunes;
            supervisor = new Supervisor(this);

            Item item;
            try {
                state.IsMaging = true;
                var resetMinMaxScan = !restarting;
                
                if (!mageQueue.Empty && selectNewFromQueue) {
                    actions.Execute(actionFactory.SelectItemFromQueue());
                    Thread.Sleep(1000);
                }
                
                dataProvider.Reset(resetMinMaxScan);
                dataProvider.FetchData();
                item = dataProvider.Item();
                try {
                    state.PreviousHistory = history.Analyse(dataProvider.History());
                } catch (Exception) {
                    // if we couldn't resolve previous history, no worries.
                }
                
                if (!mageQueue.Empty
                    && selectNewFromQueue
                    && mageQueue.Peek().Config.PresetIndex != null
                    && !configManager.ConfigIsSetForItem(item)) {
                    throw new ItemDoesNotMatchPresetException(item);
                }

                configManager.EnforceConfigSetForItem(item);
                configManager.RemoveFallenUnconfiguredStats(item);
                itemInfo = new ItemInfo {
                    Runes = dataProvider.Runes()
                };
                // Persist item info
                if (previousItem != null && item.Equals(previousItem)) {
                    state.Sink = previousSink;
                    state.PreviousItem = previousItem;
                } else {
                    state.PreviousItem = null;
                    state.Sink = 0;
                }
            } catch (Exception) {
                state.IsMaging = false;
                throw;
            }
            state.IsPreparing = false;
            state.IsRestarting = false;
            
            return item;
        }

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        private void DoMage(bool restarting=false) {
            bool autoShutdown = false;

            if (!mageQueue.Empty) {

                var i = 0;
                while (!mageQueue.Empty) {
                    
                    state.IsMaging = true;
                
                    actions.Execute(actionFactory.RemoveItemFromMagingTable());
                    if (IsMaging) Thread.Sleep(750);
                    actions.Execute(actionFactory.InventorySelectAllAction());
                    if (IsMaging) Thread.Sleep(500);
                    actions.Execute(actionFactory.InventorySelectEquipmentAction());
                    if (IsMaging) Thread.Sleep(500);

                    if (IsMaging) {
                        autoShutdown = DoMageWithoutCheckingQueue(i++ == 0, true, restarting);
                        if (!mageQueue.Empty)
                            Thread.Sleep(500);
                    }
                    
                    
                    if (!IsMaging)
                        break;
                }
                
            } else {
                autoShutdown = DoMageWithoutCheckingQueue(true, false, restarting);
            }
            
            Finished?.Invoke(
                this, 
                new MagingJobFinishedEventArgs(state.PreviousItem!, configManager.Config!, autoShutdown));
        }

        private bool DoMageWithoutCheckingQueue(bool runStartedEvent, bool fromQueue, bool restarting=false) {
            var autoShutdown = false;
            var stopMage = false;
            try {
                var item = PrepareMage(fromQueue, restarting);

                if (IsMaging && runStartedEvent)
                    Started?.Invoke(this, new MagingJobStartedEventArgs(restarting, item, configManager.Config!, false));

                actions.Execute(actionFactory.InventorySelectResourcesAction());
                Thread.Sleep(30);
                actions.Execute(actionFactory.InventoryClearSelectionAction());

                while (IsMaging) {
                    ticks++;
                    new Tick(this).Execute();
                }

                stopMage = !(state.PreviousAction is Finish);
                state.IsMaging = true;
                if (!mageQueue.Empty && state.PreviousAction is Finish)
                    mageQueue.Dequeue();

            } catch (OutOfRunesException exception) {
                autoShutdown = true;
                stopMage = true;
                Error?.Invoke(this, new MagingJobErrorEventArgs(exception));
            } catch (NoItemToMageFoundException exception) {
                autoShutdown = restarting;
                stopMage = true;
                Error?.Invoke(this, new MagingJobErrorEventArgs(exception));
            } catch (UserForbiddenException exception) {
                autoShutdown = restarting;
                stopMage = true;
                Error?.Invoke(this, new MagingJobErrorEventArgs(exception));
            } catch (ItemDoesNotMatchPresetException exception) {
                autoShutdown = true;
                stopMage = true;
                Error?.Invoke(this, new MagingJobErrorEventArgs(exception));
            } catch (ItemHasChangedException exception) {
                autoShutdown = true;
                stopMage = true;
                dataProvider.Scan?.Save();
                Error?.Invoke(this, new MagingJobErrorEventArgs(exception));
            } catch (ItemHasNotChangedException exception) {
                autoShutdown = true;
                stopMage = true;
                dataProvider.Scan?.Save();
                Error?.Invoke(this, new MagingJobErrorEventArgs(exception));
            } catch (OperationCanceledException exception) {
                autoShutdown = false;
                stopMage = true;
                Error?.Invoke(this, new MagingJobErrorEventArgs(exception));
            } catch (Exception exception) {
                autoShutdown = true;
                if (state.Step == State.JobStep.EXECUTING_COMBINE)
                    unsuccessfulCombineTicks++;

                if (exception is AggregateException aggregateException) {
                    Debug.WriteLine("Aggregate exception!");
                    foreach (var aggregateExceptionInnerException in aggregateException.InnerExceptions) {
                        Debug.WriteLine(aggregateExceptionInnerException.Message);
                        Debug.WriteLine(aggregateExceptionInnerException.StackTrace);
                    }
                } else {
                    Debug.WriteLine(exception.Message);
                    Debug.WriteLine(exception.StackTrace);
                }

                var additionalInfo = !Helpers.System.IsRunnningAsAdmin()
                    ? "Please try running Inkybot as an administrator."
                    : "";

                if (Properties.Settings.Default.autoRestartBot) {
                    Warning?.Invoke(this, new MagingJobErrorEventArgs(exception, additionalInfo));
                    Thread.Sleep(1000);

                    if (IsMaging) {
                        return DoMageWithoutCheckingQueue(true, false, true);
                    }

                    if (restarting) {
                        Error?.Invoke(this, new MagingJobErrorEventArgs(exception, additionalInfo));
                    }
                } else {
                    Error?.Invoke(this, new MagingJobErrorEventArgs(exception, additionalInfo));
                }

                stopMage = true;
            }

            state.IsMaging = true; // If an error occured during preparation, we still want to stop properly
            if (mageQueue.Empty || stopMage)
                StopMage();
            autoShutdown |= state.PreviousAction is Inkybot.Actions.Finish;

            return autoShutdown;
        }
        
        private void OnActionExecuted(object sender, ActionExecutedEventArgs e) {
            if (e.action is Finish)
                state.IsMaging = false;
        }

        public void OnConfigModified(object sender, ConfigModifiedEventArgs e) {
            var magingAI = serviceContainer.GetService<DofusMagingAIContract>();
            if (!(magingAI is DofusMagingAI) && !(magingAI is DofusStandardStatsMagingAI) && !state.IsRestarting)
                return;

            if (e.Changed && (!state.IsPreparing || state.IsRestarting)) {
                if (state.IsRestarting && state.PreviousItem != null)
                    throw new ItemHasChangedException(state.PreviousItem);
                StopMage();
            }
        }

        private void OnMagingAiChanged(object sender, MagingAIChangedEventArgs e) {
            if (!state.IsPreparing)
                StopMage();
        }
        
        private void OnMagingMoved(object sender, MageQueueMovedEventArgs e) {
            if (mageQueue.Peek() == e.QueueItem || e.Index == 0)
                StopMage();
        }

        private void OnMagingRemoved(object sender, MageQueueMovedEventArgs e) {
            if (e.Index == 0)
                StopMage();
        }

        private void OnMagingEnqueued(object sender, MageQueueMovedEventArgs e) {
            if (e.Index == 0)
                StopMage();
        }
            

        public void Dispose() =>
            StopMage();
    }
}
