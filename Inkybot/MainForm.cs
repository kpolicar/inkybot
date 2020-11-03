using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Events;
using Inkybot.Helpers;
using Inkybot.Services;
using Tesseract;


namespace Inkybot
{
    public partial class MainForm : Form
    {
        private StatsForm statsForm;
        private readonly ApiDataProvider api;
        private readonly DofusMagingJob magingJob;

        private Task<bool> tokenRefresh;

        public MainForm() {
            InitializeComponent();
            toastPanel.Hide();
            mageInfoPanel.Hide();
            debugScreenshotButton.Hide();
            
            ocrIndicatorPanel.BringToFront();
            InitializeDofusClient();

            Program.Services.AddService(typeof(DofusDataProvider), new ScreenReaderDataProvider(hWndDocked));
            var mouse = (Win32Mouse) Program.Services.GetService(typeof(Mouse));
            mouse.SetRelativeToHandle(hWndDocked);

            statsForm = new StatsForm(this);
            
            api = (ApiDataProvider) Program.Services.GetService(typeof(ApiDataProvider));
            magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            magingJob.Error += OnError;
            statsForm.Error += OnError;
            MainFormDomainEvents();
            MainFormEvents();
            api.UserFetched += OnUserDetailsUpdated;
            InitAuth();

            Closing += (sender, args) => {
                if (debugging) StopDebugging();
            };
        }

        private void OnError(object sender, ExceptionEventArgs e) {
            Invoke(new MethodInvoker(delegate {
                toastLabel.Text = e.exception.Message;
                toastPanel.Show();
                toastPanel.BringToFront();
            }));
        }
        
        private void paintOcrIndicators(object sender, EventArgs eventArgs) {
            var g = ocrIndicatorPanel.CreateGraphics();

            var pen = new Pen(Color.Red, 2);

            var drawRect = Responsive.ResponsiveRectangle(new Responsive.Measurement {
                Rectangle = Rect.FromCoords(187, 343, 398, 667),
                Width =  777,
                Height = 933
            }, ocrIndicatorPanel.Width, ocrIndicatorPanel.Height);
            
            //var statsRect = new Rectangle(626, 300, 980 - 626, 39 * 14);
            g.DrawRectangle(pen, drawRect);
            //g.DrawRectangle(pen, DofusScreenScan.HistoryBounds);
            pen.Dispose();
            g.Dispose();
        }
    }
}
