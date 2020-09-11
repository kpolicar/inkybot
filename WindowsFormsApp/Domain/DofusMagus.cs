using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Windows.Devices.Input;
using WindowsFormsApp.Events;
using WindowsFormsApp.Services;

namespace WindowsFormsApp
{
    public class DofusMagus
    {
        public event EventHandler Started;
        public event EventHandler Stopped;
        public event EventHandler RuneSelected;
        public event StatsEventHandler StatsCollected;
        
        public DofusCommandIssuer commandIssuer;
        public DofusDataProvider dataProvider;
        public Thread magus;
        private Item item;
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
            while (shouldContinueMaging) {
                var stats = await dataProvider.Stats();
                StatsCollected?.Invoke(this, new StatsEventArgs(stats));
                
                item = new Item(stats);
                commandIssuer.SelectRune(500, 500);
                
                Thread.Sleep(1000);
            }
        }
    }
}