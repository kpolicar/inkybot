using System;
using System.Windows.Forms;

namespace Inkybot
{
    public partial class WaitingForDofusForm : Form
    {
        public WaitingForDofusForm() {
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
