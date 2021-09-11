using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Inkybot.Resources;

namespace Inkybot
{
    public partial class MageQueueForm : Form
    {
        public MageQueueForm() {
            InitializeComponent();
        }

        private void MageQueueForm_Loaded(object sender, EventArgs e) {
            LoadPresetsToComboBox();
        }

        private void LoadPresetsToComboBox() {
            var presets = Properties.Settings.Default.presets?.Presets ?? new ItemPreset[] {};
            
            statPresetComboBox.DataSource =
                presets.Select(preset => preset.Name)
                    .Prepend("Default")
                    .ToArray();
        }
    }
}

