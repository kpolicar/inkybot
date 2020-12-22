using System;
using System.Diagnostics;
using System.Windows.Forms;
using Inkybot.Actions;
using Inkybot.Domain;
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
            
            var actionHandler = (ActionHandler) Program.Services.GetService(typeof(ActionHandler));
            actionHandler.ActionExecuted += OnMagingAction;
        }

        private void OnMagingAction(object sender, ActionExecutedEventArgs e) {
            if (!(e.action is CombineRune combine) || !combine.Exo) return;
            
            BeginInvoke(new MethodInvoker(delegate {
                int count; 
                var parsed = int.TryParse(exoAttemptsValueLabel.Text, out count);
                count = parsed ? ++count : 0;
                exoAttemptsValueLabel.Text = count.ToString();
            }));
        }

        private void OnMagingSinkChanged(object sender, SinkChangedEventArgs e) {
            BeginInvoke(new MethodInvoker(delegate {
                sinkValueLabel.Text = Convert.ToInt32(Math.Floor(e.Sink)) + "";
            }));
        }

        private void OnMagingStopped(object sender, EventArgs e) {
            Invoke(new MethodInvoker(delegate {
                toggleMageButton.Text = resources.GetString("toggleMageButton.Text");
                toggleMageButton.Enabled = false;
                mageInfoPanel.Hide();
                exoAttemptsLabel.Hide();
                exoAttemptsValueLabel.Hide();
            }));
        }

        private void OnMagingFinished(object sender, MagingJobFinishedEventArgs e) {
            Invoke(new MethodInvoker(delegate {
                toggleMageButton.Enabled = true;
            }));
        }

        private void OnMagingStarted(object sender, MagingJobEventArgs e) {
            Invoke(new MethodInvoker(delegate {
                toggleMageButton.Enabled = true;
            }));

            if (e.Item.HasExo || e.Item.IsOvermaged) {
                StartMageExoOverConfirmDialog();
            }

            if (!screenReader.IsSupportedItem(e.Item) || !screenReader.IsSupportedConfig(e.Config)) {
                StartMageUnsupportedDialog();
            }
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
                toggleMageButton.Enabled = false;
            }));
        }
    }
}
