using System;
using System.Diagnostics;
using System.Threading;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Events;
using Inkybot.Services;

namespace Inkybot
{
    public class DofusMagingJob
    {
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

        public DofusMagingJob() {
            magus = (DofusMagingAI) Program.Services.GetService(typeof(DofusMagingAI));
            actions = (ActionHandler) Program.Services.GetService(typeof(ActionHandler));
            history = (IItemHistoryAnalyzer) Program.Services.GetService(typeof(IItemHistoryAnalyzer));
            previousHistory = new ItemHistoryAnalysis(new MageHistoryRecord[] { }, history);
            configManager = (ConfigManager) Program.Services.GetService(typeof(ConfigManager));
            configManager.ConfigChanged += OnConfigChanged;
        }

        public bool IsMaging { get; private set; }

        internal float Sink {
            get => sink;
            set {
                sink = value;
                SinkChanged?.Invoke(this, new SinkChangedEventArgs(sink));
            }
        }

        public event EventHandler Started;
        public event EventHandler Stopped;
        public event EventHandler<SinkChangedEventArgs> SinkChanged;

        public void BeginMage(bool begin) {
            if (begin)
                BeginMage();
            else
                StopMage();
        }

        public void BeginMage() {
            dataProvider = (DofusDataProvider) Program.Services.GetService(typeof(DofusDataProvider));

            job = new Thread(DoMage);
            job.Start();
            Started?.Invoke(this, EventArgs.Empty);
        }

        public void StopMage() {
            IsMaging = false;
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        private void PrepareMage() {
            state = DofusMagingJobState.DOING_FIRST_COMBINE;
            IsMaging = true;
            Sink = 0f;
            previousAction = null;
            previousHistory = null;
            
            dataProvider.FetchData();
            var stats = dataProvider.Stats();
            configManager.EnforceConfigSetForStats(stats);
        }

        private void DoMage() {
            try {
                PrepareMage();
                while (IsMaging) new DofusMagingJobTick(this).Execute();
            } catch (Exception exception) {
                Error?.Invoke(this, new MagingJobErrorEventArgs(exception));
            } finally {
                StopMage();
            }
        }
        
        public void OnConfigChanged(object sender, ConfigChangedEventArgs eventArgs) {
            StopMage();
        }
    }
}
