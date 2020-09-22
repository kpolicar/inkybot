using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WindowsFormsApp.Contracts;
using WindowsFormsApp.Services;
using Mouse = WindowsFormsApp.Contracts.Mouse;

namespace WindowsFormsApp
{
  public partial class MainForm : Form
  {
    private Process pDocked;
    private IntPtr hWndDocked;
    private StatsForm statsForm;
    private IndicatorForm indicatorForm;

    
    public MainForm()
    {
      InitializeComponent();
      
      InitializeDofusClient();

      
      Program.Services.AddService(typeof(DofusDataProvider), new ScreenReaderDataProvider(hWndDocked));
      var mouse = (Win32Mouse) Program.Services.GetService(typeof(Mouse));
      mouse.SetRelativeToHandle(hWndDocked);
      statusBarPanel2.Subscribe(panel1);
      
      statsForm = new StatsForm(this);
      statsForm.Show();
      // indicatorForm = new IndicatorForm(panel1);
      // indicatorForm.Show();

    }

    private void InitializeDofusClient() {
      pDocked = Process.Start(Program.debug ? @"notepad" : "A:/Games/Dofus/dofus.exe");
      WindowHelpers.DockProcess(pDocked, panel1, ref hWndDocked);
      WindowHelpers.RemoveWindowBorders(hWndDocked);
    }

    private void button1_Click(object sender, EventArgs e) {
      panel2.Width = 0;
    }
  }
}
