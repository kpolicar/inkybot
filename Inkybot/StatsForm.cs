using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Domain.Repositories;
using Inkybot.Events;
using Inkybot.Resources;
using Inkybot.Services;

namespace Inkybot
{
    public partial class StatsForm : Form
    {
        public event EventHandler<ExceptionEventArgs> Error;
        private readonly DofusDataProvider dataProvider;
        private DofusMagingJob magingJob;
        private MainForm mainForm;
        private ConfigManager configManager;
        private ItemStatRepository itemStats;

        public StatsForm(MainForm mainForm) {
            InitializeComponent();
            InitializeCustomComponents();
            this.mainForm = mainForm;
            magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            dataProvider = (DofusDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            configManager = (ConfigManager) Program.Services.GetService(typeof(ConfigManager));
        }

        private void StatsForm_Loaded(object sender, EventArgs e) {
            dataProvider.FetchedStats += OnStatsFetched;
            exoStatComboBox.DataSource = Stat.Stats.Select(stat => stat.DisplayName).ToArray();
        }

        private void OnStatsFetched(object sender, StatsEventArgs e) {
            itemStats = e.stats;
            Invoke(new MethodInvoker(UpdateDataGridView));
        }

        private void UpdateDataGridView() {
            if (!DataGridViewMatchesItem()) {
                configManager.ResetConfig(itemStats);
                RebuildDataGridView();
            } else {
                for (var i = 0; i < itemStats.Length; i++) {
                    dataGridView1[1,i].Value = itemStats[i].value;
                }
            }
        }

        private ItemStatRow OnRow(int index) {
            var row = dataGridView1.Rows[index];
            return (ItemStatRow) row.Tag;
        }

        private ItemStat InRepository(int index) {
            return itemStats[index];
        }

        private void SkipFallenExoRows(ref int rowIndex, int repoIndex) {
            while (OnRow(rowIndex).Exo && !InRepository(repoIndex).Exo && rowIndex < dataGridView1.Rows.Count) {
                rowIndex++;
            }
        }

        private bool DataGridViewMatchesItem() {
            var configuredCount = dataGridView1.Rows.Count;
            // All the current stats must always be configured
            if (itemStats.Length > configuredCount)
                return false;

            for (int rowIndex = 0, repoIndex = 0; rowIndex < configuredCount; rowIndex++, repoIndex++) {

                SkipFallenExoRows(ref rowIndex, repoIndex);
                if (rowIndex < configuredCount)
                    break;
                
                // If there are more configured stats, they must be exos
                if (rowIndex >= itemStats.Length) {
                    if (OnRow(rowIndex).Exo)
                        continue;
                    return false;
                }
                    
                    
                if (OnRow(rowIndex).Stat != InRepository(repoIndex).stat) {
                    return false;
                }
            }
            

            return true;
        }

        private void RebuildDataGridView() {
            dataGridView1.Rows.Clear();

            foreach (var itemStat in itemStats) {
                dataGridView1.Rows.Add(itemStat.stat.DisplayName, itemStat.value, itemStat.max);
                var index = dataGridView1.Rows.Count-1;
                var row = dataGridView1.Rows[index];
                row.Tag = new ItemStatRow(itemStat);
            }
        }

        private delegate void StatsUpdatedCallback(object sender, StatsEventArgs e);

        private void StatsForm_VisibleChanged(object sender, EventArgs e) {
            if (!Visible || magingJob.IsMaging) return;

            var fetchStats = new ThreadStart(delegate {
                try {
                    dataProvider.FetchData();
                    dataProvider.Stats();
                } catch (Exception exception) {
                    Error?.Invoke(this, new ExceptionEventArgs(exception));
                    Debug.WriteLine(exception.Message);
                    Debug.WriteLine(exception.StackTrace);
                }
            });
            
            new Thread(fetchStats).Start();
        }

        private void StatsForm_Closing(object sender, CancelEventArgs cancelEventArgs) {
            cancelEventArgs.Cancel = true;
            Hide();
        }

        private void StatsForm_OnChangeValue(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex != 2) return;
            var row = dataGridView1.Rows[e.RowIndex];

            var statRow = (ItemStatRow) row.Tag;
            var stat = statRow.Stat;
            
            var max = int.Parse(row.Cells[e.ColumnIndex].Value.ToString());
            var statConfig = new StatConfig(stat.changeToPaRuneThreshold, stat.changeToRaRuneThreshold, max);

            configManager.ChangeStatConfig(stat, statConfig);
        }

        private class ItemStatRow
        {
            public readonly Stat Stat;
            public readonly bool Exo;

            public ItemStatRow() {
                Exo = true;
            }

            public ItemStatRow(ItemStat itemStat) {
                Stat = itemStat.stat;
                Exo = itemStat.Exo;
            }

            public static bool operator ==(ItemStatRow operand1, ItemStatRow operand2) {
                return operand1.Stat == operand2.Stat && operand1.Exo == operand2.Exo;
            }

            public static bool operator !=(ItemStatRow operand1, ItemStatRow operand2) {
                return !(operand1 == operand2);
            }
        }

        private void exoStatComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            
        }

        private void addExoButton_Click(object sender, EventArgs e) {
            throw new NotImplementedException();
        }
    }
}
