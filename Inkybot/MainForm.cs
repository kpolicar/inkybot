using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Actions;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Events;
using Inkybot.Helpers;
using Inkybot.Services;
using Tesseract;
using Debug = System.Diagnostics.Debug;


namespace Inkybot
{
    public partial class MainForm : Form
    {
        private StatsForm statsForm;
        private readonly ApiClient api;
        private readonly DofusMagingJob magingJob;
        private ConfigForm configForm;

        public MainForm() {
            InitializeComponent();
            toastPanel.Hide();
            mageInfoPanel.Hide();
            debugScreenshotButton.Hide();
            exoAttemptsLabel.Hide();
            exoAttemptsValueLabel.Hide();
            
            InitializeDofusClient();

            var dataProvider = (ScreenReaderDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            dataProvider.BindTo(hWndDocked);
            var mouse = (Win32Input) Program.Services.GetService(typeof(Input));
            mouse.SetRelativeToHandle(hWndDocked);
            
            var actions = (MouseActionFactory) Program.Services.GetService(typeof(ActionFactory));
            actions.setRelativeToControl(dofusClientPanel);

            statsForm = new StatsForm(this);
            configForm = new ConfigForm();
            
            api = (ApiClient) Program.Services.GetService(typeof(ApiClient));
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
                toastLabel.Text = e.Message;
                toastPanel.Show();
                toastPanel.BringToFront();
            }));
        }

        private void configButton_Click(object sender, EventArgs e) {
            configForm.Show();
        }
    }
}
