using System;
using System.Windows.Forms;
using Inkybot.Actions;
using Inkybot.Events;
using Inkybot.Services;

namespace Inkybot
{
    public partial class MainForm
    {
        private void MainFormDomainEvents() {
            magingJob.Started += OnMagingStarted;
            magingJob.Preparing += OnMagingPreparing;
            magingJob.Stopped += OnMagingStopped;
            magingJob.Finished += OnMagingFinished;
            magingJob.SinkChanged += OnMagingSinkChanged;
            
            var actionHandler = Program.Services.GetService<ActionHandler>();
            actionHandler.ActionExecuted += OnMagingAction;
        }

        private void OnMagingAction(object sender, ActionExecutedEventArgs e) {
            if (!(e.action is CombineRune combine) || !combine.Exo || !combine.Rune.Stat.Config.HighSinkStat)
                return;
            
            BeginInvoke(new MethodInvoker(delegate {
                int count; 
                var parsed = int.TryParse(exoAttemptsValueLabel.Text, out count);
                count = parsed ? ++count : 0;
                exoAttemptsValueLabel.Text = count.ToString();
            }));
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
            FileEventLogger.MagingLogger.Debug("Maging has been stopped!");
        }

        private void OnMagingFinished(object sender, MagingJobFinishedEventArgs e) {
            Invoke(new MethodInvoker(delegate {
                toggleMageButton.Enabled = true;
                debugScreenshotButton.Enabled = true;
            }));
            Win32.SetThreadExecutionState(Win32.EXECUTION_STATE.ES_CONTINUOUS);
            FileEventLogger.MagingLogger.Debug("Maging has finished");
        }

        private void OnMagingStarted(object sender, MagingJobEventArgs e) {
            Win32.SetThreadExecutionState(
                Win32.EXECUTION_STATE.ES_CONTINUOUS
                | Win32.EXECUTION_STATE.ES_DISPLAY_REQUIRED
                | Win32.EXECUTION_STATE.ES_SYSTEM_REQUIRED);
            
            if (config.UserSettings.ShowUserWarnings &&
                (e.Item.HasExo || e.Item.IsOvermaged)) {
                StartMageExoOverConfirmDialog();
            }

            if (config.UserSettings.ShowUserWarnings &&
                (!screenReader.IsSupportedItem(e.Item) || !screenReader.IsSupportedConfig(e.Config))) {
                StartMageUnsupportedDialog();
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
                toggleMageButton.Text = resources.GetString("toggleMageButton.TextStop");
                mageInfoPanel.Show();
            }));
        }
    }
}
