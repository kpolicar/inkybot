using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus.Contracts;
using Inkybot.Events;
using NLog;
using DofusMagingJobContract = Inkybot.Contracts.DofusMagingJob;

namespace Inkybot.Services
{
    internal class KamasTrackingJob : HasDependencies, IDisposable
    {
        private const int ScanDelayMs = 50;
        private const int WaitTimeoutMs = 5000;
        private const int MaxReasonableBalanceDifference = 300000;

        private static readonly Logger Log = LogManager.GetLogger("mage");

        private ServiceContainer serviceContainer = null!;
        private Input input = null!;
        private UserSettingsConfigManager userSettings = null!;
        private ScreenReaderDofusMagingJob magingJob = null!;
        private ActionFactory actionFactory = null!;

        private readonly AutoResetEvent scanRequested = new AutoResetEvent(false);
        private Thread? thread;
        private volatile bool running;

        public void BindDependencies(ServiceContainer serviceContainer) {
            this.serviceContainer = serviceContainer;
            input = serviceContainer.GetService<Input>();
            userSettings = serviceContainer.GetService<UserSettingsConfigManager>();
            magingJob = (ScreenReaderDofusMagingJob) serviceContainer.GetService<DofusMagingJobContract>();
            actionFactory = serviceContainer.GetService<ActionFactory>();

            magingJob.Started += OnMagingStarted;
            magingJob.Stopped += OnMagingStopped;
            magingJob.Finished += OnMagingStopped;

            var actionHandler = serviceContainer.GetService<ActionHandler>();
            actionHandler.ActionExecuted += OnActionExecuted;
        }

        private void OnMagingStarted(object sender, MagingJobStartedEventArgs e) {
            if (!userSettings.EnableKamasCalculation) return;
            Start();
        }

        private void OnMagingStopped(object sender, EventArgs e) {
            Stop();
        }

        private void OnActionExecuted(object sender, ActionExecutedEventArgs e) {
            if (running && e.action is CombineRune)
                scanRequested.Set();
        }

        private void Start() {
            if (running) return;
            running = true;
            thread = new Thread(ScanLoop) { IsBackground = true, Name = "KamasTracker" };
            thread.Start();
        }

        private void Stop() {
            if (!running) return;
            running = false;
            scanRequested.Set();
        }

        private void ScanLoop() {
            while (running) {
                scanRequested.WaitOne(WaitTimeoutMs);
                if (!running) break;

                try {
                    input.WaitForInputDone();
                    Thread.Sleep(20);
                    if (!running) break;

                    actionFactory.MoveToInventoryHover().Execute();

                    Thread.Sleep(ScanDelayMs);
                    if (!running) break;

                    var scan = new ScreenReaderDataProvider.DofusScreenScan(
                        serviceContainer,
                        Measurements.HistoryBounds,
                        saveToDisk: false,
                        deferredScreenshot: false);

                    try {
                        var balance = scan.AverageItemBalance().Result;
                        if (balance.HasValue) {
                            var previousBalance = magingJob.Session.Balance;
                            var difference = Math.Abs(balance.Value - previousBalance);

                            if (previousBalance > 0 && difference > MaxReasonableBalanceDifference) {
                                Log.Warn($"Kamas tracker: ignored suspicious value {balance.Value} (previous: {previousBalance}, diff: {difference})");
                            } else {
                                Log.Info($"Kamas tracker: read {balance.Value} (previous: {previousBalance})");
                                magingJob.BalanceTracker?.Update(balance.Value);
                            }
                        } else {
                            Log.Debug("Kamas tracker: OCR returned null");
                        }
                    } finally {
                        scan.DisposeAsync();
                    }
                } catch (Exception ex) {
                    Log.Error(ex, $"Kamas tracker: scan failed - {ex.Message}");
                }
            }
        }

        public void Dispose() {
            Stop();
        }
    }
}
