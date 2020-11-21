using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Controls;
using Inkybot.Events;
using Inkybot.Helpers;
using Inkybot.Services;
using Tesseract;
using Debug = System.Diagnostics.Debug;

namespace Inkybot
{
    public partial class MainForm
    {
        private Dictionary<Rectangle, Responsive.Measurement> ocrIndicators = new Dictionary<Rectangle, Responsive.Measurement>();
        
        private bool debugging;
        #if DEBUG
        private Gma.System.MouseKeyHook.IKeyboardMouseEvents m_GlobalHook;
        #endif
        private Rectangle latestHistoryOcrIndicatorControl;

        private void InitOcrIndicators() {
            RegisterOcrIndicator(Measurements.StatMinBounds);
            RegisterOcrIndicator(Measurements.StatMaxBounds);
            RegisterOcrIndicator(Measurements.StatValuesBounds);
            //RegisterOcrIndicator(DofusScreenScan.ShortHistoryBoundsMeasurement);

            foreach (var runeBoundingBox in Measurements.RuneBoundsIndividualMeasurements) {
                RegisterOcrIndicator(runeBoundingBox);
            }
            latestHistoryOcrIndicatorControl = RegisterOcrIndicator(ScreenReaderDataProvider.DofusScreenScan.LatestHistoryBounds);

            ScreenReaderDataProvider.DofusScreenScan.LatestHistoryBoundsChanged += OnLatestHistoryBoundsChanged;
        }

        private void OnLatestHistoryBoundsChanged(object sender, ScanBoundsChanged e) {
            ocrIndicators[latestHistoryOcrIndicatorControl] = e.ScanBounds;
            BeginInvoke(new MethodInvoker(() => {
                FitOcrIndicatorRectangle(latestHistoryOcrIndicatorControl, ocrIndicators[latestHistoryOcrIndicatorControl]);
            }));
        }

        private Rectangle RegisterOcrIndicator(Responsive.Measurement measurement) {
            var control = new Rectangle();
            Controls.Add(control);
            control.BackColor = System.Drawing.SystemColors.Control;
            
            ocrIndicators.Add(control, measurement);
            return control;
        }

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
            foreach (var ocrIndicatorControl in ocrIndicators) {
                ocrIndicatorControl.Key.Show();
                ocrIndicatorControl.Key.BringToFront();
            }
        }

        private void HideOcrIndicators() {
            foreach (var ocrIndicatorControl in ocrIndicators) {
                ocrIndicatorControl.Key.Hide();
            }
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
            BeginInvoke(new MethodInvoker(() => {
                foreach (var ocrIndicatorControl in ocrIndicators) {
                    FitOcrIndicatorRectangle(ocrIndicatorControl.Key, ocrIndicatorControl.Value);
                }
            }));
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
