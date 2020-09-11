using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WindowsFormsApp
{
  public partial class MainForm : Form
  {
    private Process pDocked;
    private IntPtr hWndDocked;
    private DofusCommandIssuer command;
    private StatsForm statsForm;

    public MainForm()
    {
      InitializeComponent();
      InitializeDofusClient();
      
      Program.Services.AddService(typeof(DofusDataProvider), new ScreenReader(hWndDocked));
      statusBarPanel2.Subscribe(panel1);
      statsForm = new StatsForm(this);
      statsForm.Show();
    }

    private void InitializeDofusClient() {
      pDocked = Process.Start(Program.debug ? @"notepad" : "A:/Games/Dofus/dofus.exe");
      WindowHelpers.DockProcess(pDocked, panel1, ref hWndDocked);
      WindowHelpers.RemoveWindowBorders(hWndDocked);
    }
  }
}
