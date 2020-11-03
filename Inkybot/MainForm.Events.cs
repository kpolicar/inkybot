using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Actions;


namespace Inkybot
{
    public partial class MainForm
    {
        private void MainFormEvents() {
            InitializeKeyboardShortcuts();
        }
        
        private void InitializeKeyboardShortcuts() {
            Closing += (sender, e) => {
                magingJob.StopMage();
            };
        }
        
        private void ocrIndicatorPanel_VisibleChanged(object sender, EventArgs e) {
            if (ocrIndicatorPanel.Visible)
                paintTimer.Start();
            else
                paintTimer.Stop();
        }

        private void ocrIndicatorPanel_Click(object sender, EventArgs e) {
            ocrIndicatorPanel.Hide();
        }

        private void MainForm_VisibleChanged(object sender, EventArgs e) {
            if (!Visible && magingJob.IsMaging)
                magingJob.StopMage();
        }
        
        private void toggleMageButton_Click(object sender, EventArgs e) {
            if (api.Connection == null) return;
            
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

        private void toastPanelCloseButton_Click(object sender, EventArgs e) {
            toastPanel.Hide();
        }

        private void debugScreenshotButton_Click(object sender, EventArgs e) {

            var takeScreenshot = new ThreadStart(delegate {
                var scan = new DofusScreenScan(hWndDocked, true);
                scan.History();
                scan.Stats();
            });
            
            new Thread(takeScreenshot).Start();
        }
    }
}
