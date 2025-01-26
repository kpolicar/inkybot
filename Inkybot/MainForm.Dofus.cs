using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Dofus.Contracts;
using Inkybot.Domain;
using Inkybot.Events;
using Inkybot.Services;
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
                    do {
                        var processes = Process.GetProcesses();
                        var dofusProcesses = processes
                            .Where(process =>
                                (process.ProcessName.IndexOf("dofus", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 process.ProcessName.IndexOf("?tasis", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 process.ProcessName.IndexOf(Properties.Settings.Default.dofusProcessName, StringComparison.OrdinalIgnoreCase) >= 0)
                                && process.MainWindowTitle != "")
                            .ToArray();

                        if (dofusProcesses.Length == 1) {
                            pDofus = dofusProcesses[0];
                        } else if (dofusProcesses.Length > 1) {
                            waitingForm.Invoke(new MethodInvoker(() => {
                                waitingForm.UpdateProcessList(dofusProcesses);
                            }));
                        }

                        await Task.Delay(1000);
                    } while (pDofus == null);
                    
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

            BindServicesToDockedWindow();
            
            
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

        private void OnClickInsert() {
            var inp = new Win32Input();
            inp.SetRelativeToHandle(pDofus!.MainWindowHandle);
            var x=1088;
            var y=344;
            inp.Click(x,y);
        }

        private void OnDofusProcessSelected(object sender, ProcessEventArgs e) {
            pDofus = e.Process;
        }


        private void BindServicesToDockedWindow() {
            var screen = (Win32ScreenCapture) Program.Services.GetService<ScreenCapture>();
            screen.BindTo(this.Handle, dofusClientPanel);
            
            var mouse = (Win32Input) Program.Services.GetService<Input>();
            mouse.SetRelativeToHandle(hWndDocked);
            
            var actions = (MouseActionFactory) Program.Services.GetService<ActionFactory>();
            actions.SetRelativeToControl(dofusClientPanel);
        }
    }
}
