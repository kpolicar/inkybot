using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WindowsFormsApp
{
  public partial class MainForm : Form
  {
    private Process pDocked;
    private IntPtr hWndDocked;
    private ScreenReader screenReader;
    private StatsForm statsForm;

    public MainForm()
    {
      InitializeComponent();
      InitializeDofusClient();
      
      Program.Services.AddService(typeof(DofusDataProvider), screenReader = new ScreenReader(hWndDocked));
      statusBarPanel2.Subscribe(panel1);
      statsForm = new StatsForm(this);
      statsForm.Show();
      KeyDown += Form1_KeyDown;
    }

    private void InitializeDofusClient() {
      pDocked = Process.Start(@"notepad");
      WindowHelpers.DockProcess(pDocked, panel1, ref hWndDocked);
      WindowHelpers.RemoveWindowBorders(hWndDocked);
    }

    private async void Form1_KeyDown(object sender, KeyEventArgs e)
    {
      if (!e.Control)
        return;
        
      var results = await screenReader.Stats();
      statsForm.DisplayStats(results);
    }
  }
}
