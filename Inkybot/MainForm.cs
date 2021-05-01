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
        private ApiClient api = null!;
        private DofusMagingJob magingJob;
        private ScreenReaderDataProvider screenReader;
        private ConfigForm configForm;
        private AuthManager auth = null!;
        private ConfigManager config;
        private StatisticsForm statisticsForm;

        public MainForm() {
            InitializeComponent();
            Text += $" ({Program.Version})";
            
            magingJob = Program.Services.GetService<DofusMagingJob>();
            screenReader = (ScreenReaderDataProvider) Program.Services.GetService<DofusDataProvider>();
            config = (ConfigManager) Program.Services.GetService<MageConfigManager>();
            
            InitOcrIndicators();
            toastPanel.Hide();
            mageInfoPanel.Hide();
            debugScreenshotButton.Hide();
            exoAttemptsLabel.Hide();
            exoAttemptsValueLabel.Hide();
            Hide();
            
            InitAuth();

            Shown += MainForm_OnLoad;

            setupForm = new StatsForm();
            setupForm.Error += OnError;
            configForm = new ConfigForm();
            magingJob.Error += OnError;
            statisticsForm = new StatisticsForm();
            
            MainFormDomainEvents();
            MainFormEvents();
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
