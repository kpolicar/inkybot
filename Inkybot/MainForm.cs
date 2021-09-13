using System;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Dofus.Contracts;
using Inkybot.Events;
using Inkybot.Services;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;


namespace Inkybot
{
    public partial class MainForm : Form
    {
        private StatsForm setupForm;
        private AnalyticsReporter analytics;
        private ApiClient api = null!;
        private MageQueueManager mageQueue;
        private DofusMagingJob magingJob;
        private ScreenReaderDataProvider screenReader;
        private ConfigForm configForm;
        private AuthManager auth = null!;
        private ConfigManager config;
        private StatisticsForm statisticsForm;
        private MageQueueForm mageQueueForm;
        private int autoShutdownTimeElapsed;

        public MainForm() {
            InitializeComponent();
            Text += $" ({Program.Version})";
            
            magingJob = Program.Services.GetService<DofusMagingJob>();
            screenReader = (ScreenReaderDataProvider) Program.Services.GetService<DofusDataProvider>();
            config = (ConfigManager) Program.Services.GetService<MageConfigManager>();
            analytics = Program.Services.GetService<AnalyticsReporter>();
            mageQueue = Program.Services.GetService<MageQueueManager>();
            var userSettingsConfigManager = (FileSystemUserSettingsConfigManager)
                Program.Services.GetService<UserSettingsConfigManager>();
            
            InitOcrIndicators();
            toastPanel.Hide();
            debugScreenshotButton.Hide();
            Hide();
            
            InitAuth();

            Shown += MainForm_OnLoad;

            setupForm = new StatsForm();
            setupForm.Error += OnError;
            configForm = new ConfigForm(setupForm);
            magingJob.Error += OnError;
            statisticsForm = new StatisticsForm();
            mageQueueForm = new MageQueueForm();
            
            MainFormDomainEvents();
            MainFormEvents();
        }
        
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // WM_SYSCOMMAND
            if (m.Msg == 0x00A3) { // WM_NCLBUTTONDBLCLK
                OnResizeEnd(EventArgs.Empty);
            }
            
            if (m.Msg == 0x0112)
            {
                if (m.WParam == new IntPtr(0xF030) // Maximize event - SC_MAXIMIZE from Winuser.h
                    || m.WParam == new IntPtr(0xF120)) // Restore event - WM_NCLBUTTONDBLCLK from Winuser.h
                {
                    OnResizeEnd(EventArgs.Empty);
                }
                if (m.WParam == new IntPtr(0xF020)) // Maximize event - SC_MINIMIZE from Winuser.h
                {
                    OnMinimize();
                }
            }
        }

        private void OnMinimize() {
            magingJob.StopMage();
        }

        private void MainForm_OnLoad(object sender, EventArgs eventArgs) {
            Hide();
            var openedDofusSuccessfully = InitializeDofusClient();
            if (!openedDofusSuccessfully) {
                Close();
                return;
            }
            DoLoginDialog();
        }

        private void OnError(object sender, ExceptionEventArgs e) {
            Invoke(new MethodInvoker(delegate {
                toastLabel.Text = e.Message;
                toastPanel.Show();
                toastPanel.BringToFront();
            }));
        }
    }
}
