using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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
      
      statsForm = new StatsForm(this);
      statsForm.Show();
      // indicatorForm = new IndicatorForm(panel1);
      // indicatorForm.Show();
    }

    private void InitializeDofusClient() {
      pDocked = Process.Start(Program.debug ? @"notepad" : "A:/Saved Games/Dofus/dofus.exe");
      WindowHelpers.DockProcess(pDocked, panel1, ref hWndDocked);
      WindowHelpers.RemoveWindowBorders(hWndDocked);
    }

    private void button1_Click(object sender, EventArgs e) {
      panel3.Show();
    }

    private void paintOcrIndicators(object sender, EventArgs eventArgs)
    {
      var g = panel3.CreateGraphics();
      
      Pen pen = new Pen(Color.Red, 2);
      g.DrawRectangle(pen, new Rectangle(626, 300, 980-626, 39*11));
      g.DrawRectangle(pen, new Rectangle(352, 137, 590-352, 835-137));
      pen.Dispose();
      g.Dispose();
    }

    private void panel3_VisibleChanged(object sender, EventArgs e)
    {
      if (panel3.Visible)
        paintTimer.Start();
      else
        paintTimer.Stop();
    }
    
    private void panel3_Click(object sender, EventArgs e) {
      panel3.Hide();
    }

    private void Form1_Resize(object sender, EventArgs e) {
    }
  }
}
