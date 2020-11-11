using System;
using System.Diagnostics;
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
                Debug.WriteLine(statConfig.ChangeToPaRuneThreshold);
                
                // var rowIndex = statsDataGridView.Rows.Add(
                //     statConfig.DisplayName,
                //     ParseConfigThreshold(statConfig.ChangeToPaRuneThreshold),
                //     ParseConfigThreshold(statConfig.ChangeToRaRuneThreshold),
                //     ParseConfigThreshold(statConfig.MaxValueAtWhichSmRuneCanHit),
                //     ParseConfigThreshold(statConfig.MaxValueAtWhichPaRuneCanHit)
                //     );
                // statsDataGridView.Rows[rowIndex].Tag = statConfig;
            }
        }

        protected string ParseConfigThreshold(int threshold) {
            return threshold == int.MaxValue ? "-" : threshold.ToString();
        }
    }
}

