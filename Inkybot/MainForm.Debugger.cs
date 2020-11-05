using System;
using System.Diagnostics;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using Inkybot.Helpers;
using Debug = System.Diagnostics.Debug;

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
            sidebarPanel.BringToFront();
            mousePositionLabel.Show();
            debugScreenshotButton.Show();
            Resize += onWindowResize;
            
            ocrIndicatorStatsRectangleLeftVertical.Show();
            ocrIndicatorStatsRectangleRightVertical.Show();
            ocrIndicatorStatsRectangleTopHorizontal.Show();
            ocrIndicatorStatsRectangleBottomHorizontal.Show();
            ocrIndicatorHistoryRectangleLeftVertical.Show();
            ocrIndicatorHistoryRectangleRightVertical.Show();
            ocrIndicatorHistoryRectangleTopHorizontal.Show();
            ocrIndicatorHistoryRectangleBottomHorizontal.Show();
            
            ocrIndicatorStatsRectangleLeftVertical.BringToFront();
            ocrIndicatorStatsRectangleRightVertical.BringToFront();
            ocrIndicatorStatsRectangleTopHorizontal.BringToFront();
            ocrIndicatorStatsRectangleBottomHorizontal.BringToFront();
            ocrIndicatorHistoryRectangleLeftVertical.BringToFront();
            ocrIndicatorHistoryRectangleRightVertical.BringToFront();
            ocrIndicatorHistoryRectangleTopHorizontal.BringToFront();
            ocrIndicatorHistoryRectangleBottomHorizontal.BringToFront();
            
            OnResize(EventArgs.Empty);

            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.MouseMove += GlobalHookMouseMoveExt;
        }

        private void onWindowResize(object sender, EventArgs e) {
            var width = dofusClientPanel.Width;
            var height = dofusClientPanel.Height;
            var statRect = Responsive.ResponsiveRectangle(DofusScreenScan.StatBoundsMeasurement, width, height);
            var historyRect = Responsive.ResponsiveRectangle(DofusScreenScan.HistoryBoundsMeasurement, width, height);

            ocrIndicatorStatsRectangleLeftVertical.Top = statRect.Top;
            ocrIndicatorStatsRectangleLeftVertical.Left = statRect.Left;
            ocrIndicatorStatsRectangleLeftVertical.Height = statRect.Height;
            
            ocrIndicatorStatsRectangleRightVertical.Top = statRect.Top;
            ocrIndicatorStatsRectangleRightVertical.Left = statRect.Right;
            ocrIndicatorStatsRectangleRightVertical.Height = statRect.Height;
            
            ocrIndicatorStatsRectangleTopHorizontal.Top = statRect.Top;
            ocrIndicatorStatsRectangleTopHorizontal.Left = statRect.Left;
            ocrIndicatorStatsRectangleTopHorizontal.Width = statRect.Width;
            
            ocrIndicatorStatsRectangleBottomHorizontal.Top = statRect.Bottom;
            ocrIndicatorStatsRectangleBottomHorizontal.Left = statRect.Left;
            ocrIndicatorStatsRectangleBottomHorizontal.Width = statRect.Width+3;

            ocrIndicatorHistoryRectangleLeftVertical.Top = historyRect.Top;
            ocrIndicatorHistoryRectangleLeftVertical.Left = historyRect.Left;
            ocrIndicatorHistoryRectangleLeftVertical.Height = historyRect.Height;
            
            ocrIndicatorHistoryRectangleRightVertical.Top = historyRect.Top;
            ocrIndicatorHistoryRectangleRightVertical.Left = historyRect.Right;
            ocrIndicatorHistoryRectangleRightVertical.Height = historyRect.Height;
            
            ocrIndicatorHistoryRectangleTopHorizontal.Top = historyRect.Top;
            ocrIndicatorHistoryRectangleTopHorizontal.Left = historyRect.Left;
            ocrIndicatorHistoryRectangleTopHorizontal.Width = historyRect.Width;
            
            ocrIndicatorHistoryRectangleBottomHorizontal.Top = historyRect.Bottom;
            ocrIndicatorHistoryRectangleBottomHorizontal.Left = historyRect.Left;
            ocrIndicatorHistoryRectangleBottomHorizontal.Width = historyRect.Width+3;
        }
        
        private void StopDebugging() {
            mousePositionLabel.Hide();
            debugScreenshotButton.Hide();
            m_GlobalHook.Dispose();
            Resize -= onWindowResize;
            
            ocrIndicatorStatsRectangleLeftVertical.Hide();
            ocrIndicatorStatsRectangleRightVertical.Hide();
            ocrIndicatorStatsRectangleTopHorizontal.Hide();
            ocrIndicatorStatsRectangleBottomHorizontal.Hide();
            ocrIndicatorHistoryRectangleLeftVertical.Hide();
            ocrIndicatorHistoryRectangleRightVertical.Hide();
            ocrIndicatorHistoryRectangleTopHorizontal.Hide();
            ocrIndicatorHistoryRectangleBottomHorizontal.Hide();
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
