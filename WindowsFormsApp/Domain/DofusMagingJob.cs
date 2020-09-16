using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Windows.Devices.Input;
using WindowsFormsApp.Contracts;
using WindowsFormsApp.Events;
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
        private List<IAction> history;
        private Config config;

        public DofusMagingJob() {
            magus = (DofusMagingAI) Program.Services.GetService(typeof(DofusMagingAI));
            config = new Config();
        }
        
        public void BeginMage(bool begin) {
            if (!begin) {
                StopMage();
                return;
            }
            dataProvider = (DofusDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            
            shouldContinueMaging = true;
            magus.SetHistory(history = new List<IAction>());
            magus.SetConfig(config);
            
            job = new Thread(DoMage);
            job.Start();
            Started?.Invoke(this, null);
        }

        public void StopMage() {
            shouldContinueMaging = false;
            Stopped?.Invoke(this, null);
        }

        public async void DoMage() {
            while (shouldContinueMaging) {
                dataProvider.FetchData();
                
                var itemHistory = dataProvider.History();

                foreach (var record in itemHistory) {
                    try {
                        Debug.WriteLine(record.ChangeInSink);

                    }
                    catch (Exception e) {
                        Debug.WriteLine(e.Message);
                        Debug.WriteLine(record.landed);
                    }
                }

                
                Thread.Sleep(2000);
                return;

                var itemStats = dataProvider.Stats();
                StatsCollected?.Invoke(this, new StatsEventArgs(itemStats));

                if (itemStats.Length > 0) {
                    var action = magus.ResolveAction(itemStats);
                    action.Execute();
                    history.Add(action);
                }
                Debug.WriteLine("History count: "+history.Count);

                Thread.Sleep(2000);
            }
        }
    }
}