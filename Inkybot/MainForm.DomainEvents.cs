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

        private void OnMagingStarted(object sender, EventArgs e) {
            Invoke(new MethodInvoker(delegate {
                // Todo: check if is configured for exos
                toggleMageButton.Enabled = true;
            }));
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

        private void OnUserDetailsUpdated(object sender, FetchedUserEventArgs e) {
            var user = e.user;
            usernameLabel.Text = user.name;
            subscribedInfoLabel.Text = resources.GetString("subscribedInfoLabel.Text") + "\n" + user.subscribed_to!.Value.ToString("dd/MM/yyyy");
        }
    }
}
