using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Dofus.Contracts;
using Inkybot.Events;
using Inkybot.Services;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;
using Win32Input = Inkybot.Services.Win32Input.Win32Input;


namespace Inkybot
{
    public partial class MainForm : Form
    {
        private SettingsForm settingsForm;
        private AnalyticsReporter analytics;
        private ApiClient api = null!;
        private MageQueueManager mageQueue;
        private DofusMagingJob magingJob;
        private ScreenReaderDataProvider screenReader;
        private AuthManager auth = null!;
        private ConfigManager config;
        private StatisticsForm statisticsForm;
        private MageQueueForm mageQueueForm;
        private int autoShutdownTimeElapsed;
        private ActionHandler actions;
        private ActionFactory actionFactory;
        private Win32Input win32Input;

        public MainForm() {
            InitializeComponent();
            Text += $" ({Program.Version})";
            
            magingJob = Program.Services.GetService<DofusMagingJob>();
            screenReader = (ScreenReaderDataProvider) Program.Services.GetService<DofusDataProvider>();
            config = (ConfigManager) Program.Services.GetService<MageConfigManager>();
            analytics = Program.Services.GetService<AnalyticsReporter>();
            mageQueue = Program.Services.GetService<MageQueueManager>();
            actions = Program.Services.GetService<ActionHandler>();
            actionFactory = Program.Services.GetService<ActionFactory>();
            win32Input = (Win32Input) Program.Services.GetService<Input>();
            var userSettingsConfigManager = (FileSystemUserSettingsConfigManager)
                Program.Services.GetService<UserSettingsConfigManager>();
            
            InitOcrIndicators();
            toastPanel.Hide();
            debugScreenshotButton.Hide();
            Hide();
            
            InitAuth();

            Shown += MainForm_OnLoad;

            settingsForm = new SettingsForm();
            settingsForm.Error += OnError;
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

        private bool dontLogoutOnVisibleChanged = false;

        private void MainForm_OnLoad(object sender, EventArgs eventArgs) {
            dontLogoutOnVisibleChanged = true;
            var success = DoLoginDialog();
            if (!success)
                return;
            
            Hide();
            var openedDofusSuccessfully = InitializeDofusClient();
            if (!openedDofusSuccessfully) {
                Close();
            }

            Show();
            dontLogoutOnVisibleChanged = false;
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
