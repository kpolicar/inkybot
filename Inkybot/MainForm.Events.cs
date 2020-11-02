using System;
using System.Diagnostics;
using System.Windows.Forms;


namespace Inkybot
{
    public partial class MainForm
    {
        private void MainFormEvents() {
            InitializeKeyboardShortcuts();
        }
        
        private void InitializeKeyboardShortcuts() {
            KeyboardHook.Init();
            KeyboardHook.KeyPressed += (sender, e) => {
                if (e.KeyCode == Keys.F2)
                    toggleMageButton_Click(sender, e);
            };

            Closing += (sender, e) => {
                KeyboardHook.Release();
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
            
            magingJob.BeginMage(!magingJob.IsMaging);
        }

        private void helpButton_Click(object sender, EventArgs e) {
            Process.Start($"{Server.BaseUrl}/release/{Program.VersionEndpoint}");
        }
    }
}
