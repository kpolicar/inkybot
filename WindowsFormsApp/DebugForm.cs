using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Media.Ocr;

namespace WindowsFormsApp
{
    public partial class DebugForm : Form
    {
        private MainForm mainForm;

        public DebugForm(MainForm mainForm) {
            InitializeComponent();
            this.mainForm = mainForm;
        }

        public void DisplayStats(Dictionary<string, string> stats) {
            dataGridView1.Rows.Clear();

            foreach (var stat in stats) {
                dataGridView1.Rows.Add(stat.Key, stat.Value);
            }
        }
    }
}