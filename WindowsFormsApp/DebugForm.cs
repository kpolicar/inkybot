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

            
            dataGridView1.Rows.Clear();

            foreach (var line in results.Lines) {
                var pair = line.Text.Split(new [] { ' ' }, 2);
                dict[pair[1]] = pair[0];
                
                dataGridView1.Rows.Add(pair[1], pair[0]);
            }
        }
    }
}