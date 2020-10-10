using System;
using System.Net.Http;
using System.Windows.Forms;
using WindowsFormsApp.Api;

namespace WindowsFormsApp
{
    public partial class LoginForm : Form
    {
        private readonly ApiDataProvider api;

        public LoginForm(string errorMessage) : this() {
            api = (ApiDataProvider) Program.Services.GetService(typeof(ApiDataProvider));
            this.errorMessage.Text = errorMessage;
        }

        public LoginForm() {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            throw new NotImplementedException();
        }

        private async void button1_Click(object sender, EventArgs e) {
            try {
                var connection = await Auth.Login(usernameTextBox.Text, passwordTextBox.Text);

                if (connection == null) {
                    errorMessage.Text = "Incorrect username or password!";
                    return;
                }

                var user = await api.User();
                if (!user.is_subscribed) {
                    errorMessage.Text = "User is not subscribed!";
                    return;
                }
            } catch (HttpRequestException requestException) {
                errorMessage.Text = "Something went wrong on our end.\nPlease try again later.";
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
