using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Services;
using DofusMagingJobContract = Inkybot.Contracts.DofusMagingJob;

namespace Inkybot.Domain
{
    public class DofusMagingJob : DofusMagingJobContract
    {
        public event EventHandler Started;
        public event EventHandler Stopped;
        public event EventHandler<MagingJobFinishedEventArgs> Finished;
        public event EventHandler<SinkChangedEventArgs> SinkChanged;
        public event EventHandler<MagingJobErrorEventArgs> Error;

        internal ActionHandler actions;

        public Config Config;

        internal DofusDataProvider dataProvider;
        internal IItemHistoryAnalyzer history;

        public Thread job;
        internal DofusMagingAI magus;
        internal IAction previousAction;
        internal ItemHistoryAnalysis? previousHistory;
        private float sink;
        internal DofusMagingJobState state;
        private ConfigManager configManager;
        internal Stopwatch historyCheckTimeout;
        private DofusMagingJobItemInfo itemInfo;
        

        public DofusMagingJob() {
            actions = (ActionHandler) Program.Services.GetService(typeof(ActionHandler));
            history = (IItemHistoryAnalyzer) Program.Services.GetService(typeof(IItemHistoryAnalyzer));
            previousHistory = new ItemHistoryAnalysis(new MageHistoryRecord[] { }, history);
            configManager = (ConfigManager) Program.Services.GetService(typeof(ConfigManager));
            configManager.ConfigModified += OnConfigModified;
            
            historyCheckTimeout = new Stopwatch();
        }

        public bool IsMaging { get; private set; }

        internal float Sink {
            get => sink;
            set {
                SinkChanged?.Invoke(this, new SinkChangedEventArgs(sink, value));
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
            dataProvider = (DofusDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            magus = (DofusMagingAI) Program.Services.GetService(typeof(DofusMagingAI));

            job = new Thread(DoMage);
            job.Start();
            Started?.Invoke(this, EventArgs.Empty);
        }

        public void StopMage() {
            if (!IsMaging) return;
            
            IsMaging = false;
            Stopped?.Invoke(this, EventArgs.Empty);
            historyCheckTimeout.Reset();
        }

        private void PrepareMage() {
            state = DofusMagingJobState.STANDARD;
            Sink = 0f;
            previousAction = null;
            previousHistory = null;
            
            dataProvider.FetchData();
            dataProvider.Item();
            itemInfo = new DofusMagingJobItemInfo {
                Runes = dataProvider.Runes()
            };
            IsMaging = true;
        }

        private void DoMage() {
            try {
                PrepareMage();
                while (IsMaging) new DofusMagingJobTick(this).Execute();
            } catch (Exception exception) {
                
                var additionalInfo = !Helpers.System.IsRunnningAsAdmin() ?
                    "Please try running Inkybot as an administrator." : "";
                
                Error?.Invoke(this, new MagingJobErrorEventArgs(exception, additionalInfo));
                Debug.WriteLine(exception.StackTrace);
            } finally {
                StopMage();
            }
            
            Finished?.Invoke(this, new MagingJobFinishedEventArgs());
        }
        
        public void OnConfigModified(object sender, ConfigModifiedEventArgs e) {
            if (e.Changed)
                StopMage();
        }
    }
}
