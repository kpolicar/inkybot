using System;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Api.Resources;
using Inkybot.Events;
using Inkybot.Exceptions;

namespace Inkybot
{
    public partial class MainForm
    {
        private const int SubscriptionCheckRequestMaxAttempts = 3;
        private int SubscriptionCheckRequestAttempts = 0;

        private void InitAuth() {
            api = (ApiClient) Program.Services.GetService(typeof(ApiClient));
            api.UserFetched += OnUserFetched;
            VisibleChanged += AuthenticatedForm_VisibleChanged;
            auth = Program.Services.GetService<AuthManager>();
        }

        private void AuthenticatedForm_VisibleChanged(object sender, EventArgs e) {
            if (Visible) {
                subscriptionCheckTimer.Start();
            } else {
                subscriptionCheckTimer.Stop();
                auth.Logout();
            }
        }

        public bool DoLoginDialog(string message = "") {
            Hide();
            
            var form = new WelcomeDialogue(this, message);
            form.PathChanged += (sender, args) => InitializeDofusClient();
            #if DEBUG
            Debugging.WelcomeDialogue.Bind(form);
            #endif

            var result = form.ShowDialog(this);
            var loginSuccess = result == DialogResult.OK;

            if (loginSuccess)
                Show();
            else
                Close();
            return loginSuccess;
        }


        private async void OnSubscriptionCheckTimer(object sender, EventArgs eventArgs) {
            SubscriptionCheckRequestAttempts++;
            try {
                var user = await api.User();
                if (!user.is_subscribed && !user.is_free_trial)
                    throw new UserNotSubscribedException();
            } catch (Exception exception) {
                if (exception is HttpRequestException && SubscriptionCheckRequestAttempts < SubscriptionCheckRequestMaxAttempts) {
                    OnSubscriptionCheckTimer(sender, eventArgs);
                    return;
                }
                
                var message = exception switch {
                    HttpRequestException _ => "Something went wrong!",
                    UserNotSubscribedException _ =>
                        "User is no longer subscribed!\nPlease extend your subscription to resume.",
                    _ => ""
                };
                Debug.WriteLine(exception.Message);
                magingJob.StopMage();
                DoLoginDialog(message);
            }

            SubscriptionCheckRequestAttempts = 0;
        }
        
        private void OnUserFetched(object sender, FetchedUserEventArgs e) {
            var user = e.user;
            UpdateUserDetails(user);
        }

        private void UpdateUserDetails(User user) {
            usernameLabel.Text = user.name;
            if (user.is_subscribed) {
                subscribedInfoLabel.Text =
                    resources.GetString("subscribedInfoLabel.Text") + "\n" +
                    user.subscribed_to!.Value.ToString("dd/MM/yyyy");
            } else if (user.is_free_trial) {
                subscribedInfoLabel.Text = resources.GetString("subscribedInfoLabel.FreeTrial");
            }
        }
    }
}
