using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Controls;
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
            var takeScreenshot = new ThreadStart(TakeScreenshotsAndOpenFolder);
            
            new Thread(takeScreenshot).Start();
            debugScreenshotButton.Enabled = false;
        }

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        private void TakeScreenshotsAndOpenFolder() {
            using var scan = new ScreenReaderDataProvider.DofusScreenScan(Program.Services, Measurements.HistoryBounds, true, true);
            var files = new List<string>();
            scan.Saved += (_, fileEvent) => files.Add(fileEvent.FullPath);
            scan.CaptureScreenshot();

            for (var numOfTries = 0; numOfTries < 3; numOfTries++) {
                try {
                    scan.MinMaxStats().Wait();
                    scan.History().Wait();
                    scan.Stats().Wait();
                    break;
                } catch (OcrEngineNotReadyYetException) {
                }
                numOfTries++;
            }

            Invoke(new MethodInvoker(delegate {
                debugScreenshotButton.Enabled = true;
            }));
                
            var folderPath = Path.Combine(AppContext.BaseDirectory, @"debug\images");
            folderPath = folderPath.Replace("/", "\\");
            try {
                WindowHelpers.OpenFolderAndSelectFiles(folderPath, files.Select(fullPath => fullPath.Replace("/", "\\")).ToArray());
            } catch (Exception e) {
                Debug.WriteLine(e);
            }
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

        private void EnqueueRectangle_AddToQueue(object sender, ControlEventArgs eventArgs) {
            var rectangle = (eventArgs.Control as EnqueueRectangle)!;
            mageQueue.Enqueue(rectangle, ocrIndicators[rectangle]);
        }

        private void EnqueueRectangle_RemoveFromQueue(object sender, ControlEventArgs eventArgs) {
            var rectangle = (eventArgs.Control as EnqueueRectangle)!;
            mageQueue.Remove(rectangle);
        }

        private void showMageQueueButton_Click(object sender, EventArgs e) {
            throw new NotImplementedException();
        }
    }
}
