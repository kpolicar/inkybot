using System;
using System.Globalization;
using System.Windows.Forms;
using Inkybot.Actions;
using Inkybot.Events;
using Inkybot.Services;

namespace Inkybot
{
    public partial class MainForm
    {
        protected bool hasShownUnsupportedWarning = false;

        private void MainFormDomainEvents() {
            magingJob.Starting += OnMagingStarting;
            magingJob.Started += OnMagingStarted;
            if (magingJob is ScreenReaderDofusMagingJob screenReaderDofusMagingJob)
                screenReaderDofusMagingJob.SensitiveMage += OnSensitiveMage;
            magingJob.Preparing += OnMagingPreparing;
            magingJob.Stopped += OnMagingStopped;
            magingJob.Finished += OnMagingFinished;
            magingJob.SinkChanged += OnMagingSinkChanged;
            magingJob.BalanceSpent += OnMagingBalanceSpent;
            analytics.ExoAttempt += OnExoAttempt;
        }

        private void OnExoAttempt(object sender, EventArgs e) {
            int count; 
            var parsed = int.TryParse(exoAttemptsValueLabel.Text, out count);
            count = parsed ? ++count : 0;
            exoAttemptsValueLabel.Text = count.ToString();
        }

        private void OnMagingBalanceSpent(object sender, BalanceChangedEventArgs e) {
            BeginInvoke(new MethodInvoker(delegate {
                if (e.Balance >= 10000) {
                    kamasSpentValueLabel.Text = Math.Round(e.Balance / 1000000d, 2).ToString(CultureInfo.InvariantCulture) + "mk";
                } else {
                    kamasSpentValueLabel.Text = e.Balance.ToString(CultureInfo.InvariantCulture) + "k";
                }
            }));
        }

        private void OnSensitiveMage(object sender, MagingJobStartedEventArgs e) {
            StartMageExoOverConfirmDialog();
        }

        private void OnMagingSinkChanged(object sender, SinkChangedEventArgs e) {
            BeginInvoke(new MethodInvoker(delegate {
                sinkValueLabel.Text = Convert.ToInt32(Math.Floor(e.Sink)).ToString();
            }));
        }

        private void OnMagingStopped(object sender, EventArgs e) {
            Invoke(new MethodInvoker(delegate {
                toggleMageButton.Text = resources.GetString("toggleMageButton.Text");
                toggleMageButton.Enabled = false;
            }));
        }

        private void OnMagingFinished(object sender, MagingJobFinishedEventArgs e) {
            Invoke(new MethodInvoker(delegate {
                toggleMageButton.Enabled = true;
                debugScreenshotButton.Enabled = true;
                if (configForm.AutoShutdownDelay > 0 && !HasManuallyStoppedMaging && e.AutoShutdown)
                    StartAutoShutdownCounter();
                EnableDebugging();
                HasManuallyStoppedMaging = false;
            }));
            Win32.SetThreadExecutionState(Win32.EXECUTION_STATE.ES_CONTINUOUS);
        }

        private void StartAutoShutdownCounter() {
            autoShutdownTimeElapsed = 0;
            shutdownToastPanel.Show();
            shutdownToastPanel.BringToFront();
            autoShutdownTimer.Start();
        }

        private void StopAutoShutdownCounter() {
            shutdownToastPanel.Hide();
            autoShutdownTimer.Stop();
            
            if (autoShutdownTimeElapsed > 0) {
                toastLabel.Text = resources.GetString("autoShutdownTimeElapsed.TextStopped")!;
                toastPanel.Show();
                toastPanel.BringToFront();
            }
            
            autoShutdownTimeElapsed = 0;
            RefreshAutoShutdownLabels();
        }
        
        private void OnMagingStarting(object sender, EventArgs e) {
            Win32.SetThreadExecutionState(
                Win32.EXECUTION_STATE.ES_CONTINUOUS
                | Win32.EXECUTION_STATE.ES_DISPLAY_REQUIRED
                | Win32.EXECUTION_STATE.ES_SYSTEM_REQUIRED);
        }

        private void OnMagingStarted(object sender, MagingJobStartedEventArgs e) {
            if (e.Restarting) return;
            
            Invoke(new MethodInvoker(DisableDebugging));

            if (!hasShownUnsupportedWarning && config.UserSettings.ShowUserWarnings &&
                (!screenReader.IsSupportedItem(e.Item) || !screenReader.IsSupportedConfig(e.Config)))
            {
                StartMageUnsupportedDialog();
                hasShownUnsupportedWarning = true;
            }
                
            Invoke(new MethodInvoker(delegate {
                debugScreenshotButton.Enabled = false;
            }));
        }

        private void StartMageUnsupportedDialog() {
            var confirmation =
                MessageBox.Show(
                    resources.GetString("popup.warning_unsupported"),
                    resources.GetString("popup.title_confirmation"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (confirmation == DialogResult.No)
                magingJob.StopMage();
        }

        private void StartMageExoOverConfirmDialog() {
            var confirmation =
                MessageBox.Show(
                    resources.GetString("popup.warning_sensitive"),
                    resources.GetString("popup.title_confirmation"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (confirmation == DialogResult.No)
                magingJob.StopMage();
        }

        private void OnMagingPreparing(object sender, EventArgs e) {
            Invoke(new MethodInvoker(delegate {
                exoAttemptsLabel.Show();
                exoAttemptsValueLabel.Show();
                kamasSpentLabel.Show();
                kamasSpentValueLabel.Show();
                toggleMageButton.Text = resources.GetString("toggleMageButton.TextStop");
                mageInfoPanel.Show();
            }));
        }
    }
}
