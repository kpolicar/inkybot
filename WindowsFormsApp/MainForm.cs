using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WindowsFormsApp
{
  public partial class MainForm : Form
  {
    private Process pDocked;
    private IntPtr hWndOriginalParent;
    private IntPtr hWndDocked;
    private Ocr ocr;
    private DebugForm debugForm;


    public MainForm()
    {
      InitializeComponent();
      dockIt();
      statusBarPanel2.Subscribe(panel1);
      ocr = new Ocr(hWndDocked);
      debugForm = new DebugForm(this);
      debugForm.Show();
      KeyDown += Form1_KeyDown;
    }
    
    private async void Form1_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Control) {
        var results = await ocr.stats();
        debugForm.DisplayStats(results);
      }
    }
    
    private void dockIt() {
      if (hWndDocked != IntPtr.Zero) //don't do anything if there's already a window docked.
        return;

      pDocked = Process.Start(@"notepad");
      //pDocked = Process.Start("A:/Games/Dofus/dofus.exe");
      if (pDocked == null)
        return;
      
      while (hWndDocked == IntPtr.Zero)
      {
        pDocked.WaitForInputIdle(1000);
        pDocked.Refresh();
        if (pDocked.HasExited)
        {
          return;
        }
        hWndDocked = pDocked.MainWindowHandle;
      }
      
      hWndOriginalParent = Win32.SetParent(hWndDocked, panel1.Handle);
      
      int style = Win32.GetWindowLong(pDocked.MainWindowHandle, Win32.GWL_STYLE);
      Win32.SetWindowLong(pDocked.MainWindowHandle, Win32.GWL_STYLE, (style & ~Win32.WS_CAPTION));

      panel1.SizeChanged += Panel1_Resize;
      Panel1_Resize(new object(), new EventArgs());
    }

    private void undockIt()
    {
      Win32.SetParent(hWndDocked, hWndOriginalParent);
    }

    private void Panel1_Resize(object sender, EventArgs e)
    {
      //Change the docked windows size to match its parent's size. 
      Win32.MoveWindow(hWndDocked, 0, 0, panel1.Width, panel1.Height, true);
    }
  }
}
