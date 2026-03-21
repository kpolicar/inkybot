using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
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
        private readonly List<(Control control, int column, int row)> runeIndicators = new List<(Control, int, int)>();
        private Control statMinIndicator = null!;
        private Control statMaxIndicator = null!;
        private Control statValuesIndicator = null!;

        private bool debugging;
        //private static Gma.System.MouseKeyHook.IKeyboardMouseEvents m_GlobalHook;
        private Control latestHistoryOcrIndicatorControl = null!;
        private System.Windows.Forms.Timer _rowDetectionDebounceTimer = null!;

        private void InitOcrIndicators() {
            statMinIndicator = RegisterOcrIndicator(Measurements.StatColumnBounds(Measurements.StatMinBounds));
            statMaxIndicator = RegisterOcrIndicator(Measurements.StatColumnBounds(Measurements.StatMaxBounds));
            statValuesIndicator = RegisterOcrIndicator(Measurements.StatColumnBounds(Measurements.StatValuesBounds));

            RegisterOcrIndicator(Measurements.InventoryAverageItemValueBounds);
            RegisterOcrIndicator(Measurements.InventorySearchTextBox);
            RegisterOcrIndicator(Program.Lang.TwoLetterISOLanguageName == "fr" ? Measurements.SinkFrMeasurement : Measurements.SinkMeasurement);

            for (var col = 0; col < 3; col++) {
                for (var row = 0; row < 13; row++) {
                    var control = RegisterOcrIndicator(Measurements.RuneBoxBounds(col, row));
                    runeIndicators.Add((control, col, row));
                }
            }

            latestHistoryOcrIndicatorControl = RegisterOcrIndicator(screenReader.LatestHistoryBounds);
            screenReader.LatestHistoryBoundsChanged += OnLatestHistoryProcessed;

            var rowDetector = Program.Services.GetService<RowSpacingDetector>();
            rowDetector.RowHeightChanged += OnRowHeightChanged;

            _rowDetectionDebounceTimer = new System.Windows.Forms.Timer { Interval = 750 };
            _rowDetectionDebounceTimer.Tick += OnRowDetectionDebounceTimerTick;
        }

        private void OnRowHeightChanged(object sender, EventArgs e) {
            BeginInvoke(new MethodInvoker(() => {
                foreach (var (control, col, row) in runeIndicators) {
                    var m = Measurements.RuneBoxBounds(col, row);
                    ocrIndicators[control] = m;
                    if (debugging) FitOcrIndicatorRectangle(control, m);
                }
                UpdateStatColumnIndicator(statMinIndicator, Measurements.StatMinBounds);
                UpdateStatColumnIndicator(statMaxIndicator, Measurements.StatMaxBounds);
                UpdateStatColumnIndicator(statValuesIndicator, Measurements.StatValuesBounds);
            }));
        }

        private void UpdateStatColumnIndicator(Control control, Responsive.Measurement fullBounds) {
            var clamped = Measurements.StatColumnBounds(fullBounds);
            ocrIndicators[control] = clamped;
            if (debugging) FitOcrIndicatorRectangle(control, clamped);
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

            // Trigger immediate row height detection so indicators are accurate from the start
            _rowDetectionDebounceTimer.Stop();
            _rowDetectionDebounceTimer.Start();

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
            int padding = indicatorControl is EnqueueRectangle ? 2
                : IsRowIndicator(indicatorControl) ? 0 : 6;
            rect.X -= padding;
            rect.Y -= padding;
            rect.X += dofusClientPanel.Location.X;
            rect.Width += padding * 2;
            rect.Height += padding * 2;
            if (indicatorControl is Crosshair) {
                rect.X += rect.Width / 2;
                rect.Y += rect.Height / 2;
                rect.Width = rect.Height;
            }
            indicatorControl.Bounds = rect;
        }

        private bool IsRowIndicator(Control c) {
            if (c == statMinIndicator || c == statMaxIndicator || c == statValuesIndicator) return true;
            foreach (var (control, _, _) in runeIndicators) if (control == c) return true;
            return false;
        }

        private void onWindowResize(object sender, EventArgs e) {
            BeginInvoke(new MethodInvoker(() => {
                foreach (var ocrIndicatorControl in ocrIndicators) {
                    FitOcrIndicatorRectangle(ocrIndicatorControl.Key, ocrIndicatorControl.Value);
                }
            }));

            // Debounce: restart 750ms timer to trigger row height re-detection
            _rowDetectionDebounceTimer.Stop();
            _rowDetectionDebounceTimer.Start();
        }

        private void OnRowDetectionDebounceTimerTick(object sender, EventArgs e) {
            _rowDetectionDebounceTimer.Stop();
            new Thread(() => {
                try {
                    using var scan = new ScreenReaderDataProvider.DofusScreenScan(
                        Program.Services, Measurements.HistoryBounds, false, true);
                    scan.CaptureScreenshot();
                    scan.MinMaxStats().Wait();
                } catch (Exception ex) {
                    Debug.WriteLine("Row detection on resize failed: " + ex.Message);
                }
            }).Start();
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
            _rowDetectionDebounceTimer.Stop();

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
