using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Inkybot.Domain;

namespace Inkybot
{
    public partial class ConfigForm : Form
    {
        public ConfigForm() {
            InitializeComponent();
            //var ini = Stat.Stats.Where(sta => sta.Identifier == "initiative").First();
        }

        public void ConfigForm_OnLoad(object sender, EventArgs eventArgs) {
            foreach (var stat in Stat.Stats) {
                
                if (stat.DisplayName == null)
                    Debug.WriteLine(stat.Identifier);
                
                var rowIndex = statsDataGridView.Rows.Add(
                    stat.DisplayName,
                    ParseConfigThreshold(stat.ChangeToPaRuneThreshold),
                    ParseConfigThreshold(stat.ChangeToRaRuneThreshold),
                    ParseConfigThreshold(stat.MaxValueAtWhichSmRuneCanLand),
                    ParseConfigThreshold(stat.MaxValueAtWhichPaRuneCanLand)
                    );
                statsDataGridView.Rows[rowIndex].Tag = stat;
            }

            restoreHighSinkStatsCheckbox.Checked = Properties.Settings.Default.restoreHighSinkStatImmediately;
        }

        protected string ParseConfigThreshold(int threshold) {
            return threshold == int.MaxValue ? "-" : threshold.ToString();
        }

        private void ConfigForm_OnChangeValue(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex < 1 || e.ColumnIndex > 4 || e.RowIndex < 0) return;
            var row = statsDataGridView.Rows[e.RowIndex];
            var cell = row.Cells[e.ColumnIndex];

            var stat = (Stat) row.Tag;
            try {
                switch (e.ColumnIndex) {
                    case 1:
                        stat.ChangeToPaRuneThreshold = int.Parse(cell.Value.ToString());
                        break;
                    case 2:
                        stat.ChangeToRaRuneThreshold = int.Parse(cell.Value.ToString());
                        break;
                    case 3:
                        stat.MaxValueAtWhichSmRuneCanLand = int.Parse(cell.Value.ToString());
                        break;
                    case 4:
                        stat.MaxValueAtWhichPaRuneCanLand = int.Parse(cell.Value.ToString());
                        break;
                }
            } catch (FormatException) {
                
            }
        }

        private void ConfigForm_OnValidatingValue(object sender, DataGridViewCellValidatingEventArgs e) {
            if (e.ColumnIndex < 1 || e.ColumnIndex > 4 || e.RowIndex < 0) return;
            var row = statsDataGridView.Rows[e.RowIndex];
            var cell = row.Cells[e.ColumnIndex];

            var isNumber = int.TryParse(cell.Value.ToString(), out _);
            e.Cancel = !isNumber;
        }

        private void ConfigForm_OnRestoreHighSinkStatsCheckboxCheckedChanged(object sender, EventArgs e) {
            Properties.Settings.Default.restoreHighSinkStatImmediately = restoreHighSinkStatsCheckbox.Checked;
            Properties.Settings.Default.Save();
        }

        private void ConfigForm_Closing(object sender, CancelEventArgs cancelEventArgs) {
            cancelEventArgs.Cancel = true;
            Hide();
        }
    }
}

