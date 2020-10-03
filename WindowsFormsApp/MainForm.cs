using System;
using System.Diagnostics;
using System.Drawing;
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

    
    public MainForm()
    {
      InitializeComponent();
      panel3.BringToFront();

      InitializeDofusClient();

      
      Program.Services.AddService(typeof(DofusDataProvider), new ScreenReaderDataProvider(hWndDocked));
      var mouse = (Win32Mouse) Program.Services.GetService(typeof(Mouse));
      mouse.SetRelativeToHandle(hWndDocked);
      
      statsForm = new StatsForm(this);
      statsForm.Show();
      magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
      magingJob.Started += onMagingStarted;
      magingJob.Stopped += onMagingStopped;
      InitializeKeyboardShortcuts();
    }
    
    
    private void InitializeKeyboardShortcuts() {
      KeyboardHook.Init();
      KeyboardHook.KeyPressed += (sender, e) => {
        if (e.KeyCode == Keys.F2)
          toggleMageButton_Click(sender, e);
      };
      
      Closing += (sender, e) => {
        KeyboardHook.Release();
        magingJob.StopMage();
      };
    }

    private void InitializeDofusClient() {
      pDocked = Process.Start(Program.debug ? @"notepad" : "A:/Saved Games/Dofus/dofus.exe");
      WindowHelpers.DockProcess(pDocked, panel1, ref hWndDocked);
      WindowHelpers.RemoveWindowBorders(hWndDocked);
    }

    private bool debugging = false;
    private DofusMagingJob magingJob;

    private void debugButton_Click(object sender, EventArgs e) {
      if (debugging = !debugging)  {
        panel3.Show();
        panel2.BringToFront();
        debugButton.Text = "Stop Debug";
      }
      else  {
        panel3.Hide();
        debugButton.Text = "Debug";
      }
    }
    
    
    private void onMagingStopped(object sender, EventArgs e)
    {
      toggleMageButton.Text = "Start\n(F2)";
    }

    private void onMagingStarted(object sender, EventArgs e)
    {
      toggleMageButton.Text = "Stop\n(F2)";
    }

    private void toggleMageButton_Click(object sender, EventArgs e)
    {
      magingJob.BeginMage(!magingJob.IsMaging);
    }

    private void helpButton_Click(object sender, EventArgs e) {
      // Open help on website
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
