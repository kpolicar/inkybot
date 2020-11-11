using System.ComponentModel;

namespace Inkybot
{
    partial class ConfigForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigForm));
            this.statsDataGridView = new System.Windows.Forms.DataGridView();
            this.StatColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PaRuneThresholdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RaRuneThresholdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaxSmRuneCanHitColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaxPaRuneCanHitColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.restoreHighSinkStatsCheckbox = new System.Windows.Forms.CheckBox();
            this.creditsNameLabel = new System.Windows.Forms.Label();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.creditsLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize) (this.statsDataGridView)).BeginInit();
            this.bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // statsDataGridView
            // 
            this.statsDataGridView.AllowUserToAddRows = false;
            this.statsDataGridView.AllowUserToResizeColumns = false;
            this.statsDataGridView.AllowUserToResizeRows = false;
            this.statsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.statsDataGridView.BackgroundColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.statsDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.statsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.statsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.statsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {this.StatColumn, this.PaRuneThresholdColumn, this.RaRuneThresholdColumn, this.MaxSmRuneCanHitColumn, this.MaxPaRuneCanHitColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (60)))), ((int) (((byte) (60)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.statsDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.statsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statsDataGridView.EnableHeadersVisualStyles = false;
            this.statsDataGridView.GridColor = System.Drawing.Color.FromArgb(((int) (((byte) (70)))), ((int) (((byte) (70)))), ((int) (((byte) (70)))));
            this.statsDataGridView.Location = new System.Drawing.Point(0, 0);
            this.statsDataGridView.MultiSelect = false;
            this.statsDataGridView.Name = "statsDataGridView";
            this.statsDataGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (64)))), ((int) (((byte) (64)))), ((int) (((byte) (64)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(((int) (((byte) (64)))), ((int) (((byte) (64)))), ((int) (((byte) (64)))));
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.statsDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.statsDataGridView.RowHeadersVisible = false;
            this.statsDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.statsDataGridView.Size = new System.Drawing.Size(775, 592);
            this.statsDataGridView.TabIndex = 1;
            // 
            // StatColumn
            // 
            this.StatColumn.HeaderText = "Stat";
            this.StatColumn.Name = "StatColumn";
            this.StatColumn.ReadOnly = true;
            // 
            // PaRuneThresholdColumn
            // 
            this.PaRuneThresholdColumn.HeaderText = "PA Rune Threshold";
            this.PaRuneThresholdColumn.Name = "PaRuneThresholdColumn";
            // 
            // RaRuneThresholdColumn
            // 
            this.RaRuneThresholdColumn.HeaderText = "RA Rune Threshold";
            this.RaRuneThresholdColumn.Name = "RaRuneThresholdColumn";
            // 
            // MaxSmRuneCanHitColumn
            // 
            this.MaxSmRuneCanHitColumn.HeaderText = "Max (SM Rune)";
            this.MaxSmRuneCanHitColumn.Name = "MaxSmRuneCanHitColumn";
            // 
            // MaxPaRuneCanHitColumn
            // 
            this.MaxPaRuneCanHitColumn.HeaderText = "Max (RA Rune)";
            this.MaxPaRuneCanHitColumn.Name = "MaxPaRuneCanHitColumn";
            // 
            // restoreHighSinkStatsCheckbox
            // 
            this.restoreHighSinkStatsCheckbox.AutoSize = true;
            this.restoreHighSinkStatsCheckbox.Dock = System.Windows.Forms.DockStyle.Left;
            this.restoreHighSinkStatsCheckbox.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.restoreHighSinkStatsCheckbox.ForeColor = System.Drawing.SystemColors.Control;
            this.restoreHighSinkStatsCheckbox.Location = new System.Drawing.Point(15, 10);
            this.restoreHighSinkStatsCheckbox.Name = "restoreHighSinkStatsCheckbox";
            this.restoreHighSinkStatsCheckbox.Size = new System.Drawing.Size(194, 21);
            this.restoreHighSinkStatsCheckbox.TabIndex = 0;
            this.restoreHighSinkStatsCheckbox.Text = "Restore high sink stats immediately";
            this.restoreHighSinkStatsCheckbox.UseVisualStyleBackColor = true;
            // 
            // creditsNameLabel
            // 
            this.creditsNameLabel.AutoSize = true;
            this.creditsNameLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.creditsNameLabel.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.creditsNameLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.creditsNameLabel.Location = new System.Drawing.Point(707, 10);
            this.creditsNameLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.creditsNameLabel.Name = "creditsNameLabel";
            this.creditsNameLabel.Size = new System.Drawing.Size(53, 13);
            this.creditsNameLabel.TabIndex = 1;
            this.creditsNameLabel.Text = "Tomolone\r\n";
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.creditsLabel);
            this.bottomPanel.Controls.Add(this.creditsNameLabel);
            this.bottomPanel.Controls.Add(this.restoreHighSinkStatsCheckbox);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 551);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.bottomPanel.Size = new System.Drawing.Size(775, 41);
            this.bottomPanel.TabIndex = 3;
            // 
            // creditsLabel
            // 
            this.creditsLabel.AutoSize = true;
            this.creditsLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.creditsLabel.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.creditsLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.creditsLabel.Location = new System.Drawing.Point(596, 10);
            this.creditsLabel.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.creditsLabel.Name = "creditsLabel";
            this.creditsLabel.Size = new System.Drawing.Size(111, 13);
            this.creditsLabel.TabIndex = 2;
            this.creditsLabel.Text = "Configured with  ❤ ️ by";
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.ClientSize = new System.Drawing.Size(775, 592);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.statsDataGridView);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Name = "ConfigForm";
            this.Text = "Config - Inkybot";
            this.Load += new System.EventHandler(this.ConfigForm_OnLoad);
            ((System.ComponentModel.ISupportInitialize) (this.statsDataGridView)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel bottomPanel;

        private System.Windows.Forms.Label creditsLabel;
        private System.Windows.Forms.Label creditsNameLabel;

        private System.Windows.Forms.CheckBox restoreHighSinkStatsCheckbox;

        private System.Windows.Forms.DataGridViewTextBoxColumn MaxPaRuneCanHitColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn MaxSmRuneCanHitColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn PaRuneThresholdColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn RaRuneThresholdColumn;

        private System.Windows.Forms.DataGridView statsDataGridView;

        private System.Windows.Forms.DataGridViewTextBoxColumn StatColumn;

        #endregion
    }
}

