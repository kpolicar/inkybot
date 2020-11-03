using System;
using System.Diagnostics;
using System.Threading;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Services;
using Timer = System.Windows.Forms.Timer;

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
        internal Timer historyCheckTimeout;

        public DofusMagingJob() {
            magus = (DofusMagingAI) Program.Services.GetService(typeof(DofusMagingAI));
            history = (IItemHistoryAnalyzer) Program.Services.GetService(typeof(IItemHistoryAnalyzer));
            previousHistory = new ItemHistoryAnalysis(new MageHistoryRecord[] { }, history);
            configManager = (ConfigManager) Program.Services.GetService(typeof(ConfigManager));
            configManager.ConfigChanged += OnConfigChanged;
            
            historyCheckTimeout = new Timer();
            historyCheckTimeout.Interval = 3000;
            historyCheckTimeout.Tick += HandleHistoryCheckTimeout;
            historyCheckTimeout.Enabled = false;
        }

        private void HandleHistoryCheckTimeout(object sender, EventArgs e) {
            Debug.WriteLine("ops");
            throw new HistoryChangeCheckTimeoutException(
                "Mage history was expected to change within 3 seconds, but did not. " +
                "This may be the result of a poor internet connection");
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
            Sink = 0f;
            previousAction = null;
            previousHistory = null;
            
            dataProvider.FetchData();
            var stats = dataProvider.Stats();
            configManager.EnforceConfigSetForStats(stats);
            IsMaging = true;
        }

        private void DoMage() {
            try {
                PrepareMage();
                while (IsMaging) new DofusMagingJobTick(this).Execute();
            } catch (Exception exception) {
                Error?.Invoke(this, new MagingJobErrorEventArgs(exception));
                Debug.WriteLine(exception.StackTrace);
            } finally {
                StopMage();
            }
        }
        
        public void OnConfigChanged(object sender, ConfigChangedEventArgs eventArgs) {
            StopMage();
        }
    }
}
