using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using WindowsFormsApp.Actions;
using WindowsFormsApp.Contracts;
using WindowsFormsApp.Events;
using WindowsFormsApp.Exceptions;
using WindowsFormsApp.Services;

namespace WindowsFormsApp
{
    public class DofusMagingJob
    {
        public event EventHandler Started;
        public event EventHandler Stopped;
        public event EventHandler RuneSelected;
        public event StatsEventHandler StatsCollected;

        public DofusDataProvider dataProvider;
        public Thread job;
        private bool shouldContinueMaging;
        private DofusMagingAI magus;
        private Config config;
        private IItemHistoryAnalyzer history;
        private ActionHandler actions;

        public DofusMagingJob() {
            magus = (DofusMagingAI) Program.Services.GetService(typeof(DofusMagingAI));
            actions = (ActionHandler) Program.Services.GetService(typeof(ActionHandler));
            history = (IItemHistoryAnalyzer) Program.Services.GetService(typeof(IItemHistoryAnalyzer));
            config = new Config();
            previousHistory = new ItemHistoryAnalysis(new MageHistoryRecord[] {}, history);
        }
        
        public void BeginMage(bool begin) {
            if (!begin) {
                StopMage();
                return;
            }
            dataProvider = (DofusDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            
            shouldContinueMaging = true;
            sinkHasInit = false;
            magus.SetConfig(config);
            
            job = new Thread(DoMage);
            job.Start();
            Started?.Invoke(this, null);
        }

        public void StopMage() {
            shouldContinueMaging = false;
            Stopped?.Invoke(this, null);
        }

        private ItemHistoryAnalysis previousHistory;
        private float sink;
        private bool sinkHasInit;

        private IAction DoAction() {
            var itemStats = dataProvider.Stats();
            var action = magus.ResolveAction(itemStats);
            actions.Execute(action);
            return action;
        }

        public async void DoMage() {
            try {
                // Do initial actions until ready for main loop
                IAction setupAction;
                do {
                    dataProvider.FetchData();
                    setupAction = DoAction();
                    Thread.Sleep(300);
                } while (!(setupAction is Combine));

                Thread.Sleep(500);


                bool hasCombined = false;

                do {
                    dataProvider.FetchData();
                    var itemHistory = history.Analyse(dataProvider.History());

                    if (!sinkHasInit) {
                        sink = 0;
                        sinkHasInit = true;
                    }


                    if (hasCombined) {
                        var historyHasChanged = itemHistory.IsDifferentFrom(previousHistory);
                        Debug.WriteLine("History has changed: " + historyHasChanged);
                        if (!historyHasChanged) {
                            Thread.Sleep(50);
                            continue;
                        }

                        sink += itemHistory.history.Last().ChangeInSink;
                        sink = Math.Max(0f, sink);

                        Debug.WriteLine("sink: " + sink);
                    }

                    var itemStats = dataProvider.Stats();
                    StatsCollected?.Invoke(this, new StatsEventArgs(itemStats));

                    if (itemStats.Length > 0) {
                        var action = magus.ResolveAction(itemStats);
                        actions.Execute(action);
                        hasCombined = action is Combine;
                    }

                    previousHistory = itemHistory;

                    if (!hasCombined) {
                        Thread.Sleep(300);
                    }
                } while (shouldContinueMaging);
            }
            catch (Exception e) {
                StopMage();
                Debug.WriteLine("EXCEPTION: "+e.Message);
            } 
        }
    }
}