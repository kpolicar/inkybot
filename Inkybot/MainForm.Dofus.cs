using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Dofus.Contracts;
using Inkybot.Domain;
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
            
            var result = new WaitingForDofusForm().ShowDialog(this);
            if (result != DialogResult.OK) {
                return false;
            }

            var waitingForm = new WaitingForDofusForm();

            Task.Run(async () => {
                do {
                    var processes = Process.GetProcesses();
                    var dofusProcesses = processes
                        .Where(process => process.ProcessName.IndexOf("dofus", StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToArray();
                    
                    if (dofusProcesses.Length == 1) {
                        pDofus = dofusProcesses[0];
                        waitingForm.DialogResult = DialogResult.OK;
                        waitingForm.Close();
                    } else if(dofusProcesses.Length > 1) {
                        waitingForm.UpdateProcessList(dofusProcesses);
                    }
                    await Task.Delay(1000);
                } while (pDofus == null);
            });
            
            var resultWaiting = waitingForm.ShowDialog(this);
            if (resultWaiting != DialogResult.OK || pDofus == null) {
                return false;
            }
            
            parentHandle = WindowHelpers.DockProcess(pDofus!, dofusClientPanel, ref hWndDocked);
            WindowHelpers.RemoveWindowBorders(hWndDocked);

            BindServicesToDockedWindow();

            return true;
        }
        

        private void BindServicesToDockedWindow() {
            var screen = (Win32ScreenCapture) Program.Services.GetService<ScreenCapture>();
            screen.BindTo(hWndDocked);
            
            var mouse = (Win32Input) Program.Services.GetService<Input>();
            mouse.SetRelativeToHandle(hWndDocked);
            
            var actions = (MouseActionFactory) Program.Services.GetService<ActionFactory>();
            actions.SetRelativeToControl(dofusClientPanel);
        }
    }
}
