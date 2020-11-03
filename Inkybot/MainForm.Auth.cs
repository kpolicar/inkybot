using System;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Exceptions;

namespace Inkybot
{
    public partial class MainForm
    {

        private void InitAuth() {
            Load += AuthenticatedForm_Load;
            VisibleChanged += AuthenticatedForm_VisibleChanged;
        }

        private void AuthenticatedForm_Load(object sender, EventArgs eventArgs) {
            DoLoginDialog();
        }

        private void AuthenticatedForm_VisibleChanged(object sender, EventArgs e) {
            if (Visible) {
                subscriptionCheckTimer.Start();
            } else {
                subscriptionCheckTimer.Stop();
                AuthManager.Logout();
            }
        }

        public bool DoLoginDialog(string message = "") {
            Hide();
            
            var form = new WelcomeDialogue(this, message);
            form.PathChanged += (sender, args) => InitializeDofusClient();
            
            var result = form.ShowDialog(this);
            var loginSuccess = result == DialogResult.OK;

            if (loginSuccess)
                Show();
            else
                Close();
            return loginSuccess;
        }


        private async void OnSubscriptionCheckTimer(object sender, EventArgs eventArgs) {
            try {
                var user = await api.User();
                if (!user.is_subscribed)
                    throw new UserNotSubscribedException();
            } catch (Exception exception) {
                var message = exception switch {
                    HttpRequestException _ => "Something went wrong!",
                    UserNotSubscribedException _ =>
                        "User is no longer subscribed!\nPlease extend your subscription to resume.",
                    _ => ""
                };
                Debug.WriteLine(exception.Message);
                DoLoginDialog(message);
            }
        }
    }
}
