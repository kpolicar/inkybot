using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Domain;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Services;
using DofusMagingJobContract = Inkybot.Contracts.DofusMagingJob;

namespace Inkybot.Services
{
    public partial class ScreenReaderDofusMagingJob : DofusMagingJobContract, InjectableService
    {
        public event EventHandler<MagingJobEventArgs> Started;
        public event EventHandler Stopped;
        public event EventHandler Preparing;
        public event EventHandler<MagingJobFinishedEventArgs> Finished;
        public event EventHandler<SinkChangedEventArgs> SinkChanged;
        public event EventHandler<BalanceChangedEventArgs> BalanceChanged;
        public event EventHandler<RuneQuantityChangedEventArgs> RuneQuantityChanged;
        public event EventHandler<MagingJobErrorEventArgs> Error;
        public event EventHandler<MagingJobErrorEventArgs> Warning;

        internal ActionHandler actions;
        internal ActionFactory actionFactory;

        public Config Config;

        internal ScreenReaderDataProvider dataProvider;
        internal IItemHistoryAnalyzer history;

        public Thread job;
        internal DofusMagingAI magus;
        internal IAction previousAction;
        internal ItemHistoryAnalysis? previousHistory;
        private Item? previousItem;
        private float sink;
        private int balance;
        internal State state;
        private ConfigManager configManager;
        internal Stopwatch changeTimeout;
        internal CurrentItemInfo itemInfo;


        public ScreenReaderDofusMagingJob() {
            previousHistory = new ItemHistoryAnalysis(new MageHistoryRecord[] { }, history);
            changeTimeout = new Stopwatch();
        }
        
        public void BindDependencies() {
            actions = Program.Services.GetService<ActionHandler>();
            actionFactory = Program.Services.GetService<ActionFactory>();
            history = Program.Services.GetService<IItemHistoryAnalyzer>();
            configManager = Program.Services.GetService<ConfigManager>();
            dataProvider = (ScreenReaderDataProvider) Program.Services.GetService<DofusDataProvider>();
            configManager.ConfigModified += OnConfigModified;
        }

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
                SinkChanged?.Invoke(this, new SinkChangedEventArgs(previousItem, sink, value));
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
            magus = (DofusMagingAI) Program.Services.GetService(typeof(DofusMagingAI));

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
            
            dataProvider.FetchData();
            var item = dataProvider.Item();
            itemInfo = new CurrentItemInfo {
                Runes = dataProvider.Runes()
            };
            
            IsMaging = true;
            Started?.Invoke(this, new MagingJobEventArgs(item));
        }

        private void DoMage() {
            try {
                PrepareMage();
                actions.Execute(actionFactory.InventorySelectResourcesAction());
                
                while (IsMaging) new Tick(this).Execute();
            } catch (OperationCanceledException) {
                Debug.WriteLine("operation cancelled!");
            } catch (AggregateException agg_ex) {
                //just get first exception, it will contain the most relevant error.
                var ex = agg_ex.InnerExceptions[0];
                Debug.WriteLine("/---aggregate");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                Debug.WriteLine("---/");
            }
            catch (Exception exception) {

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
            
            Finished?.Invoke(this, new MagingJobFinishedEventArgs(previousItem));
        }
        
        public void OnConfigModified(object sender, ConfigModifiedEventArgs e) {
            if (e.Changed)
                StopMage();
        }
    }
}
