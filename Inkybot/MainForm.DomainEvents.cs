using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Inkybot.Actions;
using Inkybot.Controls;
using Inkybot.Events;
using Inkybot.Services;

namespace Inkybot
{
    public partial class MainForm
    {
        protected bool hasShownUnsupportedWarning = false;

        private void MainFormDomainEvents() {
            mageQueue.Enqueued += OnMagingEnqueued;
            mageQueue.Dequeued += OnMagingDequeuedOrRemoved;
            mageQueue.Head += OnMagingHead;
            mageQueue.Removed += OnMagingDequeuedOrRemoved;
            mageQueue.Moved += OnMagingMoved;
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
            BeginInvoke(new MethodInvoker(() => {
                exoAttemptsValueLabel.Text = count.ToString();
            }));
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
            
            // Double confirmation for high sink stats
            if (magingJob.IsMaging && e.Item.Stats.ExoStats.Any(exoStat => exoStat.Value > 0 && exoStat.Stat.Config.HighSinkStat)) {
                var confirmation =
                    MessageBox.Show(
                        resources.GetString("popup.warning_sensitive_highsinkstat"),
                        resources.GetString("popup.title_confirmation"),
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                if (confirmation == DialogResult.No)
                    magingJob.StopMage();
            }
        }

        private void OnMagingSinkChanged(object sender, SinkChangedEventArgs e) {
            BeginInvoke(new MethodInvoker(delegate {
                sinkValueLabel.Text = e.Sink.ToString(CultureInfo.InvariantCulture);
            }));
        }

        private void OnMagingStopped(object sender, EventArgs e) {
            UpdateQueueControls();
            Invoke(new MethodInvoker(delegate {
                toggleMageButton.Text = resources.GetString("toggleMageButton.Text");
                toggleMageButton.Enabled = false;
            }));
        }

        private void OnMagingFinished(object sender, MagingJobFinishedEventArgs e) {
            Invoke(new MethodInvoker(delegate {
                toggleMageButton.Enabled = true;
                debugScreenshotButton.Enabled = true;
                if (settingsForm.AutoShutdownDelay > 0 && !HasManuallyStoppedMaging && e.AutoShutdown)
                    StartAutoShutdownCounter();
                EnableDebugging();
                HasManuallyStoppedMaging = false;
                if (mageQueueForm.Visible)
                    ShowQueueControls();
            }));
            Win32.SetThreadExecutionState(Win32.EXECUTION_STATE.ES_CONTINUOUS);
        }

        private void OnMagingEnqueued(object sender, MageQueueEventArgs e) {
            var control = e.QueueItem.Control;
            
            MarkQueueRectangleAsEnqueued(control);
            UpdateQueueControls();
        }
        
        private void OnMagingDequeuedOrRemoved(object sender, MageQueueEventArgs e) {
            var control = e.QueueItem.Control;
            
            UnmarkQueueRectangleAsEnqueued(control);
            if (!magingJob.IsMaging) {
                UpdateQueueControls();
            }
        }

        private void OnMagingHead(object sender, MageQueueEventArgs e) {
            UpdateQueueControls();
        }

        private void OnMagingMoved(object sender, MageQueueMovedEventArgs mageQueueMovedEventArgs) {
            UpdateQueueControls();
        }

        private void UpdateQueueControls() =>
            BeginInvoke(new MethodInvoker(delegate {
                if (magingJob.IsMaging) {
                    nextInQueuePreviewPictureBox.Image = mageQueue.Queue.Count >= 2
                        ? mageQueue.Queue[1].ItemPreview
                        : null;
                    nextInQueueLabel.Visible = nextInQueuePreviewPictureBox.Visible = mageQueue.Queue.Count >= 2;
                } else {
                    nextInQueuePreviewPictureBox.Image = !mageQueue.Empty
                        ? mageQueue.Peek().ItemPreview
                        : null;
                    nextInQueueLabel.Visible = nextInQueuePreviewPictureBox.Visible = !mageQueue.Empty;
                }
            }));

        private void MarkQueueRectangleAsEnqueued(EnqueueRectangle control) =>
            BeginInvoke(new MethodInvoker(delegate {
                control.EditConfigMenuItem.Visible = control.RemoveFromQueueMenuItem.Visible = true;
                control.AddToQueueMenuItem.Visible = false;
                control.ForeColor = control.BackColor = Color.ForestGreen;
                control.BorderWidth = 4;
                control.NewOnLocationChanged(EventArgs.Empty);
                control.BringToFront();
            }));

        private void UnmarkQueueRectangleAsEnqueued(EnqueueRectangle control) =>
            BeginInvoke(new MethodInvoker(delegate {
                control.EditConfigMenuItem.Visible = control.RemoveFromQueueMenuItem.Visible = false;
                control.AddToQueueMenuItem.Visible = true;
                control.ForeColor = control.BackColor = System.Drawing.SystemColors.Control;
                control.BorderWidth = 2;
                control.NewOnLocationChanged(EventArgs.Empty);
            }));

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
            
            Invoke(new MethodInvoker(delegate {
                DisableDebugging();
                HideQueueControls();
            }));

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
