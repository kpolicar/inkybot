using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using WindowsFormsApp.Events;

namespace WindowsFormsApp
{
    public partial class StatsForm : Form
    {
        private MainForm mainForm;
        private bool isMaging = false;
        private DofusMagus magus;


        public StatsForm(MainForm mainForm) {
            InitializeComponent();
            this.mainForm = mainForm;
            magus = (DofusMagus) Program.Services.GetService(typeof(DofusMagus));
            magus.StatsCollected += StatsUpdated;

            InitializeKeyboardShortcuts();
            
            Disposed += (sender, e) => {
                KeyboardHook.Release();
                magus.StopMage();
            };
        }

        private void InitializeKeyboardShortcuts() {
            KeyboardHook.Init();
            KeyboardHook.KeyPressed += (sender, e) => {
                if (e.KeyCode == Keys.F2)
                    button1_Click(sender, e);
            };
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

        private void button1_Click(object sender, EventArgs e) {
            magus.BeginMage(isMaging = !isMaging);
            button1.Text = isMaging ? "Stop (F2)" : "Begin (F2)";
        }
    }
}