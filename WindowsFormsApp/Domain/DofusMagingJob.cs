using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Windows.Devices.Input;
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

        public async void DoMage() {
            Debug.WriteLine("started maging!");
            bool hasCombined = false;
            
            while (shouldContinueMaging) {
                dataProvider.FetchData();
                
                var itemHistory = history.Analyse(dataProvider.History());

                if (hasCombined) {
                    var historyHasChanged = itemHistory.IsDifferentFrom(previousHistory);
                    if (!historyHasChanged) {
                        Thread.Sleep(50);
                        continue;
                    }
                }
                
                try {
                    var a = itemHistory.CalculateSink();
                    Debug.WriteLine("sink: "+a);
                }
                catch (CouldNotResolveSinkException e) {
                    Debug.WriteLine("could not resolve sink exception!");
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
            }
        }
    }
}