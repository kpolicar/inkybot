using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Events;
using Inkybot.Services;

namespace Inkybot
{
    public partial class StatsForm : Form
    {
        private readonly DofusDataProvider dataProvider;
        private DofusMagingJob magingJob;
        private MainForm mainForm;
        private ConfigManager configManager;

        private Item.ItemStat[] _stats;
        private Item.ItemStat[] Stats {
            get => _stats;
            set => UpdateDataGridView(_stats = value);
        }

        private Config _config;
        private Config Config {
            get => _config;
            set => _config = value;
        }

        public StatsForm(MainForm mainForm) {
            InitializeComponent();
            this.mainForm = mainForm;
            magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            dataProvider = (DofusDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            configManager = (ConfigManager) Program.Services.GetService(typeof(ConfigManager));
            dataProvider.FetchedStats += (sender, args) => Stats = args.stats;
        }

        private void StatsUpdated(object sender, StatsEventArgs e) {
            configManager.EnforceConfigSetForStats(e.stats);
            if (!Visible) return;
            
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (dataGridView1.InvokeRequired) {
                StatsUpdatedCallback d = StatsUpdated;
                Invoke(d, sender, e);
            } else {
                UpdateDataGridView(e.stats);
            }
        }

        private void UpdateDataGridView(Item.ItemStat[] itemStats) {
            if (!DataGridViewMatchesItem(itemStats)) {
                RebuildDataGridView(itemStats);
            } else {
                for (var i = 0; i < itemStats.Length; i++) {
                    dataGridView1[1,i].Value = itemStats[i].value;
                }
            }
        }

        private bool DataGridViewMatchesItem(Item.ItemStat[] itemStats) {
            if (itemStats.Length != dataGridView1.Rows.Count) return false;

            for (var i = 0; i < itemStats.Length; i++) {
                if (itemStats[i].stat.DisplayName != dataGridView1[0,i].Value.ToString()) {
                    return false;
                }
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
            dataProvider.Stats();
            //magingJob.ChangeConfig(new Config(stats));
            // magingJob.Config.For(stat).maximum
        }

        private void StatsForm_Closing(object sender, CancelEventArgs cancelEventArgs) {
            cancelEventArgs.Cancel = true;
            Hide();
        }
    }
}
