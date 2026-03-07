using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Inkybot.Contracts;
using Inkybot.Dofus.Contracts;
using Inkybot.Domain;
using Inkybot.Events;
using Inkybot.Services;
using Inkybot.Services.Win32Input;
using UserSettings = Inkybot.Properties.Settings;

namespace Inkybot
{
    public partial class MainForm
    {
        private Process? pDofus;
        private IntPtr hWndDocked;
        private IntPtr parentHandle;

        private bool InitializeDofusClient() {
            if (pDofus != null && !pDofus.HasExited) {
                pDofus.Kill();
                hWndDocked = IntPtr.Zero;
            }

            var waitingForm = new WaitingForDofusForm();
            waitingForm.SelectedDofusProcess += OnDofusProcessSelected;

            Task.Run(async () => {
                try {
                    await WaitForDofusProcessLoop(waitingForm);
                    waitingForm.Invoke(new MethodInvoker(() => {
                        waitingForm.DialogResult = DialogResult.OK;
                    }));
                } catch (Exception e) {
                    Debug.WriteLine(e);
                }
            });
            
            var resultWaiting = waitingForm.ShowDialog(this);
            if (resultWaiting != DialogResult.OK || pDofus == null) {
                return false;
            }

            parentHandle = WindowHelpers.DockProcess(pDofus!, dofusClientPanel, ref hWndDocked);
            FileEventLogger.SystemLogger.Info("Dofus window title: " + pDofus!.MainWindowTitle);
            WindowHelpers.RemoveWindowBorders(hWndDocked);
            
            Win32Input.SetTargetProcessId(pDofus.Id);

            Task.Run(async () => {
                try {
                    Win32Input.Init();
                    await Win32Input.WaitForHookReady();
                    this.BeginInvoke(new Action(() =>
                    {
                        try {
                            BindServicesToDockedWindow();
                        } catch (Exception e) {
                            Debug.WriteLine(e);
                        }
                    }));
                } catch (Exception e) {
                    Debug.WriteLine(e);
                }
            });
            
            // m_GlobalHook = Gma.System.MouseKeyHook.Hook.GlobalEvents();
            // m_GlobalHook.KeyDown += (sender, args) => {
            //     if (args.KeyCode == Keys.Escape) {
            //         if (magingJob.IsMaging)
            //             magingJob.StopMage();
            //     }
            //     if (args.KeyCode == Keys.F6)
            //         OnClickInsert();
            // };

            return true;
        }

        private async Task WaitForDofusProcessLoop(WaitingForDofusForm waitingForm) {
            do {
                var processes = Process.GetProcesses();
                var dofusProcesses = processes
                    .Where(process => process.ProcessName.IndexOf("dofus", StringComparison.OrdinalIgnoreCase) >= 0 && process.MainWindowTitle != "")
                    .ToArray();

                if (dofusProcesses.Length == 1) {
                    pDofus = dofusProcesses[0];
                } else if (dofusProcesses.Length > 1) {
                    waitingForm.Invoke(new MethodInvoker(() => {
                        waitingForm.UpdateProcessList(dofusProcesses);
                    }));
                }
                await Task.Delay(1000);
                
                if (pDofus != null) {
                    var prefs = DetectUserGame.ReadDofusPreferences();
                    if (prefs != DetectUserGame.DofusPreferences.Ideal) {
                        pDofus = null;
                        foreach (var p in dofusProcesses) {
                            try { p.Kill(); } catch { /* ignore */ }
                        }
                        waitingForm.Invoke(new MethodInvoker(() => {
                            waitingForm.ShowErrorMessage(
                                "Dofus was terminated to adjust UI preferences.\nPlease restart it via the Ankama Launcher.");
                        waitingForm.UpdateProcessList(Array.Empty<Process>());
                        }));
                        await PatchDofusPreferencesToIdeal();
                    }
                }
            } while (pDofus == null);
        }

        private static async Task PatchDofusPreferencesToIdeal() {
            var ideal = DetectUserGame.DofusPreferences.Ideal;
            var localLow = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                @"AppData\LocalLow");
            var prefsPath = Path.Combine(localLow, @"Ankama\Dofus\RELEASE\Shared\dofus.json");
            for (var i = 0; i < 50; i++) {
                try {
                    var json = JObject.Parse(File.ReadAllText(prefsPath));
                    json["uiScale"]!["value"]                      = ideal.uiScale.value;
                    json["renderingScale"]!["value"]!["value"]     = ideal.renderingScale.value.value;
                    json["renderingScale"]!["value"]!["isMute"]    = ideal.renderingScale.value.isMute;
                    json["dofusQuality"]!["value"]                 = ideal.dofusQuality.value;
                    json["windowResolutionMode"]!["value"]         = ideal.windowResolutionMode.value;
                    json["windowDisplayMode"]!["value"]            = ideal.windowDisplayMode.value;
                    json["globalFontSize"]!["value"]               = ideal.globalFontSize.value;
                    File.WriteAllText(prefsPath, json.ToString());
                } catch { /* ignore */ }
                await Task.Delay(100);
            }
        }

        private void OnClickInsert() {
            actions.Execute(actionFactory.InventorySelectEquipmentAction());
        }

        private void OnDofusProcessSelected(object sender, ProcessEventArgs e) {
            pDofus = e.Process;
        }


        private void BindServicesToDockedWindow() {
            var screen = (WinScreenRecorderScreenCapture) Program.Services.GetService<ScreenCapture>();
            screen.BindTo(this.Handle, dofusClientPanel, sidebarPanel.Width, sidebarRightPanel.Width, this);
            
            var mouse = (Win32Input) Program.Services.GetService<Input>();
            mouse.SetRelativeToHandle(hWndDocked);
            
            var actions = (MouseActionFactory) Program.Services.GetService<ActionFactory>();
            actions.SetRelativeToControl(dofusClientPanel);
        }
    }
}
