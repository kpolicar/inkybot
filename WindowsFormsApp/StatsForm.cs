using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp.Contracts;
using WindowsFormsApp.Events;

namespace WindowsFormsApp
{
    public partial class StatsForm : Form
    {
        private MainForm mainForm;
        private DofusMagingJob magingJob;
        private DofusDataProvider dataProvider;

        public StatsForm(MainForm mainForm) {
            InitializeComponent();
            this.mainForm = mainForm;
            magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            dataProvider = (DofusDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            dataProvider.FetchedStats += StatsUpdated;
        }


        delegate void StatsUpdatedCallback(object sender, StatsEventArgs e);
        private void StatsUpdated(object sender, StatsEventArgs e)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (dataGridView1.InvokeRequired)
            { 
                StatsUpdatedCallback d = StatsUpdated;
                Invoke(d, sender, e);
            }
            else
            {
                dataGridView1.Rows.Clear();

                foreach (var stat in e.stats) {
                    dataGridView1.Rows.Add(stat.stat.DisplayName, stat.value);
                }
            }
        }
    }
}