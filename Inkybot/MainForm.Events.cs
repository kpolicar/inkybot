using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Controls;
using Inkybot.Domain;
using Inkybot.Exceptions;
using Inkybot.Helpers;
using Inkybot.Services;
using Inkybot.Services.Win32Input;
using Debug = System.Diagnostics.Debug;
using Timer = System.Windows.Forms.Timer;


namespace Inkybot
{
    public partial class MainForm
    {
        private ConcurrentDictionary<EnqueueRectangle, Responsive.Measurement> queueControls = new ConcurrentDictionary<EnqueueRectangle, Responsive.Measurement>();
        
        
        public bool HasManuallyStoppedMaging { get; set; }
        
        private void MainFormEvents() {
            Closing += (sender, args) => {
                magingJob.StopMage();
                StopDebugging();
                ReleaseDofusClaim();
                if (pDofus != null) {
                    WindowHelpers.RestoreWindowBorders(hWndDocked);
                    WindowHelpers.UndockProcess(pDofus.MainWindowHandle, parentHandle);
                    //m_GlobalHook.Dispose();
                }
            };

            ResizeEnd += OnWindowResize_FitQueueControls;
            mageQueueForm.VisibleChanged += MageQueueFormVisibleChanged;

            foreach (var inventoryBoundingBox in Measurements.InventoryBoundsIndividualMeasurements) {
                RegisterQueueControl(inventoryBoundingBox);
            }
        }
        private void ShowQueueControls() {
            
            foreach (var control in queueControls) {
                control.Key.Show();
                control.Key.BringToFront();
            }

            foreach (var mageQueueItem in mageQueue.Queue) {
                mageQueueItem.Control.BringToFront();
            }
            OnResizeEnd(EventArgs.Empty);
        }

        private void HideQueueControls() {
            foreach (var control in queueControls) {
                control.Key.Hide();
            }
        }

        private void MageQueueFormVisibleChanged(object sender, EventArgs e) {
            if (mageQueueForm.Visible && !magingJob.IsMaging)
                ShowQueueControls();
            else {
                HideQueueControls();
            }
        }

        private void OnWindowResize_FitQueueControls(object sender, EventArgs e) =>
            BeginInvoke(new MethodInvoker(() => {
                foreach (var control in queueControls) {
                    FitOcrIndicatorRectangle(control.Key, control.Value);
                }
            }));

        private EnqueueRectangle RegisterQueueControl(Responsive.Measurement measurement) {
            var control = new EnqueueRectangle() {
                Visible = false
            };
            control.EditConfigMenuItem.Text = resources.GetString("enqueueRectangle.EditConfigText");
            control.AddToQueueMenuItem.Text = resources.GetString("enqueueRectangle.AddToQueueText");
            control.RemoveFromQueueMenuItem.Text = resources.GetString("enqueueRectangle.RemoveFromQueueText");
            control.Tooltip = resources.GetString("enqueueRectangle.Tooltip")!;
            queueControls[control] = measurement;
            control.BackColor = System.Drawing.SystemColors.Control;
            control.ForeColor = System.Drawing.SystemColors.Control;
            Controls.Add(control);
            
            control.AddToQueueMenuItem.Click +=
                (sender, _) => EnqueueRectangle_AddToQueue(sender, new ControlEventArgs(control));
            control.RemoveFromQueueMenuItem.Click +=
                (sender, _) => EnqueueRectangle_RemoveFromQueue(sender, new ControlEventArgs(control));
            control.EditConfigMenuItem.Click +=
                (sender, _) => EnqueueRectangle_Edit(sender, new ControlEventArgs(control));

            return control;
        }
        
        private void MainForm_VisibleChanged(object sender, EventArgs e) {
            if (!Visible && magingJob.IsMaging)
                magingJob.StopMage();
            if (!Visible) {
                setupForm.Hide();
                configForm.Hide();
                statisticsForm.Hide();
            }
        }
        
        private void toggleMageButton_Click(object sender, EventArgs e) {
            // configForm.MinimumSize = new Size(720, 640);
            // configForm.Size = new Size(720, 640);
            // setupForm.MinimumSize = new Size(720, 640);
            // setupForm.Size = new Size(720, 640);
            // MinimumSize = new Size(1920, 1080);
            // Size = new Size(1280, 720);
            MetricsLogger.Track(magingJob.IsMaging ? "ui_mage_stop_clicked" : "ui_mage_start_clicked");
            toastPanel.Hide();
            StopAutoShutdownCounter();

            magingJob.BeginMage(!magingJob.IsMaging);
            if (!magingJob.IsMaging)
                HasManuallyStoppedMaging = true;
        }

        private void helpButton_Click(object sender, EventArgs e) {
            MetricsLogger.Track("ui_help_opened");
            Process.Start(Server.UsageInstructions);
        }

        private void statsButton_Click(object sender, EventArgs e) {
            MetricsLogger.Track("ui_setup_form_opened");
            if (!setupForm.Visible) setupForm.Show();
            else setupForm.Focus();
        }

        private void configButton_Click(object sender, EventArgs e) {
            MetricsLogger.Track("ui_config_form_opened");
            if (!configForm.Visible) configForm.Show();
            else configForm.Focus();
        }

        private void toastPanelCloseButton_Click(object sender, EventArgs e) {
            toastPanel.Hide();
        }

        private void debugScreenshotButton_Click(object sender, EventArgs e) {
            MetricsLogger.Track("ui_debug_screenshot");
            var takeScreenshot = new ThreadStart(TakeScreenshotsAndOpenFolder);
            
            new Thread(takeScreenshot).Start();
            debugScreenshotButton.Enabled = false;
        }

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        private void TakeScreenshotsAndOpenFolder() {
            try {
                using var scan =
                    new ScreenReaderDataProvider.DofusScreenScan(Program.Services, Measurements.HistoryBounds, true,
                        true);
                var files = new List<string>();
                scan.Saved += (_, fileEvent) => files.Add(fileEvent.FullPath);
                scan.CaptureScreenshot();

                for (var numOfTries = 0; numOfTries < 3; numOfTries++) {
                    try {
                        scan.MinMaxStats().Wait();
                        scan.History().Wait();
                        scan.Stats().Wait();
                        scan.Sink().Wait();
                        break;
                    } catch (OcrEngineNotReadyYetException) {
                    }

                    numOfTries++;
                }

                Invoke(new MethodInvoker(delegate { debugScreenshotButton.Enabled = true; }));

                var folderPath = Path.Combine(AppContext.BaseDirectory, @"debug\images");
                folderPath = folderPath.Replace("/", "\\");
                WindowHelpers.OpenFolderAndSelectFiles(folderPath,
                    files.Select(fullPath => fullPath.Replace("/", "\\")).ToArray());
            } catch (Exception e) {
                Invoke(new MethodInvoker(delegate { debugScreenshotButton.Enabled = true; }));
                Debug.WriteLine(e);
            }
        }

        private void hallOfFameButton_Click(object sender, EventArgs e) {
            MetricsLogger.Track("ui_hall_of_fame_opened");
            Process.Start(Server.HallOfFameUrl);
        }

        private void statisticsButton_Click(object sender, EventArgs e) {
            if (auth.User != null && !auth.User.canViewStatistics) {
                var text = !auth.User.onUnlimitedPlan && !auth.User.onStandardPlan
                    ? resources.GetString("popup.error_notavailable_unlimitedstandard_plan")
                    : resources.GetString("popup.error_notavailable_current_plan");
                
                MessageBox.Show(
                    text,
                    resources.GetString("popup.error_restricted"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            
            MetricsLogger.Track("ui_statistics_form_opened");
            if (!statisticsForm.Visible) statisticsForm.Show();
            else statisticsForm.Focus();
        }

        private void subscribePlanUpgradeLinkLabel_OnClick(object sender, EventArgs e) {
            Process.Start(Server.SubscribeUrl);
        }

        private void shutdownToastPanelCloseButton_Click(object sender, EventArgs e) {
            StopAutoShutdownCounter();
        }

        private void OnResize(object sender, EventArgs e) {
            this.shutdownToastPanel.Location = 
                new Point(ClientSize.Width / 2 - shutdownToastPanel.Size.Width / 2, 
                    ClientSize.Height / 2 - shutdownToastPanel.Size.Height);
        }

        private void OnAutoShutdownTimer(object sender, EventArgs e) {
            autoShutdownTimeElapsed += autoShutdownTimer.Interval;
            RefreshAutoShutdownLabels();
            var timeLeft = configForm.AutoShutdownDelay - autoShutdownTimeElapsed;
            if (timeLeft <= 0) {
                Application.Exit();
            }
        }

        private void RefreshAutoShutdownLabels() {
            var timeLeft = configForm.AutoShutdownDelay - autoShutdownTimeElapsed;
            var timeLeftInSeconds = timeLeft / 1000;
            
            shutdownToastValueLabel.Text = timeLeftInSeconds >= 60
                ? resources.GetString("autoShutdownTimeElapsed.TextMinutes")!
                    .Replace(":value", Math.Max(2, timeLeftInSeconds / 60).ToString())
                : timeLeftInSeconds > 1
                    ? resources.GetString("autoShutdownTimeElapsed.TextSeconds")!
                        .Replace(":value", timeLeftInSeconds.ToString())
                    : resources.GetString("autoShutdownTimeElapsed.TextSecond")!;
        }

        private void EnqueueRectangle_AddToQueue(object sender, ControlEventArgs eventArgs) {
            if (auth.User?.canUseMageQueue ?? false) {
                MetricsLogger.Track("ui_queue_item_added");
                var rectangle = (eventArgs.Control as EnqueueRectangle)!;
                Task.Run(() => mageQueue.Enqueue(rectangle, queueControls[rectangle]));
            } else {
                ShowMagingQueueRestrictedPopup();
            }
        }

        private void EnqueueRectangle_RemoveFromQueue(object sender, ControlEventArgs eventArgs) {
            MetricsLogger.Track("ui_queue_item_removed");
            var rectangle = (eventArgs.Control as EnqueueRectangle)!;
            mageQueue.Remove(rectangle);
        }

        private void showMageQueueButton_Click(object sender, EventArgs e) {
            MetricsLogger.Track("ui_mage_queue_opened");
            Task.Run(async () => {
                if (config.UserSettings.EnableSafeMageQueueing) {
                    actions.Execute(actionFactory.InventorySelectAllAction(), true);
                    await Task.Delay(500);
                }
                actions.Execute(actionFactory.InventorySelectEquipmentAction(), true);
                await Task.Delay(50);
                win32Input.ReleaseCursor();
            });
            
            if ((auth.User?.canUseMageQueue ?? false) || !mageQueue.Empty) {
                if (!mageQueueForm.Visible) mageQueueForm.Show();
                else mageQueueForm.Focus();
            } else {
                ShowMagingQueueRestrictedPopup();
            }
        }

        private void ShowMagingQueueRestrictedPopup() {
            var text = resources.GetString("popup.error_notavailable_current_plan");
                
            MessageBox.Show(
                text,
                resources.GetString("popup.error_restricted"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void MainForm_Loaded(object sender, EventArgs e) {
            _ = mageQueueForm.Handle;
            _ = configForm.Handle;
            _ = setupForm.Handle;
        }

        private void EnqueueRectangle_Edit(object sender, ControlEventArgs e) {
            var rectangle = (e.Control as EnqueueRectangle)!;
            if (!mageQueueForm.Visible) mageQueueForm.Show();
            else mageQueueForm.Focus();
            
            var mageQueueItem =  mageQueue.Queue.Find(item => item.Control.Equals(rectangle));
            _ = mageQueueForm.Highlight(mageQueueItem);
        }

        private void kamasSpentValueResetButton_Click(object sender, EventArgs e) {
            MetricsLogger.Track("ui_kamas_counter_reset");
            (magingJob as ScreenReaderDofusMagingJob)?.ResetBalance();
            kamasSpentValueLabel.Text = resources.GetString("kamasSpentValueLabel.Text");
        }

        private void exoAttemptsValueResetButton_Click(object sender, EventArgs e) {
            MetricsLogger.Track("ui_exo_counter_reset");
            BeginInvoke(new MethodInvoker(() => {
                exoAttemptsValueLabel.Text = "0";
            }));
        }
        
        private void OnKamasSpentValueResetButtonPaint(object sender, PaintEventArgs e) {
            base.OnPaint(e);
            var format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            e.Graphics.DrawString(
                "⟲",
                kamasSpentValueResetButton.Font,
                new SolidBrush(kamasSpentValueResetButton.ForeColor),
                kamasSpentValueResetButton.ClientRectangle,
                format);
        }
        private void OnExoAttemptsValueResetButtonPaint(object sender, PaintEventArgs e) {
            base.OnPaint(e);
            var format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            e.Graphics.DrawString(
                "⟲",
                exoAttemptsValueResetButton.Font,
                new SolidBrush(exoAttemptsValueResetButton.ForeColor),
                exoAttemptsValueResetButton.ClientRectangle,
                format);
        }
    }
}
