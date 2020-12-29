using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Inkybot.Domain;
using Inkybot.Helpers;

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
            autoRestartBotCheckbox.Checked = Properties.Settings.Default.autoRestartBot;
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
                        stat.ChangeToPaRuneThreshold = Numbers.Parse(cell.Value.ToString()) ?? int.MaxValue;
                        break;
                    case 2:
                        stat.ChangeToRaRuneThreshold = Numbers.Parse(cell.Value.ToString()) ?? int.MaxValue;
                        break;
                    case 3:
                        stat.MaxValueAtWhichSmRuneCanLand = Numbers.Parse(cell.Value.ToString()) ?? int.MaxValue;
                        break;
                    case 4:
                        stat.MaxValueAtWhichPaRuneCanLand = Numbers.Parse(cell.Value.ToString()) ?? int.MaxValue;
                        break;
                }
            } catch (FormatException) {
                
            }
        }

        private void ConfigForm_OnRestoreHighSinkStatsCheckboxCheckedChanged(object sender, EventArgs e) {
            Properties.Settings.Default.restoreHighSinkStatImmediately = restoreHighSinkStatsCheckbox.Checked;
            Properties.Settings.Default.Save();
        }

        private void ConfigForm_Closing(object sender, CancelEventArgs cancelEventArgs) {
            cancelEventArgs.Cancel = true;
            Hide();
        }

        private void ConfigForm_OnAutoRestartBotCheckboxCheckedChanged(object sender, EventArgs e) {
            Properties.Settings.Default.autoRestartBot = autoRestartBotCheckbox.Checked;
            Properties.Settings.Default.Save();
        }
    }
}

