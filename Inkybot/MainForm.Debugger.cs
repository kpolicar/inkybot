using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Controls;
using Inkybot.Events;
using Inkybot.Helpers;
using Inkybot.Services;
using Debug = System.Diagnostics.Debug;
using Rectangle = Inkybot.Controls.Rectangle;

namespace Inkybot
{
    public partial class MainForm
    {
        private ConcurrentDictionary<Control, Responsive.Measurement> ocrIndicators = new ConcurrentDictionary<Control, Responsive.Measurement>();
        private ConcurrentQueue<Control> queuedItemsIndicators = new ConcurrentQueue<Control>();
        
        private bool debugging;
        #if DEBUG
        private Gma.System.MouseKeyHook.IKeyboardMouseEvents m_GlobalHook;
        #endif
        private Control latestHistoryOcrIndicatorControl = null!;

        private void InitOcrIndicators() {
            RegisterOcrIndicator(Measurements.StatMinBounds);
            RegisterOcrIndicator(Measurements.StatMaxBounds);
            RegisterOcrIndicator(Measurements.StatValuesBounds);
            RegisterOcrIndicator(Measurements.InventoryAverageItemValueBounds);
            RegisterOcrIndicator(Measurements.InventorySearchTextBox);

            foreach (var runeBoundingBox in Measurements.RuneBoundsIndividualMeasurements) {
                RegisterOcrIndicator(runeBoundingBox);
            }
            latestHistoryOcrIndicatorControl = RegisterOcrIndicator(screenReader.LatestHistoryBounds);

            screenReader.LatestHistoryBoundsChanged += OnLatestHistoryProcessed;
        }

        private void OnLatestHistoryProcessed(object sender, ScanBoundsChanged e) {
            ocrIndicators[latestHistoryOcrIndicatorControl] = e.ScanBounds;
            BeginInvoke(new MethodInvoker(() => {
                FitOcrIndicatorRectangle(latestHistoryOcrIndicatorControl, ocrIndicators[latestHistoryOcrIndicatorControl]);
            }));
        }

        private void AddQueuedItemIndicator() {
            var measurement = Measurements.InventoryBoundsIndividualMeasurements
                .Skip(queuedItemsIndicators.Count)
                .First();

            var control = (Rectangle) RegisterOcrIndicator(measurement);

            control.BorderWidth = 4;
            control.BackColor = control.ForeColor = Color.SeaGreen;
            control.Show();
            control.BringToFront();
            OnResize(EventArgs.Empty);

            foreach (var queuedItemsIndicator in queuedItemsIndicators) {
                (queuedItemsIndicator as Rectangle)!.BorderWidth = 2;
                queuedItemsIndicator.BackColor = queuedItemsIndicator.ForeColor = System.Drawing.SystemColors.Control;
            }
            queuedItemsIndicators.Enqueue(control);
        }

        private Control RegisterOcrIndicator(Responsive.Measurement measurement, bool crosshair=false) {
            Control control;
            if (crosshair) {
                control = new Crosshair();
            } else {
                control = new Rectangle();
            }
            Controls.Add(control);
            control.BackColor = System.Drawing.SystemColors.Control;
            control.ForeColor = System.Drawing.SystemColors.Control;
            
            ocrIndicators[control] = measurement;
            return control;
        }

        private void debugButton_Click(object sender, EventArgs e) {
            if (debugging = !debugging) {
                StartDebugging();
            } else {
                StopDebugging();
            }
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
            debugging = true;
            #if DEBUG
            mousePositionLabel.Show();
            #endif
            debugScreenshotButton.Show();
            Resize += onWindowResize;
            debugButton.Text = resources.GetString("debugButton.TextStop");
            
            ShowOcrIndicators();
            
            OnResize(EventArgs.Empty);

            #if DEBUG
            m_GlobalHook = Gma.System.MouseKeyHook.Hook.GlobalEvents();
            m_GlobalHook.MouseMove += GlobalHookMouseMoveExt;
            #endif
        }

        private void FitOcrIndicatorRectangle(Control indicatorControl, Responsive.Measurement measurements) {
            var width = dofusClientPanel.Width;
            var height = dofusClientPanel.Height;
            
            var rect = Responsive.ResponsiveRectangle(measurements, width, height);
            rect.X -= 2;
            rect.Y -= 2;
            rect.X += dofusClientPanel.Location.X;
            rect.Width += 4;
            rect.Height += 4;
            if (indicatorControl is Crosshair) {
                rect.X += rect.Width / 2;
                rect.Y += rect.Height / 2;
                rect.Width = rect.Height;
            }
            indicatorControl.Bounds = rect;
        }

        private void onWindowResize(object sender, EventArgs e) {
            BeginInvoke(new MethodInvoker(() => {
                foreach (var ocrIndicatorControl in ocrIndicators) {
                    FitOcrIndicatorRectangle(ocrIndicatorControl.Key, ocrIndicatorControl.Value);
                }
            }));
        }

        private void DisableDebugging() {
            if (debugging)
                StopDebugging();
            debugButton.Enabled = false;
        }

        private void EnableDebugging() =>
            debugButton.Enabled = true;
        
        private void StopDebugging() {
            debugging = false;
            debugButton.Text = resources.GetString("debugButton.Text");
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
