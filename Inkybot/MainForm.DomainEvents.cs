using System;
using System.Windows.Forms;
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
        }

        private void OnMagingSinkChanged(object sender, SinkChangedEventArgs e) {
            Invoke(new MethodInvoker(delegate { sinkValueLabel.Text = Convert.ToInt32(e.sink) + ""; }));
        }
        
        private void OnMagingStopped(object sender, EventArgs e) {
            Invoke(new MethodInvoker(delegate {
                toggleMageButton.Text = "START";
                toggleMageButton.Enabled = false;
                mageInfoPanel.Hide();
            }));
        }

        private void OnMagingFinished(object sender, MagingJobFinishedEventArgs e) {
            Invoke(new MethodInvoker(delegate {
                toggleMageButton.Enabled = true;
            }));
        }

        private void OnMagingStarted(object sender, EventArgs e) {
            Invoke(new MethodInvoker(delegate {
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
