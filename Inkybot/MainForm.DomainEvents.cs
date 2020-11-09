using System;
using System.Windows.Forms;
using Inkybot.Actions;
using Inkybot.Events;

namespace Inkybot
{
    public partial class MainForm
    {
        private void MainFormDomainEvents() {
            magingJob.Started += OnMagingStarted;
            magingJob.Stopped += OnMagingStopped;
            magingJob.Finished += OnMagingFinished;
            magingJob.SinkChanged += OnMagingSinkChanged;
            var actionHandler = (ActionHandler) Program.Services.GetService(typeof(ActionHandler));
            actionHandler.ActionExecuted += OnMagingAction;
        }

        private void OnMagingAction(object sender, ActionExecutedEventArgs e) {
            if (e.action is Combine combine) {
                if (combine.Exo) {
                    int count; 
                    var parsed = int.TryParse(exoAttemptsValueLabel.Text, out count);
                    count = parsed ? ++count : 0;
                    exoAttemptsValueLabel.Text = count.ToString();
                }
            }
        }

        private void OnMagingSinkChanged(object sender, SinkChangedEventArgs e) {
            Invoke(new MethodInvoker(delegate { sinkValueLabel.Text = Convert.ToInt32(e.sink) + ""; }));
        }
        
        private void OnMagingStopped(object sender, EventArgs e) {
            Invoke(new MethodInvoker(delegate {
                toggleMageButton.Text = "START";
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
                exoAttemptsLabel.Show();
                exoAttemptsValueLabel.Show();
                toggleMageButton.Text = "STOP";
                mageInfoPanel.Show();
            }));
        }
        
        private void OnUserDetailsUpdated(object sender, FetchedUserEventArgs e) {
            var user = e.user;
            usernameLabel.Text = user.name;
            subscribedInfoLabel.Text = "Subscribed to:\n" + user.subscribed_to!.Value.ToString("dd/MM/yyyy");
        }
    }
}
