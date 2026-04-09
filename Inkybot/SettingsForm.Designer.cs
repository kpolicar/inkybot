using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Inkybot
{
    partial class SettingsForm
    {
        private IContainer components = null;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.itemTabPage = new System.Windows.Forms.TabPage();
            this.runesTabPage = new System.Windows.Forms.TabPage();
            this.preferencesTabPage = new System.Windows.Forms.TabPage();
            this.customizeTabPage = new System.Windows.Forms.TabPage();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
            //
            // tabControl
            //
            this.tabControl.Controls.Add(this.itemTabPage);
            this.tabControl.Controls.Add(this.runesTabPage);
            this.tabControl.Controls.Add(this.preferencesTabPage);
            this.tabControl.Controls.Add(this.customizeTabPage);
            this.tabControl.Dock = DockStyle.Fill;
            this.tabControl.Location = new Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.tabControl.DrawItem += TabControl_DrawItem;
            this.tabControl.Padding = new Point(12, 6);
            //
            // itemTabPage
            //
            this.itemTabPage.BackColor = Color.FromArgb(30, 30, 30);
            this.itemTabPage.Name = "itemTabPage";
            this.itemTabPage.Padding = new Padding(4);
            this.itemTabPage.Text = "Item";
            //
            // runesTabPage
            //
            this.runesTabPage.BackColor = Color.FromArgb(30, 30, 30);
            this.runesTabPage.Name = "runesTabPage";
            this.runesTabPage.Padding = new Padding(4);
            this.runesTabPage.Text = "Runes";
            //
            // preferencesTabPage
            //
            this.preferencesTabPage.BackColor = Color.FromArgb(30, 30, 30);
            this.preferencesTabPage.Name = "preferencesTabPage";
            this.preferencesTabPage.Padding = new Padding(4);
            this.preferencesTabPage.Text = "Preferences";
            //
            // customizeTabPage
            //
            this.customizeTabPage.BackColor = Color.FromArgb(30, 30, 30);
            this.customizeTabPage.Name = "customizeTabPage";
            this.customizeTabPage.Padding = new Padding(4);
            this.customizeTabPage.Text = "Customize";
            //
            // SettingsForm
            //
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ClientSize = new Size(800, 560);
            this.Controls.Add(this.tabControl);
            this.MinimumSize = new Size(640, 480);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Name = "SettingsForm";
            this.Text = "Setup";
            this.Closing += new CancelEventHandler(this.SettingsForm_Closing);
            this.VisibleChanged += new System.EventHandler(this.SettingsForm_VisibleChanged);
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void TabControl_DrawItem(object sender, DrawItemEventArgs e) {
            var tabPage = tabControl.TabPages[e.Index];
            var tabRect = tabControl.GetTabRect(e.Index);
            var isSelected = e.Index == tabControl.SelectedIndex;

            var backColor = isSelected ? Color.FromArgb(30, 30, 30) : Color.FromArgb(20, 20, 20);
            var foreColor = isSelected ? SystemColors.Control : SystemColors.ControlDark;

            using (var brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, tabRect);

            var format = new StringFormat {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            using (var brush = new SolidBrush(foreColor))
                e.Graphics.DrawString(tabPage.Text, tabControl.Font, brush, tabRect, format);
        }

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage itemTabPage;
        private System.Windows.Forms.TabPage runesTabPage;
        private System.Windows.Forms.TabPage preferencesTabPage;
        private System.Windows.Forms.TabPage customizeTabPage;
    }
}
