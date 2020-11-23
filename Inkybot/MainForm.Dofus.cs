using System;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Exceptions;
using Inkybot.Services;

namespace Inkybot
{
    public partial class MainForm
    {
        private Process pDofus;
        private IntPtr hWndDocked;

        private bool InitializeDofusClient() {
            if (pDofus != null && !pDofus.HasExited) {
                pDofus.Kill();
                hWndDocked = IntPtr.Zero;
            }
            
            if (Properties.Settings.Default.dofusPath == "") {
                var result = new DofusPathForm().ShowDialog(this);
                if (result != DialogResult.OK) {
                    return false;
                }
            }

            //pDofus = Process.Start("notepad.exe");
            pDofus = Process.Start(Properties.Settings.Default.dofusPath);
            WindowHelpers.DockProcess(pDofus, dofusClientPanel, ref hWndDocked);
            WindowHelpers.RemoveWindowBorders(hWndDocked);

            BindServicesToDockedWindow();

            return true;
        }
        

        private void BindServicesToDockedWindow() {
            var dataProvider = (ScreenReaderDataProvider) Program.Services.GetService<DofusDataProvider>();
            dataProvider.BindTo(hWndDocked);
            
            var mouse = (Win32Input) Program.Services.GetService<Input>();
            mouse.SetRelativeToHandle(hWndDocked);
            
            var actions = (MouseActionFactory) Program.Services.GetService<ActionFactory>();
            actions.setRelativeToControl(dofusClientPanel);
        }
    }
}
