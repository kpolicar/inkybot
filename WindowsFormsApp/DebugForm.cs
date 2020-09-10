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

        public void DisplayStats(OcrResult results) {
            IDictionary<string, string> dict = new Dictionary<string, string>();

            tableLayoutPanel1.RowCount = 0;
            tableLayoutPanel1.Controls.Clear();

            foreach (var line in results.Lines) {
                var pair = line.Text.Split(new char[] { ' ' }, 2);
                dict[pair[1]] = pair[0];
                
                //add a new RowStyle as a copy of the previous one
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / results.Lines.Count));
                tableLayoutPanel1.Controls.Add(new Label() { Text = pair[1] }, 0, tableLayoutPanel1.RowCount);
                tableLayoutPanel1.Controls.Add(new Label() { Text = pair[0] }, 1, tableLayoutPanel1.RowCount);
                tableLayoutPanel1.Controls.Add(new NumericUpDown() { Maximum = 1000, Margin = new Padding {All = 0}}, 2, tableLayoutPanel1.RowCount);
                //increase panel rows count by one
                tableLayoutPanel1.RowCount++;
            }
        }
    }
}