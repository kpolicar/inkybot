using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp.Api;
using WindowsFormsApp.Contracts;
using WindowsFormsApp.Services;
using ApiDataProvider = WindowsFormsApp.Api.ApiDataProvider;


namespace WindowsFormsApp
{
    public partial class MainForm : Form
    {
        private readonly ApiDataProvider api;
        private readonly DofusMagingJob magingJob;

        private IntPtr hWndDocked;
        private Process pDocked;
        private Task<bool> tokenRefresh;
        private ScreenReaderDataProvider clientDataProvider;

        public MainForm() {
            InitializeComponent();
            ocrIndicatorPanel.BringToFront();
            InitializeDofusClient();

            Program.Services.AddService(typeof(DofusDataProvider), new ScreenReaderDataProvider(hWndDocked));
            var mouse = (Win32Mouse) Program.Services.GetService(typeof(Mouse));
            mouse.SetRelativeToHandle(hWndDocked);

            statsForm = new StatsForm(this);
            api = (ApiDataProvider) Program.Services.GetService(typeof(ApiDataProvider));
            magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            MainFormDomainEvents();
            MainFormEvents();
            api.UserFetched += OnUserDetailsUpdated;
            InitAuth();

            Closing += (sender, args) => {
                if (debugging) StopDebugging();
            };
        }

        private void InitializeDofusClient() {
            pDocked = Process.Start(Program.debug ? @"notepad" : "A:/Saved Games/Dofus/dofus.exe");
            WindowHelpers.DockProcess(pDocked, dofusClientPanel, ref hWndDocked);
            WindowHelpers.RemoveWindowBorders(hWndDocked);
        }

        private void paintOcrIndicators(object sender, EventArgs eventArgs) {
            var g = ocrIndicatorPanel.CreateGraphics();

            var pen = new Pen(Color.Red, 2);
            g.DrawRectangle(pen, new Rectangle(626, 300, 980 - 626, 39 * 11));
            g.DrawRectangle(pen, DofusScreenScan.HistoryBounds);
            pen.Dispose();
            g.Dispose();
        }
    }
}
