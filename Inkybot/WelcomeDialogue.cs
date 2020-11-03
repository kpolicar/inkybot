using System;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Events;

namespace Inkybot
{
    public partial class WelcomeDialogue : Form
    {
        public event EventHandler<PathChangedEventArgs> PathChanged; 
        private readonly ApiDataProvider api;

        public WelcomeDialogue(MainForm mainForm, string errorMessage) : this() {
            api = (ApiDataProvider) Program.Services.GetService(typeof(ApiDataProvider));
            this.errorMessage.Text = errorMessage;
        }

        public WelcomeDialogue() {
            InitializeComponent();
            newVersionLabel.Hide();
        }

        private async void button1_Click(object sender, EventArgs e) {
            errorMessage.Text = "";
            try {
                var connection = await AuthManager.Login(usernameTextBox.Text, passwordTextBox.Text);

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
                errorMessage.Text = "Could not connect to server.\nPlease try again later.";
                return;
            }

            Properties.Settings.Default.email = usernameTextBox.Text;
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
    }
}
