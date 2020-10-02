using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WindowsFormsApp.Actions;
using WindowsFormsApp.Contracts;
using WindowsFormsApp.Events;

namespace WindowsFormsApp
{
    public class DofusMagingJob
    {
        public event EventHandler Started;
        public event EventHandler Stopped;
        public event EventHandler<SinkChangedEventArgs> SinkChanged;

        public Thread job;
        private bool shouldContinueMaging;
        private Config config;

        internal DofusDataProvider dataProvider;
        internal DofusMagingAI magus;
        internal IItemHistoryAnalyzer history;
        internal ActionHandler actions;
        internal ItemHistoryAnalysis previousHistory;
        private float sink; 
        internal float Sink {
            get => sink;
            set {
                sink = value;
                SinkChanged?.Invoke(this, new SinkChangedEventArgs(sink));
            }
        }
        internal IAction previousAction;
        internal DofusMagingJobState state;

        public DofusMagingJob() {
            magus = (DofusMagingAI) Program.Services.GetService(typeof(DofusMagingAI));
            actions = (ActionHandler) Program.Services.GetService(typeof(ActionHandler));
            history = (IItemHistoryAnalyzer) Program.Services.GetService(typeof(IItemHistoryAnalyzer));
            magus.SetConfig(config = new Config());
            previousHistory = new ItemHistoryAnalysis(new MageHistoryRecord[] {}, history);
        }

        public void BeginMage(bool begin)
        {
            if (begin)
                BeginMage();
            else
                StopMage();
        }

        public void BeginMage() {
            dataProvider = (DofusDataProvider) Program.Services.GetService(typeof(DofusDataProvider));

            state = DofusMagingJobState.DOING_FIRST_COMBINE;
            shouldContinueMaging = true;
            Sink = 0f;
            previousAction = null;
            previousHistory = null;
            
            job = new Thread(DoMage);
            job.Start();
            Started?.Invoke(this, EventArgs.Empty);
        }

        public void StopMage() {
            shouldContinueMaging = false;
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        private void DoMage()
        {
            try {
                while (shouldContinueMaging) {
                    new DofusMagingJobTick(this).Execute();
                }
            } catch (Exception e) {
                StopMage();
                Debug.WriteLine("EXCEPTION: "+e.Message);
                Debug.WriteLine(e.StackTrace);
            } 
        }
    }
}