using System;
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

        public DofusCommandIssuer commandIssuer;
        public DofusDataProvider dataProvider;
        public Thread magus;
        private bool shouldContinueMaging;
        
        public void BeginMage(bool begin) {
            if (!begin) {
                StopMage();
                return;
            }
            commandIssuer = (DofusCommandIssuer) Program.Services.GetService(typeof(DofusCommandIssuer));
            dataProvider = (DofusDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            
            shouldContinueMaging = true;
            magus = new Thread(DoMage);
            magus.Start();
            Started?.Invoke(this, null);
        }

        public void StopMage() {
            shouldContinueMaging = false;
            Stopped?.Invoke(this, null);
        }

        public async void DoMage() {
            int row = 0;
            int column = 0;
            
            while (shouldContinueMaging) {
                var itemStats = await dataProvider.Stats();
                StatsCollected?.Invoke(this, new StatsEventArgs(itemStats));

                commandIssuer.SelectRune(row, column);

                if (++column > 2) {
                    column = 0;
                    if (++row > 13) 
                        row = 0;
                }
                
                Thread.Sleep(200);
            }
        }
    }
}