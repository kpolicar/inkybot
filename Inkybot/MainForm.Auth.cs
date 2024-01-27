using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Api.Resources;
using Inkybot.Contracts;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Properties;

namespace Inkybot
{
    public partial class MainForm
    {
        private const int SubscriptionCheckRequestMaxAttempts = 3;
        private int SubscriptionCheckRequestAttempts = 0;

        private void InitAuth() {
            api = Program.Services.GetService<ApiClient>();
            api.UserFetched += OnUserFetched;
            VisibleChanged += AuthenticatedForm_VisibleChanged;
            auth = Program.Services.GetService<AuthManager>();
        }

        private void AuthenticatedForm_VisibleChanged(object sender, EventArgs e) {
            if (Visible) {
                subscriptionCheckTimer.Start();
            } else {
                subscriptionCheckTimer.Stop();
                if (!dontLogoutOnVisibleChanged)
                    auth.Logout();
            }
        }

        public bool DoLoginDialog(string message = "") {
            Hide();

            using var form = new WelcomeDialogue(this, message);
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


        private async void OnSubscriptionCheckTimer(object sender, EventArgs eventArgs) =>
            await FetchUserAndUpdateForm();

        private async Task FetchUserAndUpdateForm() {
            if (!Visible)
                return;
            SubscriptionCheckRequestAttempts++;
            try {
                var user = await api.User();
                if (!user.is_subscribed && !user.is_free_trial)
                    throw new UserNotSubscribedException();
            } catch (Exception exception) {
                if (SubscriptionCheckRequestAttempts < SubscriptionCheckRequestMaxAttempts) {
                    Debug.WriteLine("OnSubscriptionCheckTimer reattempt "+SubscriptionCheckRequestAttempts);
                    await Task.Delay(15000);
                    await FetchUserAndUpdateForm();
                    return;
                }

                var message = exception switch {
                    HttpRequestException e => resources.GetString("subscriptiontimer.httpexception")+"\nMessage: "+e.Message,
                    UserNotSubscribedException =>
                        resources.GetString("subscriptiontimer.nolongersubscribed")!+"\n"+
                        resources.GetString("subscriptiontimer.nolongersubscribed_pleaseextend")!,
                    { } e => resources.GetString("subscriptiontimer.generalexception")+"\nMessage: "+e.Message
                };
                Debug.WriteLine(exception.Message);
                magingJob.StopMage();
                DoLoginDialog(message);
            }

            SubscriptionCheckRequestAttempts = 0;
        }
        
        private void OnUserFetched(object sender, FetchedUserEventArgs e) {
            var user = e.user;
            Invoke(new MethodInvoker(() => {
                UpdateUserDetails(user);
            }));
        }

        private void UpdateUserDetails(User user) {
            usernameLabel.Text = user.name;
            if (user.is_subscribed) {
                subscribedInfoLabel.Text =
                    resources.GetString("subscribedInfoLabel.Text") + "\n" +
                    (user.onUnlimitedPlan ? resources.GetString("subscribedInfoLabel.TextUnlimited")
                        : user.onStandardPlan ? resources.GetString("subscribedInfoLabel.TextStandard")
                        : user.onStarterPlan ? resources.GetString("subscribedInfoLabel.TextStarter")
                        : "-");
            } else if (user.is_free_trial) {
                var text = resources.GetString("subscribedInfoLabel.FreeTrial");
                if (user.free_trial_ends_at != null)
                    text += "\n"+user.free_trial_ends_at;
                
                subscribedInfoLabel.Text = text;
            } else {
                subscribedInfoLabel.Text = resources.GetString("subscribedInfoLabel.Text") + "-";
            }
            subscribePlanUpgradeLinkLabel.Visible = !user.onUnlimitedPlan;
        }
    }
}
