using System;
using System.Diagnostics;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Services;
using UserSettings = Inkybot.Properties.Settings;

namespace Inkybot
{
    public partial class MainForm
    {
        private Process? pDofus;
        private IntPtr hWndDocked;

        private bool InitializeDofusClient() {
            if (pDofus != null && !pDofus.HasExited) {
                pDofus.Kill();
                hWndDocked = IntPtr.Zero;
            }
            
            var gameVersion = DetectUserGame.ReadRelease();
            var dofusPath =
                gameVersion != null && DetectUserGame.HasValidGamePath(gameVersion) && UserSettings.Default.dofusPath == ""
                ? gameVersion.ExeLocation
                : UserSettings.Default.dofusPath;
            
            if (dofusPath == "") {
                var result = new DofusPathForm().ShowDialog(this);
                if (result != DialogResult.OK) {
                    return false;
                }
            }

            pDofus = Process.Start(dofusPath);
            WindowHelpers.DockProcess(pDofus!, dofusClientPanel, ref hWndDocked);
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
            actions.SetRelativeToControl(dofusClientPanel);
        }
    }
}
