using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Api.Resources;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Events;
using Inkybot.Exceptions;

namespace Inkybot
{
    public partial class WelcomeDialogue : Form
    {
        public event EventHandler<PathChangedEventArgs>? PathChanged; 
        private readonly ApiClient api;
        private AuthManager auth;

        public WelcomeDialogue(MainForm mainForm, string errorMessage) : this() {
            this.errorMessage.Text = errorMessage;
        }

        public WelcomeDialogue() {
            InitializeComponent();
            usernameTextBox.Text = Properties.Settings.Default.email;
            passwordTextBox.Text = Properties.Settings.Default.password;
            newVersionLabel.Hide();
            rememberPasswordCheckbox.Checked = Properties.Settings.Default.password.Length > 0;
            auth = Program.Services.GetService<AuthManager>();
            api = Program.Services.GetService<ApiClient>();
        }

        private async void button1_Click(object sender, EventArgs e) {
            errorMessage.Text = "";
            button1.Enabled = false;
            try {
                var connection = await auth.Login(usernameTextBox.Text, passwordTextBox.Text);
                button1.Enabled = true;

                if (connection == null) {
                    errorMessage.Text = resources.GetString("errorMessage.TextIncorrect");
                    return;
                }

                var user = await api.User();
                await HandleUserSubscriptionStatus(user);

            } catch (UserTrialHasExpiredException) {
                button1.Enabled = true;
                errorMessage.Text = resources.GetString("errorMessage.TextTrialExpired");
                return;
            } catch (UserNotSubscribedException) {
                button1.Enabled = true;
                errorMessage.Text = resources.GetString("errorMessage.TextUnsubscribed");
                return;
            } catch (HttpRequestException) {
                button1.Enabled = true;
                errorMessage.Text = resources.GetString("errorMessage.TextConnectionError");
                return;
            }

            Properties.Settings.Default.email = usernameTextBox.Text;
            Properties.Settings.Default.password = rememberPasswordCheckbox.Checked ? passwordTextBox.Text : "";
            Properties.Settings.Default.Save();
            DialogResult = DialogResult.OK;
        }

        private async Task HandleUserSubscriptionStatus(User user) {
            _ = (user.is_subscribed, user.is_free_trial, user.free_trial_available) switch {
                (true, _, _) => true,
                (false, true, _) => true,
                (false, false, true) => await TryStartFreeTrial()
                    ? true
                    : throw new UserNotSubscribedException(),
                _ => throw new UserNotSubscribedException(),
            };
        }

        private async Task<bool> TryStartFreeTrial() {
            var confirmation =
                MessageBox.Show(
                    resources.GetString("popup.ask_trial"),
                    resources.GetString("popup.title_trial"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (confirmation == DialogResult.Yes) {
                var trial = await api.BeginFreeTrial();
                if (trial.expired)
                    throw new UserTrialHasExpiredException();
                await api.User();
                return true;
            }
            return false;
        }

        private async void LoginForm_Load(object sender, EventArgs e) {
            try {
                var newestVersion = await api.NewestVersion();
                if (Program.VersionNumber != newestVersion.number) {
                    newVersionLabel.Show();
                    var tooltip = new ToolTip();
                    tooltip.SetToolTip(newVersionLabel, Program.Version+" » "+newestVersion.name);
                }
            } catch (Exception) {
                // ignored
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

        private void dofusPathButton_Clicked(object sender, EventArgs eventArgs) {
            var changedPath = new DofusPathForm().ShowDialog(this);
            if (changedPath == DialogResult.OK)
                PathChanged?.Invoke(this, new PathChangedEventArgs());
        }

        private void resetSettings_Clicked(object sender, EventArgs eventArgs) {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
                
            Application.Restart();
        }

        private void switchLanguageButton_Clicked(object sender, EventArgs eventArgs) {
            Properties.Settings.Default.locale =
                Program.Lang.TwoLetterISOLanguageName == "" ?
                    Properties.Resources.FrenchLocaleCode :
                    Properties.Resources.EnglishLocaleCode;
            Properties.Settings.Default.Save();
                
            Application.Restart();
        }

        private void settingsDropdownButton_Click(object sender, EventArgs e) {
            throw new System.NotImplementedException();
        }
    }
}
