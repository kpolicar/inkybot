using System.Windows.Forms;
using WindowsFormsApp.Contracts;
using WindowsFormsApp.Events;

namespace WindowsFormsApp
{
    public partial class StatsForm : Form
    {
        private readonly DofusDataProvider dataProvider;
        private DofusMagingJob magingJob;
        private MainForm mainForm;

        public StatsForm(MainForm mainForm) {
            InitializeComponent();
            this.mainForm = mainForm;
            magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            dataProvider = (DofusDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            dataProvider.FetchedStats += StatsUpdated;
        }

        private void StatsUpdated(object sender, StatsEventArgs e) {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (dataGridView1.InvokeRequired) {
                StatsUpdatedCallback d = StatsUpdated;
                Invoke(d, sender, e);
            } else {
                dataGridView1.Rows.Clear();

                foreach (var stat in e.stats) dataGridView1.Rows.Add(stat.stat.DisplayName, stat.value);
            }
        }


        private delegate void StatsUpdatedCallback(object sender, StatsEventArgs e);
    }
}
