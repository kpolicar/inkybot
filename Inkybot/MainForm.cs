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

        private Rectangle TransformBounds1(int p1x, int p1y, int p2x, int p2y, int w, int h) {
            var p1y_ratio = p1y / h;
            var p1x_ratio = p1x / w;
            var rw = p2x - p1x;
            var rh = p2y - p1y;
            
            var rw_ratio = rw / w;
            var rh_ratio = rh / h;
            
            return new Rectangle(
                p1x_ratio,
                p1y_ratio,
                rw_ratio,
                rh_ratio
                );
        }

        private Rectangle TransformBounds(Rectangle bounds) {
            var dimensions = ocrIndicatorPanel.Size;
            var w = 1d * dimensions.Width;
            var h = 1d * dimensions.Height;
            var perfectRatio = 0.8d; // h/w
            var ratio = h/w;
            
            var gameH = h;
            var yOffset = 0d;
            if (ratio > perfectRatio) {
                gameH = perfectRatio * w;
                yOffset = (h - gameH) / 2;
            }
            
            var gameW = w;
            var xOffset = 0d;
            if (ratio < perfectRatio) {
                gameW = h / perfectRatio;
                xOffset = (w - gameW) / 2;
            }


            return new Rectangle(
                (int) (bounds.X*gameW + xOffset), 
                (int) (bounds.Y*gameH + yOffset),
                (int) (bounds.Width*gameW),
                (int) (bounds.Height*gameH));
        }
        
        private void paintOcrIndicators(object sender, EventArgs eventArgs) {
            var g = ocrIndicatorPanel.CreateGraphics();
            var dimensions = ocrIndicatorPanel.Size;
            var w = 1d * dimensions.Width;
            var h = 1d * dimensions.Height;
            //var xOffset = 0d;
            var perfectRatio = 0.8d; // h/w
            var ratio = h/w;
            
            
            //var pW = h / perfectRatio;
            //xOffset = (w - pW) / 2;
            //xOffset = ratio < perfectRatio ? xOffset : 0;
            //var pX1 = (int) (0.24362*(w + xOffset));
            //var pX2 = (int) (0.51802*(w + xOffset));
            
            //if (ratio > perfectRatio)
            //    Debug.WriteLine("ratio broken");
            var gameH = h;
            var yOffset = 0d;
            if (ratio > perfectRatio) {
                gameH = perfectRatio * w;
                yOffset = (h - gameH) / 2;
            }
            
            var gameW = w;
            var xOffset = 0d;
            if (ratio < perfectRatio) {
                gameW = h / perfectRatio;
                xOffset = (w - gameW) / 2;
            }


            var pen = new Pen(Color.Red, 2);
            var statsRect = new Rectangle(
                //(int) (0.35582*(w + xOffset)), 
                (int) (0.24362*gameW + xOffset), 
                (int) (0.29841*gameH + yOffset),
                //(int) (0.1534*w),
                (int) (0.2744*gameW),
                (int) (0.53050*gameH));
            
            //var statsRect = new Rectangle(626, 300, 980 - 626, 39 * 14);
            g.DrawRectangle(pen, statsRect);
            //g.DrawRectangle(pen, DofusScreenScan.HistoryBounds);
            pen.Dispose();
            g.Dispose();
        }
    }
}
