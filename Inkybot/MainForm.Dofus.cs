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
        private Mutex? dofusClaim;

        private static bool IsDofusProcessClaimed(int processId) {
            try {
                using (Mutex.OpenExisting($@"Global\Inkybot_DofusPID_{processId}"))
                    return true;
            } catch (WaitHandleCannotBeOpenedException) {
                return false;
            } catch (AbandonedMutexException) {
                return false;
            }
        }

        private void ClaimDofusProcess(int processId) {
            dofusClaim = new Mutex(true, $@"Global\Inkybot_DofusPID_{processId}");
        }

        private void ReleaseDofusClaim() {
            if (dofusClaim != null) {
                try { dofusClaim.ReleaseMutex(); } catch { }
                dofusClaim.Dispose();
                dofusClaim = null;
            }
        }

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
            WindowHelpers.RemoveWindowBorders(hWndDocked);
            ClaimDofusProcess(pDofus.Id);

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
                    .Where(process => !IsDofusProcessClaimed(process.Id))
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
                    var allPrefs = DetectUserGame.ReadAllDofusPreferences();
                    var nonIdeal = allPrefs
                        .Where(p => p.prefs != DetectUserGame.DofusPreferences.Ideal)
                        .ToArray();
                    if (nonIdeal.Length > 0) {
                        pDofus = null;
                        foreach (var p in dofusProcesses) {
                            try { p.Kill(); } catch { /* ignore */ }
                        }
                        // Note: dofusProcesses is already filtered to exclude
                        // processes claimed by other Inkybot instances (see above),
                        // so this only kills unclaimed processes.
                        waitingForm.Invoke(new MethodInvoker(() => {
                            waitingForm.ShowErrorMessage(
                                "Dofus was terminated to adjust UI preferences.\nPlease restart it via the Ankama Launcher.");
                            waitingForm.UpdateProcessList(Array.Empty<Process>());
                        }));
                        await PatchDofusPreferencesToIdeal(nonIdeal.Select(p => p.path).ToArray());
                    }
                }
            } while (pDofus == null);
        }

        private static async Task PatchDofusPreferencesToIdeal(string[] paths) {
            var ideal = DetectUserGame.DofusPreferences.Ideal;
            for (var i = 0; i < 50; i++) {
                foreach (var prefsPath in paths) {
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
                    } catch { /* ignore - file may be empty or locked */ }
                }
                await Task.Delay(100);
            }
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
