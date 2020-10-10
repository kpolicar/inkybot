using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp.Api;
using WindowsFormsApp.Contracts;
using WindowsFormsApp.Events;
using WindowsFormsApp.Exceptions;
using WindowsFormsApp.Resources.Api;
using WindowsFormsApp.Services;

namespace WindowsFormsApp
{
    public partial class MainForm : AuthenticatedForm
    {
        private readonly ApiDataProvider api;
        private IntPtr hWndDocked;
        private readonly DofusMagingJob magingJob;
        private Process pDocked;
        private readonly StatsForm statsForm;
        private Task<bool> tokenRefresh;

        public MainForm() {
            InitializeComponent();
            ocrIndicatorPanel.BringToFront();
            InitializeDofusClient();

            Program.Services.AddService(typeof(DofusDataProvider), new ScreenReaderDataProvider(hWndDocked));
            var mouse = (Win32Mouse) Program.Services.GetService(typeof(Mouse));
            mouse.SetRelativeToHandle(hWndDocked);

            statsForm = new StatsForm(this);
            magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            api = (ApiDataProvider) Program.Services.GetService(typeof(ApiDataProvider));
            BindToMagingEvents();
            InitializeKeyboardShortcuts();
            api.UserFetched += OnUserDetailsUpdated;

            Closing += (sender, args) => {
                if (debugging) StopDebugging();
            };
        }

        private void OnUserDetailsUpdated(object sender, FetchedUserEventArgs e) {
            UpdateUserDetails(e.user);
        }


        private async void OnUserDetailsTimer(object sender, EventArgs eventArgs) {
            if (tokenRefresh != null && !tokenRefresh.IsCompleted) {
                tokenRefresh.ContinueWith(task => { OnUserDetailsTimer(sender, eventArgs); });
                return;
            }

            try {
                var user = await api.User();
                UpdateUserDetails(user);
            } catch (Exception exception) {
                var message = exception switch {
                    HttpRequestException _ => "Something went wrong!",
                    UserNotSubscribedException _ =>
                        "User is no longer subscribed!\nPlease extend your subscription to resume.",
                    _ => ""
                };
            }
        }

        private void UpdateUserDetails(User user) {
            usernameLabel.Text = user.name;
            subscribedInfoLabel.Text = "Subscribed to:\n" + user.subscribed_to;
        }

        private void BindToMagingEvents() {
            magingJob.Started += OnMagingStarted;
            magingJob.Stopped += OnMagingStopped;
            magingJob.SinkChanged += OnMagingSinkChanged;
        }

        private void OnMagingSinkChanged(object sender, SinkChangedEventArgs e) {
            Invoke(new MethodInvoker(delegate { sinkValueLabel.Text = Convert.ToInt32(e.sink) + ""; }));
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

        private void OnMagingStopped(object sender, EventArgs e) {
            Invoke(new MethodInvoker(delegate { toggleMageButton.Text = "Start\n(F2)"; }));
        }

        private void OnMagingStarted(object sender, EventArgs e) {
            Invoke(new MethodInvoker(delegate { toggleMageButton.Text = "Stop\n(F2)"; }));
        }

        private void toggleMageButton_Click(object sender, EventArgs e) {
            magingJob.BeginMage(!magingJob.IsMaging);
        }

        private void helpButton_Click(object sender, EventArgs e) {
            // Open help on website
        }

        private void paintOcrIndicators(object sender, EventArgs eventArgs) {
            var g = ocrIndicatorPanel.CreateGraphics();

            var pen = new Pen(Color.Red, 2);
            g.DrawRectangle(pen, new Rectangle(626, 300, 980 - 626, 39 * 11));
            g.DrawRectangle(pen, new Rectangle(352, 137, 590 - 352, 835 - 137));
            pen.Dispose();
            g.Dispose();
        }

        private void ocrIndicatorPanel_VisibleChanged(object sender, EventArgs e) {
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

        private void MainForm_VisibleChanged(object sender, EventArgs e) {
            if (!Visible) {
                magingJob.StopMage();
                userDetailsTimer.Stop();
                authTokenRefreshTimer.Stop();
            }
        }
    }
}
