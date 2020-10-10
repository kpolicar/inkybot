using System;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Forms;
using WindowsFormsApp.Api;

namespace WindowsFormsApp
{
    public partial class DofusPathForm : Form
    {
        public DofusPathForm() {
            InitializeComponent();
        }

        private void pathChangeButton_Click(object sender, EventArgs e) {
            var result = dofusFileDialog.ShowDialog();
            if (result == DialogResult.OK) {
                Properties.Settings.Default.dofusPath = dofusFileDialog.FileName;
                Properties.Settings.Default.Save();
                DialogResult = DialogResult.OK;
            }
        }
    }
}
