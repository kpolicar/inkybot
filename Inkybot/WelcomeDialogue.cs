using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageMagick;
using Inkybot.Api;
using Inkybot.Api.Resources;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Properties;

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
            usernameTextBox.Text = Settings.Default.email;
            passwordTextBox.Text = Settings.Default.password;
            if (Settings.Default.DisableOpenCL)
                disableOpenCLLabel.Text = resources.GetString("disableOpenCLLabel.Text_enable");
            newVersionLabel.Hide();
            rememberPasswordCheckbox.Checked = Properties.Settings.Default.password.Length > 0;
            auth = Program.Services.GetService<AuthManager>();
            api = Program.Services.GetService<ApiClient>();
            openSettingsInFileExplorer.Visible = File.Exists(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);
        }


        private async void button1_Click(object sender, EventArgs e) {
            await PerformLogin();
        }

        private async Task PerformLogin() {
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
            } catch (HttpRequestException ex) {
                button1.Enabled = true;
                errorMessage.Text = resources.GetString("errorMessage.TextConnectionError")+"\nMessage: "+ex.Message;
                return;
            } catch (Exception ex) {
                button1.Enabled = true;
                errorMessage.Text = resources.GetString("errorMessage.TextUnknown");
                Debug.WriteLine(ex);
                return;
            }

            var firstTime = Settings.Default.FirstTime;
            Properties.Settings.Default.email = usernameTextBox.Text;
            Properties.Settings.Default.password = rememberPasswordCheckbox.Checked ? passwordTextBox.Text : "";
            Properties.Settings.Default.Save();
            
            if (firstTime) {
                Hide();
                var result = new FirstTimeInfoForm().ShowDialog(this);
                if (result != DialogResult.OK) {
                    DialogResult = DialogResult.Abort;
                    return;
                }
            }
            
            Settings.Default.FirstTime = false;
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

            // Offline mode: log in automatically so the user never has to touch this dialog.
            // auth.Login / api.User are instant mocks, so this closes the dialog with
            // DialogResult.OK right away (still showing FirstTimeInfoForm on a first run).
            if (Program.OfflineMode)
                await PerformLogin();
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

        private void resetSettings_Clicked(object sender, EventArgs eventArgs) {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
                
            Application.Restart();
        }

        private void switchLanguageButton_Clicked(object sender, EventArgs eventArgs) {
            Properties.Settings.Default.locale =
                Program.Lang.TwoLetterISOLanguageName == "" ||
                Program.Lang.TwoLetterISOLanguageName == Properties.Resources.EnglishLocaleCode ?
                    Properties.Resources.FrenchLocaleCode :
                    Properties.Resources.EnglishLocaleCode;
            Properties.Settings.Default.Save();
                
            Application.Restart();
        }

        private void disableOpenCL_Clicked(object sender, EventArgs e) {
            if (!Settings.Default.DisableOpenCL) {
                var confirmation =
                    MessageBox.Show(
                        resources.GetString("popup.warning_disableopencl"),
                        resources.GetString("popup.warning_disableopencl_title"),
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                
                if (confirmation != DialogResult.Yes) 
                    return;
            }

            Settings.Default.DisableOpenCL = !Settings.Default.DisableOpenCL;
            Settings.Default.Save();
            OpenCL.IsEnabled = !Settings.Default.DisableOpenCL;
            
            disableOpenCLLabel.Text = Settings.Default.DisableOpenCL
                ? resources.GetString("disableOpenCLLabel.Text_enable")
                : resources.GetString("disableOpenCLLabel.Text");
        }

        private void openSettingsInFileExplorer_Clicked(object sender, EventArgs e) {
            var path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            if (path != null) {
                var lastIndexOfSlash = path.LastIndexOf('\\');
                Process.Start(path.Substring(0, lastIndexOfSlash));
            }
        }

        private void button1_EnableChanged(object sender, EventArgs e) {
            button1.BackColor = button1.Enabled
                ? System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))))
                : System.Drawing.SystemColors.ControlDarkDark;
        }
    }
}
