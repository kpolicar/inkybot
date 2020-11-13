using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Exceptions;
using Inkybot.Services;


namespace Inkybot
{
    public partial class MainForm
    {
        private void MainFormEvents() {
            Closing += (sender, args) => {
                magingJob.StopMage();
                StopDebugging();
            };
        }
        
        private void MainForm_VisibleChanged(object sender, EventArgs e) {
            if (!Visible && magingJob.IsMaging)
                magingJob.StopMage();
        }
        
        private void toggleMageButton_Click(object sender, EventArgs e) {
            toastPanel.Hide();

            magingJob.BeginMage(!magingJob.IsMaging);
        }

        private void helpButton_Click(object sender, EventArgs e) {
            Process.Start($"{Server.BaseUrl}/release/{Program.VersionEndpoint}");
        }
        
        private void statsButton_Click(object sender, EventArgs e) {
            if (!statsForm.Visible) statsForm.Show();
            else statsForm.Hide();
        }
        
        private void configButton_Click(object sender, EventArgs e) {
            if (!configForm.Visible) configForm.Show();
            else configForm.Hide();
        }

        private void toastPanelCloseButton_Click(object sender, EventArgs e) {
            toastPanel.Hide();
        }

        private void debugScreenshotButton_Click(object sender, EventArgs e) {
            var takeScreenshot = new ThreadStart(delegate {
                var scan = new DofusScreenScan(hWndDocked, true);

                for (var numOfTries = 0; numOfTries < 3; numOfTries++) {
                    try {
                        scan.History();
                        scan.Stats();
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
    }
}
