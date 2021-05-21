using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Inkybot.Domain;

namespace Inkybot
{
    public partial class FirstTimeInfoForm : Form
    {
        private int continueTimerElapsedTime;
        private const int timeoutToEnableButton = 5000;

        public FirstTimeInfoForm() {
            InitializeComponent();
            UpdateContinueButtonText();
        }
        
        private void continueButton_Click(object sender, EventArgs e) {
            Properties.Settings.Default.FirstTime = false;
            Properties.Settings.Default.Save();
            DialogResult = DialogResult.OK;
        }

        private void OnFirstTimeInfoFormShown(object sender, EventArgs e) {
            enableContinueTimer.Start();
        }

        private void OnEnableContinueTimerTick(object sender, EventArgs e) {
            continueTimerElapsedTime += enableContinueTimer.Interval;
            UpdateContinueButtonText();
        }

        private void UpdateContinueButtonText() {
            if (continueTimerElapsedTime >= timeoutToEnableButton) {
                continueButton.Text = resources.GetString("continueButton.Text")!;
                continueButton.Enabled = true;
                continueButton.BackColor = Color.Black;
            } else {
                continueButton.Text = ((timeoutToEnableButton - continueTimerElapsedTime) / 1000).ToString();
                continueButton.Enabled = false;
                continueButton.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            }
        }

        private void OnUsageInstructionsLinkLabelClick(object sender, EventArgs e) {
            Process.Start(Server.UsageInstructions);
        }

        private void OnDiscordLinkLabelClick(object sender, EventArgs e) {
            Process.Start(Server.DiscordLink);
        }
    }
}
