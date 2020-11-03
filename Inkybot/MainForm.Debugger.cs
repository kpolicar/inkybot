using System;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace Inkybot
{
    public partial class MainForm
    {
        private bool debugging;
        private IKeyboardMouseEvents m_GlobalHook;

        private void debugButton_Click(object sender, EventArgs e) {
            if (debugging = !debugging) {
                StartDebugging();
                debugButton.Text = "STOP DEBUG";
            } else {
                StopDebugging();
                debugButton.Text = "DEBUG";
            }
        }

        private void StartDebugging() {
            ocrIndicatorPanel.Show();
            sidebarPanel.BringToFront();
            ocrIndicatorPanel.BringToFront();
            mousePositionLabel.Show();
            debugScreenshotButton.Show();

            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.MouseMove += GlobalHookMouseMoveExt;
        }

        private void StopDebugging() {
            ocrIndicatorPanel.Hide();
            mousePositionLabel.Hide();
            debugScreenshotButton.Hide();
            m_GlobalHook.Dispose();
        }


        private void GlobalHookMouseMoveExt(object sender, MouseEventArgs e) {
            var full = dofusClientPanel.Size;
            var pos = dofusClientPanel.PointToClient(e.Location);
            if (pos.X < 0 || pos.X > dofusClientPanel.Size.Width || pos.Y < 0 || pos.Y > dofusClientPanel.Size.Height)
                return;
            mousePositionLabel.Text = $@"x: {pos.X}, y: {pos.Y}"+"\n";
            mousePositionLabel.Text += $@"w: {full.Width}, h: {full.Height}";
        }
    }
}
