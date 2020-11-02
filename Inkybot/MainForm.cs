using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Events;
using Inkybot.Services;


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
            var dimensions = ocrIndicatorPanel.Size;
            var w = 1d * dimensions.Width;
            var h = 1d * dimensions.Height;
            var xOffset = 0d;
            var yOffset = 0d;
            var perfectRatio = 0.8d; // h/w

            var pW = h / perfectRatio;
            xOffset = (w - pW) / 2;
            var pX1 = (int) (0.24362*(w + xOffset));
            var pX2 = (int) (0.51802*(w + xOffset));

            var pen = new Pen(Color.Red, 2);
            var statsRect = new Rectangle(
                //(int) (0.35582*(w + xOffset)), 
                pX1, 
                (int) (0.29841*h),
                //(int) (0.1534*w),
                pX2 - pX1,
                (int) (0.53050*h));
            
            //var statsRect = new Rectangle(626, 300, 980 - 626, 39 * 14);
            g.DrawRectangle(pen, statsRect);
            g.DrawRectangle(pen, DofusScreenScan.HistoryBounds);
            pen.Dispose();
            g.Dispose();
        }

        private void statsButton_Click(object sender, EventArgs e) {
            if (!statsForm.Visible) statsForm.Show();
            else statsForm.Hide();
        }

        private void toastPanelCloseButton_Click(object sender, EventArgs e) {
            toastPanel.Hide();
        }
    }
}
