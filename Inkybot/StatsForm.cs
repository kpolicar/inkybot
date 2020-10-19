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

        public StatsForm(MainForm mainForm) {
            InitializeComponent();
            this.mainForm = mainForm;
            magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            dataProvider = (DofusDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            configManager = (ConfigManager) Program.Services.GetService(typeof(ConfigManager));
        }

        private void StatsForm_Loaded(object sender, EventArgs e) {
            dataProvider.FetchedStats += OnStatsFetched;
        }

        private void OnStatsFetched(object sender, StatsEventArgs e) {
            Invoke(new MethodInvoker(delegate {
                UpdateDataGridView(e.stats);
            }));
        }

        private void UpdateDataGridView(Item.ItemStat[] itemStats) {
            if (!DataGridViewMatchesItem(itemStats)) {
                configManager.ResetConfig(itemStats);
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
            if (!Visible || magingJob.IsMaging) return;

            dataProvider.FetchData();
            dataProvider.Stats();
        }

        private void StatsForm_Closing(object sender, CancelEventArgs cancelEventArgs) {
            cancelEventArgs.Cancel = true;
            Hide();
        }

        private void StatsForm_OnChangeValue(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex != 2) return;
            var row = dataGridView1.Rows[e.RowIndex];
            
            var stat = Stat.Stats.First(stat => stat.DisplayName == row.Cells[0].Value.ToString());
            
            var max = int.Parse(row.Cells[e.ColumnIndex].Value.ToString());
            var statConfig = new StatConfig(stat.changeToPaRuneThreshold, stat.changeToRaRuneThreshold, max);

            configManager.ChangeStatConfig(stat, statConfig);
        }
    }
}
