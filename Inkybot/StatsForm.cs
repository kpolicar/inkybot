using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Events;

namespace Inkybot
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
            if (!Visible) return;
            
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (dataGridView1.InvokeRequired) {
                StatsUpdatedCallback d = StatsUpdated;
                Invoke(d, sender, e);
            } else {
                EnforceDataGridMatchesStats(e.stats);
            }
        }

        private void EnforceDataGridMatchesStats(Item.ItemStat[] itemStats) {
            if (DataGridMatchesStats(itemStats)) return;
            
            magingJob.ChangeConfig(new Config(itemStats));
            RebuildDataGridView(itemStats);
        }

        private bool DataGridMatchesStats(Item.ItemStat[] itemStats) {
            if (itemStats.Length != dataGridView1.Rows.Count) return false;
            
            var i = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows) {
                if (row.ToString() != itemStats[i].stat.DisplayName)
                    return false;
                ++i;
            }
            return true;
        }

        private void RebuildDataGridView(Item.ItemStat[] itemStats) {
            dataGridView1.Rows.Clear();
            
            foreach (var stat in itemStats)
                dataGridView1.Rows.Add(stat.stat.DisplayName, stat.value, stat.max);
        }

        private delegate void StatsUpdatedCallback(object sender, StatsEventArgs e);

        private void StatsForm_VisibleChanged(object sender, EventArgs e) {
            if (!Visible) return;
            
            dataProvider.FetchData();
            var stats = dataProvider.Stats();
            //magingJob.ChangeConfig(new Config(stats));
            // magingJob.Config.For(stat).maximum
        }
    }
}
