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
        
        //const double perfectRatio = 0.8;

        public struct RatioRect
        {
            public double x;
            public double y;
            public double width;
            public double height;
            
            public RatioRect(double x, double y, double width, double height) {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
            }
        }

        private RatioRect PointsToRatioRect(int p1x, int p1y, int p2x, int p2y, int w, int h) {
            
            var perfectRatio = 0.8d; // h/w
            var ratio = 1f*h/w;
            
            double gameW = w;
            var xOffset = 0d;
            if (ratio < perfectRatio) {
                gameW = h / perfectRatio;
                xOffset = (w - gameW) / 2;
                
                p1x -= (int) xOffset;
                p2x -= (int) xOffset;
            }
            
            double gameH = h;
            var yOffset = 0d;
            if (ratio > perfectRatio) {
                gameH = w * perfectRatio;
                yOffset = (h - gameH) / 2;
                
                p1y -= (int) yOffset;
                p2y -= (int) yOffset;
            }
            
            //Debug.WriteLine($"gameW:{gameW}, xOffset:{xOffset}, w: {w}, p1x: {p1x}");


            var p1x_ratio = 1d * p1x / gameW;
            var p1y_ratio = 1d * p1y / gameH;
            var rw = p2x - p1x;
            var rh = p2y - p1y;
            
            var rw_ratio = 1d * rw / gameW;
            var rh_ratio = 1d * rh / gameH;
            
            return new RatioRect(
                p1x_ratio,
                p1y_ratio,
                rw_ratio,
                rh_ratio
                );
        }

        // Convert a ratio rect to the rectangle that will be drawn on screen (responsive)
        private Rectangle RatioRectToScreenRect(RatioRect bounds) {
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
                (int) (bounds.x*gameW + xOffset), 
                (int) (bounds.y*gameH + yOffset),
                (int) (bounds.width*gameW),
                (int) (bounds.height*gameH));
        }
        
        private void paintOcrIndicators(object sender, EventArgs eventArgs) {
            var g = ocrIndicatorPanel.CreateGraphics();
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


            var pen = new Pen(Color.Red, 2);
            //var statsRect = new Rectangle(
            //    //(int) (0.35582*(w + xOffset)), 
            //    (int) (0.24362*gameW + xOffset), 
            //    (int) (0.29841*gameH + yOffset),
            //    //(int) (0.1534*w),
            //    (int) (0.2744*gameW),
            //    (int) (0.53050*gameH));

            var statBounds = DofusScreenScan.StatBounds;
            var statsRect = PointsToRatioRect(187, 343, 398, 667, 777, 933);
            //var statsRect = PointsToRatioRect(634, 303, 980, 842, 1920, 1017);
            //var statsRect = PointsToRatioRect(302, 303, 638, 830, 1257, 1008);
            var drawRect = RatioRectToScreenRect(statsRect);
            
            //var statsRect = new Rectangle(626, 300, 980 - 626, 39 * 14);
            g.DrawRectangle(pen, drawRect);
            //g.DrawRectangle(pen, DofusScreenScan.HistoryBounds);
            pen.Dispose();
            g.Dispose();
        }
    }
}
