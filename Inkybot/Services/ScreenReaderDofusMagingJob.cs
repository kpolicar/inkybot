using System;
using System.Diagnostics;
using System.Threading;
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

        private Supervisor? supervisor;
        private Thread? job;
        private State state;
        private ItemInfo itemInfo;
        private Stopwatch changeTimeout = new Stopwatch();


        public void BindDependencies(ServiceContainer serviceContainer) {
            actions = serviceContainer.GetService<ActionHandler>();
            actionFactory = serviceContainer.GetService<ActionFactory>();
            configManager = serviceContainer.GetService<ConfigManager>();
            var magingAiManager = serviceContainer.GetService<MagingAIServiceManager>();
            dataProvider = (ScreenReaderDataProvider) serviceContainer.GetService<DofusDataProvider>();
            configManager.ConfigModified += OnConfigModified;
            this.serviceContainer = serviceContainer;
            magingAiManager.MagingAIChanged += OnMagingAiChanged;
        }

        private int Balance {
            get => state.Balance;
            set {
                if (state.Balance != 0)
                    BalanceChanged?.Invoke(this, new BalanceChangedEventArgs(state.Balance, value));
                state.Balance = value;
            }
        }
        private float Sink {
            get => state.Sink;
            set {
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
            magus = serviceContainer.GetService<DofusMagingAIContract>();

            job = new Thread(DoMage);
            job.Start();
            Preparing?.Invoke(this, EventArgs.Empty);
        }

        public void StopMage() {
            if (!state.IsMaging) return;
            
            state.IsMaging = false;
            Stopped?.Invoke(this, EventArgs.Empty);
            changeTimeout.Reset();
        }

        private void PrepareMage() {
            var (previousItem, previousSink) = (state.PreviousItem, state.Sink);
            state.Reset();
            state.IsPreparing = true;
            supervisor = new Supervisor(this);

            try {
                state.IsMaging = true;
                dataProvider.Reset();
                dataProvider.FetchData();
                var item = dataProvider.Item();
                configManager.EnforceConfigSetForItem(item);
                configManager.RemoveFallenUnconfiguredStats(item);
                itemInfo = new ItemInfo {
                    Runes = dataProvider.Runes()
                };
                // Persist item info
                if (previousItem != null && item.Equals(previousItem)) {
                    state.Sink = previousSink;
                    state.PreviousItem = previousItem;
                }
            
                if (IsMaging)
                    Started?.Invoke(this, new MagingJobEventArgs(item, configManager.Config!));
            } catch (Exception) {
                state.IsMaging = false;
                throw;
            }
            state.IsPreparing = false;
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
                dataProvider.Scan?.Save();
                Debug.WriteLine("item has changed!");
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
            
            state.IsMaging = true; // If an error occured during preparation, we still want to stop properly
            StopMage();
            
            Finished?.Invoke(this, new MagingJobFinishedEventArgs(state.PreviousItem!, configManager.Config!));
        }
        
        public void OnConfigModified(object sender, ConfigModifiedEventArgs e) {
            var magingAI = serviceContainer.GetService<DofusMagingAIContract>();
            if (!(magingAI is DofusMagingAI) && !(magingAI is DofusStandardStatsMagingAI))
                return;
            
            if (e.Changed && !state.IsPreparing)
                StopMage();
        }
        
        private void OnMagingAiChanged(object sender, MagingAIChangedEventArgs e) =>
            StopMage();

        public void Dispose() =>
            StopMage();
    }
}
