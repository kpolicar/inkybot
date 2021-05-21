using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Exceptions;
using Inkybot.Services;


namespace Inkybot
{
    public partial class MainForm
    {
        public bool HasManuallyStoppedMaging { get; set; }
        
        private void MainFormEvents() {
            Closing += (sender, args) => {
                magingJob.StopMage();
                StopDebugging();
            };
        }
        
        private void MainForm_VisibleChanged(object sender, EventArgs e) {
            if (!Visible && magingJob.IsMaging)
                magingJob.StopMage();
            if (!Visible) {
                setupForm.Hide();
                configForm.Hide();
                statisticsForm.Hide();
            }
        }
        
        private void toggleMageButton_Click(object sender, EventArgs e) {
            toastPanel.Hide();
            StopAutoShutdownCounter();

            magingJob.BeginMage(!magingJob.IsMaging);
            if (!magingJob.IsMaging)
                HasManuallyStoppedMaging = true;
        }

        private void helpButton_Click(object sender, EventArgs e) {
            Process.Start($"{Server.BaseUrl}/release/{Program.VersionEndpoint}#usage");
        }
        
        private void statsButton_Click(object sender, EventArgs e) {
            if (!setupForm.Visible) setupForm.Show();
            else setupForm.Focus();
        }
        
        private void configButton_Click(object sender, EventArgs e) {
            if (!configForm.Visible) configForm.Show();
            else configForm.Focus();
        }

        private void toastPanelCloseButton_Click(object sender, EventArgs e) {
            toastPanel.Hide();
        }

        private void debugScreenshotButton_Click(object sender, EventArgs e) {
            var takeScreenshot = new ThreadStart(async delegate {
                var scan = new ScreenReaderDataProvider.DofusScreenScan(hWndDocked, Program.Services, Measurements.HistoryBounds, true);

                for (var numOfTries = 0; numOfTries < 3; numOfTries++) {
                    try {
                        await scan.MinMaxStats();
                        await scan.History();
                        await scan.Stats();
                        break;
                    } catch (OcrEngineNotReadyYetException) {
                    }
                    numOfTries++;
                }

                Invoke(new MethodInvoker(delegate {
                    debugScreenshotButton.Enabled = true;
                }));
            });
            
            new Thread(takeScreenshot).Start();
            debugScreenshotButton.Enabled = false;
        }

        private void hallOfFameButton_Click(object sender, EventArgs e) {
            Process.Start(Server.HallOfFameUrl);
        }

        private void statisticsButton_Click(object sender, EventArgs e) {
            if (auth.User != null && !auth.User.canViewStatistics) {
                var text = !auth.User.onUnlimitedPlan && !auth.User.onStandardPlan
                    ? resources.GetString("popup.error_notavailable_unlimitedstandard_plan")
                    : resources.GetString("popup.error_notavailable_current_plan");
                
                MessageBox.Show(
                    text,
                    resources.GetString("popup.error_restricted"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            
            if (!statisticsForm.Visible) statisticsForm.Show();
            else statisticsForm.Focus();
        }

        private void subscribePlanUpgradeLinkLabel_OnClick(object sender, EventArgs e) {
            Process.Start(Server.SubscribeUrl);
        }

        private void shutdownToastPanelCloseButton_Click(object sender, EventArgs e) {
            StopAutoShutdownCounter();
        }

        private void OnResize(object sender, EventArgs e) {
            this.shutdownToastPanel.Location = 
                new Point(ClientSize.Width / 2 - shutdownToastPanel.Size.Width / 2, 
                    ClientSize.Height / 2 - shutdownToastPanel.Size.Height);
        }

        private void OnAutoShutdownTimer(object sender, EventArgs e) {
            autoShutdownTimeElapsed += autoShutdownTimer.Interval;
            RefreshAutoShutdownLabels();
            var timeLeft = configForm.AutoShutdownDelay - autoShutdownTimeElapsed;
            if (timeLeft <= 0) {
                Application.Exit();
            }
        }

        private void RefreshAutoShutdownLabels() {
            var timeLeft = configForm.AutoShutdownDelay - autoShutdownTimeElapsed;
            var timeLeftInSeconds = timeLeft / 1000;
            
            shutdownToastValueLabel.Text = timeLeftInSeconds >= 60
                ? resources.GetString("autoShutdownTimeElapsed.TextMinutes")!
                    .Replace(":value", Math.Max(2, timeLeftInSeconds / 60).ToString())
                : timeLeftInSeconds > 1
                    ? resources.GetString("autoShutdownTimeElapsed.TextSeconds")!
                        .Replace(":value", timeLeftInSeconds.ToString())
                    : resources.GetString("autoShutdownTimeElapsed.TextSecond")!;
        }
    }
}
