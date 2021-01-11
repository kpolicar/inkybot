using System;
using System.Diagnostics;
using System.Threading;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Domain;
using Inkybot.Events;
using Inkybot.Exceptions;
using Debug = System.Diagnostics.Debug;
using DofusMagingJobContract = Inkybot.Contracts.DofusMagingJob;
using DofusMagingAIContract = Inkybot.Contracts.DofusMagingAI;
using IAction = Inkybot.Domain.IAction;

namespace Inkybot.Services
{
    public partial class ScreenReaderDofusMagingJob : DofusMagingJobContract, IDisposable, HasDependencies
    {
        public event EventHandler<MagingJobEventArgs>? Started;
        public event EventHandler? Stopped;
        public event EventHandler? Preparing;
        public event EventHandler<MagingJobFinishedEventArgs>? Finished;
        public event EventHandler<SinkChangedEventArgs>? SinkChanged;
        public event EventHandler<BalanceChangedEventArgs>? BalanceChanged;
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

        private Thread? job;
        private IAction? previousAction;
        private ItemHistoryAnalysis? previousHistory;
        private Item? previousItem;
        private float sink;
        private int balance;
        private State state;
        private ItemInfo itemInfo;
        private Stopwatch changeTimeout;
        private bool previousCheckHadRunOutOfRunes = false;
        private bool previousActionWasExo = false;


        public ScreenReaderDofusMagingJob() {
            previousHistory = new ItemHistoryAnalysis(new MageHistoryRecord[] { }, history);
            changeTimeout = new Stopwatch();
        }
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            actions = serviceContainer.GetService<ActionHandler>();
            actionFactory = serviceContainer.GetService<ActionFactory>();
            configManager = serviceContainer.GetService<ConfigManager>();
            dataProvider = (ScreenReaderDataProvider) serviceContainer.GetService<DofusDataProvider>();
            configManager.ConfigModified += OnConfigModified;
            this.serviceContainer = serviceContainer;
        }

        public bool IsPreparing;
        public bool IsMaging { get; private set; }

        internal int Balance {
            get => balance;
            set {
                if (balance != 0)
                    BalanceChanged?.Invoke(this, new BalanceChangedEventArgs(balance, value));
                balance = value;
            }
        }
        internal float Sink {
            get => sink;
            set {
                SinkChanged?.Invoke(this, new SinkChangedEventArgs(previousItem!, configManager.Config!, sink, value));
                sink = value;
            }
        }

        public void BeginMage(bool begin) {
            if (begin)
                BeginMage();
            else
                StopMage();
        }

        public void BeginMage() {
            if (IsMaging) return;
            magus = serviceContainer.GetService<DofusMagingAIContract>();

            previousCheckHadRunOutOfRunes = false;
            job = new Thread(DoMage);
            job.Start();
            Preparing?.Invoke(this, EventArgs.Empty);
        }

        public void StopMage() {
            if (!IsMaging) return;
            
            IsMaging = false;
            Stopped?.Invoke(this, EventArgs.Empty);
            changeTimeout.Reset();
        }

        private void PrepareMage() {
            state = State.STANDARD;
            Sink = 0f;
            balance = 0;
            previousAction = null;
            previousHistory = null;
            previousItem = null;
            IsPreparing = true;

            try {
                IsMaging = true;
                dataProvider.Reset();
                dataProvider.FetchData();
                var item = dataProvider.Item();
                configManager.EnforceConfigSetForItem(item);
                configManager.RemoveFallenUnconfiguredStats(item);
                itemInfo = new ItemInfo {
                    Runes = dataProvider.Runes()
                };
            
                if (IsMaging)
                    Started?.Invoke(this, new MagingJobEventArgs(item, configManager.Config!));
            } catch (Exception) {
                IsMaging = false;
                throw;
            }
            IsPreparing = false;
        }

        private void DoMage() {
            try {
                PrepareMage();
                actions.Execute(actionFactory.InventorySelectResourcesAction());
                Thread.Sleep(30);
                actions.Execute(actionFactory.InventoryClearSelectionAction());

                while (IsMaging) new Tick(this).Execute();
                // } catch (OutOfRunesException exception) {
                // Error?.Invoke(this, new MagingJobErrorEventArgs(exception));
            } catch (ItemHasChangedException) {
                dataProvider.Scan!.Save();
                Debug.WriteLine("item has changed!");
            } catch (ExoAfterExoAttemptException ex) {
                Error?.Invoke(this, new MagingJobErrorEventArgs(ex, "Stopping bot to prevent possibly ruining item."));
                dataProvider.Scan!.Save();
                Debug.WriteLine("operation cancelled!");
            } catch (OperationCanceledException) {
                Debug.WriteLine("operation cancelled!");
            } catch (Exception exception) {

                var additionalInfo = !Helpers.System.IsRunnningAsAdmin()
                    ? "Please try running Inkybot as an administrator."
                    : "";

                Error?.Invoke(this, new MagingJobErrorEventArgs(exception, additionalInfo));
                Debug.WriteLine(exception.Message);
                Debug.WriteLine(exception.StackTrace);

                if (Properties.Settings.Default.autoRestartBot) {
                    Thread.Sleep(1000);
                    if (IsMaging) {
                        DoMage();
                        return;
                    }
                }
            }

            IsMaging = true; // If an error occured during preparation, we still want to stop properly
            StopMage();
            
            Finished?.Invoke(this, new MagingJobFinishedEventArgs(previousItem!, configManager.Config!));
        }
        
        public void OnConfigModified(object sender, ConfigModifiedEventArgs e) {
            if (e.Changed && !IsPreparing)
                StopMage();
        }

        public void Dispose() {
            StopMage();
        }
    }
}
