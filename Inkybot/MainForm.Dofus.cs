using System;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Exceptions;

namespace Inkybot
{
    public partial class MainForm
    {
        private Process pDofus;
        private IntPtr hWndDocked;
        
        private void InitializeDofusClient() {
            if (pDofus != null && !pDofus.HasExited) {
                pDofus.Kill();
                hWndDocked = IntPtr.Zero;
            }
            
            if (Properties.Settings.Default.dofusPath == "") {
                var result = new DofusPathForm().ShowDialog(this);
                if (result != DialogResult.OK) {
                    // Todo: fix
                    Close();
                    return;
                }
            }

            //pDofus = Process.Start("notepad.exe");
            pDofus = Process.Start(Properties.Settings.Default.dofusPath);
            WindowHelpers.DockProcess(pDofus, dofusClientPanel, ref hWndDocked);
            WindowHelpers.RemoveWindowBorders(hWndDocked);
        }
    }
}
