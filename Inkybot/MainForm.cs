using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Actions;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Events;
using Inkybot.Helpers;
using Inkybot.Services;
using Tesseract;
using Debug = System.Diagnostics.Debug;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;


namespace Inkybot
{
    public partial class MainForm : Form
    {
        private StatsForm statsForm;
        private ApiClient api = null!;
        private DofusMagingJob magingJob;
        private ScreenReaderDataProvider screenReader;
        private ConfigForm configForm;
        private AuthManager auth = null!;

        public MainForm() {
            InitializeComponent();
            
            magingJob = Program.Services.GetService<DofusMagingJob>();
            screenReader = (ScreenReaderDataProvider) Program.Services.GetService<DofusDataProvider>();
            
            InitOcrIndicators();
            toastPanel.Hide();
            mageInfoPanel.Hide();
            debugScreenshotButton.Hide();
            exoAttemptsLabel.Hide();
            exoAttemptsValueLabel.Hide();
            Hide();
            
            InitAuth();

            Shown += MainForm_OnLoad;

            statsForm = new StatsForm();
            statsForm.Error += OnError;
            configForm = new ConfigForm();
            magingJob.Error += OnError;
            
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
