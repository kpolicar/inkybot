using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Controls;
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

        private void OnScreenshotStart(object sender, EventArgs eventArgs) {
            if (!debugging) return;
            
            Invoke(new MethodInvoker(() => {
                HideOcrIndicators();
            }));
            Thread.Sleep(50);
        }

        private void OnScreenshotEnd(object sender, EventArgs eventArgs) {
            if (!debugging) return;
            
            BeginInvoke(new MethodInvoker(() => {
                ShowOcrIndicators();
            }));
        }

        private void ShowOcrIndicators() {
            historyOcrIndicatorRectangle.Show();
            shortHistoryOcrIndicatorRectangle.Show();
            statsValuesOcrIndicatorRectangle.Show();
            statsMaxesOcrIndicatorRectangle.Show();
            statsMinsOcrIndicatorRectangle.Show();
            
            historyOcrIndicatorRectangle.BringToFront();
            shortHistoryOcrIndicatorRectangle.BringToFront();
            statsValuesOcrIndicatorRectangle.BringToFront();
            statsMaxesOcrIndicatorRectangle.BringToFront();
            statsMinsOcrIndicatorRectangle.BringToFront();
        }

        private void HideOcrIndicators() {
            historyOcrIndicatorRectangle.Hide();
            shortHistoryOcrIndicatorRectangle.Hide();
            statsValuesOcrIndicatorRectangle.Hide();
            statsMaxesOcrIndicatorRectangle.Hide();
            statsMinsOcrIndicatorRectangle.Hide();
        }

        private void StartDebugging() {
            sidebarPanel.BringToFront();
            #if DEBUG
            mousePositionLabel.Show();
            #endif
            debugScreenshotButton.Show();
            Resize += onWindowResize;
            
            ShowOcrIndicators();
            
            OnResize(EventArgs.Empty);

            #if DEBUG
            m_GlobalHook = Gma.System.MouseKeyHook.Hook.GlobalEvents();
            m_GlobalHook.MouseMove += GlobalHookMouseMoveExt;
            #endif
        }

        private void FitOcrIndicatorRectangle(Rectangle indicatorControl, Responsive.Measurement measurements) {
            var width = dofusClientPanel.Width;
            var height = dofusClientPanel.Height;
            
            var rect = Responsive.ResponsiveRectangle(measurements, width, height);
            rect.X -= 2;
            rect.Y -= 2;
            rect.Width += 4;
            rect.Height += 4;
            indicatorControl.Bounds = rect;
        }

        private void onWindowResize(object sender, EventArgs e) {

            FitOcrIndicatorRectangle(statsMinsOcrIndicatorRectangle, DofusScreenScan.StatMinBoundsMeasurement);
            FitOcrIndicatorRectangle(statsMaxesOcrIndicatorRectangle, DofusScreenScan.StatMaxBoundsMeasurement);
            FitOcrIndicatorRectangle(statsValuesOcrIndicatorRectangle, DofusScreenScan.StatValuesBoundsMeasurement);
            FitOcrIndicatorRectangle(historyOcrIndicatorRectangle, DofusScreenScan.HistoryBoundsMeasurement);
            FitOcrIndicatorRectangle(shortHistoryOcrIndicatorRectangle, DofusScreenScan.ShortHistoryBoundsMeasurement);
        }
        
        private void StopDebugging() {
            
            mousePositionLabel.Hide();
            debugScreenshotButton.Hide();
            #if DEBUG
            m_GlobalHook?.Dispose();
            #endif
            Resize -= onWindowResize;
            
            HideOcrIndicators();
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
