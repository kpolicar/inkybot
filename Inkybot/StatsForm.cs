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
        private bool hasExoRows;

        public StatsForm(MainForm mainForm) {
            InitializeComponent();
            InitializeCustomComponents();
            this.mainForm = mainForm;
            magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            dataProvider = (DofusDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            configManager = (ConfigManager) Program.Services.GetService(typeof(ConfigManager));

            StatColumn.DataSource = Stat.Stats.Select(stat => stat.DisplayName).ToArray();
        }

        private void StatsForm_Loaded(object sender, EventArgs e) {
            dataProvider.FetchedStats += OnStatsFetched;
        }

        private void OnStatsFetched(object sender, StatsEventArgs e) {
            Invoke(new MethodInvoker(delegate {
                UpdateDataGridView(e.stats);
            }));
        }

        private void UpdateDataGridView(ItemStatRepository itemStats) {
            if (!DataGridViewMatchesItem(itemStats)) {
                configManager.ResetConfig(itemStats);
                RebuildDataGridView(itemStats);
            } else {
                for (var i = 0; i < itemStats.Length; i++) {
                    dataGridView1[1,i].Value = itemStats[i].value;
                }
            }
        }

        private bool DataGridViewMatchesItem(ItemStatRepository itemStats) {
            var configuredCount = dataGridView1.Rows.Count;
            if (itemStats.Length > configuredCount)
                return false;

            for (var i = 0; i < configuredCount; i++) {
                var row = dataGridView1.Rows[i];
                var itemStat = (ItemStatRow) row.Tag;
                
                if (i >= itemStats.Length) {
                    if (itemStat.Exo)
                        continue;
                    return false;
                }
                
                if (itemStats[i].stat != itemStat.Stat) {
                    return false;
                }
            }
            

            return true;
        }

        private void RebuildDataGridView(ItemStatRepository itemStats) {
            dataGridView1.Rows.Clear();
            hasExoRows = false;

            foreach (var itemStat in itemStats) {
                dataGridView1.Rows.Add(itemStat.stat.DisplayName, itemStat.value, itemStat.max);
                var index = dataGridView1.Rows.Count-1;
                var row = dataGridView1.Rows[index];
                row.Tag = new ItemStatRow(itemStat);
                
                hasExoRows = itemStat.Exo || hasExoRows;
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
            
            var stat = Stat.Stats.First(stat => stat.DisplayName == row.Cells[0].Value.ToString());
            
            var max = int.Parse(row.Cells[e.ColumnIndex].Value.ToString());
            var statConfig = new StatConfig(stat.changeToPaRuneThreshold, stat.changeToRaRuneThreshold, max);

            configManager.ChangeStatConfig(stat, statConfig);
        }

        private void addExoButton_Click(object sender, EventArgs e) {
            AddExoRow();
        }

        private void AddExoRow() {
            dataGridView1.Rows.Add(Stat.Stats.First().DisplayName, 0, 0);
            
            var index = dataGridView1.Rows.Count-1;
            var row = dataGridView1.Rows[index];
            row.Tag = new ItemStatRow();
            row.DefaultCellStyle = exoCellStyle;
            
            var statCell = row.Cells[0] as DataGridViewComboBoxCell;
            statCell.ReadOnly = false;
            statCell.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
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
    }
}
