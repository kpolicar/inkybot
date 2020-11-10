using System;
using System.Windows.Forms;

namespace Inkybot
{
    public partial class ConfigForm : Form
    {
        public ConfigForm() {
            InitializeComponent();
        }

        public void ConfigForm_OnLoad(object sender, EventArgs eventArgs) {
            foreach (var statConfig in Stat.Stats) {
                
                var rowIndex = statsDataGridView.Rows.Add(
                    statConfig.DisplayName,
                    ParseConfigThreshold(statConfig.changeToPaRuneThreshold),
                    ParseConfigThreshold(statConfig.changeToRaRuneThreshold),
                    ParseConfigThreshold(statConfig.maxValueSmRuneCanHit),
                    ParseConfigThreshold(statConfig.maxValuePaRuneCanHit)
                    );
                statsDataGridView.Rows[rowIndex].Tag = statConfig;
            }
        }

        protected string ParseConfigThreshold(int threshold) {
            return threshold == int.MaxValue ? "-" : threshold.ToString();
        }
    }
}

