using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Inkybot.Events;

namespace Inkybot
{
    public partial class SettingsForm : Form
    {
        public event EventHandler<ExceptionEventArgs>? Error;

        private StatsForm itemTab;
        private ConfigForm runesTab;
        private CustomizeControl customizeTab;

        public int AutoShutdownDelay => runesTab.AutoShutdownDelay;

        public SettingsForm() {
            InitializeComponent();

            itemTab = new StatsForm();
            itemTab.Error += (s, e) => Error?.Invoke(s, e);
            runesTab = new ConfigForm(itemTab);
            customizeTab = new CustomizeControl();

            SetupTabs();
        }

        private void SetupTabs() {
            // Item tab
            itemTabPage.Controls.Add(itemTab);

            // Runes tab — ConfigForm without the userSettingsPanel
            runesTab.UserSettingsPanel.Parent = preferencesTabPage;
            runesTab.UserSettingsPanel.Dock = DockStyle.Top;
            runesTabPage.Controls.Add(runesTab);

            // Customize tab
            customizeTabPage.Controls.Add(customizeTab);
        }

        public void RefreshStats() {
            itemTab.RefreshStats();
        }

        private void SettingsForm_Closing(object sender, CancelEventArgs e) {
            e.Cancel = true;
            Hide();
        }

        private void SettingsForm_VisibleChanged(object sender, EventArgs e) {
            if (Visible) {
                itemTab.RefreshStats();
            }
        }
    }
}
