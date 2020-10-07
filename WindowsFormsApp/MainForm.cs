using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using WindowsFormsApp.Contracts;
using WindowsFormsApp.Services;
using Gma.System.MouseKeyHook;
using Mouse = WindowsFormsApp.Contracts.Mouse;

namespace WindowsFormsApp
{
  public partial class MainForm : Form
  {
    private Process pDocked;
    private IntPtr hWndDocked;
    private StatsForm statsForm;
    private DofusMagingJob magingJob;
    private Auth auth;

    public MainForm()
    {
      InitializeComponent();
      ocrIndicatorPanel.BringToFront();
      InitializeDofusClient();
      auth = (Auth) Program.Services.GetService(typeof(Auth));
      
      Program.Services.AddService(typeof(DofusDataProvider), new ScreenReaderDataProvider(hWndDocked));
      var mouse = (Win32Mouse) Program.Services.GetService(typeof(Mouse));
      mouse.SetRelativeToHandle(hWndDocked);
      
      statsForm = new StatsForm(this);
      magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
      BindToMagingEvents();
      InitializeKeyboardShortcuts();
      
      Closing += (sender, args) =>  {
        if (debugging) StopDebugging();
      };
    }

    private bool DoLoginDialog()
    {
      var result = new LoginForm().ShowDialog(this);
      return result == DialogResult.OK;
    }

    private void MainForm_Load(object sender, EventArgs eventArgs)
    {
      InitFormWithLoginDiaog();
    }

    private bool InitFormWithLoginDiaog()
    {
      var loginSuccess = DoLoginDialog();
      if (!loginSuccess) {
        Close();
        return false;
      }

      userDetailsTimer.Start();
      authTokenRefreshTimer.Start();
      OnUserDetailsTimer(this, EventArgs.Empty);
      
      return true;
    }

    private void OnUserDetailsTimer(object sender, EventArgs eventArgs)
    {
      if (tokenRefresh != null && !tokenRefresh.IsCompleted)  {
        tokenRefresh.ContinueWith(task => {
          OnUserDetailsTimer(sender, eventArgs);
        });
        return;
      }

      UpdateUserDetails();
    }

    private async Task UpdateUserDetails()
    {
      try
      {
        var user = await auth.User();
        usernameLabel.Text = user.name;
      }
      catch (HttpRequestException exception)
      {
        var success = InitFormWithLoginDiaog();
        if (success)
          Show();
      }
    }

    private Task<bool> tokenRefresh;

    private async void OnAuthTokenRefreshTimer(object sender, EventArgs eventArgs)
    {
      tokenRefresh = auth.RefreshToken();
      if (await tokenRefresh) {
        Debug.WriteLine("Refresh success");
      }
      else
      {
        Debug.WriteLine("Refresh failed");
      }
    }

    private void BindToMagingEvents() {
      magingJob.Started += OnMagingStarted;
      magingJob.Stopped += OnMagingStopped;
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
      WindowHelpers.DockProcess(pDocked, dofusClientPanel, ref hWndDocked);
      WindowHelpers.RemoveWindowBorders(hWndDocked);
    }
    
    private void OnMagingStopped(object sender, EventArgs e)
    {
      toggleMageButton.Text = "Start\n(F2)";
    }

    private void OnMagingStarted(object sender, EventArgs e)
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
      var g = ocrIndicatorPanel.CreateGraphics();
      
      Pen pen = new Pen(Color.Red, 2);
      g.DrawRectangle(pen, new Rectangle(626, 300, 980-626, 39*11));
      g.DrawRectangle(pen, new Rectangle(352, 137, 590-352, 835-137));
      pen.Dispose();
      g.Dispose();
    }

    private void ocrIndicatorPanel_VisibleChanged(object sender, EventArgs e)
    {
      if (ocrIndicatorPanel.Visible)
        paintTimer.Start();
      else
        paintTimer.Stop();
    }
    
    private void ocrIndicatorPanel_Click(object sender, EventArgs e) {
      ocrIndicatorPanel.Hide();
    }

    private void Form1_Resize(object sender, EventArgs e) {
    }
  }
}
