using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

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
        }

        protected string ParseConfigThreshold(int threshold) {
            return threshold == int.MaxValue ? "-" : threshold.ToString();
        }

        private void ConfigForm_OnChangeValue(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex < 1 || e.ColumnIndex > 4 || e.RowIndex < 0) return;
            var row = statsDataGridView.Rows[e.RowIndex];

            var stat = (Stat) row.Tag;
            switch (e.ColumnIndex) {
                case 1:
                    stat.ChangeToPaRuneThreshold = int.Parse(row.Cells[e.ColumnIndex].Value.ToString());
                    break;
                case 2:
                    stat.ChangeToRaRuneThreshold = int.Parse(row.Cells[e.ColumnIndex].Value.ToString());
                    break;
                case 3:
                    stat.MaxValueAtWhichSmRuneCanLand = int.Parse(row.Cells[e.ColumnIndex].Value.ToString());
                    break;
                case 4:
                    stat.MaxValueAtWhichPaRuneCanLand = int.Parse(row.Cells[e.ColumnIndex].Value.ToString());
                    break;
            }
        }
    }
}

