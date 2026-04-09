using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Inkybot
{
    partial class CustomizeControl
    {
        private IContainer components = null;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.splitContainer = new SplitContainer();
            this.chatPanel = new Panel();
            this.chatHistoryTextBox = new RichTextBox();
            this.chatInputPanel = new Panel();
            this.chatInputTextBox = new TextBox();
            this.sendButton = new Button();
            this.codePanel = new Panel();
            this.scriptTabControl = new TabControl();
            this.codeTopPanel = new Panel();
            this.statusLabel = new Label();
            this.codeBottomPanel = new Panel();
            this.applyButton = new Button();
            this.saveButton = new Button();

            ((ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.SuspendLayout();
            this.chatPanel.SuspendLayout();
            this.chatInputPanel.SuspendLayout();
            this.codePanel.SuspendLayout();
            this.codeTopPanel.SuspendLayout();
            this.codeBottomPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // splitContainer
            //
            this.splitContainer.Dock = DockStyle.Fill;
            this.splitContainer.SplitterDistance = 300;
            this.splitContainer.SplitterWidth = 3;
            this.splitContainer.Panel1.Controls.Add(this.chatPanel);
            this.splitContainer.Panel2.Controls.Add(this.codePanel);
            this.splitContainer.Panel1MinSize = 200;
            this.splitContainer.Panel2MinSize = 200;
            this.splitContainer.BackColor = Color.FromArgb(20, 20, 20);
            //
            // chatPanel
            //
            this.chatPanel.Dock = DockStyle.Fill;
            this.chatPanel.Controls.Add(this.chatHistoryTextBox);
            this.chatPanel.Controls.Add(this.chatInputPanel);
            //
            // chatHistoryTextBox
            //
            this.chatHistoryTextBox.Dock = DockStyle.Fill;
            this.chatHistoryTextBox.BackColor = Color.FromArgb(25, 25, 25);
            this.chatHistoryTextBox.ForeColor = Color.FromArgb(220, 220, 220);
            this.chatHistoryTextBox.Font = new Font("Segoe UI", 9.5f);
            this.chatHistoryTextBox.BorderStyle = BorderStyle.None;
            this.chatHistoryTextBox.ReadOnly = true;
            this.chatHistoryTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            //
            // chatInputPanel
            //
            this.chatInputPanel.Dock = DockStyle.Bottom;
            this.chatInputPanel.Height = 36;
            this.chatInputPanel.Padding = new Padding(0, 4, 0, 0);
            this.chatInputPanel.Controls.Add(this.chatInputTextBox);
            this.chatInputPanel.Controls.Add(this.sendButton);
            //
            // chatInputTextBox
            //
            this.chatInputTextBox.Dock = DockStyle.Fill;
            this.chatInputTextBox.BackColor = Color.FromArgb(40, 40, 40);
            this.chatInputTextBox.ForeColor = Color.FromArgb(220, 220, 220);
            this.chatInputTextBox.Font = new Font("Segoe UI", 9.5f);
            this.chatInputTextBox.BorderStyle = BorderStyle.FixedSingle;
            this.chatInputTextBox.KeyDown += new KeyEventHandler(this.chatInputTextBox_KeyDown);
            //
            // sendButton
            //
            this.sendButton.Dock = DockStyle.Right;
            this.sendButton.Width = 60;
            this.sendButton.Text = "Send";
            this.sendButton.FlatStyle = FlatStyle.Flat;
            this.sendButton.FlatAppearance.BorderSize = 0;
            this.sendButton.BackColor = Color.FromArgb(50, 50, 50);
            this.sendButton.ForeColor = SystemColors.Control;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            //
            // codePanel
            //
            this.codePanel.Dock = DockStyle.Fill;
            this.codePanel.Controls.Add(this.scriptTabControl);
            this.codePanel.Controls.Add(this.codeBottomPanel);
            this.codePanel.Controls.Add(this.codeTopPanel);
            //
            // codeTopPanel
            //
            this.codeTopPanel.Dock = DockStyle.Top;
            this.codeTopPanel.Height = 24;
            this.codeTopPanel.BackColor = Color.FromArgb(20, 20, 20);
            this.codeTopPanel.Controls.Add(this.statusLabel);
            //
            // statusLabel
            //
            this.statusLabel.Dock = DockStyle.Right;
            this.statusLabel.Width = 120;
            this.statusLabel.TextAlign = ContentAlignment.MiddleRight;
            this.statusLabel.Font = new Font("Segoe UI", 9f);
            this.statusLabel.ForeColor = Color.FromArgb(140, 140, 140);
            this.statusLabel.Text = "";
            this.statusLabel.Padding = new Padding(0, 0, 6, 0);
            //
            // scriptTabControl
            //
            this.scriptTabControl.Dock = DockStyle.Fill;
            this.scriptTabControl.ShowToolTips = true;
            this.scriptTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.scriptTabControl.DrawItem += ScriptTabControl_DrawItem;
            this.scriptTabControl.Padding = new Point(8, 4);
            this.scriptTabControl.SelectedIndexChanged += new System.EventHandler(this.scriptTabControl_SelectedIndexChanged);
            //
            // codeBottomPanel
            //
            this.codeBottomPanel.Dock = DockStyle.Bottom;
            this.codeBottomPanel.Height = 34;
            this.codeBottomPanel.Padding = new Padding(0, 4, 0, 0);
            this.codeBottomPanel.Controls.Add(this.saveButton);
            this.codeBottomPanel.Controls.Add(this.applyButton);
            //
            // applyButton
            //
            this.applyButton.Dock = DockStyle.Left;
            this.applyButton.Width = 80;
            this.applyButton.Text = "Apply";
            this.applyButton.FlatStyle = FlatStyle.Flat;
            this.applyButton.FlatAppearance.BorderSize = 0;
            this.applyButton.BackColor = Color.FromArgb(40, 80, 40);
            this.applyButton.ForeColor = SystemColors.Control;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            //
            // saveButton
            //
            this.saveButton.Dock = DockStyle.Left;
            this.saveButton.Width = 90;
            this.saveButton.Text = "Save as .cs";
            this.saveButton.FlatStyle = FlatStyle.Flat;
            this.saveButton.FlatAppearance.BorderSize = 0;
            this.saveButton.BackColor = Color.FromArgb(50, 50, 50);
            this.saveButton.ForeColor = SystemColors.Control;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            //
            // CustomizeControl
            //
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.Controls.Add(this.splitContainer);
            this.Dock = DockStyle.Fill;
            this.Name = "CustomizeControl";

            ((ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.chatPanel.ResumeLayout(false);
            this.chatInputPanel.ResumeLayout(false);
            this.chatInputPanel.PerformLayout();
            this.codePanel.ResumeLayout(false);
            this.codeTopPanel.ResumeLayout(false);
            this.codeBottomPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void ScriptTabControl_DrawItem(object sender, DrawItemEventArgs e) {
            var tabPage = scriptTabControl.TabPages[e.Index];
            var tabRect = scriptTabControl.GetTabRect(e.Index);
            var isSelected = e.Index == scriptTabControl.SelectedIndex;

            var backColor = isSelected ? Color.FromArgb(30, 30, 30) : Color.FromArgb(20, 20, 20);
            var foreColor = isSelected ? SystemColors.Control : SystemColors.ControlDark;

            using (var brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, tabRect);

            var format = new StringFormat {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            using (var brush = new SolidBrush(foreColor))
                e.Graphics.DrawString(tabPage.Text, scriptTabControl.Font, brush, tabRect, format);
        }

        private SplitContainer splitContainer;
        private Panel chatPanel;
        private RichTextBox chatHistoryTextBox;
        private Panel chatInputPanel;
        private TextBox chatInputTextBox;
        private Button sendButton;
        private Panel codePanel;
        private TabControl scriptTabControl;
        private Panel codeTopPanel;
        private Label statusLabel;
        private Panel codeBottomPanel;
        private Button applyButton;
        private Button saveButton;
    }
}
