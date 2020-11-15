using System;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Domain;
using Inkybot.Events;

namespace Inkybot
{
    public partial class WelcomeDialogue : Form
    {
        public event EventHandler<PathChangedEventArgs> PathChanged; 
        private readonly ApiClient api;

        public WelcomeDialogue(MainForm mainForm, string errorMessage) : this() {
            api = (ApiClient) Program.Services.GetService(typeof(ApiClient));
            this.errorMessage.Text = errorMessage;
        }

        public WelcomeDialogue() {
            InitializeComponent();
            usernameTextBox.Text = Properties.Settings.Default.email;
            passwordTextBox.Text = Properties.Settings.Default.password;
            newVersionLabel.Hide();
            rememberPasswordCheckbox.Checked = Properties.Settings.Default.password.Length > 0;
        }

        private async void button1_Click(object sender, EventArgs e) {
            errorMessage.Text = "";
            try {
                var connection = await AuthManager.Login(usernameTextBox.Text, passwordTextBox.Text);

                if (connection == null) {
                    errorMessage.Text = resources.GetString("errorMessage.TextIncorrect");
                    return;
                }

                var user = await api.User();
                if (!user.is_subscribed) {
                    errorMessage.Text = resources.GetString("errorMessage.TextUnsubscribed");
                    return;
                }
            } catch (HttpRequestException requestException) {
                errorMessage.Text = resources.GetString("errorMessage.TextConnectionError");
                return;
            }

            Properties.Settings.Default.email = usernameTextBox.Text;
            Properties.Settings.Default.password = rememberPasswordCheckbox.Checked ? passwordTextBox.Text : "";
            Properties.Settings.Default.Save();
            DialogResult = DialogResult.OK;
        }

        private async void LoginForm_Load(object sender, EventArgs e) {
            try {
                var newestVersion = await api.NewestVersion();
                if (Program.VersionNumber != newestVersion.number)
                    newVersionLabel.Show();
            } 
            catch (Exception exception) {
                
            }
        }

        private void linkLabel1_LinkClicked_1(object sender, EventArgs eventArgs) {
            Process.Start($"{Server.BaseUrl}/register");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start($"{Server.BaseUrl}");
        }

        private void newVersionLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start($"{Server.BaseUrl}/release/latest");
        }

        private void dofusPathLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            var changedPath = new DofusPathForm().ShowDialog(this);
            if (changedPath == DialogResult.OK)
                PathChanged?.Invoke(this, new PathChangedEventArgs());
        }

        private void switchLanguageLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Properties.Settings.Default.locale =
                Properties.Settings.Default.locale.Equals("en") ? "fr" :"en";
        }
    }
}
