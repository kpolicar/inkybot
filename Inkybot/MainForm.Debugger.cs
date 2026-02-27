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
using UserSettings = Inkybot.Properties.Settings;

namespace Inkybot
{
    public partial class MainForm
    {
        private ConcurrentDictionary<Control, Responsive.Measurement> ocrIndicators = new ConcurrentDictionary<Control, Responsive.Measurement>();
        
        private bool debugging;
        //private static Gma.System.MouseKeyHook.IKeyboardMouseEvents m_GlobalHook;
        private Control latestHistoryOcrIndicatorControl = null!;

        private void InitOcrIndicators() {
            RegisterOcrIndicator(Measurements.StatMinBounds);
            RegisterOcrIndicator(Measurements.StatMaxBounds);
            RegisterOcrIndicator(Measurements.StatValuesBounds);
            RegisterOcrIndicator(Measurements.InventoryAverageItemValueBounds);
            RegisterOcrIndicator(Measurements.InventorySearchTextBox);
            RegisterOcrIndicator(Program.Lang.TwoLetterISOLanguageName == "fr" ? Measurements.SinkFrMeasurement : Measurements.SinkMeasurement);

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

        private Control RegisterOcrIndicator(Responsive.Measurement measurement, bool crosshair=false) {
            Control control = crosshair
                ? new Crosshair()
                : new Rectangle();
            
            control.BackColor = System.Drawing.SystemColors.Control;
            control.ForeColor = System.Drawing.SystemColors.Control;
            Controls.Add(control);
            
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
            // ocrIndicators = new ConcurrentDictionary<Control, Responsive.Measurement>(); // todo temporary
            // InitOcrIndicators(); // todo temp
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
            ResizeEnd += onWindowResize;
            debugButton.Text = resources.GetString("debugButton.TextStop");
            
            ShowOcrIndicators();
            
            OnResizeBegin(EventArgs.Empty);
            OnResizeEnd(EventArgs.Empty);

            #if DEBUG
            var m_GlobalHook = Gma.System.MouseKeyHook.Hook.GlobalEvents();
            m_GlobalHook.MouseMove += GlobalHookMouseMoveExt;
            Closing += (sender, args) => m_GlobalHook.Dispose();
            #endif
        }

        private void FitOcrIndicatorRectangle(Control indicatorControl, Responsive.Measurement measurements) {
            var width = dofusClientPanel.Width;
            var height = dofusClientPanel.Height;
            
            var rect = Responsive.ResponsiveRectangle(measurements, width, height);
            rect.X -= 6;
            rect.Y -= 6;
            rect.X += dofusClientPanel.Location.X;
            rect.Width += 12;
            rect.Height += 12;
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
            ResizeEnd -= onWindowResize;
            
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
