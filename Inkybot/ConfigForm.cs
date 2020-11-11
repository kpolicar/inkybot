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
            foreach (var statConfig in Stat.Stats) {
                
                if (statConfig.DisplayName == null)
                    Debug.WriteLine(statConfig.Identifier);
                
                var rowIndex = statsDataGridView.Rows.Add(
                    statConfig.DisplayName,
                    ParseConfigThreshold(statConfig.ChangeToPaRuneThreshold),
                    ParseConfigThreshold(statConfig.ChangeToRaRuneThreshold),
                    ParseConfigThreshold(statConfig.MaxValueAtWhichSmRuneCanLand),
                    ParseConfigThreshold(statConfig.MaxValueAtWhichPaRuneCanLand)
                    );
                statsDataGridView.Rows[rowIndex].Tag = statConfig;
            }
        }

        protected string ParseConfigThreshold(int threshold) {
            return threshold == int.MaxValue ? "-" : threshold.ToString();
        }
    }
}

