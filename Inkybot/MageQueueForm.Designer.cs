using System.ComponentModel;

namespace Inkybot
{
    partial class MageQueueForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.groupBox1 = new Inkybot.Controls.GroupBox();
            this.buttonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.moveUpButton = new System.Windows.Forms.Button();
            this.moveDownButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.configPresetPanel = new System.Windows.Forms.Panel();
            this.configPresetComboBox = new Inkybot.Controls.ComboBox();
            this.configPresetLabel = new System.Windows.Forms.Label();
            this.statPresetPanel = new System.Windows.Forms.Panel();
            this.statPresetComboBox = new Inkybot.Controls.ComboBox();
            this.statPresetLabel = new System.Windows.Forms.Label();
            this.previewPanel = new System.Windows.Forms.Panel();
            this.previewPictureBox = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            this.buttonsPanel.SuspendLayout();
            this.configPresetPanel.SuspendLayout();
            this.statPresetPanel.SuspendLayout();
            this.previewPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.previewPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonsPanel);
            this.groupBox1.Controls.Add(this.configPresetPanel);
            this.groupBox1.Controls.Add(this.statPresetPanel);
            this.groupBox1.Controls.Add(this.previewPanel);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(642, 120);
            this.groupBox1.BorderColor = System.Drawing.Color.FromArgb(((int) (((byte) (70)))), ((int) (((byte) (70)))), ((int) (((byte) (70)))));
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // buttonsPanel
            // 
            this.buttonsPanel.Controls.Add(this.moveUpButton);
            this.buttonsPanel.Controls.Add(this.moveDownButton);
            this.buttonsPanel.Controls.Add(this.removeButton);
            this.buttonsPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonsPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.buttonsPanel.Location = new System.Drawing.Point(536, 14);
            this.buttonsPanel.Name = "buttonsPanel";
            this.buttonsPanel.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.buttonsPanel.Size = new System.Drawing.Size(103, 99);
            this.buttonsPanel.TabIndex = 7;
            // 
            // moveUpButton
            // 
            this.moveUpButton.AutoSize = true;
            this.moveUpButton.BackColor = System.Drawing.Color.Black;
            this.moveUpButton.FlatAppearance.BorderSize = 0;
            this.moveUpButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.moveUpButton.ForeColor = System.Drawing.SystemColors.Control;
            this.moveUpButton.Location = new System.Drawing.Point(0,0);
            this.moveUpButton.Name = "moveUpButton";
            this.moveUpButton.Size = new System.Drawing.Size(93, 26);
            this.moveUpButton.TabIndex = 5;
            this.moveUpButton.Text = "Move up";
            this.moveUpButton.UseVisualStyleBackColor = false;
            // 
            // moveDownButton
            // 
            this.moveDownButton.AutoSize = true;
            this.moveDownButton.BackColor = System.Drawing.Color.Black;
            this.moveDownButton.FlatAppearance.BorderSize = 0;
            this.moveDownButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.moveDownButton.ForeColor = System.Drawing.SystemColors.Control;
            this.moveDownButton.Location = new System.Drawing.Point(0,0);
            this.moveDownButton.Name = "moveDownButton";
            this.moveDownButton.Size = new System.Drawing.Size(93, 26);
            this.moveDownButton.TabIndex = 6;
            this.moveDownButton.Text = "Move down";
            this.moveDownButton.UseVisualStyleBackColor = false;
            // 
            // removeButton
            // 
            this.removeButton.AutoSize = true;
            this.removeButton.BackColor = System.Drawing.Color.Black;
            this.removeButton.FlatAppearance.BorderSize = 0;
            this.removeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.removeButton.ForeColor = System.Drawing.SystemColors.Control;
            this.removeButton.Location = new System.Drawing.Point(0,0);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(93, 26);
            this.removeButton.TabIndex = 4;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = false;
            // 
            // configPresetPanel
            // 
            this.configPresetPanel.Controls.Add(this.configPresetComboBox);
            this.configPresetPanel.Controls.Add(this.configPresetLabel);
            this.configPresetPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.configPresetPanel.Location = new System.Drawing.Point(303, 18);
            this.configPresetPanel.Name = "configPresetPanel";
            this.configPresetPanel.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.configPresetPanel.Size = new System.Drawing.Size(200, 99);
            this.configPresetPanel.TabIndex = 2;
            // 
            // configPresetComboBox
            // 
            this.configPresetComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.configPresetComboBox.BackColor = System.Drawing.Color.Black;
            this.configPresetComboBox.ForeColor = System.Drawing.SystemColors.Control;
            this.configPresetComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.configPresetComboBox.FormattingEnabled = true;
            this.configPresetComboBox.Location = new System.Drawing.Point(10, 26);
            this.configPresetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.configPresetComboBox.Name = "configPresetComboBox";
            this.configPresetComboBox.Size = new System.Drawing.Size(180, 24);
            this.configPresetComboBox.TabIndex = 1;
            // 
            // configPresetLabel
            // 
            this.configPresetLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.configPresetLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.configPresetLabel.Location = new System.Drawing.Point(10, 0);
            this.configPresetLabel.Name = "configPresetLabel";
            this.configPresetLabel.Size = new System.Drawing.Size(180, 23);
            this.configPresetLabel.TabIndex = 0;
            this.configPresetLabel.Text = "Config Preset";
            this.configPresetLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // statPresetPanel
            // 
            this.statPresetPanel.Controls.Add(this.statPresetComboBox);
            this.statPresetPanel.Controls.Add(this.statPresetLabel);
            this.statPresetPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.statPresetPanel.Location = new System.Drawing.Point(103, 18);
            this.statPresetPanel.Name = "statPresetPanel";
            this.statPresetPanel.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.statPresetPanel.Size = new System.Drawing.Size(200, 99);
            this.statPresetPanel.TabIndex = 1;
            // 
            // statPresetComboBox
            // 
            this.statPresetComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.statPresetComboBox.BackColor = System.Drawing.Color.Black;
            this.statPresetComboBox.ForeColor = System.Drawing.SystemColors.Control;
            this.statPresetComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.statPresetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.statPresetComboBox.FormattingEnabled = true;
            this.statPresetComboBox.Location = new System.Drawing.Point(10, 26);
            this.statPresetComboBox.Name = "statPresetComboBox";
            this.statPresetComboBox.Size = new System.Drawing.Size(180, 24);
            this.statPresetComboBox.TabIndex = 1;
            // 
            // statPresetLabel
            // 
            this.statPresetLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.statPresetLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.statPresetLabel.Location = new System.Drawing.Point(10, 0);
            this.statPresetLabel.Name = "statPresetLabel";
            this.statPresetLabel.Size = new System.Drawing.Size(180, 23);
            this.statPresetLabel.TabIndex = 0;
            this.statPresetLabel.Text = "Stat Preset";
            this.statPresetLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // previewPanel
            // 
            this.previewPanel.Controls.Add(this.previewPictureBox);
            this.previewPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.previewPanel.Location = new System.Drawing.Point(3, 18);
            this.previewPanel.Name = "previewPanel";
            this.previewPanel.Size = new System.Drawing.Size(100, 99);
            this.previewPanel.TabIndex = 8;
            // 
            // previewPictureBox
            // 
            this.previewPictureBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.previewPictureBox.Location = new System.Drawing.Point(18, 1);
            this.previewPictureBox.Name = "previewPictureBox";
            this.previewPictureBox.Size = new System.Drawing.Size(64, 64);
            this.previewPictureBox.TabIndex = 0;
            this.previewPictureBox.TabStop = false;
            // 
            // MageQueueForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.ClientSize = new System.Drawing.Size(642, 450);
            this.Controls.Add(this.groupBox1);
            this.Name = "MageQueueForm";
            this.Text = "MageQueueForm";
            this.Load += new System.EventHandler(this.MageQueueForm_Loaded);
            this.groupBox1.ResumeLayout(false);
            this.buttonsPanel.ResumeLayout(false);
            this.buttonsPanel.PerformLayout();
            this.configPresetPanel.ResumeLayout(false);
            this.statPresetPanel.ResumeLayout(false);
            this.previewPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.previewPictureBox)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel previewPanel;

        private System.Windows.Forms.FlowLayoutPanel buttonsPanel;

        private System.Windows.Forms.Button moveUpButton;
        private System.Windows.Forms.Button moveDownButton;

        private System.Windows.Forms.Button removeButton;

        private System.Windows.Forms.Label configPresetLabel;
        private System.Windows.Forms.Panel configPresetPanel;

        private Inkybot.Controls.ComboBox statPresetComboBox;
        private Inkybot.Controls.ComboBox configPresetComboBox;

        private System.Windows.Forms.Label statPresetLabel;
        private System.Windows.Forms.Panel statPresetPanel;
        private System.Windows.Forms.PictureBox previewPictureBox;

        private Inkybot.Controls.GroupBox groupBox1;

        #endregion
    }
}

