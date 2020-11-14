using System;
using System.Diagnostics;
using System.Windows.Forms;
using Inkybot.Helpers;
using Debug = System.Diagnostics.Debug;

namespace Inkybot
{
    public partial class MainForm
    {
        private bool debugging;
        #if DEBUG
        private Gma.System.MouseKeyHook.IKeyboardMouseEvents m_GlobalHook;
        #endif

        private void debugButton_Click(object sender, EventArgs e) {
            if (debugging = !debugging) {
                StartDebugging();
                debugButton.Text = resources.GetString("debugButton.TextStop");
            } else {
                StopDebugging();
                debugButton.Text = resources.GetString("debugButton.Text");
            }
        }

        private void StartDebugging() {
            sidebarPanel.BringToFront();
            #if DEBUG
            mousePositionLabel.Show();
            #endif
            debugScreenshotButton.Show();
            Resize += onWindowResize;
            
            historyOcrIndicatorRectangle.Show();
            statsOcrIndicatorRectangle.Show();
            
            historyOcrIndicatorRectangle.BringToFront();
            statsOcrIndicatorRectangle.BringToFront();
            
            OnResize(EventArgs.Empty);

            #if DEBUG
            m_GlobalHook = Gma.System.MouseKeyHook.Hook.GlobalEvents();
            m_GlobalHook.MouseMove += GlobalHookMouseMoveExt;
            #endif
        }

        private void onWindowResize(object sender, EventArgs e) {
            var width = dofusClientPanel.Width;
            var height = dofusClientPanel.Height;
            
            var statRect = Responsive.ResponsiveRectangle(DofusScreenScan.StatBoundsMeasurement, width, height);
            statRect.X -= 2;
            statRect.Y -= 2;
            statRect.Width += 4;
            statRect.Height += 4;
            var historyRect = Responsive.ResponsiveRectangle(DofusScreenScan.HistoryBoundsMeasurement, width, height);
            historyRect.X -= 2;
            historyRect.Y -= 2;
            historyRect.Width += 4;
            historyRect.Height += 4;

            statsOcrIndicatorRectangle.Bounds = statRect;
            historyOcrIndicatorRectangle.Bounds = historyRect;
        }
        
        private void StopDebugging() {
            
            mousePositionLabel.Hide();
            debugScreenshotButton.Hide();
            #if DEBUG
            m_GlobalHook.Dispose();
            #endif
            Resize -= onWindowResize;
            
            historyOcrIndicatorRectangle.Hide();
            statsOcrIndicatorRectangle.Hide();
        }


        #if DEBUG
        private void GlobalHookMouseMoveExt(object sender, MouseEventArgs e) {
            var full = dofusClientPanel.Size;
            var pos = dofusClientPanel.PointToClient(e.Location);
            if (pos.X < 0 || pos.X > dofusClientPanel.Size.Width || pos.Y < 0 || pos.Y > dofusClientPanel.Size.Height)
                return;
            mousePositionLabel.Text = $@"x: {pos.X}, y: {pos.Y}"+"\n";
            mousePositionLabel.Text += $@"w: {full.Width}, h: {full.Height}";
        }
        #endif
    }
}
