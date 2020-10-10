using System;
using System.Net.Http;
using System.Windows.Forms;
using WindowsFormsApp.Api;
using WindowsFormsApp.Domain;
using WindowsFormsApp.Exceptions;

namespace WindowsFormsApp
{
    public class AuthenticatedForm : Form
    {
        private Timer subscriptionCheckTimer;
        private ApiDataProvider api;

        public AuthenticatedForm()
        {
            Load += AuthenticatedForm_Load;
            subscriptionCheckTimer = new Timer {
                Interval = 5000
            };
            subscriptionCheckTimer.Tick += OnSubscriptionCheckTimer;
            api = (ApiDataProvider) Program.Services.GetService(typeof(ApiDataProvider));

        }
        
        private void AuthenticatedForm_Load(object sender, EventArgs eventArgs)
        {
            DoLoginDialog();
        }

        public bool DoLoginDialog(string message="")
        {
            Hide();
            var result = new LoginForm(message).ShowDialog(this);
            var loginSuccess = result == DialogResult.OK;
            
            if (!loginSuccess)
                Show();
            else
                Close();
            return loginSuccess;
        }
        

        private async void OnSubscriptionCheckTimer(object sender, EventArgs eventArgs)
        {
            try
            {
                var user = await api.User();
                if (!user.is_subscribed)
                    DoLoginDialog();
            }
            catch (Exception exception)
            {
                var message = exception switch
                {
                    HttpRequestException _ => "Something went wrong!",
                    UserNotSubscribedException _ =>
                        "User is no longer subscribed!\nPlease extend your subscription to resume.",
                    _ => ""
                };
                DoLoginDialog(message);
            }
        }
    }
}